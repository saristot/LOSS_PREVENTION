using FastEndpoints;
using LossPrevention.API.Handlers.Requests.User.Permissions;
using LossPrevention.Application.DTO.Users;
using LossPrevention.Application.Interfaces.User;
using LossPrevention.Domain.Entities.Users;

namespace LossPrevention.DataIngestion.API.Endpoints.User.Permissions
{
    public class CreatePermissionEndpoint : Endpoint<CreatePermissionRequest, PermissionDTO>
    {
        private readonly IUserPermissionService _service;

        public CreatePermissionEndpoint(IUserPermissionService service) => _service = service;

        public override void Configure()
        {
            Post("/permissions/create");
            Permissions("CAN_CREATE_PERMISSION");
            Description(b => b.WithName("CreatePermission").Produces<Permission>(200));
        }

        public override async Task HandleAsync(CreatePermissionRequest req, CancellationToken ct)
        {
            var permission = new Permission()
            {
                PermissionName = req.PermissionName,
                PermissionText = req.PermissionText,
                Description = req.Description
            };

            var created = await _service.CreatePermissionAsync(permission);

            var permissionDTO = new PermissionDTO()
            {
                _id = created._id.ToString(),
                PermissionName = created.PermissionName,
                PermissionText = created.PermissionText,
                Description = created.Description
            };

            await SendOkAsync(permissionDTO, ct);
        }
    }
}
