using LossPrevention.API.User.Handlers.Requests.Users;
using LossPrevention.Application.DTO.Users;
using LossPrevention.Infrastructure.Helpers;
using FastEndpoints;
using MongoDB.Bson;
using LossPrevention.Application.Interfaces.User;

namespace LossPrevention.DataIngestion.API.Endpoints.User.Users;

public class ValidatePasswordEndpoint : Endpoint<ValidatePasswordRequest, bool>
{
    private readonly IUserService _userService;

    public ValidatePasswordEndpoint(IUserService userService)
    {
        _userService = userService;
    }

    public override void Configure()
    {
        Post("/users/validatepassword");
        Permissions("CAN_VIEW_USER");
        Description(b =>
        {
            b.WithName("ValidatePasswordById");
            b.Produces(200);
            b.ProducesProblem(404);
            b.ProducesProblem(500);
        });
        Summary(new EndpointSummary()
        {
            Description = "Validate the password by user id"
        });
    }

    public override async Task HandleAsync(ValidatePasswordRequest request, CancellationToken ct)
    {
        if (!ObjectId.TryParse(request.Id, out var objectId))
        {
            await SendErrorsAsync(404, ct);
            return;
        }

        var user = await _userService.GetUserByIdAsync(objectId);
        if (user is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var validatePassword = _userService.ValidatePasswordAsync(user, request.Password);

        await SendOkAsync(validatePassword, ct);
    }
}

