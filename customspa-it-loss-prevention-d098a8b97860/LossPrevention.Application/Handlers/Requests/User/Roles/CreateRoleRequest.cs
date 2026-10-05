using MongoDB.Bson;

namespace LossPrevention.DataIngestion.API.User.Handlers.Requests.Roles
{
    public sealed class CreateRoleRequest
    {
        public required string RoleName { get; set; }
        public required string Description { get; set; }
        public required List<string> Permissions { get; set; } = new List<string>();
    }
}
