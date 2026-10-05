using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace LossPrevention.Domain.Notifications
{
    public sealed class NotificationDocument
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string Type { get; set; } = "message"; // message, alert, reply
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public string From { get; set; } = ""; // User ID
        public string FromName { get; set; } = "";
        public List<string> To { get; set; } = []; // User ID, Role ID, or Group ID
        public string ToType { get; set; } = "user"; // user, role, group
        public string ReplyTo { get; set; } = ""; // Original notification ID if this is a reply
        public bool IsRead { get; set; } = false;

        public DateTime CreatedAtUtc { get; set; }
    }
}
