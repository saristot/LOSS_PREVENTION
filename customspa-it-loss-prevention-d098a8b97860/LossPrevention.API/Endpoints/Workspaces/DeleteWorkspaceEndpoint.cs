using FastEndpoints;
using LossPrevention.Application.Interfaces.Workspaces;

namespace LossPrevention.API.Endpoints.Workspaces
{
    public class DeleteWorkspaceEndpoint : EndpointWithoutRequest
    {
        private readonly IWorkspaceService _service;

        public DeleteWorkspaceEndpoint(IWorkspaceService service)
        {
            _service = service;
        }

        public override void Configure()
        {
            Delete("/workspaces/{id}");
            Permissions("CAN_MANAGE_WORKSPACES");
            Summary(s => s.Summary = "Delete a workspace.");
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var id = Route<string>("id");
            if (string.IsNullOrEmpty(id))
            {
                await SendErrorsAsync(400, ct);
                return;
            }

            try
            {
                await _service.DeleteAsync(id);
                await SendNoContentAsync(ct);
            }
            catch (KeyNotFoundException)
            {
                await SendNotFoundAsync(ct);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error deleting workspace {Id}", id);
                await SendErrorsAsync(500, ct);
            }
        }
    }

}
