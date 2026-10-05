using FastEndpoints;
using LossPrevention.Application.Handlers.Requests.Dashboard;
using LossPrevention.Application.Handlers.Responses.Dashboard;
using LossPrevention.Application.Mappings;
using LossPrevention.Domain.Dashboards;
using LossPrevention.Infrastructure.Repositories;
using MongoDB.Bson;

namespace LossPrevention.API.Handlers.Dashboards
{
    public sealed class GetDashboardEndpoint : Endpoint<GetDashboardRequest, DashboardResponse>
    {
        private readonly IMongoRepository<DashboardDocument> _repo;

        public GetDashboardEndpoint(IMongoRepository<DashboardDocument> repo)
        {
            _repo = repo;
        }

        public override void Configure()
        {
            Get("/dashboards/{id}");
            Permissions("CAN_VIEW_DASHBOARD");
            Summary(s =>
            {
                s.Summary = "Get a dashboard by id";
            });
        }

        public override async Task HandleAsync(GetDashboardRequest req, CancellationToken ct)
        {
            if (!ObjectId.TryParse(req.Id, out var oid))
            {
                await SendNotFoundAsync(ct);
                return;
            }

            var doc = await _repo.GetByIdAsync(oid);
            if (doc is null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            await SendOkAsync(doc.ToResponse(), ct);
        }
    }
}
