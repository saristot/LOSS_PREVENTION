namespace LossPrevention.API.User.Handlers.Requests.Users
{
    public sealed class ValidatePasswordRequest
    {
        public required string Id { get; set; } = string.Empty;
        public required string Password { get; set; } = string.Empty;
    }
}
