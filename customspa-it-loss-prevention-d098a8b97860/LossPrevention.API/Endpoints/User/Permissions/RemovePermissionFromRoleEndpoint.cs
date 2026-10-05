using LossPrevention.API.User.Handlers.Requests.Permissions;
using FastEndpoints;
using MongoDB.Bson;
using LossPrevention.Application.Interfaces.User;

namespace LossPrevention.DataIngestion.API.Endpoints.User.Permissions
{
    public class RemovePermissionFromRoleEndpoint : Endpoint<RemovePermissionFromRoleRequest>
    {
        private readonly IUserPermissionService _service;

        public RemovePermissionFromRoleEndpoint(IUserPermissionService service) => _service = service;

        public override void Configure()
        {
            Delete("/permissions");
            Permissions("CAN_ASSIGN_PERMISSION");
            Description(b => b.WithName("RemovePermissionFromRole").Produces(200));
        }

        public override async Task HandleAsync(RemovePermissionFromRoleRequest req, CancellationToken ct)
        {
            await _service.RemovePermissionFromRoleAsync(ObjectId.Parse(req.RoleId), ObjectId.Parse(req.PermissionId));
            await SendOkAsync(ct);
        }
    }
}
