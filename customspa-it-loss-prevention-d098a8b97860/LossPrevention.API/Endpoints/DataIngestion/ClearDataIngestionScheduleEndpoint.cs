using FastEndpoints;
using LossPrevention.Application.Interfaces.DataIngestion;

namespace LossPrevention.API.Endpoints.DataIngestion;

public class ClearDataIngestionScheduleEndpoint : EndpointWithoutRequest
{
    private readonly IDataIngestionService _service;

    public ClearDataIngestionScheduleEndpoint(IDataIngestionService service)
    {
        _service = service;
    }

    public override void Configure()
    {
        Delete("/api/data-ingestion/schedule");
        Permissions("CAN_MANAGE_DATA_INGESTION");
        Summary(s => s.Summary = "Clear data ingestion schedule.");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            await _service.ClearScheduleAsync();
            await SendOkAsync(ct);
        }
        catch (Exception ex)
        {
            AddError("clear", $"Error clearing schedule: {ex.Message}");
            await SendErrorsAsync(500, ct);
        }
    }
}