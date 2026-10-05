namespace LossPrevention.API.User.Handlers.Requests.Users
{
    public sealed class GetUserByUsernameRequest
    {
        public required string Username { get; set; } = string.Empty;
    }
}
