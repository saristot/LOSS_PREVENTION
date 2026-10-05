using LossPrevention.Domain.Entities.Rules;
using MongoDB.Bson;

namespace LossPrevention.Application.Helpers
{
    public static class RuleHelper
    {
        public static bool EvaluateRule(BsonDocument document, RuleConfiguration rule)
        {
            if (string.IsNullOrEmpty(rule.FieldPath))
                return false;

            var values = GetValuesByPath(document, rule.FieldPath);

            if (rule.SumValues)
            {
                var numericValues = values.Where(v => v.IsNumeric).Select(v => v.ToDecimal());
                var sum = numericValues.Sum();
                return CompareValue(sum, rule);
            }

            return values.Any(v => CompareValue(v, rule));
        }

        private static List<BsonValue> GetValuesByPath(BsonDocument document, string path)
        {
            var segments = path.Split('.');
            return Traverse(document, segments, 0);
        }

        private static List<BsonValue> Traverse(BsonValue current, string[] segments, int index)
        {
            if (index >= segments.Length)
                return new List<BsonValue> { current };

            var nextSegment = segments[index];

            if (current is BsonDocument doc && doc.Contains(nextSegment))
                return Traverse(doc[nextSegment], segments, index + 1);

            if (current is BsonArray arr)
                return arr.SelectMany(item => Traverse(item, segments, index)).ToList();

            return new List<BsonValue>();
        }

        private static bool CompareValue(BsonValue value, RuleConfiguration rule)
        {
            if (rule.AllowRangeCheck)
            {
                if (!value.IsNumeric)
                    return false;

                var decimalValue = value.ToDecimal();
                var min = rule.MinValue ?? decimal.MinValue;
                var max = rule.MaxValue ?? decimal.MaxValue;

                return decimalValue >= min && decimalValue <= max;
            }

            return value != null && value.ToString().Equals(rule.ValueToCheck, StringComparison.OrdinalIgnoreCase);
        }

        private static bool CompareValue(decimal sum, RuleConfiguration rule)
        {
            var min = rule.MinValue ?? decimal.MinValue;
            var max = rule.MaxValue ?? decimal.MaxValue;

            return sum >= min && sum <= max;
        }
    }
}
