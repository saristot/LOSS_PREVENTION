using FastEndpoints;
using LossPrevention.Application.Interfaces.DataIngestion;
using LossPrevention.Application.DTO.DataIngestion;

namespace LossPrevention.API.Endpoints.DataIngestion;

public class UpdateRecurrenceOptionsEndpoint : Endpoint<UpdateRecurrenceOptionsRequest>
{
    private readonly IDataIngestionService _service;

    public UpdateRecurrenceOptionsEndpoint(IDataIngestionService service)
    {
        _service = service;
    }

    public override void Configure()
    {
        Put("/api/data-ingestion/recurrence-options");
        Permissions("CAN_MANAGE_DATA_INGESTION");
        Summary(s => s.Summary = "Update recurrence options.");
    }

    public override async Task HandleAsync(UpdateRecurrenceOptionsRequest req, CancellationToken ct)
    {
        try
        {
            await _service.UpdateRecurrenceOptionsAsync(req.RecurrenceOptions);
            await SendOkAsync(ct);
        }
        catch (Exception ex)
        {
            AddError("update", $"Error updating recurrence options: {ex.Message}");
            await SendErrorsAsync(500, ct);
        }
    }
}