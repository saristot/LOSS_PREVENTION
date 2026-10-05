using FastEndpoints;
using LossPrevention.Application.Handlers.Requests.Groups;
using LossPrevention.Application.Handlers.Responses.Groups;
using LossPrevention.Application.Mappings;
using LossPrevention.Domain.Groups;
using LossPrevention.Infrastructure.Repositories;

namespace LossPrevention.API.Endpoints.Groups
{
    public sealed class CreateGroupEndpoint : Endpoint<CreateGroupRequest, GroupResponse>
    {
        private readonly IMongoRepository<GroupDocument> _repo;

        public CreateGroupEndpoint(IMongoRepository<GroupDocument> repo)
        {
            _repo = repo;
        }

        public override void Configure()
        {
            Post("/groups");
            Permissions("CAN_MANAGE_GROUPS");
            Summary(s =>
            {
                s.Summary = "Create a group";
                s.Description = "Creates a new group with members.";
            });
        }

        public override async Task HandleAsync(CreateGroupRequest req, CancellationToken ct)
        {
            var now = DateTime.UtcNow;
            var doc = req.ToDocument(now);
            await _repo.InsertOneAsync(doc);
            await SendOkAsync(doc.ToResponse(), ct);
        }
    }
}
