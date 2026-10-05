using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LossPrevention.Domain.Entities.DataIngestion;

public sealed class DataIngestionConfiguration
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

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

    public DateTime? LastRunAt { get; set; }

    // NEW: Manual Load flag - when true, schedule is optional
    public bool ManualLoad { get; set; } = false;

    // Schedule Configuration
    public string ScheduleType { get; set; } = "one-time"; // "one-time" | "recurring"
    public DateTime? ScheduleDate { get; set; }
    public string? ScheduleTime { get; set; }
    public string Recurrence { get; set; } = "daily"; // "daily" | "weekly"
    public List<string> SelectedDaysOfWeek { get; set; } = new();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool UseMappings { get; set; }
}