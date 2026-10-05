using System.Text.Json;

namespace LossPrevention.Application.Helpers
{
    public static class JsonHelper
    {
        public static object? ConvertJsonElement(object? value)
        {
            if (value is not JsonElement json) return value;

            return json.ValueKind switch
            {
                JsonValueKind.String => json.GetString(),
                JsonValueKind.Number => json.TryGetInt64(out var l) ? l :
                                        json.TryGetDecimal(out var d) ? d :
                                        json.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => json.ToString()
            };
        }
    }
}
