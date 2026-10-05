using FastEndpoints;
using LossPrevention.Application.Interfaces.User;
using LossPrevention.API.User.Handlers.Requests.Permissions;
using MongoDB.Bson;

namespace LossPrevention.DataIngestion.API.Endpoints.User.Permissions
{
    public class AddPermissionToRoleEndpoint : Endpoint<AddPermissionToRoleRequest>
    {
        private readonly IUserPermissionService _service;

        public AddPermissionToRoleEndpoint(IUserPermissionService service) => _service = service;

        public override void Configure()
        {
            Post("/permissions/add-permission");
            Permissions("CAN_ASSIGN_PERMISSION");
            Description(b => b.WithName("AddPermissionToRole").Produces(200));
        }

        public override async Task HandleAsync(AddPermissionToRoleRequest req, CancellationToken ct)
        {
            await _service.AddPermissionToRoleAsync(ObjectId.Parse(req.RoleId), ObjectId.Parse(req.PermissionId));
            await SendOkAsync(ct);
        }
    }
}
