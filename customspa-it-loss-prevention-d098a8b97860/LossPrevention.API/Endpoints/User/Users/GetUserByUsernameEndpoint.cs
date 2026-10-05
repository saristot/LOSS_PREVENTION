using LossPrevention.API.User.Handlers.Requests.Users;
using LossPrevention.Application.DTO.Users;
using LossPrevention.Domain.Entities.Users;
using FastEndpoints;
using LossPrevention.Application.Interfaces.User;

namespace LossPrevention.DataIngestion.API.Endpoints.User.Users;

public class GetUserByUsernameEndpoint : Endpoint<GetUserByUsernameRequest, UserDTO>
{
    private readonly IUserService _userService;

    public GetUserByUsernameEndpoint(IUserService userService)
    {
        _userService = userService;
    }

    public override void Configure()
    {
        Get("/users/username/{Username}");
        Permissions("CAN_VIEW_USER");
        Description(b =>
        {
            b.WithName("GetByUserUsername");
            b.Produces(200);
            b.ProducesProblem(404);
            b.ProducesProblem(500);
        });
        Summary(new EndpointSummary()
        {
            Description = "Get user by specific username"
        });
    }

    public override async Task HandleAsync(GetUserByUsernameRequest request, CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(request.Username))
        {
            await SendErrorsAsync(404, ct);
            return;
        }

        var user = await _userService.GetUserByUsernameAsync(request.Username);

        if (user is null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var userDto = new UserDTO()
        {
            _id = user._id.ToString(),
            Username = user.Username,
            PasswordHash = user.PasswordHash,
            PasswordSalt = user.PasswordSalt,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            RegistrationDate = user.RegistrationDate,
            IsActive = user.IsActive,
            Roles = user.Roles.Select(x => x.ToString()).ToList()
        };

        await SendOkAsync(userDto, ct);
    }
}

