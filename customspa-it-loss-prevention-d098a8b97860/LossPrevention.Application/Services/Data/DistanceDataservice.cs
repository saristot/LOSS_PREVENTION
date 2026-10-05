using LossPrevention.Application.DTO.Data;
using LossPrevention.Application.Helpers;
using LossPrevention.Application.Interfaces.Data;
using LossPrevention.Domain.Entities.Data;
using LossPrevention.Infrastructure.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LossPrevention.Application.Services.Data
{
    public sealed class DistanceDataService : IDistanceDataservice
    {
        private readonly IMongoRepository<BsonDocument> _reportDataRepository;
        private readonly IMappingService _mappingService;

        public DistanceDataService(IMongoRepository<BsonDocument> reportDataRepository, IMappingService mappingService)
        {
            _reportDataRepository = reportDataRepository;
            _mappingService = mappingService;
        }

        public async Task<List<DistanceResultDto>> GetDistanceAsync(
            string sourceDocumentId,
            string startDateField,
            string endDateField,
            DateTime startDate,
            DateTime endDate,
            string keyField,
            List<string> comparisonFields)
        {
            // Remove _id, so we dont compare that field.
            comparisonFields = comparisonFields.Where(x => x != "_id").ToList();

            var filterDefinition = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Gte(startDateField, startDate),
                Builders<BsonDocument>.Filter.Lte(endDateField, endDate)
            );

            var allDocs = await _reportDataRepository.FindManyAsync(filterDefinition);

            var allMapped = new List<Dictionary<string, object>>();

            var mappings = await _mappingService.GetAllMappings();

            foreach (var doc in allDocs)
            {
                var mapped = new Dictionary<string, object>();

                foreach (var map in mappings.Where(m => m.CollectionName == "ReportData" && comparisonFields.Contains(m.Name)))
                {
                    if (BsonHelper.TryGetNestedValue(doc, map.Name, out var val))
                    {
                        object parsed = map.DataType switch
                        {
                            "Decimal" => val.IsBsonArray ? val.AsBsonArray.Select(v => v.ToDecimal()).ToList() : val.ToDecimal(),
                            "Integer" => val.IsBsonArray ? val.AsBsonArray.Select(v => v.ToInt32()).ToList() : val.ToInt32(),
                            "Boolean" => val.IsBsonArray ? val.AsBsonArray.Select(v => v.ToBoolean()).ToList() : val.ToBoolean(),
                            "DateTime" => val.IsBsonArray ? val.AsBsonArray.Select(v => v.ToUniversalTime()).ToList() : val.ToUniversalTime(),
                            _ => val.IsBsonArray ? val.AsBsonArray.Select(v => v.ToString()).ToList() : val.ToString()
                        };

                        mapped[map.Name] = parsed;
                    }
                }

                //foreach (var countField in comparisonFields.Where(f => f.EndsWith("Count")))
                //{
                //    var baseField = countField.Substring(0, countField.Length - "Count".Length);

                //    if (!mapped.ContainsKey(countField) &&
                //        doc.TryGetValue(baseField, out var arrayVal) &&
                //        arrayVal.IsBsonArray)
                //    {
                //        mapped[countField] = arrayVal.AsBsonArray.Count;
                //    }
                //}

                mapped["_id"] = doc["_id"].ToString();
                mapped["FullDocument"] = doc;
                allMapped.Add(mapped);
            }

            var target = allMapped.FirstOrDefault(x => x["_id"].ToString() == sourceDocumentId);
            if (target == null)
            {
                Console.WriteLine("Target not found.");
                return new List<DistanceResultDto>();
            }

            var others = allMapped.Where(x => x["_id"].ToString() != target["_id"].ToString()).ToList();

            double maxDistance = 0;
            var preDistances = new List<(Dictionary<string, object> Item, double Distance, List<string> UsedFields)>();

            foreach (var item in others)
            {
                var (distance, usedFields) = DistanceHelper.ComputeDistance(target, item, comparisonFields);
                if (usedFields.Count >= 3)
                {
                    preDistances.Add((item, distance, usedFields));
                    if (distance > maxDistance) maxDistance = distance;
                }
            }

            var distanceResultList = new List<(string Id, double Distance, double Score, List<string> UsedFields, BsonDocument Doc)>();

            foreach (var (item, distance, usedFields) in preDistances)
            {
                double fieldMatch = (double)usedFields.Count / comparisonFields.Count;
                double distanceMatch = maxDistance > 0 ? 1.0 - (distance / maxDistance) : 1.0;
                double similarityScore = fieldMatch * 0.5 + distanceMatch * 0.5;

                distanceResultList.Add((item["_id"].ToString(), distance, similarityScore, usedFields, (BsonDocument)item["FullDocument"]));
            }

            var nearest = distanceResultList
                .OrderByDescending(r => r.Score)
                .ThenBy(r => r.Distance)
                .Take(10)
                .ToList();

            var results = new List<DistanceResultDto>();
            foreach (var (id, distance, score, usedFields, doc) in nearest)
            {
                double fieldMatch = (double)usedFields.Count / comparisonFields.Count;
                double distanceMatch = maxDistance > 0 ? 1.0 - (distance / maxDistance) : 1.0;


                // Get all compared fields values
                var comparedFields = new Dictionary<string, object>();

                foreach (var field in usedFields)
                {
                    if (BsonHelper.TryGetNestedValue(doc, field, out var bsonVal))
                    {
                        if (bsonVal.IsBsonArray)
                        {
                            var list = new List<object>();
                            foreach (var val in bsonVal.AsBsonArray)
                            {
                                list.Add(BsonHelper.ConvertBsonValue(val));
                            }

                            if (list.All(v => v is decimal or double or float or int))
                            {
                                var numericList = list.Select(Convert.ToDouble).ToList();
                                comparedFields[field] = Math.Round(numericList.Average(), 2);
                            }
                            else
                            {
                                comparedFields[field] = list;
                            }
                        }
                        else
                        {
                            var val = BsonHelper.ConvertBsonValue(bsonVal);
                            comparedFields[field] = val is bool b ? (b ? 1 : 0) : val;
                        }

                    }
                }

                if (!string.IsNullOrWhiteSpace(keyField) && BsonHelper.TryGetNestedValue(doc, keyField, out var keyValue))
                {
                    var keyFieldValue = keyValue;



                    var obj = new DistanceResultDto()
                    {
                        Id = id,
                        Score = $"{score * 100:F2}%",
                        FieldMatch = $"{fieldMatch * 100:F2}%",
                        DistanceMatch = $"{distanceMatch * 100:F2}%",
                        FieldsUsed = string.Join(", ", usedFields),
                        KeyField = new KeyField() { Name = keyField, Value = keyFieldValue.AsString },
                        ComparedFields = comparedFields
                    };

                    results.Add(obj);
                }
            }

            return results;
        }
    }

}
