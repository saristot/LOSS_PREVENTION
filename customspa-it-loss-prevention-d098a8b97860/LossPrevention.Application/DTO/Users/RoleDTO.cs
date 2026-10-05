namespace LossPrevention.Application.DTO.Users
{
    public sealed class RoleDTO
    {
        public string _id { get; set; } = string.Empty;
        public required string RoleName { get; set; }
        public required string Description { get; set; }
        public required List<string> Permissions { get; set; } = new List<string>();
    }
}