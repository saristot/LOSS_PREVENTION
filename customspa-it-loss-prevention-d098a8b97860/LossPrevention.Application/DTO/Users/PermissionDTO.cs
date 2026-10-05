namespace LossPrevention.Application.DTO.Users
{
    public sealed class PermissionDTO
    {
        public string _id { get; set; } = string.Empty;
        public required string PermissionName { get; set; }
        public required string PermissionText { get; set; }
        public required string Description { get; set; }
    }
}