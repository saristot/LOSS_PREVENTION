using LossPrevention.API.User.Handlers.Requests.Permissions;
using FastEndpoints;
using MongoDB.Bson;
using LossPrevention.Domain.Entities.Users;
using LossPrevention.Application.Interfaces.User;

namespace LossPrevention.DataIngestion.API.Endpoints.User.Permissions
{
    public class GetPermissionByIdEndpoint : Endpoint<GetPermissionByIdRequest, Permission>
    {
        private readonly IUserPermissionService _service;

        public GetPermissionByIdEndpoint(IUserPermissionService service) => _service = service;

        public override void Configure()
        {
            Get("/permissions/by-id");
            Permissions("CAN_VIEW_PERMISSION");
            Description(b => b.WithName("GetPermissionById").Produces<Permission>(200));
        }

        public override async Task HandleAsync(GetPermissionByIdRequest req, CancellationToken ct)
        {
            var permission = await _service.GetPermissionByIdAsync(ObjectId.Parse(req.PermissionId));
            if (permission is null) await SendNotFoundAsync(ct);
            else await SendOkAsync(permission, ct);
        }
    }

}
