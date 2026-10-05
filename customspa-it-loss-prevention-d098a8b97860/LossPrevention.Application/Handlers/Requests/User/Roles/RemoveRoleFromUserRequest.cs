namespace LossPrevention.DataIngestion.API.User.Handlers.Requests.Roles
{
    public sealed class RemoveRoleFromUserRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string RoleId { get; set; } = string.Empty;
    }
}
