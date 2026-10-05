using FastEndpoints;
using LossPrevention.Application.Handlers.Requests.Dashboard;
using LossPrevention.Domain.Dashboards;
using LossPrevention.Infrastructure.Repositories;
using MongoDB.Bson;

namespace LossPrevention.API.Handlers.Dashboards
{
    public sealed class DeleteDashboardEndpoint : Endpoint<DeleteDashboardRequest>
    {
        private readonly IMongoRepository<DashboardDocument> _repo;

        public DeleteDashboardEndpoint(IMongoRepository<DashboardDocument> repo)
        {
            _repo = repo;
        }

        public override void Configure()
        {
            Delete("/dashboards/{id}");
            Permissions("CAN_MANAGE_DASHBOARDS");
            Summary(s =>
            {
                s.Summary = "Delete a dashboard";
            });
        }

        public override async Task HandleAsync(DeleteDashboardRequest req, CancellationToken ct)
        {
            if (!ObjectId.TryParse(req.Id, out var oid))
            {
                await SendNotFoundAsync(ct);
                return;
            }

            var result = await _repo.DeleteByIdAsync(oid);
            if (result.DeletedCount == 0)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            await SendOkAsync(ct);
        }
    }
}
