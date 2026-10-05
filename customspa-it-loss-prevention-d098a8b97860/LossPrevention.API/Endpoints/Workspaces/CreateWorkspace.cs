using FastEndpoints;
using LossPrevention.Application.DTO.Workspaces;
using LossPrevention.Application.Interfaces.Workspaces;

namespace LossPrevention.API.Endpoints.Workspaces
{
    public class CreateWorkspaceEndpoint : Endpoint<WorkspaceDTO, WorkspaceDTO>
    {
        private readonly IWorkspaceService _service;

        public CreateWorkspaceEndpoint(IWorkspaceService service)
        {
            _service = service;
        }

        public override void Configure()
        {
            Post("/workspaces");
            Permissions("CAN_MANAGE_WORKSPACES");
            Summary(s => s.Summary = "Create a new workspace.");
        }

        public override async Task HandleAsync(WorkspaceDTO req, CancellationToken ct)
        {
            try
            {
                var result = await _service.AddAsync(req);
                await SendOkAsync(result, ct);
            }
            catch (ArgumentException ex)
            {
                AddError(ex.Message);
                await SendErrorsAsync(400, ct);
            }
        }
    }
}
