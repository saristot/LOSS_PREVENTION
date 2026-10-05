using LossPrevention.API.User.Handlers.Requests.Users;
using LossPrevention.Application.DTO.Users;
using FastEndpoints;
using MongoDB.Bson;
using LossPrevention.Application.Interfaces.User;

namespace LossPrevention.DataIngestion.API.Endpoints.User.Users;

public class GetUserByIdEndpoint : Endpoint<GetUserByIdRequest, UserDTO>
{
    private readonly IUserService _userService;

    public GetUserByIdEndpoint(IUserService userService)
    {
        _userService = userService;
    }

    public override void Configure()
    {
        Get("/users/id/{Id}");
        Permissions("CAN_VIEW_USER");
        Description(b =>
        {
            b.WithName("GetByUserId");
            b.Produces(200);
            b.ProducesProblem(404);
            b.ProducesProblem(500);
        });
        Summary(new EndpointSummary()
        {
            Description = "Get user by specific Id"
        });

    }

    public override async Task HandleAsync(GetUserByIdRequest request, CancellationToken ct)
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
            Roles = user.Roles.Select(x => x.ToString()).ToList(),
            LockField = user.LockField,
            LockValue = user.LockValue
        };

        await SendOkAsync(userDto, ct);
    }
}

