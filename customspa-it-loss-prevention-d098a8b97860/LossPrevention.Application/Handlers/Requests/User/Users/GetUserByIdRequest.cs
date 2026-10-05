namespace LossPrevention.API.User.Handlers.Requests.Users
{
    public sealed class GetUserByIdRequest
    {
        public required string Id { get; set; } = string.Empty;
    }
}
