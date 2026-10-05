using LossPrevention.Domain.Entities.Workspaces;

namespace LossPrevention.Application.Entities.Workspaces
{
    public sealed class Tab
    {
        public string Id { get; set; } = default!;
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public List<Field> SelectedFields { get; set; } = [];
        public string GroupByField { get; set; } = string.Empty;
        public Query Query { get; set; } = new();
        public bool DesignMode { get; set; } = true;
    }

}
