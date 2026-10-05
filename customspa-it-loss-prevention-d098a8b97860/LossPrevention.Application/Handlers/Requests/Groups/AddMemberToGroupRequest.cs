namespace LossPrevention.Application.Handlers.Requests.Groups
{
    public sealed class AddMemberToGroupRequest
    {
        public string GroupId { get; set; } = "";
        public string UserId { get; set; } = "";
    }
}
