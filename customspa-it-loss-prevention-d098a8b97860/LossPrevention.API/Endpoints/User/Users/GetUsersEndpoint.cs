using LossPrevention.Application.DTO.Users;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using LossPrevention.Application.Interfaces.User;

namespace LossPrevention.DataIngestion.API.Endpoints.User.Users;

public class GetUsersEndpoint : EndpointWithoutRequest<IEnumerable<UserDTO>>
{
    private readonly IUserService _userService;

    public GetUsersEndpoint(IUserService userService)
    {
        _userService = userService;
    }

    public override void Configure()
    {
        Get("/users");
        Permissions("CAN_VIEW_USER");
        Description(b =>
        {
            b.WithName("GetAllUsers");
            b.Produces(200);
            b.ProducesProblem(500);
        });
        Summary(new EndpointSummary()
        {
            Description = "This endpoint retrieves a comprehensive list of all users..."
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var users = await _userService.GetAllUsersAsync();
        var userDtos = users.Select(x => new UserDTO() { _id = x._id.ToString(), Username = x.Username, PasswordHash = x.PasswordHash, PasswordSalt = x.PasswordSalt, Email = x.Email, FirstName = x.FirstName, LastName = x.LastName, RegistrationDate = x.RegistrationDate, IsActive = x.IsActive, Roles = x.Roles.Select(x => x.ToString()).ToList(), LockField = x.LockField, LockValue = x.LockValue });
        await SendOkAsync(userDtos, ct);
    }
}