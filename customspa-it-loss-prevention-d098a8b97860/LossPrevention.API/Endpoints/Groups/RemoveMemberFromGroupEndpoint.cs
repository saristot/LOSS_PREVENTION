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
    public sealed class RemoveMemberFromGroupEndpoint : Endpoint<RemoveMemberFromGroupRequest, GroupResponse>
    {
        private readonly IMongoRepository<GroupDocument> _repo;

        public RemoveMemberFromGroupEndpoint(IMongoRepository<GroupDocument> repo)
        {
            _repo = repo;
        }

        public override void Configure()
        {
            Delete("/groups/{groupId}/members/{userId}");
            Permissions("CAN_MANAGE_GROUPS");
            Summary(s => s.Summary = "Remove a member from a group");
        }

        public override async Task HandleAsync(RemoveMemberFromGroupRequest req, CancellationToken ct)
        {
            var groupIdStr = Route<string>("groupId");
            var userIdStr = Route<string>("userId");

            if (string.IsNullOrWhiteSpace(groupIdStr) || !ObjectId.TryParse(groupIdStr, out var groupOid))
            {
                AddError("groupId", "Invalid group id.");
                await SendErrorsAsync(400, ct);
                return;
            }

            if (string.IsNullOrWhiteSpace(userIdStr) || !ObjectId.TryParse(userIdStr, out var userOid))
            {
                AddError("userId", "Invalid user id.");
                await SendErrorsAsync(400, ct);
                return;
            }

            var filter = Builders<GroupDocument>.Filter.Eq(x => x.Id, groupOid);
            var existing = await _repo.FindOneAsync(filter);
            if (existing is null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            // Remove member if present
            if (existing.Members.Remove(userOid))
            {
                existing.UpdatedAtUtc = DateTime.UtcNow;

                var updateDefinition = Builders<GroupDocument>.Update
                    .Set(g => g.Members, existing.Members)
                    .Set(g => g.UpdatedAtUtc, existing.UpdatedAtUtc);

                await _repo.UpdateOneAsync(filter, updateDefinition);
            }

            await SendOkAsync(existing.ToResponse(), ct);
        }
    }
}
