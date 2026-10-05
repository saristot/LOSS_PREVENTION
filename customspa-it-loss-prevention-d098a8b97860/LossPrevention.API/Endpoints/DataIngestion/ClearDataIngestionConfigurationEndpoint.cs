using FastEndpoints;
using LossPrevention.Application.Interfaces.DataIngestion;

namespace LossPrevention.API.Endpoints.DataIngestion;

public class ClearDataIngestionConfigurationEndpoint : EndpointWithoutRequest
{
    private readonly IDataIngestionService _service;

    public ClearDataIngestionConfigurationEndpoint(IDataIngestionService service)
    {
        _service = service;
    }

    public override void Configure()
    {
        Delete("/api/data-ingestion");
        Permissions("CAN_MANAGE_DATA_INGESTION");
        Summary(s => s.Summary = "Clear complete data ingestion configuration.");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            await _service.ClearConfigurationAsync();
            await SendOkAsync(ct);
        }
        catch (Exception ex)
        {
            AddError("clear", $"Error clearing configuration: {ex.Message}");
            await SendErrorsAsync(500, ct);
        }
    }
}