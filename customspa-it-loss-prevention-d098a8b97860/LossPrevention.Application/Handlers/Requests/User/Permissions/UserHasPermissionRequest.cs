namespace LossPrevention.API.User.Handlers.Requests.Permissions
{
    public sealed class UserHasPermissionRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string PermissionName { get; set; } = string.Empty;
    }
}
