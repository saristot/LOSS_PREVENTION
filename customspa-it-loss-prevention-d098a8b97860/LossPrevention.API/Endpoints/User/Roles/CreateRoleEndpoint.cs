using LossPrevention.DataIngestion.API.User.Handlers.Requests.Roles;
using FastEndpoints;
using MongoDB.Bson;
using LossPrevention.Application.DTO.Users;
using LossPrevention.Domain.Entities.Users;
using LossPrevention.Application.Interfaces.User;

namespace LossPrevention.DataIngestion.API.Endpoints.User.Roles;

public class CreateRoleEndpoint : Endpoint<CreateRoleRequest, RoleDTO>
{
    private readonly IUserRoleService _userRoleService;

    public CreateRoleEndpoint(IUserRoleService userRoleService)
    {
        _userRoleService = userRoleService;
    }

    public override void Configure()
    {
        Post("/roles");
        Permissions("CAN_CREATE_ROLE");
        Description(b =>
        {
            b.WithName("AddRole");
            b.Produces(200);
            b.ProducesProblem(400);
            b.ProducesProblem(500);

        });
        Summary(new EndpointSummary() { Summary = "Add New Role." });
    }

    public override async Task HandleAsync(CreateRoleRequest request, CancellationToken ct)
    {
        //TODDO: check if permissions actually exist before assigning.
        var role = new Role()
        {
            RoleName = request.RoleName,
            Description = request.Description,
            Permissions = request.Permissions.Select(x => ObjectId.Parse(x)).ToList()
        };

        var result = await _userRoleService.CreateRoleAsync(role);

        var roleDTO = new RoleDTO()
        {
            _id = result._id.ToString(),
            RoleName = result.RoleName,
            Description = result.Description,
            Permissions = result.Permissions.Select(x => x.ToString()).ToList()
        };

        await SendOkAsync(roleDTO, ct);
    }
}

