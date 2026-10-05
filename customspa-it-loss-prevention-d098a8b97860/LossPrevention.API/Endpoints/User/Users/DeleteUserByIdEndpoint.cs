using LossPrevention.API.User.Handlers.Requests.Users;
using LossPrevention.Application.DTO.Users;
using FastEndpoints;
using MongoDB.Bson;
using LossPrevention.Application.Interfaces.User;

namespace LossPrevention.DataIngestion.API.Endpoints.User.Users;

public class DeleteUserByIdEndpoint : Endpoint<DeleteUserByIdRequest, UserDTO>
{
    private readonly IUserService _userService;

    public DeleteUserByIdEndpoint(IUserService userService)
    {
        _userService = userService;
    }

    public override void Configure()
    {
        Delete("/users/id/{UserId}");
        Permissions("CAN_DELETE_USER");
        Description(b =>
        {
            b.WithName("DeleteByUserId");
            b.Produces(200);
            b.ProducesProblem(404);
            b.ProducesProblem(500);
        });
        Summary(new EndpointSummary()
        {
            Description = "Delete user by specific Id"
        });
    }

    public override async Task HandleAsync(DeleteUserByIdRequest request, CancellationToken ct)
    {
        if (!ObjectId.TryParse(request.UserId, out var objectId))
        {
            await SendErrorsAsync(404, ct);
            return;
        }

        var deleteResult = await _userService.DeleteUserAsync(objectId);

        if (deleteResult == false)
        {
            await SendErrorsAsync(400, ct);
            return;
        }

        await SendOkAsync(ct);
    }
}

