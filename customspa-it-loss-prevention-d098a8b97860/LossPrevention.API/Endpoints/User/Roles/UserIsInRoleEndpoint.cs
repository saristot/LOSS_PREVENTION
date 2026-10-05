using LossPrevention.DataIngestion.API.User.Handlers.Requests.Roles;
using FastEndpoints;
using MongoDB.Bson;
using LossPrevention.Application.Interfaces.User;

namespace LossPrevention.DataIngestion.API.Endpoints.User.Roles
{
    public sealed class UserIsInRoleEndpoint : Endpoint<UserIsInRoleRequest, bool>
    {
        private readonly IUserRoleService _service;

        public UserIsInRoleEndpoint(IUserRoleService service) => _service = service;

        public override void Configure()
        {
            Get("/roles/is-in-role");
            Permissions("CAN_VIEW_ROLE");
            Description(b => b.WithName("UserIsInRole").Produces<bool>(200));
        }

        public override async Task HandleAsync(UserIsInRoleRequest req, CancellationToken ct)
        {
            var result = await _service.UserIsInRoleAsync(ObjectId.Parse(req.UserId), req.RoleName);
            await SendOkAsync(result, ct);
        }
    }
}
