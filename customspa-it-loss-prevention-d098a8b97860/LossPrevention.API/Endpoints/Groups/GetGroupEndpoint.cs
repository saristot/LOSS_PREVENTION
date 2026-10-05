using FastEndpoints;
using LossPrevention.Application.Handlers.Requests.Groups;
using LossPrevention.Application.Handlers.Responses.Groups;
using LossPrevention.Application.Mappings;
using LossPrevention.Domain.Groups;
using LossPrevention.Infrastructure.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LossPrevention.API.Endpoints.Groups
{
    public sealed class GetGroupEndpoint : Endpoint<GetGroupRequest, GroupResponse>
    {
        private readonly IMongoRepository<GroupDocument> _repo;

        public GetGroupEndpoint(IMongoRepository<GroupDocument> repo)
        {
            _repo = repo;
        }

        public override void Configure()
        {
            Get("/groups/{id}");
            Permissions("CAN_VIEW_GROUPS");
            Summary(s => s.Summary = "Get a group by ID");
        }

        public override async Task HandleAsync(GetGroupRequest req, CancellationToken ct)
        {
            var idStr = Route<string>("id");
            if (string.IsNullOrWhiteSpace(idStr) || !ObjectId.TryParse(idStr, out var oid))
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
