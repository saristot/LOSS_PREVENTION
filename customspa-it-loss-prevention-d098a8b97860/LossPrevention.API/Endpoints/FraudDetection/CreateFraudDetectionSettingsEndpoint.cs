using FastEndpoints;
using LossPrevention.Application.DTO.FraudDetection;
using LossPrevention.Domain.Entities.FraudDetection;
using LossPrevention.Infrastructure.Repositories;
using MongoDB.Bson;

namespace LossPrevention.API.Endpoints.FraudDetection;

public sealed class CreateFraudDetectionSettingsEndpoint : Endpoint<CreateFraudDetectionSettingsRequest, FraudDetectionSettingsDTO>
{
    private readonly IMongoRepository<FraudDetectionSettings> _repo;

    public CreateFraudDetectionSettingsEndpoint(IMongoRepository<FraudDetectionSettings> repo)
    {
        _repo = repo;
    }

    public override void Configure()
    {
        Post("/api/fraud-detection/settings");
        Permissions("CAN_MANAGE_FRAUD_SETTINGS");
        Summary(s =>
        {
            s.Summary = "Create fraud detection settings";
            s.Description = "Creates a new fraud detection settings document with threshold configurations.";
        });
    }

    public override async Task HandleAsync(CreateFraudDetectionSettingsRequest req, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var currentUser = User?.Identity?.Name ?? "system";

        var settings = new FraudDetectionSettings
        {
            Id = ObjectId.GenerateNewId(),
            Thresholds = MapToEntity(req.Thresholds),
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = currentUser,
            UpdatedBy = currentUser
        };

        await _repo.InsertOneAsync(settings);

        var dto = MapToDTO(settings);
        await SendCreatedAtAsync<GetFraudDetectionSettingsEndpoint>(null, dto, cancellation: ct);
    }

