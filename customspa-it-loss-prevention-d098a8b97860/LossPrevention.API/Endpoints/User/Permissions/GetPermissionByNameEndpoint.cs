using LossPrevention.API.User.Handlers.Requests.Permissions;
using FastEndpoints;
using LossPrevention.Domain.Entities.Users;
using LossPrevention.Application.Interfaces.User;

namespace LossPrevention.DataIngestion.API.Endpoints.User.Permissions
{
    public class GetPermissionByNameEndpoint : Endpoint<GetPermissionByNameRequest, Permission>
    {
        private readonly IUserPermissionService _service;

        public GetPermissionByNameEndpoint(IUserPermissionService service) => _service = service;

        public override void Configure()
        {
            Get("/permissions/by-name");
            Permissions("CAN_VIEW_PERMISSION");
            Description(b => b.WithName("GetPermissionByName").Produces<Permission>(200));
        }

        public override async Task HandleAsync(GetPermissionByNameRequest req, CancellationToken ct)
        {
            var permission = await _service.GetPermissionByNameAsync(req.PermissionName);
            if (permission is null) await SendNotFoundAsync(ct);
            else await SendOkAsync(permission, ct);
        }
    }
}
