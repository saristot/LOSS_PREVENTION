namespace LossPrevention.Application.DTO.FraudDetection;

public sealed class FraudDetectionSettingsDTO
{
    public string? Id { get; set; }
    public FraudThresholdConfigDTO Thresholds { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;
}

public sealed class FraudThresholdConfigDTO
{
    public HighValueThresholdDTO HighValue { get; set; } = new();
    public ExtremeHighValueThresholdDTO ExtremeHighValue { get; set; } = new();
    public LowValueThresholdDTO LowValue { get; set; } = new();
    public RefundAmountThresholdDTO RefundAmount { get; set; } = new();
    public RefundCountThresholdDTO RefundCount { get; set; } = new();
    public VoidAmountThresholdDTO VoidAmount { get; set; } = new();
    public VoidCountThresholdDTO VoidCount { get; set; } = new();
    public DiscountAmountThresholdDTO DiscountAmount { get; set; } = new();
    public DiscountCountThresholdDTO DiscountCount { get; set; } = new();
    public ManualOverrideThresholdDTO ManualOverride { get; set; } = new();
    public PriceOverrideThresholdDTO PriceOverride { get; set; } = new();
    public ItemQuantityThresholdDTO ItemQuantity { get; set; } = new();
    public GiftCardAmountThresholdDTO GiftCardAmount { get; set; } = new();
    public GiftCardCountThresholdDTO GiftCardCount { get; set; } = new();
    public PaymentMethodsThresholdDTO PaymentMethods { get; set; } = new();
    public LoyaltyPointsThresholdDTO LoyaltyPoints { get; set; } = new();
    public NoSaleEventsThresholdDTO NoSaleEvents { get; set; } = new();
    public VelocityThresholdDTO Velocity { get; set; } = new();
    public TemporalThresholdDTO Temporal { get; set; } = new();
    public SplitTransactionThresholdDTO SplitTransaction { get; set; } = new();
    public SweetheartingThresholdDTO Sweethearting { get; set; } = new();
    public ReturnFraudThresholdDTO ReturnFraud { get; set; } = new();
}

// Threshold DTO classes
public sealed class HighValueThresholdDTO
{
    public double Percentile { get; set; }
    public double StdDevMultiplier { get; set; }
    public double MinimumValue { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class ExtremeHighValueThresholdDTO
{
    public double Percentile { get; set; }
    public double Multiplier { get; set; }
    public double MinimumValue { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class LowValueThresholdDTO
{
    public double Percentile { get; set; }
    public double MeanMultiplier { get; set; }
    public double MinMultiplier { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class RefundAmountThresholdDTO
{
    public double Percentile { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class RefundCountThresholdDTO
{
    public double Percentile { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class VoidAmountThresholdDTO
{
    public double Percentile { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class VoidCountThresholdDTO
{
    public double Percentile { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class DiscountAmountThresholdDTO
{
    public double Percentile { get; set; }
    public double MeanMultiplier { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class DiscountCountThresholdDTO
{
    public double Percentile { get; set; }
    public int Minimum { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class ManualOverrideThresholdDTO
{
    public double Percentile { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class PriceOverrideThresholdDTO
{
    public double Percentile { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class ItemQuantityThresholdDTO
{
    public double Percentile { get; set; }
    public double Multiplier { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class GiftCardAmountThresholdDTO
{
    public double Percentile { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class GiftCardCountThresholdDTO
{
    public double Percentile { get; set; }
    public int Minimum { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class PaymentMethodsThresholdDTO
{
    public double Percentile { get; set; }
    public int Minimum { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class LoyaltyPointsThresholdDTO
{
    public double Percentile { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class NoSaleEventsThresholdDTO
{
    public double Percentile { get; set; }
    public int Minimum { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class VelocityThresholdDTO
{
    public int WindowMinutes { get; set; }
    public int MinTransactions { get; set; }
    public double Percentile { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class TemporalThresholdDTO
{
    public int OffHoursStart { get; set; }
    public int OffHoursEnd { get; set; }
    public double WeekendPenalty { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class SplitTransactionThresholdDTO
{
    public int WindowMinutes { get; set; }
    public double ThresholdProximity { get; set; }
    public int MinSplits { get; set; }
    public List<int> CommonThresholds { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class SweetheartingThresholdDTO
{
    public int MinOccurrences { get; set; }
    public double DiscountPercentile { get; set; }
    public int TimeWindowDays { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class ReturnFraudThresholdDTO
{
    public double ReturnRatePercentile { get; set; }
    public double ReturnAmountRatio { get; set; }
    public int FrequentReturner { get; set; }
    public int WindowDays { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}
