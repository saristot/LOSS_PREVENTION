using FastEndpoints;
using LossPrevention.Application.Interfaces.DataIngestion;
using LossPrevention.Application.DTO.DataIngestion;

namespace LossPrevention.API.Endpoints.DataIngestion;

public class GetDataIngestionConfigurationEndpoint : EndpointWithoutRequest<DataIngestionConfigurationDTO>
{
    private readonly IDataIngestionService _service;

    public GetDataIngestionConfigurationEndpoint(IDataIngestionService service)
    {
        _service = service;
    }

    public override void Configure()
    {
        Get("/api/data-ingestion");
        Permissions("CAN_VIEW_DATA_INGESTION");
        Summary(s => s.Summary = "Get data ingestion configuration.");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var configuration = await _service.GetConfigurationAsync();
        
        if (configuration == null)
        {
            await SendOkAsync(new DataIngestionConfigurationDTO(), ct);
            return;
        }

        var dto = new DataIngestionConfigurationDTO
        {
            Id = configuration.Id,
            SelectedSources = configuration.SelectedSources,
            SelectedFileType = configuration.SelectedFileType,
            SftpHost = configuration.SftpHost,
            SftpPort = configuration.SftpPort,
            SftpUsername = configuration.SftpUsername,
            SftpPassword = configuration.SftpPassword,
            SftpRemoteDirectory = configuration.SftpRemoteDirectory,
            FileSystemPath = configuration.FileSystemPath,
            ScheduleType = configuration.ScheduleType,
            ScheduleDate = configuration.ScheduleDate,
            ScheduleTime = configuration.ScheduleTime,
            Recurrence = configuration.Recurrence,
            SelectedDaysOfWeek = configuration.SelectedDaysOfWeek,
            UseMappings = configuration.UseMappings
        };

        await SendOkAsync(dto, ct);
    }
}