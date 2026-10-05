using FastEndpoints;
using LossPrevention.Application.Interfaces.User;
using LossPrevention.DataIngestion.API.User.Handlers.Requests.Users;
using MongoDB.Bson;

namespace LossPrevention.DataIngestion.API.Endpoints.User.Roles;

public class AddUserToRoleEndpoint : Endpoint<AddUserToRoleRequest, bool>
{
    private readonly IUserRoleService _userRoleService;

    public AddUserToRoleEndpoint(IUserRoleService userRoleService)
    {
        _userRoleService = userRoleService;
    }

    public override void Configure()
    {
        Post("/roles/user");
        Permissions("CAN_ASSIGN_ROLE");
        Description(b =>
        {
            b.WithName("AddUserToRole");
            b.Produces(200);
            b.ProducesProblem(400);
            b.ProducesProblem(500);

        });
        Summary(new EndpointSummary() { Summary = "Add User To Role using ids." });
    }

    public override async Task HandleAsync(AddUserToRoleRequest request, CancellationToken ct)
    {
        if (!ObjectId.TryParse(request.UserId, out var userId))
        {
            await SendErrorsAsync(400, ct);
            return;
        }

        if (!ObjectId.TryParse(request.RoleId, out var roleId))
        {
            await SendErrorsAsync(400, ct);
            return;
        }

        var result = await _userRoleService.AddUserToRoleAsync(userId, roleId);

        await SendOkAsync(result, ct);
    }
}

