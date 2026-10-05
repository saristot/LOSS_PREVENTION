using MongoDB.Bson;

namespace LossPrevention.API.User.Handlers.Requests.Users
{
    public sealed class CreateUserRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; } = string.Empty;
        public required string Email { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required bool IsActive { get; set; } = true;
        public List<string> Roles { get; set; } = new List<string>();
    }
}
