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
    public sealed class UpdateGroupEndpoint : Endpoint<UpdateGroupRequest, GroupResponse>
    {
        private readonly IMongoRepository<GroupDocument> _repo;

        public UpdateGroupEndpoint(IMongoRepository<GroupDocument> repo)
        {
            _repo = repo;
        }

        public override void Configure()
        {
            Put("/groups/{id}");
            Permissions("CAN_MANAGE_GROUPS");
            Summary(s => s.Summary = "Update a group");
        }

        public override async Task HandleAsync(UpdateGroupRequest req, CancellationToken ct)
        {
            var idStr = Route<string>("id");
            if (string.IsNullOrWhiteSpace(idStr) || !ObjectId.TryParse(idStr, out _))
            {
                AddError("id", "Invalid group id.");
                await SendErrorsAsync(400, ct);
                return;
            }

            var filter = Builders<GroupDocument>.Filter.Eq(x => x.Id, ObjectId.Parse(idStr));
            var existing = await _repo.FindOneAsync(filter);
            if (existing is null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            existing.Apply(req, DateTime.UtcNow);

            var updateDefinition = Builders<GroupDocument>.Update
                .Set(g => g.Name, existing.Name)
                .Set(g => g.Description, existing.Description)
                .Set(g => g.Members, existing.Members)
                .Set(g => g.UpdatedAtUtc, existing.UpdatedAtUtc);

            await _repo.UpdateOneAsync(filter, updateDefinition);

            await SendOkAsync(existing.ToResponse(), ct);
        }
    }
}
