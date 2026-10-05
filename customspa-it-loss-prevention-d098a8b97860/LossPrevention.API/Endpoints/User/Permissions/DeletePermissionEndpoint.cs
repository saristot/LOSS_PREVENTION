using LossPrevention.API.User.Handlers.Requests.Permissions;
using FastEndpoints;
using MongoDB.Bson;
using LossPrevention.Application.Interfaces.User;

namespace LossPrevention.DataIngestion.API.Endpoints.User.Permissions
{
    public class DeletePermissionEndpoint : Endpoint<DeletePermissionRequest>
    {
        private readonly IUserPermissionService _service;

        public DeletePermissionEndpoint(IUserPermissionService service) => _service = service;

        public override void Configure()
        {
            Delete("/permissions/delete");
            Permissions("CAN_DELETE_PERMISSION");
            Description(b => b.WithName("DeletePermission").Produces(200));
        }

        public override async Task HandleAsync(DeletePermissionRequest req, CancellationToken ct)
        {
            if (!ObjectId.TryParse(req.PermissionId, out var objectId))
            {
                AddError(nameof(req.PermissionId), "PermissionId must be a valid ObjectId.");
                await SendErrorsAsync(400, ct);
                return;
            }

            var success = await _service.DeletePermissionAsync(objectId);
            if (!success) await SendNotFoundAsync(ct);
            else await SendNoContentAsync(ct);
        }
    }
}
