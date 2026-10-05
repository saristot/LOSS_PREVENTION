using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LossPrevention.Domain.Entities.Users
{
    public class PasswordResetToken
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId _id { get; set; }

        [BsonElement("TokenHash")]
        public string TokenHash { get; set; } = string.Empty;

        [BsonElement("UserId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId UserId { get; set; }

        [BsonElement("Email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("ExpiryDate")]
        public DateTime ExpiryDate { get; set; }

        [BsonElement("IsUsed")]
        public bool IsUsed { get; set; } = false;

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; }
    }
}
