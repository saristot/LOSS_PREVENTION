using FastEndpoints;
using LossPrevention.Application.Interfaces.DataIngestion;
using LossPrevention.Application.DTO.DataIngestion;

namespace LossPrevention.API.Endpoints.DataIngestion;

public class UpdateDataIngestionConfigurationEndpoint : Endpoint<DataIngestionConfigurationDTO, DataIngestionConfigurationDTO>
{
    private readonly IDataIngestionService _service;

    public UpdateDataIngestionConfigurationEndpoint(IDataIngestionService service)
    {
        _service = service;
    }

    public override void Configure()
    {
        Put("/api/data-ingestion");
        Permissions("CAN_MANAGE_DATA_INGESTION");
        Summary(s => s.Summary = "Save complete data ingestion configuration.");
    }

    public override async Task HandleAsync(DataIngestionConfigurationDTO req, CancellationToken ct)
    {
        try
        {
            // Validate required fields
            var errors = new List<string>();
            
            if (req.SelectedSources == null || req.SelectedSources.Count == 0)
                errors.Add("At least one source must be selected");
            
            if (string.IsNullOrWhiteSpace(req.SelectedFileType))
                errors.Add("File type is required");
            
            var source = req.SelectedSources?.FirstOrDefault();
            if (source == "sftp")
            {
                if (string.IsNullOrWhiteSpace(req.SftpHost)) errors.Add("SFTP host is required");
                if (req.SftpPort < 1 || req.SftpPort > 65535) errors.Add("SFTP port must be between 1 and 65535");
                if (string.IsNullOrWhiteSpace(req.SftpUsername)) errors.Add("SFTP username is required");
                if (string.IsNullOrWhiteSpace(req.SftpPassword)) errors.Add("SFTP password is required");
                if (string.IsNullOrWhiteSpace(req.SftpRemoteDirectory)) errors.Add("SFTP remote directory is required");
            }
            else if (source == "filesystem")
            {
                if (string.IsNullOrWhiteSpace(req.FileSystemPath)) errors.Add("File system path is required");
            }
            
            if (!req.ManualLoad)
            {
                if (string.IsNullOrWhiteSpace(req.ScheduleTime))
                    errors.Add("Schedule time is required");

                if (req.ScheduleType == "one-time" && !req.ScheduleDate.HasValue)
                    errors.Add("Schedule date is required for one-time schedule");

                if (req.ScheduleType == "recurring" && req.Recurrence == "weekly" &&
                    (req.SelectedDaysOfWeek == null || req.SelectedDaysOfWeek.Count == 0))
                    errors.Add("At least one day must be selected for weekly recurrence");
            }
            
            if (errors.Count > 0)
            {
                foreach (var error in errors)
                {
                    AddError("validation", error);
                }
                await SendErrorsAsync(400, ct);
                return;
            }

            var savedConfiguration = await _service.SaveConfigurationAsync(req);
            
            var responseDto = new DataIngestionConfigurationDTO
            {
                Id = savedConfiguration.Id,
                SelectedSources = savedConfiguration.SelectedSources,
                SelectedFileType = savedConfiguration.SelectedFileType,
                SftpHost = savedConfiguration.SftpHost,
                SftpPort = savedConfiguration.SftpPort,
                SftpUsername = savedConfiguration.SftpUsername,
                SftpPassword = savedConfiguration.SftpPassword,
                SftpRemoteDirectory = savedConfiguration.SftpRemoteDirectory,
                FileSystemPath = savedConfiguration.FileSystemPath,
                ScheduleType = savedConfiguration.ScheduleType,
                ScheduleDate = savedConfiguration.ScheduleDate,
                ScheduleTime = savedConfiguration.ScheduleTime,
                Recurrence = savedConfiguration.Recurrence,
                SelectedDaysOfWeek = savedConfiguration.SelectedDaysOfWeek,
                UseMappings = savedConfiguration.UseMappings
            };

            await SendOkAsync(responseDto, ct);
        }
        catch (Exception ex)
        {
            AddError("save", $"Error saving configuration: {ex.Message}");
            await SendErrorsAsync(500, ct);
        }
    }
}