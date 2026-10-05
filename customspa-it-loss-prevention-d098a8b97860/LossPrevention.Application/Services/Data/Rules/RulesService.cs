using LossPrevention.Application.Helpers;
using LossPrevention.Application.Interfaces.Data;
using LossPrevention.Application.Interfaces.Data.Rules;
using LossPrevention.Application.Services.Data;
using LossPrevention.Domain.Entities.Data;
using LossPrevention.Domain.Entities.Rules;
using LossPrevention.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections;

namespace LossPrevention.Application.Services.Rules
{
    public sealed class RuleConfigurationService : IRuleConfigurationService
    {
        private readonly IMongoRepository<BsonDocument> _reportDataRepository;
        private readonly IMongoRepository<RuleConfiguration> _rulesRepository;
        private readonly IMappingService _mappingService;

        public RuleConfigurationService(
            IMongoRepository<BsonDocument> reportDataRepository,
            IMongoRepository<RuleConfiguration> rulesRepository,
            IMappingService mappingService)
        {
            _reportDataRepository = reportDataRepository;
            _rulesRepository = rulesRepository;
            _mappingService = mappingService;
        }


        public async Task<Dictionary<string, int>> ApplyRulesAsync()
        {
            var docs = (await _reportDataRepository.GetAllAsync()).ToList();
            var rules = (await _rulesRepository.GetAllAsync()).Where(r => r.Enabled).ToList();

            var results = new List<Dictionary<string, object>>();

            // Delete existing FraudFlags from all documents for speed
            var unsetUpdate = Builders<BsonDocument>.Update.Unset("FraudFlags");
            await _reportDataRepository.Collection.UpdateManyAsync(FilterDefinition<BsonDocument>.Empty, unsetUpdate);

            foreach (var doc in docs)
            {
                var id = doc.GetValue("_id").ToString();
                var fraudFlags = new BsonDocument();

                foreach (var rule in rules)
                {
                    var isMatch = RuleHelper.EvaluateRule(doc, rule);
                    fraudFlags[rule.RuleName] = isMatch;
                }

                // Add the new FraudFlags
                doc["FraudFlags"] = fraudFlags;

                var filter = Builders<BsonDocument>.Filter.Eq("_id", doc["_id"]);
                await _reportDataRepository.Collection.ReplaceOneAsync(filter, doc);

                results.Add(new Dictionary<string, object>
                {
                    ["_id"] = id,
                    ["FraudFlags"] = fraudFlags
                });
            }

            // Delete any existing fraud flags from mappings
            var mappings = (await _mappingService.GetAllMappings()).ToList();
            foreach (var mapping in mappings)
            {
                if (mapping.Name.Contains("FraudFlags"))
                {
                    await _mappingService.DeleteMappingAsync(mapping._id);
                }
            }

            // Ensure new fields are added.
            await _mappingService.ProcessMappings("ReportData", 1000);

            return new Dictionary<string, int>
            {
                ["TotalDocuments"] = results.Count
            };
        }

        public async Task<IEnumerable<RuleConfiguration>> GetAllRulesAsync()
        {
            return await _rulesRepository.GetAllAsync();
        }

        public async Task<RuleConfiguration?> GetRuleByIdAsync(ObjectId id)
        {
            var filter = Builders<RuleConfiguration>.Filter.Eq(r => r._id, id);
            return await _rulesRepository.FindOneAsync(filter);
        }

        public async Task<RuleConfiguration> CreateRuleAsync(RuleConfiguration rule, string collectionName)
        {
            await _rulesRepository.InsertOneAsync(rule);

            var mapping = new MappingItem
            {
                Name = $"FraudFlags.{rule.RuleName}",
                Alias = $"{rule.RuleName}",
                DataType = "Boolean",
                IsVisible = true,
                IsArray = false,
                IsCalculated = false,
                IsLookup = false,
                CollectionName = collectionName
            };

            await _mappingService.AddMappingAsync(mapping);

            return rule;
        }

        public async Task<bool> UpdateRuleAsync(RuleConfiguration rule, string collectionName)
        {
            var existingRule = await _rulesRepository.FindOneAsync(r => r._id == rule._id);
            if (existingRule == null)
                return false;

            var filter = Builders<RuleConfiguration>.Filter.Eq(r => r._id, rule._id);
            var update = Builders<RuleConfiguration>.Update
                .Set(r => r.RuleName, $"{rule.RuleName}")
                .Set(r => r.RuleDescription, rule.RuleDescription)
                .Set(r => r.FieldPath, rule.FieldPath)
                .Set(r => r.ValueToCheck, rule.ValueToCheck)
                .Set(r => r.AllowRangeCheck, rule.AllowRangeCheck)
                .Set(r => r.MinValue, rule.MinValue)
                .Set(r => r.MaxValue, rule.MaxValue)
                .Set(r => r.Enabled, rule.Enabled)
                .Set(r => r.SumValues, rule.SumValues);

            var result = await _rulesRepository.UpdateOneAsync(filter, update);

            if (result.ModifiedCount > 0)
            {
                // Re-evaluate this rule for all documents
                var docs = await _reportDataRepository.GetAllAsync();
                foreach (var doc in docs)
                {
                    var isMatch = RuleHelper.EvaluateRule(doc, rule);

                    var docFilter = Builders<BsonDocument>.Filter.Eq("_id", doc["_id"]);
                    var docUpdate = Builders<BsonDocument>.Update.Set($"{rule.RuleName}", isMatch);
                    await _reportDataRepository.Collection.UpdateOneAsync(docFilter, docUpdate);
                }

                // If name changed, update mapping
                if (existingRule.RuleName != rule.RuleName)
                {
                    var mappings = await _mappingService.GetAllMappings();
                    var mappingToUpdate = mappings.FirstOrDefault(m => m.Name == $"{existingRule.RuleName}");
                    if (mappingToUpdate != null)
                    {
                        var updatedMapping = new MappingItem
                        {
                            Name = $"FraudFlags.{rule.RuleName}",
                            Alias = rule.RuleName,
                            DataType = "Boolean",
                            IsVisible = true,
                            IsArray = false,
                            IsCalculated = false,
                            IsLookup = false,
                            CollectionName = collectionName
                        };

                        await _mappingService.UpdateMappingAsync(mappingToUpdate._id.ToString(), updatedMapping);
                    }
                }
            }

            // MatchedCount > 0 means the rule exists and was processed — return true even if no
            // fields changed (MongoDB won't increment ModifiedCount for identical values).
            return result.MatchedCount > 0;
        }

        public async Task<bool> DeleteRuleAsync(ObjectId id)
        {
            // Fetch the rule to get its name
            var rule = await _rulesRepository.FindOneAsync(r => r._id == id);
            if (rule == null)
                return false;

            // Delete the rule configuration
            var result = await _rulesRepository.DeleteByIdAsync(id);

            if (result.DeletedCount > 0)
            {
                // Remove the rule entry from FraudFlags in all documents
                var update = Builders<BsonDocument>.Update.Unset($"FraudFlags.{rule.RuleName}");
                await _reportDataRepository.Collection.UpdateManyAsync(FilterDefinition<BsonDocument>.Empty, update);

                // Remove the mapping
                var mappings = await _mappingService.GetAllMappings();
                var mappingToDelete = mappings.FirstOrDefault(m => m.Name == $"FraudFlags.{rule.RuleName}");
                if (mappingToDelete != null)
                {
                    await _mappingService.DeleteMappingAsync(mappingToDelete._id);
                }

                return true;
            }

            return false;
        }

    }
}
