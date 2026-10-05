using FastEndpoints;
using LossPrevention.Application.Handlers.Requests.Groups;
using LossPrevention.Domain.Groups;
using LossPrevention.Infrastructure.Repositories;
using MongoDB.Bson;

namespace LossPrevention.API.Endpoints.Groups
{
    public sealed class DeleteGroupEndpoint : Endpoint<DeleteGroupRequest>
    {
        private readonly IMongoRepository<GroupDocument> _repo;

        public DeleteGroupEndpoint(IMongoRepository<GroupDocument> repo)
        {
            _repo = repo;
        }

        public override void Configure()
        {
            Delete("/groups/{id}");
            Permissions("CAN_MANAGE_GROUPS");
            Summary(s =>
            {
                s.Summary = "Delete a group";
            });
        }

        public override async Task HandleAsync(DeleteGroupRequest req, CancellationToken ct)
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
