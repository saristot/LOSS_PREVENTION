using MongoDB.Driver;
using System.Linq.Expressions;
using MongoDB.Bson;

namespace LossPrevention.Infrastructure.Repositories
{
    public interface IMongoRepository<TDocument> where TDocument : class
    {
        Task CreateIndexesAsync(IEnumerable<string> fieldNames);
        /// <summary>
        /// Gets a single document by its ID.
        /// </summary>
        /// <param name="id">The ID of the document.</param>
        /// <returns>The document if found, otherwise null.</returns>
        Task<TDocument> GetByIdAsync(ObjectId id);

        /// <summary>
        /// Gets a single document based on a filter.
        /// </summary>
        /// <param name="filter">The filter definition.</param>
        /// <returns>The document if found, otherwise null.</returns>
        Task<TDocument> FindOneAsync(FilterDefinition<TDocument> filter);

        /// <summary>
        /// Gets a single document based on a LINQ expression filter.
        /// </summary>
        /// <param name="filter">The LINQ expression filter.</param>
        /// <returns>The document if found, otherwise null.</returns>
        Task<TDocument> FindOneAsync(Expression<Func<TDocument, bool>> filter);

        /// <summary>
        /// Gets all documents in the collection.
        /// </summary>
        /// <returns>A list of all documents.</returns>
        Task<List<TDocument>> GetAllAsync();

        /// <summary>
        /// Gets a list of documents based on a filter.
        /// </summary>
        /// <param name="filter">The filter definition.</param>
        /// <returns>A list of documents matching the filter.</returns>
        Task<List<TDocument>> FindManyAsync(FilterDefinition<TDocument> filter);

        /// <summary>
        /// Gets a list of documents based on a LINQ expression filter.
        /// </summary>
        /// <param name="filter">The LINQ expression filter.</param>
        /// <returns>A list of documents matching the filter.</returns>
        Task<List<TDocument>> FindManyAsync(Expression<Func<TDocument, bool>> filter);

        /// <summary>
        /// Inserts a new document into the collection.
        /// </summary>
        /// <param name="document">The document to insert.</param>
        Task InsertOneAsync(TDocument document);

        /// <summary>
        /// Inserts multiple documents into the collection.
        /// </summary>
        /// <param name="documents">The list of documents to insert.</param>
        Task InsertManyAsync(IEnumerable<TDocument> documents);

        /// <summary>
        /// Updates a single document based on a filter.
        /// </summary>
        /// <param name="filter">The filter definition to identify the document to update.</param>
        /// <param name="update">The update definition specifying the changes to apply.</param>
        /// <returns>The result of the update operation.</returns>
        Task<UpdateResult> UpdateOneAsync(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update);

        /// <summary>
        /// Updates a single document based on a LINQ expression filter.
        /// </summary>
        /// <param name="filter">The LINQ expression filter to identify the document to update.</param>
        /// <param name="update">The update definition specifying the changes to apply.</param>
        /// <returns>The result of the update operation.</returns>
        Task<UpdateResult> UpdateOneAsync(Expression<Func<TDocument, bool>> filter, UpdateDefinition<TDocument> update);

        /// <summary>
        /// Updates multiple documents based on a filter.
        /// </summary>
        /// <param name="filter">The filter definition to identify the documents to update.</param>
        /// <param name="update">The update definition specifying the changes to apply.</param>
        /// <returns>The result of the update operation.</returns>
        Task<UpdateResult> UpdateManyAsync(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update);

        /// <summary>
        /// Updates multiple documents based on a LINQ expression filter.
        /// </summary>
        /// <param name="filter">The LINQ expression filter to identify the documents to update.</param>
        /// <param name="update">The update definition specifying the changes to apply.</param>
        /// <returns>The result of the update operation.</returns>
        Task<UpdateResult> UpdateManyAsync(Expression<Func<TDocument, bool>> filter, UpdateDefinition<TDocument> update);

