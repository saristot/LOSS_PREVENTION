using MongoDB.Bson;

namespace LossPrevention.Application.Helpers
{
    // Euclidean distance
    public static class DistanceHelper
    {
        public static (double Distance, List<string> FieldsUsed) ComputeDistance(Dictionary<string, object> a, Dictionary<string, object> b, List<string> fields)
        {
            double sum = 0;
            List<string> usedFields = new();

            foreach (var field in fields)
            {
                if (!a.ContainsKey(field) || !b.ContainsKey(field)) continue;

                if (!TryNormalize(a[field], out var normA) || !TryNormalize(b[field], out var normB))
                    continue;

                sum += Math.Pow(normA - normB, 2);
                usedFields.Add(field);
            }

            return (Math.Sqrt(sum), usedFields);
        }

        private static bool TryNormalize(object value, out double result)
        {
            result = 0;

            switch (value)
            {
                case int i:
                    result = i;
                    return true;
                case long l:
                    result = l;
                    return true;
                case double d:
                    result = d;
                    return true;
                case decimal dec:
                    result = (double)dec;
                    return true;
                case float f:
                    result = f;
                    return true;
                case bool b:
                    result = b ? 1 : 0;
                    return true;
                case IEnumerable<object> list:
                    var normalized = list
                        .Select(v => TryNormalize(v, out var n) ? n : (double?)null)
                        .Where(n => n.HasValue)
                        .Select(n => n.Value)
                        .ToList();
                    if (normalized.Any())
                    {
                        result = normalized.Average();
                        return true;
                    }
                    return false;
                case IEnumerable<decimal> decList:
                    if (decList.Any())
                    {
                        result = (double)decList.Average();
                        return true;
                    }
                    return false;
                case string s when double.TryParse(s, out var parsed):
                    result = parsed;
                    return true;
            }

            return false;
        }

    }
}
