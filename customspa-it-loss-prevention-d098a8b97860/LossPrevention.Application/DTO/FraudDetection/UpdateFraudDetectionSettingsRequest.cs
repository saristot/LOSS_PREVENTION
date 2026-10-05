namespace LossPrevention.Application.DTO.FraudDetection;

public sealed class UpdateFraudDetectionSettingsRequest
{
    public string Id { get; set; } = string.Empty;
    public FraudThresholdConfigDTO Thresholds { get; set; } = new();
    public DateTime UpdatedAt { get; set; }
}
