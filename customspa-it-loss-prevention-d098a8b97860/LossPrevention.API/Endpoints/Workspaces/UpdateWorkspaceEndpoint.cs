using FastEndpoints;
using LossPrevention.Application.DTO.Workspaces;
using LossPrevention.Application.Interfaces.Workspaces;

namespace LossPrevention.API.Endpoints.Workspaces
{
    public class UpdateWorkspaceEndpoint : Endpoint<WorkspaceDTO, WorkspaceDTO>
    {
        private readonly IWorkspaceService _service;

        public UpdateWorkspaceEndpoint(IWorkspaceService service)
        {
            _service = service;
        }

        public override void Configure()
        {
            Put("/workspaces/{id}");
            Permissions("CAN_MANAGE_WORKSPACES");
            Summary(s => s.Summary = "Update a workspace.");
        }

        public override async Task HandleAsync(WorkspaceDTO req, CancellationToken ct)
        {
            var id = Route<string>("id");
            if (string.IsNullOrWhiteSpace(id))
            {
                await SendNotFoundAsync(ct);
                return;
            }

            try
            {
                var result = await _service.UpdateAsync(id, req);
                await SendOkAsync(result, ct);
            }
            catch (KeyNotFoundException)
            {
                await SendNotFoundAsync(ct);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error updating workspace {Id}", id);
                await SendErrorsAsync(400, ct);
            }
        }
    }
}
