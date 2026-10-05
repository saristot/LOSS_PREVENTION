namespace LossPrevention.API.User.Handlers.Requests.Permissions
{
    public sealed class AddPermissionToRoleRequest
    {
        public string RoleId { get; set; } = string.Empty;
        public string PermissionId { get; set; } = string.Empty;
    }
}
