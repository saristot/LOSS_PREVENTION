using FastEndpoints;
using LossPrevention.Application.DTO.Users;
using LossPrevention.Application.Interfaces.User;
using LossPrevention.Domain.Entities.Users;

namespace LossPrevention.DataIngestion.API.Endpoints.User.Roles
{
    public sealed class GetAllRolesEndpoint : EndpointWithoutRequest<IEnumerable<RoleDTO>>
    {
        private readonly IUserRoleService _service;

        public GetAllRolesEndpoint(IUserRoleService service) => _service = service;

        public override void Configure()
        {
            Get("/roles/all");
            Permissions("CAN_VIEW_ROLE");
            Description(b => b.WithName("GetAllRoles").Produces<IEnumerable<Role>>(200));
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var roles = await _service.GetAllRolesAsync();

            var rolesDTOList = new List<RoleDTO>();

            foreach (var role in roles)
            {
                var roleDTO = new RoleDTO() 
                { 
                    _id = role._id.ToString(), 
                    RoleName = role.RoleName, 
                    Description = role.Description, 
                    Permissions = role.Permissions.Select(x => x.ToString()).ToList()  
                };
                rolesDTOList.Add(roleDTO);
            }

            await SendOkAsync(rolesDTOList, ct);
        }
    }
}