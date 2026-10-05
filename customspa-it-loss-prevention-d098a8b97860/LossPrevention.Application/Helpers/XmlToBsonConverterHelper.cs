using MongoDB.Bson;
using System.Globalization;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Linq;

public static class XmlToBsonConverterHelper
{
    public static BsonDocument ConvertFlattened(string xml)
    {
        var element = XElement.Parse(xml);
        var flat = new Dictionary<string, object>();
        FlattenElement(element, flat);
        return new BsonDocument(flat);
    }

    private static void FlattenElement(XElement element, Dictionary<string, object> result)
    {
        string elementName = element.Name.LocalName;

        // Add attributes with @ prefix (keep as strings unless you want to type them too)
        foreach (var attr in element.Attributes())
        {
            result[$"@{attr.Name.LocalName}"] = attr.Value;
        }

        // Handle children
        if (element.HasElements)
        {
            var childGroups = element.Elements().GroupBy(e => e.Name.LocalName);

            foreach (var group in childGroups)
            {
                string childName = group.Key;

                if (group.Count() > 1)
                {
                    var list = new List<BsonDocument>();
                    foreach (var item in group)
                    {
                        var childDoc = new Dictionary<string, object>();
                        FlattenElement(item, childDoc);
                        list.Add(new BsonDocument(childDoc));
                    }
                    result[childName] = new BsonArray(list);
                }
                else
                {
                    foreach (var item in group)
                        FlattenElement(item, result);
                }
            }
        }
        else
        {
            result[elementName] = ParseValue(element.Value);
        }
    }

    // --- helpers to decide number shape --------------------------------
    private static bool LooksLikeInteger(string s)
    {
        // optional leading sign, then digits only
        return Regex.IsMatch(s, @"^\s*[+-]?\d+\s*$");
    }

    private static bool LooksLikeDecimal(string s)
    {
        // contains a decimal point or exponent notation
        return Regex.IsMatch(s, @"[.\eE]");
    }

    private static object ParseValue(string value)
    {
        if (value is null) return null;
        value = value.Trim();

        // bool
        if (bool.TryParse(value, out var b)) return b;

        // integers FIRST (so "1025" => Int32/Int64, not Decimal128)
        if (LooksLikeInteger(value))
        {
            // Try Int32 first, then Int64
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i32))
                return i32;

            if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i64))
                return i64;
        }

        // decimals ONLY if it looks like a decimal (has '.' or exponent)
        if (LooksLikeDecimal(value))
        {
            // high precision where appropriate -> BSON Decimal128 in Mongo
            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec))
                return dec;
        }

        // dates (sane year guard)
        if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dto))
        {
            if (dto.Year is >= 1900 and <= 2100)
                return dto.UtcDateTime; // BSON DateTime
        }

        // fallback: string
        return value;
    }

    public static Dictionary<string, object> BsonToPlainDictionary(BsonDocument doc)
    {
        var dict = new Dictionary<string, object>();

        foreach (var element in doc.Elements)
        {
            dict[element.Name] = ConvertBsonValue(element.Value);
        }

        return dict;
    }

    private static object ConvertBsonValue(BsonValue value)
    {
        return value.BsonType switch
        {
            BsonType.Null => null,
            BsonType.Boolean => value.AsBoolean,
            BsonType.Int32 => value.AsInt32,
            BsonType.Int64 => value.AsInt64,
            BsonType.Double => value.AsDouble,
            BsonType.Decimal128 => ToNetDecimal(value.AsDecimal128), // <-- important
            BsonType.String => value.AsString,
            BsonType.DateTime => value.ToUniversalTime(),
            BsonType.Document => BsonToPlainDictionary(value.AsBsonDocument),
            BsonType.Array => value.AsBsonArray.Select(ConvertBsonValue).ToList(),
            BsonType.ObjectId => value.AsObjectId.ToString(),
            _ => value.ToString()
        };
    }

    private static object ToNetDecimal(Decimal128 d128)
    {
        try
        {
            // Some driver versions expose ToDecimal as a static method
            return Decimal128.ToDecimal(d128);
        }
        catch
        {
            // out-of-range for decimal — fall back to string to keep it JSON-serializable
            return d128.ToString();
        }
    }
}
