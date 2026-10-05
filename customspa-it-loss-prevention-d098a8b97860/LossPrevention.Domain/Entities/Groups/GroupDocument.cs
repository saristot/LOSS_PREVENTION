using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace LossPrevention.Domain.Groups
{
    public sealed class GroupDocument
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public List<ObjectId> Members { get; set; } = new(); // User IDs

        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
