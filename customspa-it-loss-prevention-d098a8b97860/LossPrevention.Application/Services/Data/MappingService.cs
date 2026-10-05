using LossPrevention.Application.Helpers;
using LossPrevention.Application.Interfaces.Data;
using LossPrevention.Domain.Entities.Data;
using LossPrevention.Domain.Entities.Users;
using LossPrevention.Infrastructure.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;

namespace LossPrevention.Application.Services.Data
{
    public sealed class MappingService : IMappingService
    {
        private readonly IMongoRepository<MappingItem> _mappingRepository;
        private readonly IMongoRepository<BsonDocument> _reportDataRepository;

        public MappingService(IMongoRepository<MappingItem> mappingRepository, IMongoRepository<BsonDocument> reportDataRepository)
        {
            _mappingRepository = mappingRepository;
            _reportDataRepository = reportDataRepository;
        }

        public async Task<List<MappingItem>> GetAllMappings()
        {

            var mappings = await _mappingRepository.GetAllAsync();
            var visibleMappings = mappings.Where(m => m.IsVisible).ToList();

            var transformedDocuments = new List<MappingItem>();

            foreach (var mapping in visibleMappings)
            {
                var doc = new MappingItem
                {
                    _id = mapping._id,
                    Name = mapping.Name,
                    Alias = mapping.Alias,
                    DataType = mapping.DataType,
                    IsVisible = mapping.IsVisible,
                    IsCalculated = mapping.IsCalculated,
                    IsLookup = mapping.IsLookup,
                    IsArray = mapping.IsArray,
                    LongestLength = mapping.LongestLength,
                    CollectionName = mapping.CollectionName
                };

                transformedDocuments.Add(doc);
            }

            return transformedDocuments;
        }

        public async Task AddMappingAsync(MappingItem mapping)
        {
            if (string.IsNullOrWhiteSpace(mapping.Name) || string.IsNullOrWhiteSpace(mapping.Alias))
                throw new ArgumentException("Mapping must have both Name and Alias.");

            var existing = await _mappingRepository.FindOneAsync(x =>
                x.Name == mapping.Name || x.Alias == mapping.Alias);

            if (existing != null)
                throw new InvalidOperationException($"A mapping with Name '{mapping.Name}' or Alias '{mapping.Alias}' already exists.");

            await _mappingRepository.InsertOneAsync(mapping);
        }

        public async Task UpdateMappingAsync(string id, MappingItem updatedMapping)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                throw new ArgumentException("Invalid ObjectId format.");

            var filter = Builders<MappingItem>.Filter.Eq("_id", objectId);
            var update = Builders<MappingItem>.Update
                .Set(x => x.Name, updatedMapping.Name)
                .Set(x => x.Alias, updatedMapping.Alias)
                .Set(x => x.DataType, updatedMapping.DataType)
                .Set(x => x.IsVisible, updatedMapping.IsVisible)
                .Set(x => x.IsArray, updatedMapping.IsArray)
                .Set(x => x.IsCalculated, updatedMapping.IsCalculated)
                .Set(x => x.IsLookup, updatedMapping.IsLookup)
                .Set(x => x.CollectionName, updatedMapping.CollectionName)
                .Set(x => x.LongestLength, updatedMapping.LongestLength);

            var result = await _mappingRepository.UpdateOneAsync(filter, update);
            if (result.ModifiedCount == 0)
                throw new KeyNotFoundException("Mapping with given ID was not found or unchanged.");
        }
        public async Task<DeleteResult> DeleteMappingAsync(ObjectId id)
        {
            return await _mappingRepository.DeleteByIdAsync(id);
        }

        public async Task<List<BsonDocument>> ApplyMappingsToBsonDocuments(List<BsonDocument> documents)
        {
            var existingMappings = await _mappingRepository.GetAllAsync();

            var visibleMappings = existingMappings
                .Where(m => m.IsVisible)
                .ToDictionary(m => m.Name, m => m.Alias);

            var hiddenMappings = existingMappings
                .Where(m => !m.IsVisible)
                .Select(m => m.Name)
                .ToHashSet();

            var transformedDocuments = new List<BsonDocument>();

            foreach (var doc in documents)
            {
                var newDoc = new BsonDocument();

                foreach (var element in doc.Elements)
                {
                    var fieldName = element.Name;

                    if (fieldName == "_id" || hiddenMappings.Contains(fieldName))
                    {
                        // Skip hidden field
                        continue;
                    }

                    if (visibleMappings.TryGetValue(fieldName, out var alias))
                    {
                        newDoc[alias] = element.Value;
                    }
                    else
                    {
                        // Keep the field as is if not found in mappings (optional)
                        newDoc[fieldName] = element.Value;
                    }
                }

                transformedDocuments.Add(newDoc);
            }

            return transformedDocuments;
        }

