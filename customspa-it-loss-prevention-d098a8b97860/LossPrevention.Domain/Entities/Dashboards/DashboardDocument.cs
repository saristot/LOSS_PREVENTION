using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace LossPrevention.Domain.Dashboards
{
    public sealed class DashboardDocument
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string Name { get; set; } = "";
        public ObjectId WorkspaceId { get; set; }     // <— ObjectId in DB
        public string TabId { get; set; } = "";       // <— string in DB
        public List<DashboardBlock> Blocks { get; set; } = new();

        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }

    public sealed class DashboardBlock
    {
        public string I { get; set; } = "";
        public int X { get; set; }
        public int Y { get; set; }
        public int W { get; set; }
        public int H { get; set; }
        public string Type { get; set; } = "text";
        public BsonDocument Data { get; set; } = new BsonDocument();
    }
}
