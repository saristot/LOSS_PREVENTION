namespace LossPrevention.Domain.Entities.Users
{
    public sealed class UserWithRolesAndPermissions
    {
        public required User User { get; set; }
        public required List<Role> Roles { get; set; } = new List<Role>();
        public required List<Permission> Permissions { get; set; } = new List<Permission>();
    }
}