        /// <summary>
        /// ProcessMappings method processes the mappings for a given collection name and takes a specified amount of records.
        /// </summary>
        /// <param name="collectionName"></param>
        /// <param name="amountToTake"></param>
        /// <returns></returns>
        public async Task ProcessMappings(string collectionName, int amountToTake = 10000)
        {
            var existingMappings = await _mappingRepository.FindManyAsync(_ => true);
            //var existingPaths = new HashSet<string>(existingMappings.Select(x => x.Path));
            var existingNames = new HashSet<string>(existingMappings.Select(x => x.Name));
            var existingAliases = new HashSet<string>(existingMappings.Select(x => x.Alias));

            int batchSize = 1000;
            int skip = 0;
            int amountProcessed = 0;

            while (amountProcessed < amountToTake)
            {
                var remaining = amountToTake - amountProcessed;
                var currentBatchSize = Math.Min(batchSize, remaining);

                var batch = await _reportDataRepository.FindManyAsync(_ => true, skip, currentBatchSize);
                if (!batch.Any())
                {
                    await WriteLog("No more records to process.");
                    break;
                }

                var newMappings = new List<MappingItem>();

                foreach (var doc in batch)
                {
                    var fields = GetBsonDocumentPaths(doc);

                    foreach (var kvp in fields)
                    {
                        var rawPath = kvp.Key;
                        var normalizedPath = NormalizeArrayPath(rawPath);
                        var value = kvp.Value;

                        //if (existingPaths.Contains(normalizedPath))
                        //    continue;

                        var name = GenerateFlatName(normalizedPath);
                        var alias = name;

                        if (existingNames.Contains(name) || existingAliases.Contains(alias))
                            continue;

                        var dataType = CalculateDataType(value);
                        var isArray = normalizedPath.Contains("[*]");

                        newMappings.Add(new MappingItem
                        {
                            CollectionName = collectionName,
                            Name = normalizedPath.Replace("[*]", string.Empty),
                            Alias = alias,
                            //Path = normalizedPath,
                            IsVisible = true,
                            DataType = dataType,
                            IsArray = isArray
                        });

                        //existingPaths.Add(normalizedPath);
                        existingNames.Add(name);
                        existingAliases.Add(alias);
                    }
                }

                if (newMappings.Any())
                    await _mappingRepository.InsertManyAsync(newMappings);

                amountProcessed += batch.Count;
                skip += currentBatchSize;

                await WriteLog($"Mappings - Processed {amountProcessed} documents, last batch size: {batch.Count}");
            }

            await WriteLog($"Finished processing. Total processed: {amountProcessed}");
        }

        public async Task FinalizeTypesAsync()
        {
            var sw = Stopwatch.StartNew();

            // 1) London day window
            var tz = TimeZoneInfo.FindSystemTimeZoneById(
#if WINDOWS
        "GMT Standard Time"
#else
                "Europe/London"
#endif
            );
            var nowLondon = TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz);
            var startOfDayLondon = new DateTime(nowLondon.Year, nowLondon.Month, nowLondon.Day, 0, 0, 0, DateTimeKind.Unspecified);
            var endOfDayLondon = startOfDayLondon.AddDays(1);
            var startUtc = TimeZoneInfo.ConvertTimeToUtc(startOfDayLondon, tz);
            var endUtc = TimeZoneInfo.ConvertTimeToUtc(endOfDayLondon, tz);

            // 2) _id range
            var startId = ObjectId.GenerateNewId(startUtc);
            var endId = ObjectId.GenerateNewId(endUtc);
            var todayFilter = Builders<BsonDocument>.Filter.Gte("_id", startId) &
                              Builders<BsonDocument>.Filter.Lt("_id", endId);

            // 3) Get mappings once
            var mappingItems = await _mappingRepository.GetAllAsync();

            // 4) Stream with cursor (correct batching via FindOptions)
            var findOptions = new FindOptions
            {
                BatchSize = 1000,          // tune
                NoCursorTimeout = true
            };

            using var cursor = await _reportDataRepository.Collection
                .Find(todayFilter, findOptions)
                .ToCursorAsync();

            // Bounded concurrency for CPU-bound conversion work
            var maxDegree = Math.Max(2, Environment.ProcessorCount);

