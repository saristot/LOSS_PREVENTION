using LossPrevention.Application.Interfaces.Data;
using LossPrevention.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LossPrevention.Application.Services.Data
{
    public sealed class DatabaseInitializationService : IDatabaseInitializationService
    {
        private readonly IMongoRepository<BsonDocument> _reportDataRepository;
        private readonly ILogger<DatabaseInitializationService> _logger;
        private readonly int _retentionDays;

        public DatabaseInitializationService(
            IMongoRepository<BsonDocument> reportDataRepository,
            IConfiguration configuration,
            ILogger<DatabaseInitializationService> logger)
        {
            _reportDataRepository = reportDataRepository;
            _logger = logger;
            _retentionDays = configuration.GetValue<int>("DataRetention:TransactionRetentionDays", 90);
        }

        public async Task InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Starting database initialization...");

                await EnsureTtlIndexAsync();

                _logger.LogInformation("Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database initialization");
                throw;
            }
        }

        private async Task EnsureTtlIndexAsync()
        {
            try
            {
                var collection = _reportDataRepository.Collection;
                var indexesCursor = await collection.Indexes.ListAsync();
                var indexes = await indexesCursor.ToListAsync();

                // Check if TTL index already exists on BeginDateTime
                var ttlIndexExists = indexes.Any(index =>
                {
                    if (index.TryGetValue("key", out var keyValue) && keyValue.IsBsonDocument)
                    {
                        var keyDoc = keyValue.AsBsonDocument;
                        var hasBeginDateTime = keyDoc.Contains("BeginDateTime");

                        // Check if it's actually a TTL index (has expireAfterSeconds)
                        var hasTtl = index.Contains("expireAfterSeconds");

                        return hasBeginDateTime && hasTtl;
                    }
                    return false;
                });

                if (ttlIndexExists)
                {
                    _logger.LogInformation("TTL index on BeginDateTime already exists");
                    return;
                }

                // Check if there's a regular (non-TTL) index on BeginDateTime that we need to drop
                var regularIndexOnBeginDateTime = indexes.FirstOrDefault(index =>
                {
                    if (index.TryGetValue("key", out var keyValue) && keyValue.IsBsonDocument)
                    {
                        var keyDoc = keyValue.AsBsonDocument;
                        var hasBeginDateTime = keyDoc.Contains("BeginDateTime");
                        var hasTtl = index.Contains("expireAfterSeconds");

                        // It has BeginDateTime but NOT TTL
                        return hasBeginDateTime && !hasTtl;
                    }
                    return false;
                });

                // If there's a regular index, drop it first
                if (regularIndexOnBeginDateTime != null && regularIndexOnBeginDateTime.TryGetValue("name", out var indexName))
                {
                    var oldIndexName = indexName.AsString;
                    _logger.LogInformation("Dropping existing regular index '{IndexName}' on BeginDateTime to replace with TTL index", oldIndexName);
                    await collection.Indexes.DropOneAsync(oldIndexName);
                }

                // Create TTL index
                var indexKeys = Builders<BsonDocument>.IndexKeys.Ascending("BeginDateTime");
                var indexOptions = new CreateIndexOptions
                {
                    ExpireAfter = TimeSpan.FromDays(_retentionDays),
                    Name = "ttl_BeginDateTime"
                };

                var indexModel = new CreateIndexModel<BsonDocument>(indexKeys, indexOptions);
                await collection.Indexes.CreateOneAsync(indexModel);

                _logger.LogInformation(
                    "Created TTL index on BeginDateTime with {RetentionDays} days retention",
                    _retentionDays);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create TTL index on BeginDateTime");
                throw;
            }
        }
    }
}