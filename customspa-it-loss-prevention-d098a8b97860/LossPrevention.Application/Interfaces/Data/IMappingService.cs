using LossPrevention.Domain.Entities.Data;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LossPrevention.Application.Interfaces.Data
{
    public interface IMappingService
    {
        public Task<List<MappingItem>> GetAllMappings();
        public Task AddMappingAsync(MappingItem mapping);
        public Task UpdateMappingAsync(string id, MappingItem updatedMapping);
        public Task<DeleteResult> DeleteMappingAsync(ObjectId id);
        public Task<List<BsonDocument>> ApplyMappingsToBsonDocuments(List<BsonDocument> documents);
        public Task ProcessMappings(string collectionName, int amountToTake = 10000);
        public Task FinalizeTypesAsync();
        
    }
}