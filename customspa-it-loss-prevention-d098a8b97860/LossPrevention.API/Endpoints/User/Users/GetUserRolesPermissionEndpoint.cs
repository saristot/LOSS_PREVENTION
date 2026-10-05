using LossPrevention.API.User.Handlers.Requests.Users;
using LossPrevention.Application.DTO.Users;
using FastEndpoints;
using LossPrevention.API.User.Handlers.Requests.Users;
using MongoDB.Bson;
using LossPrevention.Application.Interfaces.User;

namespace LossPrevention.DataIngestion.API.Endpoints.User.Users;

public class GetUserRolesPermissionEndpoint : Endpoint<GetUserRolesPermissionRequest, UserWithRolesAndPermissionsDTO>
{
    private readonly IUserService _userService;

    public GetUserRolesPermissionEndpoint(IUserService userService)
    {
        _userService = userService;
    }

    public override void Configure()
    {
        Get("/users/rolesandpermissions/{Id}");
        Permissions("CAN_VIEW_USER");
        Description(b =>
        {
            b.WithName("GetUserRolesPermissionById");
            b.Produces(200);
            b.ProducesProblem(404);
            b.ProducesProblem(500);
        });
        Summary(new EndpointSummary()
        {
            Description = "Get user roles and permissions by specific Id"
        });
    }

    //TODO: Check the results are correct against roles and permissions.
    public override async Task HandleAsync(GetUserRolesPermissionRequest request, CancellationToken ct)
    {
        if (!ObjectId.TryParse(request.Id, out var objectId))
        {
            await SendErrorsAsync(404, ct);
            return;
        }

        var result = await _userService.GetUserWithRolesAndPermissions(objectId);

        if (result is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var userDto = new UserWithRolesAndPermissionsDTO()
        {
            User = new UserDTO()
            {
                _id = result.User._id.ToString(),
                Username = result.User.Username,
                PasswordHash = result.User.PasswordHash,
                PasswordSalt = result.User.PasswordSalt,
                Email = result.User.Email,
                FirstName = result.User.FirstName,
                LastName = result.User.LastName,
                RegistrationDate = result.User.RegistrationDate,
                IsActive = result.User.IsActive
            },
            Roles = new List<RoleDTO>(),
            Permissions = new List<PermissionDTO>()
        };

        foreach (var role in result.Roles)
        {
            userDto.Roles.Add(new RoleDTO() { _id = role._id.ToString(), RoleName = role.RoleName, Description = role.Description, Permissions = role.Permissions.Select(x => x.ToString()).ToList() });
        }

        foreach (var permission in result.Permissions)
        {
            userDto.Permissions.Add(new PermissionDTO() { _id = permission._id.ToString(), PermissionName = permission.PermissionName, Description = permission.Description, PermissionText = permission.PermissionText });
        }

        await SendOkAsync(userDto, ct);
    }
}

