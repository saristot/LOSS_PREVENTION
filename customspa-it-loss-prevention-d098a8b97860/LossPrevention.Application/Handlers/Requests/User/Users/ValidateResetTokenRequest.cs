namespace LossPrevention.Application.Handlers.Requests.User.Users
{
    public sealed class ValidateResetTokenRequest
    {
        public required string Token { get; set; }
    }
}