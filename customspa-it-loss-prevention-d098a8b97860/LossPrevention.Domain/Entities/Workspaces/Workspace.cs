namespace LossPrevention.Application.Entities.Workspaces
{
    public sealed class Workspace
    {
        public string Id { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public List<Tab> Tabs { get; set; } = [];
    }

}
