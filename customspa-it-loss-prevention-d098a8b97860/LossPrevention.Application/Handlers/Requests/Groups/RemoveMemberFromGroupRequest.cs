namespace LossPrevention.Application.Handlers.Requests.Groups
{
    public sealed class RemoveMemberFromGroupRequest
    {
        public string GroupId { get; set; } = "";
        public string UserId { get; set; } = "";
    }
}
