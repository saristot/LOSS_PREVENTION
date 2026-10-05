using LossPrevention.Application.Helpers;
using LossPrevention.Application.Interfaces.Data;
using LossPrevention.Infrastructure.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Linq;
using System;

namespace LossPrevention.Application.Services.Data
{
    public sealed class ReportDataservice : IReportDataservice
    {
        private readonly IMongoRepository<BsonDocument> _reportDataRepository;
        private readonly IMappingService _mappingService;

        public ReportDataservice(IMongoRepository<BsonDocument> reportDataRepository, IMappingService mappingService)
        {
            _reportDataRepository = reportDataRepository;
            _mappingService = mappingService;
        }

        public async Task<(List<Dictionary<string, object>> Data, long TotalCount)> QueryReportDataAsync(BsonDocument[] pipeline, int skip, int take)
        {
            // --- Build the set of date fields from mappings ---
            var mappings = await _mappingService.GetAllMappings();
            var dateFields = mappings
                .Where(m => IsDateType(m.DataType))
                .SelectMany(m => new[] { m.Name, m.Alias })
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // --- Normalize date literals in the pipeline BEFORE running the aggregation ---
            foreach (var stage in pipeline)
                CoerceDateComparisons(stage, dateFields);

            // Step 1: Clone pipeline and add $count for total
            var countPipeline = pipeline.ToList();
            countPipeline.Add(new BsonDocument("$count", "total"));
            var countResult = await _reportDataRepository.AggregateAsync(0, 0, countPipeline.ToArray());
            var total = countResult.FirstOrDefault()?["total"].ToInt64() ?? 0;

            // Step 2: Add pagination to original pipeline
            var paginatedPipeline = pipeline.ToList();
            if (skip > 0)
                paginatedPipeline.Add(new BsonDocument("$skip", skip));
            if (take > 0)
                paginatedPipeline.Add(new BsonDocument("$limit", take));

            var rawResults = await _reportDataRepository.AggregateAsync(0, 0, paginatedPipeline.ToArray());

            // Step 3: Get only visible field aliases from mappings
            var visibleFields = mappings
                .Where(m => m.IsVisible)
                .Select(m => m.Alias)
                .ToHashSet();

            // Step 4: Convert, include unknowns (your logic effectively includes all), and exclude _id if desired
            var filtered = rawResults
                .Select(XmlToBsonConverterHelper.BsonToPlainDictionary)
                .Select(dict =>
                {
                    var flat = new Dictionary<string, object>();

                    foreach (var kv in dict)
                    {
                        // if (kv.Key == "_id") continue;

                        if (kv.Key == "GroupBy")
                        {
                            flat[GetGroupByFieldName(pipeline)] = kv.Value;
                        }
                        else if (visibleFields.Contains(kv.Key) || kv.Key == "count" || !visibleFields.Any() || !visibleFields.Contains(kv.Key))
                        {
                            flat[kv.Key] = kv.Value;
                        }
                    }

                    return flat;
                })
                .ToList();

            return (filtered, total);
        }

        private string GetGroupByFieldName(BsonDocument[] pipeline)
        {
            var groupStage = pipeline.FirstOrDefault(p => p.Contains("$group"));
            if (groupStage != null && groupStage["$group"].AsBsonDocument.Contains("_id"))
            {
                var idValue = groupStage["$group"]["_id"];
                if (idValue.IsString && idValue.AsString.StartsWith("$"))
                {
                    return idValue.AsString.TrimStart('$');
                }
            }

            return "GroupBy";
        }

        // --------------------------- DATE COERCION ---------------------------

        private static bool IsDateType(string? dt)
        {
            var s = (dt ?? "").Trim().ToLowerInvariant();
            return s is "date" or "datetime" or "timestamp";
        }

