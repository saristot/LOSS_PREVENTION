using FastEndpoints;
using LossPrevention.Application.DTO.Users;
using LossPrevention.Application.Interfaces.User;
using LossPrevention.Domain.Entities.Users;

namespace LossPrevention.DataIngestion.API.Endpoints.User.Permissions
{
    public class GetAllPermissionsEndpoint : EndpointWithoutRequest<IEnumerable<PermissionDTO>>
    {
        private readonly IUserPermissionService _service;

        public GetAllPermissionsEndpoint(IUserPermissionService service) => _service = service;

        public override void Configure()
        {
            Get("/permissions/all");
            Permissions("CAN_VIEW_PERMISSION");
            Description(b => b.WithName("GetAllPermissions").Produces<IEnumerable<Permission>>(200));
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var permissions = await _service.GetAllPermissionsAsync();

            var resultPermissions = new List<PermissionDTO>();
            foreach (var permission in permissions)
            {
                var newPermission = new PermissionDTO
                {
                    _id = permission._id.ToString(),
                    PermissionName = permission.PermissionName,
                    PermissionText = permission.PermissionText,
                    Description = permission.Description
                };

                resultPermissions.Add(newPermission);
            }

            await SendOkAsync(resultPermissions, ct);
        }
    }
}
