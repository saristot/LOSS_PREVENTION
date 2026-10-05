using LossPrevention.Domain.Entities.Data;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FraudDetectionApp.Helpers
{
    public static class MappingHelper
    {
        public static async Task<List<MappingItem>> LoadMappingsAsync(IMongoCollection<BsonDocument> mappingCollection)
        {
            var mappings = await mappingCollection.Find(new BsonDocument()).ToListAsync();

            return mappings.Select(m => new MappingItem
            {
                Name = m["Name"].AsString,
                Alias = m["Alias"].AsString,
                DataType = m["DataType"].AsString,
                IsVisible = m["IsVisible"].AsBoolean,
                CollectionName = m["CollectionName"].AsString,
                IsArray = m["IsArray"].AsBoolean
            }).ToList();
        }
    }
}
