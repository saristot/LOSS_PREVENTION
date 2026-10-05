using MongoDB.Bson;
using MongoDB.Driver.Core;
using MongoDB.Driver;
using System.Linq.Expressions;
using MongoDB.Driver.Linq;
using MongoDB.Driver.Core.Misc;

namespace LossPrevention.Infrastructure.Repositories
{
    public sealed class MongoRepository<TDocument> : IMongoRepository<TDocument> where TDocument : class
    {
        private readonly IMongoCollection<TDocument> _collection;

        public MongoRepository(IMongoClient client, string databaseName, string collectionName)
        {
            var database = client.GetDatabase(databaseName);
            _collection = database.GetCollection<TDocument>(collectionName);
        }

        public MongoRepository(string connectionString, string databaseName, string collectionName)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _collection = database.GetCollection<TDocument>(collectionName);
        }

        public IMongoCollection<TDocument> Collection => _collection;

        public async Task CreateIndexesAsync(IEnumerable<string> fieldNames)
        {
            var indexModels = fieldNames.Select(field =>
                new CreateIndexModel<TDocument>(
                    Builders<TDocument>.IndexKeys.Ascending(field)
                )).Take(20).ToList();

            if (indexModels.Any())
            {
                await _collection.Indexes.CreateManyAsync(indexModels);
            }
        }


        public async Task<TDocument> GetByIdAsync(ObjectId id)
        {
            var filter = Builders<TDocument>.Filter.Eq("_id", id); 
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<TDocument> FindOneAsync(FilterDefinition<TDocument> filter)
        {
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<TDocument> FindOneAsync(Expression<Func<TDocument, bool>> filter)
        {
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<List<TDocument>> GetAllAsync()
        {
            var result = await _collection.FindAsync(Builders<TDocument>.Filter.Empty);
            return await result.ToListAsync();
        }

        public async Task<List<TDocument>> FindManyAsync(FilterDefinition<TDocument> filter)
        {
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task<List<TDocument>> FindManyAsync(Expression<Func<TDocument, bool>> filter)
        {
            return await _collection.Find(filter).ToListAsync();
        }

        public async Task InsertOneAsync(TDocument document)
        {
            await _collection.InsertOneAsync(document);
        }

        public async Task InsertManyAsync(IEnumerable<TDocument> docs)
        {
            await _collection.InsertManyAsync(docs, new InsertManyOptions { IsOrdered = false, BypassDocumentValidation = true });
        }

        public async Task<UpdateResult> UpdateOneAsync(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update)
        {
            return await _collection.UpdateOneAsync(filter, update);
        }

        public async Task<UpdateResult> UpdateOneAsync(Expression<Func<TDocument, bool>> filter, UpdateDefinition<TDocument> update)
        {
            return await _collection.UpdateOneAsync(filter, update);
        }

        public async Task<UpdateResult> UpdateManyAsync(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update)
        {
            return await _collection.UpdateManyAsync(filter, update);
        }

        public async Task<UpdateResult> UpdateManyAsync(Expression<Func<TDocument, bool>> filter, UpdateDefinition<TDocument> update)
        {
            return await _collection.UpdateManyAsync(filter, update);
        }

        public async Task<UpdateResult> UpsertOneAsync(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, UpdateOptions options = null)
        {
            options ??= new UpdateOptions { IsUpsert = true };
            return await _collection.UpdateOneAsync(filter, update, options);
        }

        public async Task<UpdateResult> UpsertOneAsync(Expression<Func<TDocument, bool>> filter, UpdateDefinition<TDocument> update, UpdateOptions options = null)
        {
            options ??= new UpdateOptions { IsUpsert = true };
            return await _collection.UpdateOneAsync(filter, update, options);
        }

        public async Task<DeleteResult> DeleteByIdAsync(ObjectId id)
        {
            var filter = Builders<TDocument>.Filter.Eq("_id", id);
            return await _collection.DeleteOneAsync(filter);
        }

        public async Task<DeleteResult> DeleteOneAsync(FilterDefinition<TDocument> filter)
        {
            return await _collection.DeleteOneAsync(filter);
        }

        public async Task<DeleteResult> DeleteOneAsync(Expression<Func<TDocument, bool>> filter)
        {
            return await _collection.DeleteOneAsync(filter);
        }

        public async Task<DeleteResult> DeleteManyAsync(FilterDefinition<TDocument> filter)
        {
            return await _collection.DeleteManyAsync(filter);
        }

        public async Task<DeleteResult> DeleteManyAsync(Expression<Func<TDocument, bool>> filter)
        {
            return await _collection.DeleteManyAsync(filter);
        }

        public async Task<List<TResult>> AggregateAsync<TResult>(PipelineDefinition<TDocument, TResult> pipeline)
        {
            return await _collection.Aggregate<TResult>(pipeline).ToListAsync();
        }

        public async Task<List<TResult>> AggregateAsync<TResult>(params BsonDocument[] pipeline)
        {
            return await _collection.Aggregate<TResult>(pipeline).ToListAsync();
        }

        public async Task<List<BsonDocument>> AggregateAsync(int skip = 0, int take = 0, params BsonDocument[] pipelineStages)
        {
            // Create a pipeline from the provided stages
            var pipeline = new List<BsonDocument>(pipelineStages);

            // Add pagination stages if needed
            if (skip > 0)
                pipeline.Add(new BsonDocument("$skip", skip));
            if (take > 0)
                pipeline.Add(new BsonDocument("$limit", take));

            // Execute the aggregation
            using (var cursor = await _collection.AggregateAsync<BsonDocument>(pipeline))
            {
                return await cursor.ToListAsync();
            }
        }

        public async  Task<List<TDocument>> FindManyAsync(Expression<Func<TDocument, bool>> filter, int skip, int take)
        {
            return await _collection.Find(filter)
                                    .Skip(skip)
                                    .Limit(take)
                                    .ToListAsync();
        }
    }
}
