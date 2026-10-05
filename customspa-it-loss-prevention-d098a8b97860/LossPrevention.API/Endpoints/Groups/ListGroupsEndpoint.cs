using FastEndpoints;
using LossPrevention.Application.Handlers.Responses.Groups;
using LossPrevention.Domain.Groups;
using LossPrevention.Infrastructure.Repositories;
using MongoDB.Driver;

namespace LossPrevention.API.Endpoints.Groups
{
    public sealed class ListGroupsEndpoint : EndpointWithoutRequest<List<GroupResponse>>
    {
        private readonly IMongoRepository<GroupDocument> _repo;

        public ListGroupsEndpoint(IMongoRepository<GroupDocument> repo)
        {
            _repo = repo;
        }

        public override void Configure()
        {
            Get("/groups");
            Permissions("CAN_VIEW_GROUPS");
            Summary(s =>
            {
                s.Summary = "List all groups";
            });
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var filter = Builders<GroupDocument>.Filter.Empty;
            var docs = await _repo.FindManyAsync(filter);
            
            var resp = docs.Select(g => new GroupResponse
            {
                Id = g.Id.ToString(),
                Name = g.Name,
                Description = g.Description,
                Members = g.Members?.ConvertAll(m => m.ToString()) ?? new(),
                CreatedAtUtc = g.CreatedAtUtc.ToString("o"),
                UpdatedAtUtc = g.UpdatedAtUtc.ToString("o")
            }).ToList();

            await SendOkAsync(resp, ct);
        }
    }
}