            while (await cursor.MoveNextAsync())
            {
                var batch = cursor.Current.ToList(); // materialize the current server batch
                var updates = new ConcurrentBag<WriteModel<BsonDocument>>();

                await Parallel.ForEachAsync(
                    batch,
                    new ParallelOptions { MaxDegreeOfParallelism = maxDegree },
                    async (document, ct) =>
                    {
                        bool modified = false;

                        foreach (var mapping in mappingItems)
                        {
                            var original = BsonHelper.GetValueByPath(document, mapping.Name);
                            if (original != null)
                            {
                                var converted = BsonHelper.ConvertToMappedType(original, mapping);
                                if (!original.Equals(converted))
                                {
                                    BsonHelper.SetValueByPath(document, mapping.Name, converted);
                                    modified = true;
                                }
                            }
                        }

                        if (!modified) return;

                        if (!document.TryGetValue("_id", out var idBson) || idBson == BsonNull.Value)
                            return;

                        FilterDefinition<BsonDocument> filter = idBson.BsonType switch
                        {
                            BsonType.ObjectId => Builders<BsonDocument>.Filter.Eq("_id", idBson.AsObjectId),
                            BsonType.String => ObjectId.TryParse(idBson.AsString, out var oid)
                                                    ? Builders<BsonDocument>.Filter.Eq("_id", oid)
                                                    : Builders<BsonDocument>.Filter.Eq("_id", idBson), // keep as string
                            _ => Builders<BsonDocument>.Filter.Eq("_id", idBson)
                        };


                        var setters = new List<UpdateDefinition<BsonDocument>>();
                        foreach (var mapping in mappingItems)
                        {
                            if (string.Equals(mapping.Name, "_id", StringComparison.OrdinalIgnoreCase))
                                continue;

                            var value = BsonHelper.GetValueByPath(document, mapping.Name);
                            if (value != null)
                            {
                                setters.Add(Builders<BsonDocument>.Update.Set(mapping.Name, value));
                            }
                        }

                        if (setters.Count > 0)
                        {
                            var update = Builders<BsonDocument>.Update.Combine(setters);
                            updates.Add(new UpdateOneModel<BsonDocument>(filter, update));
                        }

                        await Task.CompletedTask; // satisfy async lambda
                    });

                if (!updates.IsEmpty)
                {
                    await _reportDataRepository.Collection.BulkWriteAsync(
                        updates,
                        new BulkWriteOptions { IsOrdered = false });
                }
            }

            sw.Stop();
            Console.WriteLine($"FinalizeTypesAsync Completed in {sw.Elapsed.TotalSeconds:F2} Seconds");
        }



        /// <summary>
        /// Calculate the Datatype of the field value.
        /// </summary>
        /// <param name="fieldValue"></param>
        /// <returns></returns>
        private string CalculateDataType(BsonValue value)
        {
            var fieldValue = value.AsString;

            if (bool.TryParse(fieldValue, out _))
                return "Boolean";

            if (int.TryParse(fieldValue, out _))
                return "Integer";

            if (Decimal.TryParse(fieldValue, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                return "Decimal";

            if (DateTimeOffset.TryParse(fieldValue, out var date))
            {
                // Reject unreasonable dates (e.g., year too early or far in future)
                if (date.Year >= 1900 && date.Year <= 2100)
                    return "Date";
            }

            return "String";
        }

        private Dictionary<string, string> GetBsonDocumentPaths(BsonDocument doc, string prefix = "")
        {
            var result = new Dictionary<string, string>();
            foreach (var element in doc.Elements)
            {
                var path = string.IsNullOrEmpty(prefix) ? element.Name : $"{prefix}.{element.Name}";
                if (element.Value.IsBsonDocument)
                {
                    var nested = GetBsonDocumentPaths(element.Value.AsBsonDocument, path);
                    foreach (var kv in nested)
                        result[kv.Key] = kv.Value;
                }
                else if (element.Value.IsBsonArray)
                {
                    var array = element.Value.AsBsonArray;
                    for (int i = 0; i < array.Count; i++)
                    {
                        if (array[i].IsBsonDocument)
                        {
                            var nested = GetBsonDocumentPaths(array[i].AsBsonDocument, $"{path}[{i}]");
                            foreach (var kv in nested)
                                result[kv.Key] = kv.Value;
                        }
                        else
                        {
                            result[$"{path}[{i}]"] = array[i].ToString();
                        }
                    }
                }
                else
                {
                    result[path] = element.Value.ToString();
                }
            }
            return result;
        }

        private string NormalizeArrayPath(string path)
        {
            // Generalizes array indices to [*]
            return System.Text.RegularExpressions.Regex.Replace(path, @"\[\d+\]", "[*]");
        }

        private string GenerateFlatName(string path)
        {
            // Removes dots and array markers: Tender[*].Amount => TenderAmount
            return System.Text.RegularExpressions.Regex.Replace(path, @"[\.\[\]\*]", "");
        }

        private Task WriteLog(string message)
        {
            // Replace with actual logging if needed
            Console.WriteLine(message);
            return Task.CompletedTask;
        }



    }
}
