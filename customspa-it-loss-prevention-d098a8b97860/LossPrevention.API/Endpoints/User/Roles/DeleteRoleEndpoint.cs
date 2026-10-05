using LossPrevention.DataIngestion.API.User.Handlers.Requests.Roles;
using FastEndpoints;
using MongoDB.Bson;
using LossPrevention.Application.Interfaces.User;

namespace LossPrevention.DataIngestion.API.Endpoints.User.Roles
{
    public class DeleteRoleEndpoint : Endpoint<DeleteRoleRequest>
    {
        private readonly IUserRoleService _service;

        public DeleteRoleEndpoint(IUserRoleService service) => _service = service;

        public override void Configure()
        {
            Delete("/roles/delete");
            Permissions("CAN_DELETE_ROLE");
            Description(b => b.WithName("DeleteRole").Produces(200));
        }

        public override async Task HandleAsync(DeleteRoleRequest req, CancellationToken ct)
        {
            if (!ObjectId.TryParse(req.RoleId, out var objectId))
            {
                AddError(nameof(req.RoleId), "RoleId must be a valid ObjectId.");
                await SendErrorsAsync(400, ct);
                return;
            }

            var success = await _service.DeleteRoleAsync(objectId);
            if (!success) await SendNotFoundAsync(ct);
            else await SendNoContentAsync(ct);
        }
    }
}