        /// <summary>
        /// Walks the pipeline stage and converts comparisons on known date fields
        /// from string/iso-like values to real BSON dates.
        /// </summary>
        private static void CoerceDateComparisons(BsonValue node, HashSet<string> dateFields)
        {
            if (node is BsonDocument doc)
            {
                // copy names so we can mutate while iterating
                var names = doc.Names.ToList();
                foreach (var name in names)
                {
                    var val = doc[name];

                    // If the field itself is a date field and value is a scalar or op-doc, coerce it
                    if (dateFields.Contains(name))
                    {
                        if (val.IsBsonDocument)
                        {
                            var opDoc = val.AsBsonDocument;
                            foreach (var opName in opDoc.Names.ToList())
                            {
                                var opVal = opDoc[opName];

                                if (opName is "$gt" or "$gte" or "$lt" or "$lte" or "$eq" or "$ne")
                                {
                                    if (TryCoerceDateValue(opVal, out var coerced))
                                        opDoc[opName] = coerced;
                                }
                                else if (opName is "$in" or "$nin")
                                {
                                    if (opVal.IsBsonArray)
                                    {
                                        var arr = opVal.AsBsonArray;
                                        for (int i = 0; i < arr.Count; i++)
                                            if (TryCoerceDateValue(arr[i], out var coerced)) arr[i] = coerced;
                                    }
                                }
                            }
                        }
                        else
                        {
                            // direct equality: { field: "2025-01-01T00:00" }
                            if (TryCoerceDateValue(val, out var coerced))
                                doc[name] = coerced;
                        }
                    }

                    // Recurse into nested docs/arrays ($and/$or/etc.)
                    CoerceDateComparisons(val, dateFields);
                }
            }
            else if (node is BsonArray arr)
            {
                foreach (var item in arr)
                    CoerceDateComparisons(item, dateFields);
            }
        }

        private static bool TryCoerceDateValue(BsonValue v, out BsonValue coerced)
        {
            coerced = v;

            // Already a BSON date
            if (v.IsBsonDateTime) return false;

            // Extended JSON { "$date": "..." } -> convert to BSON date
            if (v.IsBsonDocument && v.AsBsonDocument.ElementCount == 1 && v.AsBsonDocument.Contains("$date"))
            {
                var dv = v["$date"];
                if (dv.IsString && TryParseIsoishUtc(dv.AsString, out var dtFromEj))
                {
                    coerced = new BsonDateTime(dtFromEj);
                    return true;
                }
            }

            // Strings like "2025-01-01T00:00" or "2025-01-01 00:00"
            if (v.IsString)
            {
                var s = v.AsString?.Trim();
                if (!string.IsNullOrEmpty(s) && TryParseIsoishUtc(s, out var dtUtc))
                {
                    coerced = new BsonDateTime(dtUtc);
                    return true;
                }
                return false;
            }

            // Uncomment if you want to treat epoch millis as date input
            // if (v.IsInt64)
            // {
            //     try
            //     {
            //         coerced = new BsonDateTime(DateTimeOffset.FromUnixTimeMilliseconds(v.AsInt64).UtcDateTime);
            //         return true;
            //     }
            //     catch { }
            // }

            return false;
        }

        private static bool TryParseIsoishUtc(string s, out DateTime dtUtc)
        {
            dtUtc = default;

            // Normalize: allow space instead of 'T', missing 'Z', missing seconds
            var norm = s.Replace(' ', 'T');
            if (!norm.EndsWith("Z", StringComparison.OrdinalIgnoreCase)) norm += "Z";

            if (DateTimeOffset.TryParse(norm, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dto))
            {
                dtUtc = dto.UtcDateTime;
                return true;
            }
            return false;
        }

        // --------------------------- (optional) flatten helper ---------------------------
        private static void AddFieldValue(BsonElement field, Dictionary<string, string> dict, string prefix = "")
        {
            string keyPrefix = string.IsNullOrEmpty(prefix) ? field.Name : $"{field.Name}";

            if (field.Value.IsBsonArray)
            {
                BsonHelper.HandleBsonArray(field, dict); // unchanged
            }
            else if (field.Value.IsBsonDateTime)
            {
                dict[keyPrefix] = field.Value.ToUniversalTime().ToString("o");
            }
            else if (field.Value.IsBsonDocument)
            {
                foreach (var subField in field.Value.AsBsonDocument)
                {
                    AddFieldValue(subField, dict, keyPrefix);
                }
            }
            else
            {
                dict[keyPrefix] = field.Value?.ToString() ?? string.Empty;
            }
        }
    }
}
