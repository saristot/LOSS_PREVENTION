using FastEndpoints;
using LossPrevention.Application.Interfaces.DataIngestion;
using LossPrevention.Application.DTO.DataIngestion;

namespace LossPrevention.API.Endpoints.DataIngestion;

public class UpdateDataIngestionScheduleEndpoint : Endpoint<UpdateScheduleRequest>
{
    private readonly IDataIngestionService _service;

    public UpdateDataIngestionScheduleEndpoint(IDataIngestionService service)
    {
        _service = service;
    }

    public override void Configure()
    {
        Patch("/api/data-ingestion/schedule");
        Permissions("CAN_MANAGE_DATA_INGESTION");
        Summary(s => s.Summary = "Update data ingestion schedule.");
    }

    public override async Task HandleAsync(UpdateScheduleRequest req, CancellationToken ct)
    {
        try
        {
            await _service.UpdateScheduleAsync(req);
            await SendOkAsync(ct);
        }
        catch (Exception ex)
        {
            AddError("update", $"Error updating schedule: {ex.Message}");
            await SendErrorsAsync(500, ct);
        }
    }
}