using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace LossPrevention.Domain.Entities.FraudDetection;

public sealed class FraudDetectionSettings
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("thresholds")]
    public FraudThresholdConfig Thresholds { get; set; } = new();

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    [BsonElement("createdBy")]
    public string CreatedBy { get; set; } = string.Empty;

    [BsonElement("updatedBy")]
    public string UpdatedBy { get; set; } = string.Empty;
}

public sealed class FraudThresholdConfig
{
    public HighValueThreshold HighValue { get; set; } = new();
    public ExtremeHighValueThreshold ExtremeHighValue { get; set; } = new();
    public LowValueThreshold LowValue { get; set; } = new();
    public RefundAmountThreshold RefundAmount { get; set; } = new();
    public RefundCountThreshold RefundCount { get; set; } = new();
    public VoidAmountThreshold VoidAmount { get; set; } = new();
    public VoidCountThreshold VoidCount { get; set; } = new();
    public DiscountAmountThreshold DiscountAmount { get; set; } = new();
    public DiscountCountThreshold DiscountCount { get; set; } = new();
    public ManualOverrideThreshold ManualOverride { get; set; } = new();
    public PriceOverrideThreshold PriceOverride { get; set; } = new();
    public ItemQuantityThreshold ItemQuantity { get; set; } = new();
    public GiftCardAmountThreshold GiftCardAmount { get; set; } = new();
    public GiftCardCountThreshold GiftCardCount { get; set; } = new();
    public PaymentMethodsThreshold PaymentMethods { get; set; } = new();
    public LoyaltyPointsThreshold LoyaltyPoints { get; set; } = new();
    public NoSaleEventsThreshold NoSaleEvents { get; set; } = new();
    public VelocityThreshold Velocity { get; set; } = new();
    public TemporalThreshold Temporal { get; set; } = new();
    public SplitTransactionThreshold SplitTransaction { get; set; } = new();
    public SweetheartingThreshold Sweethearting { get; set; } = new();
    public ReturnFraudThreshold ReturnFraud { get; set; } = new();
}

// Threshold classes
public sealed class HighValueThreshold
{
    public double Percentile { get; set; }
    public double StdDevMultiplier { get; set; }
    public double MinimumValue { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class ExtremeHighValueThreshold
{
    public double Percentile { get; set; }
    public double Multiplier { get; set; }
    public double MinimumValue { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class LowValueThreshold
{
    public double Percentile { get; set; }
    public double MeanMultiplier { get; set; }
    public double MinMultiplier { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class RefundAmountThreshold
{
    public double Percentile { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class RefundCountThreshold
{
    public double Percentile { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class VoidAmountThreshold
{
    public double Percentile { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class VoidCountThreshold
{
    public double Percentile { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class DiscountAmountThreshold
{
    public double Percentile { get; set; }
    public double MeanMultiplier { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class DiscountCountThreshold
{
    public double Percentile { get; set; }
    public int Minimum { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class ManualOverrideThreshold
{
    public double Percentile { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class PriceOverrideThreshold
{
    public double Percentile { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class ItemQuantityThreshold
{
    public double Percentile { get; set; }
    public double Multiplier { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class GiftCardAmountThreshold
{
    public double Percentile { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class GiftCardCountThreshold
{
    public double Percentile { get; set; }
    public int Minimum { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class PaymentMethodsThreshold
{
    public double Percentile { get; set; }
    public int Minimum { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class LoyaltyPointsThreshold
{
    public double Percentile { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class NoSaleEventsThreshold
{
    public double Percentile { get; set; }
    public int Minimum { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class VelocityThreshold
{
    public int WindowMinutes { get; set; }
    public int MinTransactions { get; set; }
    public double Percentile { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class TemporalThreshold
{
    public int OffHoursStart { get; set; }
    public int OffHoursEnd { get; set; }
    public double WeekendPenalty { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class SplitTransactionThreshold
{
    public int WindowMinutes { get; set; }
    public double ThresholdProximity { get; set; }
    public int MinSplits { get; set; }
    public List<int> CommonThresholds { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class SweetheartingThreshold
{
    public int MinOccurrences { get; set; }
    public double DiscountPercentile { get; set; }
    public int TimeWindowDays { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}

public sealed class ReturnFraudThreshold
{
    public double ReturnRatePercentile { get; set; }
    public double ReturnAmountRatio { get; set; }
    public int FrequentReturner { get; set; }
    public int WindowDays { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Detects { get; set; } = string.Empty;
}
