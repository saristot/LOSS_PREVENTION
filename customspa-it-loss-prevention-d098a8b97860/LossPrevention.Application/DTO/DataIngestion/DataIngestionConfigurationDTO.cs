namespace LossPrevention.Application.DTO.DataIngestion;

public sealed class DataIngestionConfigurationDTO
{
    public string? Id { get; set; }
    public List<string> SelectedSources { get; set; } = new();
    public string SelectedFileType { get; set; } = string.Empty;

    // SFTP Configuration
    public string SftpHost { get; set; } = string.Empty;
    public int SftpPort { get; set; } = 22;
    public string SftpUsername { get; set; } = string.Empty;
    public string SftpPassword { get; set; } = string.Empty;
    public string SftpRemoteDirectory { get; set; } = string.Empty;

    // File System Configuration
    public string FileSystemPath { get; set; } = string.Empty;

    // NEW: Manual Load flag - when true, schedule is optional
    public bool ManualLoad { get; set; } = false;

    // Schedule Configuration
    public string ScheduleType { get; set; } = "one-time";
    public DateTime? ScheduleDate { get; set; }
    public string? ScheduleTime { get; set; }
    public string Recurrence { get; set; } = "daily";
    public List<string> SelectedDaysOfWeek { get; set; } = [];

    // Mappings
    public bool UseMappings { get; set; } = false;

}