using MongoDB.Bson;

namespace LossPrevention.Domain.Entities.Rules
{
    public sealed class RuleConfiguration
    {
        public ObjectId _id { get; set; }
        public string RuleName { get; set; }
        public string RuleDescription { get; set; }
        public string FieldPath { get; set; } // e.g., "Tender.Amount" or "Total"
        public string ValueToCheck { get; set; } // or use string and parse if needed
        public bool AllowRangeCheck { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public bool Enabled { get; set; }
        public bool SumValues { get; set; } // To sum values across array fields
    }

}
