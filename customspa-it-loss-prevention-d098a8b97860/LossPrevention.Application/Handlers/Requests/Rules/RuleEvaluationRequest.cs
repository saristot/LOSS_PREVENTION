using LossPrevention.Domain.Entities.Rules;

namespace LossPrevention.API.Handlers.Requests.Rules
{
    public class RuleEvaluationRequest
    {
        public Dictionary<string, object> Document { get; set; } = new();
        public List<RuleConfiguration>? Rules { get; set; } // Optional - use all enabled rules if null
    }

}