    private static FraudThresholdConfig MapToEntity(FraudThresholdConfigDTO dto)
    {
        return new FraudThresholdConfig
        {
            HighValue = new HighValueThreshold
            {
                Percentile = dto.HighValue.Percentile,
                StdDevMultiplier = dto.HighValue.StdDevMultiplier,
                MinimumValue = dto.HighValue.MinimumValue,
                Description = dto.HighValue.Description,
                Detects = dto.HighValue.Detects
            },
            ExtremeHighValue = new ExtremeHighValueThreshold
            {
                Percentile = dto.ExtremeHighValue.Percentile,
                Multiplier = dto.ExtremeHighValue.Multiplier,
                MinimumValue = dto.ExtremeHighValue.MinimumValue,
                Description = dto.ExtremeHighValue.Description,
                Detects = dto.ExtremeHighValue.Detects
            },
            LowValue = new LowValueThreshold
            {
                Percentile = dto.LowValue.Percentile,
                MeanMultiplier = dto.LowValue.MeanMultiplier,
                MinMultiplier = dto.LowValue.MinMultiplier,
                Description = dto.LowValue.Description,
                Detects = dto.LowValue.Detects
            },
            RefundAmount = new RefundAmountThreshold
            {
                Percentile = dto.RefundAmount.Percentile,
                Description = dto.RefundAmount.Description,
                Detects = dto.RefundAmount.Detects
            },
            RefundCount = new RefundCountThreshold
            {
                Percentile = dto.RefundCount.Percentile,
                Description = dto.RefundCount.Description,
                Detects = dto.RefundCount.Detects
            },
            VoidAmount = new VoidAmountThreshold
            {
                Percentile = dto.VoidAmount.Percentile,
                Description = dto.VoidAmount.Description,
                Detects = dto.VoidAmount.Detects
            },
            VoidCount = new VoidCountThreshold
            {
                Percentile = dto.VoidCount.Percentile,
                Description = dto.VoidCount.Description,
                Detects = dto.VoidCount.Detects
            },
            DiscountAmount = new DiscountAmountThreshold
            {
                Percentile = dto.DiscountAmount.Percentile,
                MeanMultiplier = dto.DiscountAmount.MeanMultiplier,
                Description = dto.DiscountAmount.Description,
                Detects = dto.DiscountAmount.Detects
            },
            DiscountCount = new DiscountCountThreshold
            {
                Percentile = dto.DiscountCount.Percentile,
                Minimum = dto.DiscountCount.Minimum,
                Description = dto.DiscountCount.Description,
                Detects = dto.DiscountCount.Detects
            },
            ManualOverride = new ManualOverrideThreshold
            {
                Percentile = dto.ManualOverride.Percentile,
                Description = dto.ManualOverride.Description,
                Detects = dto.ManualOverride.Detects
            },
            PriceOverride = new PriceOverrideThreshold
            {
                Percentile = dto.PriceOverride.Percentile,
                Description = dto.PriceOverride.Description,
                Detects = dto.PriceOverride.Detects
            },
            ItemQuantity = new ItemQuantityThreshold
            {
                Percentile = dto.ItemQuantity.Percentile,
                Multiplier = dto.ItemQuantity.Multiplier,
                Description = dto.ItemQuantity.Description,
                Detects = dto.ItemQuantity.Detects
            },
            GiftCardAmount = new GiftCardAmountThreshold
            {
                Percentile = dto.GiftCardAmount.Percentile,
                Description = dto.GiftCardAmount.Description,
                Detects = dto.GiftCardAmount.Detects
            },
            GiftCardCount = new GiftCardCountThreshold
            {
                Percentile = dto.GiftCardCount.Percentile,
                Minimum = dto.GiftCardCount.Minimum,
                Description = dto.GiftCardCount.Description,
                Detects = dto.GiftCardCount.Detects
            },
            PaymentMethods = new PaymentMethodsThreshold
            {
                Percentile = dto.PaymentMethods.Percentile,
                Minimum = dto.PaymentMethods.Minimum,
                Description = dto.PaymentMethods.Description,
                Detects = dto.PaymentMethods.Detects
            },
            LoyaltyPoints = new LoyaltyPointsThreshold
            {
                Percentile = dto.LoyaltyPoints.Percentile,
                Description = dto.LoyaltyPoints.Description,
                Detects = dto.LoyaltyPoints.Detects
            },
            NoSaleEvents = new NoSaleEventsThreshold
            {
                Percentile = dto.NoSaleEvents.Percentile,
                Minimum = dto.NoSaleEvents.Minimum,
                Description = dto.NoSaleEvents.Description,
                Detects = dto.NoSaleEvents.Detects
            },
            Velocity = new VelocityThreshold
            {
                WindowMinutes = dto.Velocity.WindowMinutes,
                MinTransactions = dto.Velocity.MinTransactions,
                Percentile = dto.Velocity.Percentile,
                Description = dto.Velocity.Description,
                Detects = dto.Velocity.Detects
            },
            Temporal = new TemporalThreshold
            {
                OffHoursStart = dto.Temporal.OffHoursStart,
                OffHoursEnd = dto.Temporal.OffHoursEnd,
                WeekendPenalty = dto.Temporal.WeekendPenalty,
                Description = dto.Temporal.Description,
                Detects = dto.Temporal.Detects
            },
            SplitTransaction = new SplitTransactionThreshold
            {
                WindowMinutes = dto.SplitTransaction.WindowMinutes,
                ThresholdProximity = dto.SplitTransaction.ThresholdProximity,
                MinSplits = dto.SplitTransaction.MinSplits,
                CommonThresholds = dto.SplitTransaction.CommonThresholds,
                Description = dto.SplitTransaction.Description,
                Detects = dto.SplitTransaction.Detects
            },
            Sweethearting = new SweetheartingThreshold
            {
                MinOccurrences = dto.Sweethearting.MinOccurrences,
                DiscountPercentile = dto.Sweethearting.DiscountPercentile,
                TimeWindowDays = dto.Sweethearting.TimeWindowDays,
                Description = dto.Sweethearting.Description,
                Detects = dto.Sweethearting.Detects
            },
            ReturnFraud = new ReturnFraudThreshold
            {
                ReturnRatePercentile = dto.ReturnFraud.ReturnRatePercentile,
                ReturnAmountRatio = dto.ReturnFraud.ReturnAmountRatio,
                FrequentReturner = dto.ReturnFraud.FrequentReturner,
                WindowDays = dto.ReturnFraud.WindowDays,
                Description = dto.ReturnFraud.Description,
                Detects = dto.ReturnFraud.Detects
            }
        };
    }

