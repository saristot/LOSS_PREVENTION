using FastEndpoints;
using LossPrevention.Application.Interfaces.User;
using LossPrevention.Domain.Entities.Users;

namespace LossPrevention.DataIngestion.API.Endpoints.User.Permissions
{
    public class UpdatePermissionEndpoint : Endpoint<Permission>
    {
        private readonly IUserPermissionService _service;

        public UpdatePermissionEndpoint(IUserPermissionService service) => _service = service;

        public override void Configure()
        {
            Put("/permissions/update");
            Permissions("CAN_UPDATE_PERMISSION");
            Description(b => b.WithName("UpdatePermission").Produces(200));
        }

        public override async Task HandleAsync(Permission permission, CancellationToken ct)
        {
            var success = await _service.UpdatePermissionAsync(permission);
            if (!success) await SendErrorsAsync(400, ct);
            else await SendOkAsync(ct);
        }
    }
}
