using LossPrevention.DataIngestion.API.User.Handlers.Requests.Roles;
using FastEndpoints;
using MongoDB.Bson;
using LossPrevention.Domain.Entities.Users;
using LossPrevention.Application.Interfaces.User;
using LossPrevention.Application.DTO.Users;

namespace LossPrevention.DataIngestion.API.Endpoints.User.Roles
{
    public sealed class GetRoleByIdEndpoint : Endpoint<GetRoleByIdRequest, RoleDTO>
    {
        private readonly IUserRoleService _service;

        public GetRoleByIdEndpoint(IUserRoleService service) => _service = service;

        public override void Configure()
        {
            Get("/roles/by-id");
            Permissions("CAN_VIEW_ROLE");
            Description(b => b.WithName("GetRoleById").Produces<Role>(200));
        }

        public override async Task HandleAsync(GetRoleByIdRequest req, CancellationToken ct)
        {
            var role = await _service.GetRoleByIdAsync(ObjectId.Parse(req.RoleId));

            if (role is null) { await SendNotFoundAsync(ct); }
            else
            {
                var roleDTO = new RoleDTO
                {
                    _id = role._id.ToString(),
                    RoleName = role.RoleName,
                    Description = role.Description,
                    Permissions = role.Permissions.Select(x => x.ToString()).ToList()
                };
                await SendOkAsync(roleDTO, ct);
            }
        }
    }
}
