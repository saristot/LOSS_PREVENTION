using MongoDB.Bson;

namespace LossPrevention.Application.Interfaces.Data
{
    public interface IXmlProcessingService
    {
        public Task<BsonDocument> ProcessAsync(string xmlString);

        Task InsertManyAsync(IEnumerable<BsonDocument> docs);
    }
}