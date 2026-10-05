namespace LossPrevention.Application.DTO.FraudDetection;

public sealed class CreateFraudDetectionSettingsRequest
{
    public FraudThresholdConfigDTO Thresholds { get; set; } = new();
}
