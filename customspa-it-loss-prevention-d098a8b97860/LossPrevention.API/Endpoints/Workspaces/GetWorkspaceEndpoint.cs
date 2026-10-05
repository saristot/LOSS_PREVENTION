using FastEndpoints;
using LossPrevention.Application.DTO.Workspaces;
using LossPrevention.Application.Interfaces.Workspaces;

namespace LossPrevention.API.Endpoints.Workspaces
{
    public class GetWorkspaceEndpoint : EndpointWithoutRequest<WorkspaceDTO>
    {
        private readonly IWorkspaceService _service;

        public GetWorkspaceEndpoint(IWorkspaceService service)
        {
            _service = service;
        }

        public override void Configure()
        {
            Get("/workspaces/{id}");
            Permissions("CAN_VIEW_WORKSPACES");
            Summary(s => s.Summary = "Returns a workspace by ID.");
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var id = Route<string>("id");
            var workspace = await _service.GetByIdAsync(id);

            if (workspace is null)
            {
                await SendNotFoundAsync(ct);
                return;
            }

            await SendOkAsync(workspace, ct);
        }
    }
}
