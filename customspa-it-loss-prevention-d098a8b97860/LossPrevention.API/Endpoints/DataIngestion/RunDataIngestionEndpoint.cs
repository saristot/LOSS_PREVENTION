using FastEndpoints;
using LossPrevention.Application.Services.DataIngestion;
using Microsoft.AspNetCore.Http;

namespace LossPrevention.API.Endpoints.DataIngestion;

public class RunDataIngestionEndpoint : EndpointWithoutRequest
{
    private readonly IFileProcessingCoordinator _coordinator;

    public RunDataIngestionEndpoint(IFileProcessingCoordinator coordinator)
    {
        _coordinator = coordinator;
    }

    public override void Configure()
    {
        Post("/api/data-ingestion/run");
        Permissions("CAN_MANAGE_DATA_INGESTION");
        Summary(s => s.Summary = "Run data ingestion immediately.");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var result = await _coordinator.RunIngestionAsync(ct);

            if (result.Success)
            {
                await SendOkAsync(new
                {
                    success = true,
                    filesProcessed = result.FilesProcessed,
                    recordsInserted = result.RecordsInserted,
                    durationSeconds = result.Duration.TotalSeconds,
                    errors = result.Errors,
                    message = $"Successfully processed {result.FilesProcessed} files and inserted {result.RecordsInserted} records"
                }, ct);
            }
            else
            {
                await SendAsync(new
                {
                    success = false,
                    filesProcessed = result.FilesProcessed,
                    recordsInserted = result.RecordsInserted,
                    durationSeconds = result.Duration.TotalSeconds,
                    errors = result.Errors,
                    message = "Data ingestion completed with errors"
                }, StatusCodes.Status500InternalServerError, ct);
            }
        }
        catch (Exception ex)
        {
            await SendAsync(new
            {
                success = false,
                message = $"Data ingestion failed: {ex.Message}"
            }, StatusCodes.Status500InternalServerError, ct);
        }
    }
}