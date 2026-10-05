using MongoDB.Bson;

namespace LossPrevention.Domain.Entities.Users
{
    public sealed class Role
    {
        public ObjectId _id { get; set; }
        public required string RoleName { get; set; }
        public required string Description { get; set; }
        public required List<ObjectId> Permissions { get; set; } = new List<ObjectId>();
    }
}
