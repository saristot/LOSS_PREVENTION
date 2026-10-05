using LossPrevention.API.User.Handlers.Requests.Users;
using LossPrevention.Application.DTO.Users;
using FastEndpoints;
using LossPrevention.Application.Interfaces.User;
using MongoDB.Bson;

namespace LossPrevention.API.Endpoints.User.Users;

public class CreateUserEndpoint : Endpoint<CreateUserRequest, UserDTO>
{
    private readonly IUserService _userService;

    public CreateUserEndpoint(IUserService userService)
    {
        _userService = userService;
    }

    public override void Configure()
    {
        Post("/users/create");
        Permissions("CAN_CREATE_USER");
        Description(b =>
        {
            b.WithName("CreateUser");
            b.Produces(200);
            b.ProducesProblem(404);
            b.ProducesProblem(500);
        });
        Summary(new EndpointSummary()
        {
            Description = "Create a new user with or without roles."
        });
    }

    public override async Task HandleAsync(CreateUserRequest request, CancellationToken ct)
    {
        var userDetails = new LossPrevention.Domain.Entities.Users.User()
        {
            Username = request.Username,
            PasswordHash = string.Empty,
            PasswordSalt = string.Empty,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            RegistrationDate = DateTime.Now,
            IsActive = request.IsActive,
            Roles = request.Roles.Select(x => ObjectId.Parse(x)).ToList()
        };

        var user = await _userService.CreateUserAsync(userDetails, request.Password);

        if (user is null)
        {
            await SendErrorsAsync(400, ct);
            return;
        }

        // Return the User with new _id
        var createdUserDto = new UserDTO()
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

        await SendAsync(createdUserDto, 201, ct);
    }
}