    private static FraudDetectionSettingsDTO MapToDTO(FraudDetectionSettings settings)
    {
        return new FraudDetectionSettingsDTO
        {
            Id = settings.Id.ToString(),
            Thresholds = MapThresholdsToDTO(settings.Thresholds),
            CreatedAt = settings.CreatedAt,
            UpdatedAt = settings.UpdatedAt,
            CreatedBy = settings.CreatedBy,
            UpdatedBy = settings.UpdatedBy
        };
    }

    private static FraudThresholdConfigDTO MapThresholdsToDTO(FraudThresholdConfig thresholds)
    {
        return new FraudThresholdConfigDTO
        {
            HighValue = new HighValueThresholdDTO
            {
                Percentile = thresholds.HighValue.Percentile,
                StdDevMultiplier = thresholds.HighValue.StdDevMultiplier,
                MinimumValue = thresholds.HighValue.MinimumValue,
                Description = thresholds.HighValue.Description,
                Detects = thresholds.HighValue.Detects
            },
            ExtremeHighValue = new ExtremeHighValueThresholdDTO
            {
                Percentile = thresholds.ExtremeHighValue.Percentile,
                Multiplier = thresholds.ExtremeHighValue.Multiplier,
                MinimumValue = thresholds.ExtremeHighValue.MinimumValue,
                Description = thresholds.ExtremeHighValue.Description,
                Detects = thresholds.ExtremeHighValue.Detects
            },
            LowValue = new LowValueThresholdDTO
            {
                Percentile = thresholds.LowValue.Percentile,
                MeanMultiplier = thresholds.LowValue.MeanMultiplier,
                MinMultiplier = thresholds.LowValue.MinMultiplier,
                Description = thresholds.LowValue.Description,
                Detects = thresholds.LowValue.Detects
            },
            RefundAmount = new RefundAmountThresholdDTO
            {
                Percentile = thresholds.RefundAmount.Percentile,
                Description = thresholds.RefundAmount.Description,
                Detects = thresholds.RefundAmount.Detects
            },
            RefundCount = new RefundCountThresholdDTO
            {
                Percentile = thresholds.RefundCount.Percentile,
                Description = thresholds.RefundCount.Description,
                Detects = thresholds.RefundCount.Detects
            },
            VoidAmount = new VoidAmountThresholdDTO
            {
                Percentile = thresholds.VoidAmount.Percentile,
                Description = thresholds.VoidAmount.Description,
                Detects = thresholds.VoidAmount.Detects
            },
            VoidCount = new VoidCountThresholdDTO
            {
                Percentile = thresholds.VoidCount.Percentile,
                Description = thresholds.VoidCount.Description,
                Detects = thresholds.VoidCount.Detects
            },
            DiscountAmount = new DiscountAmountThresholdDTO
            {
                Percentile = thresholds.DiscountAmount.Percentile,
                MeanMultiplier = thresholds.DiscountAmount.MeanMultiplier,
                Description = thresholds.DiscountAmount.Description,
                Detects = thresholds.DiscountAmount.Detects
            },
            DiscountCount = new DiscountCountThresholdDTO
            {
                Percentile = thresholds.DiscountCount.Percentile,
                Minimum = thresholds.DiscountCount.Minimum,
                Description = thresholds.DiscountCount.Description,
                Detects = thresholds.DiscountCount.Detects
            },
            ManualOverride = new ManualOverrideThresholdDTO
            {
                Percentile = thresholds.ManualOverride.Percentile,
                Description = thresholds.ManualOverride.Description,
                Detects = thresholds.ManualOverride.Detects
            },
            PriceOverride = new PriceOverrideThresholdDTO
            {
                Percentile = thresholds.PriceOverride.Percentile,
                Description = thresholds.PriceOverride.Description,
                Detects = thresholds.PriceOverride.Detects
            },
            ItemQuantity = new ItemQuantityThresholdDTO
            {
                Percentile = thresholds.ItemQuantity.Percentile,
                Multiplier = thresholds.ItemQuantity.Multiplier,
                Description = thresholds.ItemQuantity.Description,
                Detects = thresholds.ItemQuantity.Detects
            },
            GiftCardAmount = new GiftCardAmountThresholdDTO
            {
                Percentile = thresholds.GiftCardAmount.Percentile,
                Description = thresholds.GiftCardAmount.Description,
                Detects = thresholds.GiftCardAmount.Detects
            },
            GiftCardCount = new GiftCardCountThresholdDTO
            {
                Percentile = thresholds.GiftCardCount.Percentile,
                Minimum = thresholds.GiftCardCount.Minimum,
                Description = thresholds.GiftCardCount.Description,
                Detects = thresholds.GiftCardCount.Detects
            },
            PaymentMethods = new PaymentMethodsThresholdDTO
            {
                Percentile = thresholds.PaymentMethods.Percentile,
                Minimum = thresholds.PaymentMethods.Minimum,
                Description = thresholds.PaymentMethods.Description,
                Detects = thresholds.PaymentMethods.Detects
            },
            LoyaltyPoints = new LoyaltyPointsThresholdDTO
            {
                Percentile = thresholds.LoyaltyPoints.Percentile,
                Description = thresholds.LoyaltyPoints.Description,
                Detects = thresholds.LoyaltyPoints.Detects
            },
            NoSaleEvents = new NoSaleEventsThresholdDTO
            {
                Percentile = thresholds.NoSaleEvents.Percentile,
                Minimum = thresholds.NoSaleEvents.Minimum,
                Description = thresholds.NoSaleEvents.Description,
                Detects = thresholds.NoSaleEvents.Detects
            },
            Velocity = new VelocityThresholdDTO
            {
                WindowMinutes = thresholds.Velocity.WindowMinutes,
                MinTransactions = thresholds.Velocity.MinTransactions,
                Percentile = thresholds.Velocity.Percentile,
                Description = thresholds.Velocity.Description,
                Detects = thresholds.Velocity.Detects
            },
            Temporal = new TemporalThresholdDTO
            {
                OffHoursStart = thresholds.Temporal.OffHoursStart,
                OffHoursEnd = thresholds.Temporal.OffHoursEnd,
                WeekendPenalty = thresholds.Temporal.WeekendPenalty,
                Description = thresholds.Temporal.Description,
                Detects = thresholds.Temporal.Detects
            },
            SplitTransaction = new SplitTransactionThresholdDTO
            {
                WindowMinutes = thresholds.SplitTransaction.WindowMinutes,
                ThresholdProximity = thresholds.SplitTransaction.ThresholdProximity,
                MinSplits = thresholds.SplitTransaction.MinSplits,
                CommonThresholds = thresholds.SplitTransaction.CommonThresholds,
                Description = thresholds.SplitTransaction.Description,
                Detects = thresholds.SplitTransaction.Detects
            },
            Sweethearting = new SweetheartingThresholdDTO
            {
                MinOccurrences = thresholds.Sweethearting.MinOccurrences,
                DiscountPercentile = thresholds.Sweethearting.DiscountPercentile,
                TimeWindowDays = thresholds.Sweethearting.TimeWindowDays,
                Description = thresholds.Sweethearting.Description,
                Detects = thresholds.Sweethearting.Detects
            },
            ReturnFraud = new ReturnFraudThresholdDTO
            {
                ReturnRatePercentile = thresholds.ReturnFraud.ReturnRatePercentile,
                ReturnAmountRatio = thresholds.ReturnFraud.ReturnAmountRatio,
                FrequentReturner = thresholds.ReturnFraud.FrequentReturner,
                WindowDays = thresholds.ReturnFraud.WindowDays,
                Description = thresholds.ReturnFraud.Description,
                Detects = thresholds.ReturnFraud.Detects
            }
        };
    }
}
