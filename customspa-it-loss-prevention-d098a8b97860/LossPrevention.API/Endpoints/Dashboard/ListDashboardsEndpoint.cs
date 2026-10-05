using FastEndpoints;
using LossPrevention.Application.Dashboards.DTO;
using LossPrevention.Application.Handlers.Requests.Dashboard;
using LossPrevention.Application.Handlers.Responses.Dashboard;
using LossPrevention.Domain.Dashboards;
using LossPrevention.Infrastructure.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LossPrevention.API.Handlers.Dashboards
{
    public sealed class ListDashboardsEndpoint : Endpoint<ListDashboardsRequest, List<DashboardResponse>>
    {
        private readonly IMongoRepository<DashboardDocument> _repo;

        public ListDashboardsEndpoint(IMongoRepository<DashboardDocument> repo)
        {
            _repo = repo;
        }

        public override void Configure()
        {
            Get("/dashboards");
            Permissions("CAN_VIEW_DASHBOARD");
            Summary(s =>
            {
                s.Summary = "List dashboards (optionally filter by workspace/tab)";
            });
        }

        public override async Task HandleAsync(ListDashboardsRequest req, CancellationToken ct)
        {
            var filter = Builders<DashboardDocument>.Filter.Empty;

            if (!string.IsNullOrWhiteSpace(req.WorkspaceId))
                filter &= Builders<DashboardDocument>.Filter.Eq(x => x.WorkspaceId, ObjectId.Parse(req.WorkspaceId));

            if (!string.IsNullOrWhiteSpace(req.TabId))
                filter &= Builders<DashboardDocument>.Filter.Eq(x => x.TabId, req.TabId);

            var docs = await _repo.FindManyAsync(filter);
            var resp = docs.Select(d => new DashboardResponse
            {
                Id = d.Id.ToString(),
                Name = d.Name,
                WorkspaceId = d.WorkspaceId.ToString(),
                TabId = d.TabId,
                Blocks = [.. d.Blocks.Select(b => new DashboardBlockDto
                {
                    i = b.I,
                    x = b.X,
                    y = b.Y,
                    w = b.W,
                    h = b.H,
                    type = b.Type,
                    data = b.Data?.ToDictionary()
                })],
                CreatedAtUtc = d.CreatedAtUtc.ToString("o"),
                UpdatedAtUtc = d.UpdatedAtUtc.ToString("o")
            }).ToList();

            await SendOkAsync(resp, ct);
        }
    }
}
