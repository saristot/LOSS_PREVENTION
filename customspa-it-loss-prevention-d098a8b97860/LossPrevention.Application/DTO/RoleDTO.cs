using MongoDB.Bson;

namespace LossPrevention.Application.DTO
{
    public sealed class Role
    {
        public string _id { get; set; }
        public required string RoleName { get; set; }
        public required string Description { get; set; }
        public required List<string> Permissions { get; set; } = new List<string>();
    }
}
