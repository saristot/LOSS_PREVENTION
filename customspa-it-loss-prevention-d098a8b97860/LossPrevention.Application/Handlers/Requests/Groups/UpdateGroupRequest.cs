namespace LossPrevention.Application.Handlers.Requests.Groups
{
    public sealed class UpdateGroupRequest
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public List<string> Members { get; set; } = new(); // User IDs as strings
    }
}
