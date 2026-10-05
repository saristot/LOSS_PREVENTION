using FastEndpoints;
using LossPrevention.Application.DTO.Users;
using LossPrevention.Application.Interfaces.User;
using LossPrevention.Domain.Entities.Users;
using MongoDB.Bson;

namespace LossPrevention.DataIngestion.API.Endpoints.User.Roles
{
    public sealed class UpdateRoleEndpoint : Endpoint<RoleDTO>
    {
        private readonly IUserRoleService _service;

        public UpdateRoleEndpoint(IUserRoleService service) => _service = service;

        public override void Configure()
        {
            Put("/roles/update");
            Permissions("CAN_UPDATE_ROLE");
            Description(b => b.WithName("UpdateRole").Produces(200));
        }

        public override async Task HandleAsync(RoleDTO role, CancellationToken ct)
        {
            var updateRole = new Role()
            {
                _id = ObjectId.Parse(role._id),
                RoleName = role.RoleName,
                Description = role.Description,
                Permissions = role.Permissions.Select(x => ObjectId.Parse(x)).ToList()
            };

            var success = await _service.UpdateRoleAsync(updateRole);
            if (!success) await SendErrorsAsync(400, ct);
            else await SendOkAsync(ct);
        }
    }
}