        /// <summary>
        /// Upserts a single document based on a filter. If a document matching the filter exists, it's updated; otherwise, a new document is inserted.
        /// </summary>
        /// <param name="filter">The filter definition to identify the document for upsert.</param>
        /// <param name="update">The update definition specifying the changes to apply or the document to insert if not found.</param>
        /// <param name="options">Optional UpdateOptions, e.g., to specify IsUpsert = true.</param>
        /// <returns>The result of the update/insert operation.</returns>
        Task<UpdateResult> UpsertOneAsync(FilterDefinition<TDocument> filter, UpdateDefinition<TDocument> update, UpdateOptions options = null);

        /// <summary>
        /// Upserts a single document based on a LINQ expression filter. If a document matching the filter exists, it's updated; otherwise, a new document is inserted.
        /// </summary>
        /// <param name="filter">The LINQ expression filter to identify the document for upsert.</param>
        /// <param name="update">The update definition specifying the changes to apply or the document to insert if not found.</param>
        /// <param name="options">Optional UpdateOptions, e.g., to specify IsUpsert = true.</param>
        /// <returns>The result of the update/insert operation.</returns>
        Task<UpdateResult> UpsertOneAsync(Expression<Func<TDocument, bool>> filter, UpdateDefinition<TDocument> update, UpdateOptions options = null);

        /// <summary>
        /// Deletes a single document based on its ID.
        /// </summary>
        /// <param name="id">The ID of the document to delete.</param>
        /// <returns>The result of the delete operation.</returns>
        Task<DeleteResult> DeleteByIdAsync(ObjectId id);

        /// <summary>
        /// Deletes a single document based on a filter.
        /// </summary>
        /// <param name="filter">The filter definition to identify the document to delete.</param>
        /// <returns>The result of the delete operation.</returns>
        Task<DeleteResult> DeleteOneAsync(FilterDefinition<TDocument> filter);

        /// <summary>
        /// Deletes a single document based on a LINQ expression filter.
        /// </summary>
        /// <param name="filter">The LINQ expression filter to identify the document to delete.</param>
        /// <returns>The result of the delete operation.</returns>
        Task<DeleteResult> DeleteOneAsync(Expression<Func<TDocument, bool>> filter);

        /// <summary>
        /// Deletes multiple documents based on a filter.
        /// </summary>
        /// <param name="filter">The filter definition to identify the documents to delete.</param>
        /// <returns>The result of the delete operation.</returns>
        Task<DeleteResult> DeleteManyAsync(FilterDefinition<TDocument> filter);

        /// <summary>
        /// Deletes multiple documents based on a LINQ expression filter.
        /// </summary>
        /// <param name="filter">The LINQ expression filter to identify the documents to delete.</param>
        /// <returns>The result of the delete operation.</returns>
        Task<DeleteResult> DeleteManyAsync(Expression<Func<TDocument, bool>> filter);

        /// <summary>
        /// Executes an aggregate pipeline on the collection.
        /// </summary>
        /// <typeparam name="TResult">The type of the result documents.</typeparam>
        /// <param name="pipeline">The aggregation pipeline definition.</param>
        /// <returns>A list of the aggregation results.</returns>
        Task<List<TResult>> AggregateAsync<TResult>(PipelineDefinition<TDocument, TResult> pipeline);

        /// <summary>
        /// Executes an aggregate pipeline on the collection.
        /// </summary>
        /// <typeparam name="TResult">The type of the result documents.</typeparam>
        /// <param name="pipeline">The aggregation pipeline stages.</param>
        /// <returns>A list of the aggregation results.</returns>
        Task<List<TResult>> AggregateAsync<TResult>(params BsonDocument[] pipeline);

        Task<List<BsonDocument>> AggregateAsync(int skip = 0, int take = 0, params BsonDocument[] pipelineStages);
        Task<List<TDocument>> FindManyAsync(Expression<Func<TDocument, bool>> filter, int skip, int take);

        /// <summary>
        /// Returns the MongoDB collection associated with the repository.
        /// </summary>
        IMongoCollection<TDocument> Collection { get; }
    }
}

