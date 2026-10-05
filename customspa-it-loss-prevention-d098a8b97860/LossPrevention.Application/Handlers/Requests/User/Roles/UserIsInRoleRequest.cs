namespace LossPrevention.DataIngestion.API.User.Handlers.Requests.Roles
{
    public sealed class UserIsInRoleRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
    }
}
