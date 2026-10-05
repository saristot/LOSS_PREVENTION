using MongoDB.Bson;

namespace LossPrevention.Domain.Entities.Users
{
    public sealed class User
    {
        public ObjectId _id { get; set; }
        public required string Username { get; set; }
        public string PasswordHash { get; set; } = string.Empty; 
        public string PasswordSalt { get; set; } = string.Empty; 
        public required string Email { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required DateTime RegistrationDate { get; set; }
        public required bool IsActive { get; set; } = true; 
        public List<ObjectId> Roles { get; set; } = new List<ObjectId>();

        public string? LockField { get; set; }
        public string? LockValue { get; set; }
    }
}
