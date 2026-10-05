using LossPrevention.Application.Dashboards.DTO;

namespace LossPrevention.Application.Handlers.Requests.Dashboard
{
    public sealed class CreateDashboardRequest
    {
        public string Name { get; set; } = "";
        public string WorkspaceId { get; set; } = "";
        public string TabId { get; set; } = "";
        public List<DashboardBlockDto> Blocks { get; set; } = new();
    }
}
