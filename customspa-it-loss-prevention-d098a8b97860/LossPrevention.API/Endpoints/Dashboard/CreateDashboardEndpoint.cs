using FastEndpoints;
using LossPrevention.Application.Handlers.Requests.Dashboard;
using LossPrevention.Application.Handlers.Responses.Dashboard;
using LossPrevention.Application.Mappings;
using LossPrevention.Domain.Dashboards;
using LossPrevention.Infrastructure.Repositories; // IMongoRepository<T>
using MongoDB.Bson;

namespace LossPrevention.API.Handlers.Dashboards
{
    public sealed class CreateDashboardEndpoint : Endpoint<CreateDashboardRequest, DashboardResponse>
    {
        private readonly IMongoRepository<DashboardDocument> _repo;

        public CreateDashboardEndpoint(IMongoRepository<DashboardDocument> repo)
        {
            _repo = repo;
        }

        public override void Configure()
        {
            Post("/dashboards");
            Permissions("CAN_MANAGE_DASHBOARDS");
            Summary(s =>
            {
                s.Summary = "Create a dashboard";
                s.Description = "Creates a new dashboard document with blocks.";
            });
        }

        public override async Task HandleAsync(CreateDashboardRequest req, CancellationToken ct)
        {
            var now = DateTime.UtcNow;
            var doc = req.ToDocument(now);
            await _repo.InsertOneAsync(doc);
            await SendOkAsync(doc.ToResponse(), ct);
        }
    }
}
