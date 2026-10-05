using FastEndpoints;
using LossPrevention.Application.DTO.Workspaces;
using LossPrevention.Application.Interfaces.Workspaces;

namespace LossPrevention.API.Endpoints.Workspaces
{
    public class GetAllWorkspacesEndpoint : EndpointWithoutRequest<List<WorkspaceDTO>>
    {
        private readonly IWorkspaceService _service;

        public GetAllWorkspacesEndpoint(IWorkspaceService service)
        {
            _service = service;
        }

        public override void Configure()
        {
            Get("/workspaces");
            Permissions("CAN_VIEW_WORKSPACES");
            Summary(s => s.Summary = "Returns a list of all workspaces.");
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var workspaces = await _service.GetAllAsync();
            await SendOkAsync(workspaces, ct);
        }
    }
}
