namespace LossPrevention.Application.Handlers.Requests.Dashboard
{
    public sealed class ListDashboardsRequest
    {
        // query params
        public string? WorkspaceId { get; set; }
        public string? TabId { get; set; }
    }
}
