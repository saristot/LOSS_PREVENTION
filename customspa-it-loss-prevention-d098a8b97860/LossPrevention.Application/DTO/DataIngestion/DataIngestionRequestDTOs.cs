namespace LossPrevention.Application.DTO.DataIngestion;

public sealed class UpdateSourcesRequest
{
    public List<string> SelectedSources { get; set; } = new();
}

public sealed class UpdateScheduleRequest
{
    public DateTime? ScheduleDate { get; set; }
    public string? ScheduleTime { get; set; }
    public string Recurrence { get; set; } = "daily";
    public string ScheduleType { get; set; } = "one-time";
    public List<string> SelectedDaysOfWeek { get; set; } = new();
}

public sealed class UpdateRecurrenceOptionsRequest
{
    public List<RecurrenceOptionDTO> RecurrenceOptions { get; set; } = new();
}

public sealed class RecurrenceOptionDTO
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}