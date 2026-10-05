namespace LossPrevention.Application.Handlers.Responses.Groups
{
    public sealed class GroupResponse
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public List<string> Members { get; set; } = new(); // User IDs as strings
        public string CreatedAtUtc { get; set; } = "";
        public string UpdatedAtUtc { get; set; } = "";
    }
}
