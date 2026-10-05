using LossPrevention.API.User.Handlers.Requests.Permissions;
using FastEndpoints;
using MongoDB.Bson;
using LossPrevention.Application.Interfaces.User;

namespace LossPrevention.DataIngestion.API.Endpoints.User.Permissions
{
    public class UserHasPermissionEndpoint : Endpoint<UserHasPermissionRequest, bool>
    {
        private readonly IUserPermissionService _service;

        public UserHasPermissionEndpoint(IUserPermissionService service) => _service = service;

        public override void Configure()
        {
            Get("/permissions/user");
            Permissions("CAN_VIEW_PERMISSION");
            Description(b => b.WithName("UserHasPermission").Produces<bool>(200));
        }

        public override async Task HandleAsync(UserHasPermissionRequest req, CancellationToken ct)
        {
            var result = await _service.UserHasPermissionAsync(ObjectId.Parse(req.UserId), req.PermissionName);
            await SendOkAsync(result, ct);
        }
    }
}
