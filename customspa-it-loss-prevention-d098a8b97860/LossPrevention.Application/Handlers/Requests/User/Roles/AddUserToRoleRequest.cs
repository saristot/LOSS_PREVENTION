namespace LossPrevention.DataIngestion.API.User.Handlers.Requests.Users
{
    public sealed class AddUserToRoleRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string RoleId { get; set; } = string.Empty;
    }
}
