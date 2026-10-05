using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LossPrevention.Domain.Entities.DataIngestion;

public sealed class DataIngestionSchedule
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    
    public string ConfigurationId { get; set; } = string.Empty;
    public string ScheduleType { get; set; } = "one-time"; // "one-time" | "recurring"
    public DateTime? ScheduleDate { get; set; }
    public string? ScheduleTime { get; set; }
    public string Recurrence { get; set; } = "daily"; // "daily" | "weekly"
    public List<string> SelectedDaysOfWeek { get; set; } = new();
    
    public bool IsActive { get; set; } = true;
    public DateTime? LastExecuted { get; set; }
    public DateTime? NextExecution { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}