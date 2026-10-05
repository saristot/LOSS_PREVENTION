using LossPrevention.Application.Dashboards.DTO;

namespace LossPrevention.Application.Handlers.Responses.Dashboard
{
    public sealed class DashboardResponse
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string WorkspaceId { get; set; } = "";
        public string TabId { get; set; } = "";
        public List<DashboardBlockDto> Blocks { get; set; } = new();
        public string CreatedAtUtc { get; set; } = "";
        public string UpdatedAtUtc { get; set; } = "";
    }
}
