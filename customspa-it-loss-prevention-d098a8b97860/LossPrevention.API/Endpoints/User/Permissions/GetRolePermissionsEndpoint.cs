using LossPrevention.API.User.Handlers.Requests.Permissions;
using FastEndpoints;
using MongoDB.Bson;
using LossPrevention.Domain.Entities.Users;
using LossPrevention.Application.Interfaces.User;
using LossPrevention.Application.DTO.Users;

namespace LossPrevention.DataIngestion.API.Endpoints.User.Permissions
{
    public class GetRolePermissionsEndpoint : Endpoint<GetRolePermissionsRequest, IEnumerable<PermissionDTO>>
    {
        private readonly IUserPermissionService _service;

        public GetRolePermissionsEndpoint(IUserPermissionService service) => _service = service;

        public override void Configure()
        {
            Get("/permissions");
            Permissions("CAN_VIEW_PERMISSION");
            Description(b => b.WithName("GetRolePermissions").Produces<IEnumerable<Permission>>(200));
        }

        public override async Task HandleAsync(GetRolePermissionsRequest req, CancellationToken ct)
        {
            var permissions = await _service.GetRolePermissionsAsync(ObjectId.Parse(req.RoleId));

            var permissionsDTOList = new List<PermissionDTO>();

            foreach (var permission in permissions)
            {
                var permssionDTO = new PermissionDTO
                {
                    _id = permission._id.ToString(),
                    PermissionName = permission.PermissionName,
                    PermissionText = permission.PermissionText,
                    Description = permission.Description
                };
                permissionsDTOList.Add(permssionDTO);
            }


            await SendOkAsync(permissionsDTOList, ct);
        }
    }
}
