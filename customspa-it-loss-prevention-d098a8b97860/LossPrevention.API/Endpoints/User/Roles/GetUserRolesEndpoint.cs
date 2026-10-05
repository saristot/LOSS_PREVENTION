using LossPrevention.DataIngestion.API.User.Handlers.Requests.Roles;
using FastEndpoints;
using MongoDB.Bson;
using LossPrevention.Application.Interfaces.User;

namespace LossPrevention.DataIngestion.API.Endpoints.User.Roles
{
    public sealed class GetUserRolesEndpoint : Endpoint<GetUserRolesRequest, List<string>>
    {
        private readonly IUserRoleService _service;

        public GetUserRolesEndpoint(IUserRoleService service) => _service = service;

        public override void Configure()
        {
            Get("/users/roles");
            Permissions("CAN_VIEW_ROLE");
            Description(b => b.WithName("GetUserRoles").Produces<List<string>>(200));
        }

        public override async Task HandleAsync(GetUserRolesRequest req, CancellationToken ct)
        {
            var roles = await _service.GetUserRolesAsync(ObjectId.Parse(req.UserId));
            await SendOkAsync(roles.Select(r => r.ToString()).ToList(), ct);
        }
    }
}
