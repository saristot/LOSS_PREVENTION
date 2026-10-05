using LossPrevention.DataIngestion.API.User.Handlers.Requests.Roles;
using FastEndpoints;
using MongoDB.Bson;
using LossPrevention.Application.Interfaces.User;

namespace LossPrevention.DataIngestion.API.Endpoints.User.Roles
{
    public sealed class RemoveRoleFromUserEndpoint : Endpoint<RemoveRoleFromUserRequest>
    {
        private readonly IUserRoleService _service;

        public RemoveRoleFromUserEndpoint(IUserRoleService service) => _service = service;

        public override void Configure()
        {
            Delete("/roles/remove-role");
            Permissions("CAN_ASSIGN_ROLE");
            Description(b => b.WithName("RemoveRoleFromUser").Produces(200).ProducesProblem(400));
        }

        public override async Task HandleAsync(RemoveRoleFromUserRequest req, CancellationToken ct)
        {
            var success = await _service.RemoveRoleFromUserAsync(ObjectId.Parse(req.UserId), ObjectId.Parse(req.RoleId));
            if (!success) await SendErrorsAsync(400, ct);
            else await SendOkAsync(ct);
        }
    }
}
