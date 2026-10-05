using LossPrevention.Application.Dashboards.DTO;

namespace LossPrevention.Application.Handlers.Requests.Dashboard
{
    public sealed class UpdateDashboardRequest
    {
        public string Name { get; set; } = "";
        public string WorkspaceId { get; set; } = ""; // 24-char hex ObjectId (UI sends it as string)
        public string TabId { get; set; } = "";       // UI sends "tab-1" etc. => keep as string
        public List<DashboardBlockDto> Blocks { get; set; } = new();
    }
}
