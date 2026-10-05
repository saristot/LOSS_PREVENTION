using LossPrevention.API.User.Handlers.Requests.Users;
using LossPrevention.Application.DTO.Users;
using FastEndpoints;
using MongoDB.Bson;
using LossPrevention.Application.Interfaces.User;

namespace LossPrevention.DataIngestion.API.Endpoints.User.Users;

public class UpdateUserEndpoint : Endpoint<UpdateUserRequest>
{
    private readonly IUserService _userService;

    public UpdateUserEndpoint(IUserService userService)
    {
        _userService = userService;
    }

    public override void Configure()
    {
        Post("/users/update");
        Permissions("CAN_UPDATE_USER");
        Description(b =>
        {
            b.WithName("UpdateUser");
            b.Produces(204);
            b.ProducesProblem(400);
            b.ProducesProblem(500);
        });
        Summary(new EndpointSummary()
        {
            Description = "Update an existing user with or without roles."
        });
    }

    public override async Task HandleAsync(UpdateUserRequest request, CancellationToken ct)
    {
        var userDetails = new Domain.Entities.Users.User()
        {
            _id = ObjectId.Parse(request.Id),
            Username = request.Username,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            RegistrationDate = DateTime.Now,
            IsActive = request.IsActive,
            Roles = request.Roles.Select(x => ObjectId.Parse(x)).ToList(),
            LockField = request.LockField,
            LockValue = request.LockValue,
        };

        var updateResult = await _userService.UpdateUserAsync(userDetails);

        if (!updateResult)
        {
            await SendErrorsAsync(400, ct);
            return;
        }

        await SendNoContentAsync(ct);
    }
}

