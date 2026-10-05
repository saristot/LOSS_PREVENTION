namespace LossPrevention.Application.DTO.DataIngestion;

public sealed class DataIngestionScheduleDTO
{
    public string? Id { get; set; }
    public string ConfigurationId { get; set; } = string.Empty;
    public string ScheduleType { get; set; } = "one-time";
    public DateTime? ScheduleDate { get; set; }
    public string? ScheduleTime { get; set; }
    public string Recurrence { get; set; } = "daily";
    public List<string> SelectedDaysOfWeek { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public DateTime? LastExecuted { get; set; }
    public DateTime? NextExecution { get; set; }
}