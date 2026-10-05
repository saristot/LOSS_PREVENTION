using FastEndpoints;
using LossPrevention.Application.Interfaces.DataIngestion;
using LossPrevention.Application.DTO.DataIngestion;

namespace LossPrevention.API.Endpoints.DataIngestion;

public class UpdateDataIngestionSourcesEndpoint : Endpoint<UpdateSourcesRequest>
{
    private readonly IDataIngestionService _service;

    public UpdateDataIngestionSourcesEndpoint(IDataIngestionService service)
    {
        _service = service;
    }

    public override void Configure()
    {
        Patch("/api/data-ingestion/sources");
        Permissions("CAN_MANAGE_DATA_INGESTION");
        Summary(s => s.Summary = "Update selected data sources.");
    }

    public override async Task HandleAsync(UpdateSourcesRequest req, CancellationToken ct)
    {
        try
        {
            await _service.UpdateSourcesAsync(req.SelectedSources);
            await SendOkAsync(ct);
        }
        catch (Exception ex)
        {
            AddError("update", $"Error updating sources: {ex.Message}");
            await SendErrorsAsync(500, ct);
        }
    }
}