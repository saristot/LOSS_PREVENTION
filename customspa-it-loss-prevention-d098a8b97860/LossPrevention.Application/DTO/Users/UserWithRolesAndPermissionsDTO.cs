namespace LossPrevention.Application.DTO.Users
{
    public sealed class UserWithRolesAndPermissionsDTO
    {
        public required UserDTO User { get; set; }
        public required List<RoleDTO> Roles { get; set; } = new List<RoleDTO>();
        public required List<PermissionDTO> Permissions { get; set; } = new List<PermissionDTO>();
    }
}
