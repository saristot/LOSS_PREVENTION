
using MongoDB.Bson;

namespace LossPrevention.Domain.Entities.Users
{
    public sealed class Permission
    {
        public ObjectId _id { get; set; }
        public required string PermissionName { get; set; }
        public required string PermissionText { get; set; }
        public required string Description { get; set; }

    }
}
