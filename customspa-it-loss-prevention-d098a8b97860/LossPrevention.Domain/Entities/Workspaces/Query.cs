using LossPrevention.Domain.Entities.Workspaces;

namespace LossPrevention.Application.Entities.Workspaces
{
    public sealed class Query
    {
        public string Type { get; set; } = "AND";
        public List<Condition> Conditions { get; set; } = [];
        public string Id { get; set; } = string.Empty;
    }

}
