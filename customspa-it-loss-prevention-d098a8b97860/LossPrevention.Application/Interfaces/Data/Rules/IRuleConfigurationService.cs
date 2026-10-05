using LossPrevention.Domain.Entities.Rules;
using MongoDB.Bson;

namespace LossPrevention.Application.Interfaces.Data.Rules
{
    public interface IRuleConfigurationService
    {
        /// <summary>
        /// Evaluates rules against a given document and returns results as key-value pairs.
        /// </summary>
        /// <param name="request">The document and optional custom rules to evaluate.</param>
        /// <returns>A dictionary of rule results.</returns>
        Task<Dictionary<string, int>> ApplyRulesAsync();

        /// <summary>
        /// Retrieves all rule configurations.
        /// </summary>
        Task<IEnumerable<RuleConfiguration>> GetAllRulesAsync();

        /// <summary>
        /// Retrieves a specific rule by ID.
        /// </summary>
        Task<RuleConfiguration?> GetRuleByIdAsync(ObjectId id);

        /// <summary>
        /// Creates a new rule configuration.
        /// </summary>
        Task<RuleConfiguration> CreateRuleAsync(RuleConfiguration rule, string collectionName);

        /// <summary>
        /// Updates an existing rule configuration.
        /// </summary>
        Task<bool> UpdateRuleAsync(RuleConfiguration rule, string collectionName);

        /// <summary>
        /// Deletes a rule configuration by ID.
        /// </summary>
        Task<bool> DeleteRuleAsync(ObjectId id);
    }

}
