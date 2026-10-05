using LossPrevention.Domain.Entities.Data;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LossPrevention.Application.Helpers
{
    public static class BsonHelper
    {
        public static void HandleBsonArray(BsonElement field, Dictionary<string, string> fieldDict)
        {
            foreach (var item in field.Value.AsBsonArray)
            {
                if (item.IsBsonDocument)
                {
                    foreach (var subItem in item.AsBsonDocument)
                    {
                        fieldDict.TryAdd(subItem.Name, subItem.Value.ToString());
                    }
                }
            }
        }

        public static void HandleBsonDateTime(BsonElement field, Dictionary<string, string> fieldDict)
        {
            var success = DateTime.TryParseExact(field.Value.ToString(), "yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedResult);

            if (success)
            {
                _ = fieldDict.TryAdd(field.Name, parsedResult.ToString());
            }
            else
            {
                _ = fieldDict.TryAdd(field.Name, field.Value.ToString());
            }
        }

        public static void HandleBsonDocument(BsonElement field, Dictionary<string, string> fieldDict)
        {
            if (field.Name == "_id" && field.Value.IsBsonDocument)
            {
                foreach (var subItem in field.Value.AsBsonDocument)
                {
                    HandleBsonDateTime(subItem, fieldDict);
                    _ = fieldDict.TryAdd(subItem.Name, subItem.Value.ToString());
                }
            }
            else if (field.Name != "_id")
            {
                _ = fieldDict.TryAdd(field.Name, field.Value.ToString());
            }
        }
        public static object ConvertBsonValue(BsonValue val)
        {
            return val.BsonType switch
            {
                BsonType.Int32 => val.AsInt32,
                BsonType.Int64 => val.AsInt64,
                BsonType.Double => val.AsDouble,
                BsonType.Boolean => val.AsBoolean,
                BsonType.String => val.AsString,
                BsonType.Decimal128 => val.ToDecimal(), // ✅ Fixes your issue
                BsonType.DateTime => val.ToUniversalTime(),
                _ => val.ToString()
            };
        }


        public static bool TryGetNestedValue(BsonDocument doc, string path, out BsonValue value)
        {
            value = null;
            var parts = path.Split('.');
            BsonValue current = doc;

            foreach (var part in parts)
            {
                if (current is BsonDocument bdoc)
                {
                    if (!bdoc.TryGetValue(part, out current))
                    {
                        return false;
                    }
                }
                else if (current is BsonArray barray)
                {
                    var collected = new BsonArray();

                    foreach (var element in barray)
                    {
                        if (element is BsonDocument edoc && edoc.TryGetValue(part, out var innerValue))
                        {
                            collected.Add(innerValue);
                        }
                    }

                    if (collected.Count == 0)
                        return false;

                    current = collected;
                }
                else
                {
                    return false;
                }
            }

            value = current;
            return true;
        }

        public static BsonValue ConvertToMappedType(BsonValue value, MappingItem mapping)
        {
            // Fast null/empty path
            if (value == null || value.IsBsonNull || IsEmptyString(value))
                return GetDefaultOrNull(mapping);

            // Normalize type string once
            var type = mapping.DataType?.Trim();
            if (string.IsNullOrEmpty(type))
                return value;

            switch (type)
            {
                case "Integer":
                    if (TryToInt32(value, out var i32)) return new BsonInt32(i32);
                    return GetOnFail(value, mapping);

                case "Long":
                    if (TryToInt64(value, out var i64)) return new BsonInt64(i64);
                    return GetOnFail(value, mapping);

                case "Decimal":
                    if (TryToDecimal(value, out var dec)) return new BsonDecimal128(dec);
                    return GetOnFail(value, mapping);

                case "Double":
                    if (TryToDouble(value, out var dbl)) return new BsonDouble(dbl);
                    return GetOnFail(value, mapping);

                case "Boolean":
                    if (TryToBoolean(value, out var b)) return new BsonBoolean(b);
                    return GetOnFail(value, mapping);

                case "Date":
                    if (TryToDateTimeUtc(value, mapping?.Format, out var dt))
                        return new BsonDateTime(dt);
                    return GetOnFail(value, mapping);

                case "String":
                    // Avoid ToString for non-string BSON unless needed
                    if (value.IsString) return value;
                    return new BsonString(value.ToString());

                default:
                    // Unknown/unsupported type: keep original
                    return value;
            }
        }

        // ---------- helpers ----------

        private static bool IsEmptyString(BsonValue v)
            => (v is BsonString s && string.IsNullOrWhiteSpace(s.AsString)) ||
               (!v.IsString && v.IsBsonNull);

        private static BsonValue GetDefaultOrNull(MappingItem mapping)
            => mapping?.SetDefaultValue == true && !string.IsNullOrWhiteSpace(mapping.DefaultValue)
               ? BsonValue.Create(mapping.DefaultValue)
               : BsonNull.Value;

        private static BsonValue GetOnFail(BsonValue original, MappingItem mapping)
            => mapping?.SetDefaultValue == true && !string.IsNullOrWhiteSpace(mapping.DefaultValue)
               ? BsonValue.Create(mapping.DefaultValue)
               : original;

        private static bool TryToInt32(BsonValue v, out int result)
        {
            if (v.IsInt32) { result = v.AsInt32; return true; }
            if (v.IsInt64)
            {
                long l = v.AsInt64;
                if (l <= int.MaxValue && l >= int.MinValue) { result = (int)l; return true; }
            }
            if (v.IsDouble)
            {
                double d = v.AsDouble;
                if (d <= int.MaxValue && d >= int.MinValue && Math.Abs(d % 1) < double.Epsilon)
                { result = (int)d; return true; }
            }
            if (!v.IsString) { result = default; return false; }
            return int.TryParse(v.AsString, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
        }

        private static bool TryToInt64(BsonValue v, out long result)
        {
            if (v.IsInt64) { result = v.AsInt64; return true; }
            if (v.IsInt32) { result = v.AsInt32; return true; }
            if (v.IsDouble)
            {
                double d = v.AsDouble;
                if (d <= long.MaxValue && d >= long.MinValue && Math.Abs(d % 1) < double.Epsilon)
                { result = (long)d; return true; }
            }
            if (!v.IsString) { result = default; return false; }
            return long.TryParse(v.AsString, NumberStyles.Integer, CultureInfo.InvariantCulture, out result);
        }

        private static bool TryToDecimal(BsonValue v, out decimal result)
        {
            if (v.IsDecimal128) { result = v.AsDecimal; return true; }
            if (v.IsInt32) { result = v.AsInt32; return true; }
            if (v.IsInt64) { result = v.AsInt64; return true; }
            if (v.IsDouble)
            {
                // Convert with care: doubles may overflow decimal
                try { result = Convert.ToDecimal(v.AsDouble, CultureInfo.InvariantCulture); return true; }
                catch { /* avoid throw */ }
            }
            if (!v.IsString) { result = default; return false; }
            return decimal.TryParse(v.AsString, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out result);
        }

        private static bool TryToDouble(BsonValue v, out double result)
        {
            if (v.IsDouble) { result = v.AsDouble; return true; }
            if (v.IsInt32) { result = v.AsInt32; return true; }
            if (v.IsInt64) { result = v.AsInt64; return true; }
            if (!v.IsString) { result = default; return false; }
            return double.TryParse(v.AsString, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out result);
        }

        private static bool TryToBoolean(BsonValue v, out bool result)
        {
            if (v.IsBoolean) { result = v.AsBoolean; return true; }
            if (v.IsInt32) { result = v.AsInt32 != 0; return true; }
            if (v.IsInt64) { result = v.AsInt64 != 0; return true; }
            if (v.IsDouble) { result = Math.Abs(v.AsDouble) > double.Epsilon; return true; }
            if (!v.IsString) { result = default; return false; }

            var s = v.AsString?.Trim();
            if (string.IsNullOrEmpty(s)) { result = default; return false; }

            // Common truthy/falsey tokens
            switch (s.ToLowerInvariant())
            {
                case "true":
                case "t":
                case "yes":
                case "y":
                case "1":
                case "on":
                    result = true; return true;
                case "false":
                case "f":
                case "no":
                case "n":
                case "0":
                case "off":
                    result = false; return true;
                default:
                    return bool.TryParse(s, out result);
            }
        }

        private static bool TryToDateTimeUtc(BsonValue v, string format, out DateTime dtUtc)
        {
            // Already a BsonDateTime
            if (v.IsValidDateTime) // covers BsonDateTime and some numeric edge cases internally
            {
                try
                {
                    var dt = v.ToUniversalTime(); // safe for BsonDateTime
                    dtUtc = dt.Kind == DateTimeKind.Utc ? dt : dt.ToUniversalTime();
                    return true;
                }
                catch { /* fall through */ }
            }

            // Numeric epoch?
            if (v.IsInt64 || v.IsInt32 || v.IsDouble)
            {
                var asLong = v.IsInt64 ? v.AsInt64 :
                             v.IsInt32 ? (long)v.AsInt32 :
                             (long)v.AsDouble;

                // Heuristic: treat 13+ digits as milliseconds, 10 digits as seconds
                if (asLong > 10_000_000_000L) // ms
                {
                    dtUtc = DateTimeOffset.FromUnixTimeMilliseconds(asLong).UtcDateTime;
                    return true;
                }
                if (asLong >= 0 && asLong < 10_000_000_000L) // s
                {
                    dtUtc = DateTimeOffset.FromUnixTimeSeconds(asLong).UtcDateTime;
                    return true;
                }
            }

            // String parsing
            if (v.IsString)
            {
                var s = v.AsString?.Trim();
                if (!string.IsNullOrEmpty(s))
                {
                    // 1) Exact format if provided
                    if (!string.IsNullOrWhiteSpace(format)
                        && DateTime.TryParseExact(s, format, CultureInfo.InvariantCulture,
                                                  DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                                                  out var exact))
                    {
                        dtUtc = DateTime.SpecifyKind(exact, DateTimeKind.Utc);
                        return true;
                    }

                    // 2) ISO-8601 or general parse
                    if (DateTime.TryParse(s, CultureInfo.InvariantCulture,
                                          DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                                          out var any))
                    {
                        dtUtc = DateTime.SpecifyKind(any, DateTimeKind.Utc);
                        return true;
                    }

                    // 3) Numeric string epoch?
                    if (long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var epoch))
                    {
                        if (epoch > 10_000_000_000L)
                        {
                            dtUtc = DateTimeOffset.FromUnixTimeMilliseconds(epoch).UtcDateTime;
                            return true;
                        }
                        if (epoch >= 0)
                        {
                            dtUtc = DateTimeOffset.FromUnixTimeSeconds(epoch).UtcDateTime;
                            return true;
                        }
                    }
                }
            }

            dtUtc = default;
            return false;
        }

        public static BsonValue GetValueByPath(BsonDocument doc, string path)
        {
            var parts = path.Split('.');
            BsonValue current = doc;

            foreach (var part in parts)
            {
                if (current is BsonDocument subDoc && subDoc.Contains(part))
                {
                    current = subDoc[part];
                }
                else
                {
                    return null;
                }
            }

            return current;
        }

        public static void SetValueByPath(BsonDocument doc, string path, BsonValue value)
        {
            var parts = path.Split('.');
            BsonDocument current = doc;

            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (!current.Contains(parts[i]) || !(current[parts[i]] is BsonDocument))
                {
                    current[parts[i]] = new BsonDocument();
                }
                current = current[parts[i]].AsBsonDocument;
            }

            current[parts[^1]] = value;
        }

        private static DateTime ParseDate(string value, string format)
        {
            if (!string.IsNullOrWhiteSpace(format) &&
                DateTime.TryParseExact(value, format, null, System.Globalization.DateTimeStyles.None, out var dt))
                return dt;

            return DateTime.Parse(value); // fallback
        }
    }
}
