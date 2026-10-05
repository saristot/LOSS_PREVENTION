using FastEndpoints;
using LossPrevention.Application.DTO.FraudDetection;
using LossPrevention.Domain.Entities.FraudDetection;
using LossPrevention.Infrastructure.Repositories;
using MongoDB.Driver;

namespace LossPrevention.API.Endpoints.FraudDetection;

public sealed class GetFraudDetectionSettingsEndpoint : EndpointWithoutRequest<FraudDetectionSettingsDTO>
{
    private readonly IMongoRepository<FraudDetectionSettings> _repo;

    public GetFraudDetectionSettingsEndpoint(IMongoRepository<FraudDetectionSettings> repo)
    {
        _repo = repo;
    }

    public override void Configure()
    {
        Get("/api/fraud-detection/settings");
        Permissions("CAN_VIEW_FRAUD_SETTINGS");
        Summary(s => s.Summary = "Get fraud detection threshold configuration");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        // Get the most recent settings document (singleton pattern)
        var filter = Builders<FraudDetectionSettings>.Filter.Empty;
        var sort = Builders<FraudDetectionSettings>.Sort.Descending(x => x.UpdatedAt);
        
        var settings = await _repo.Collection
            .Find(filter)
            .Sort(sort)
            .Limit(1)
            .FirstOrDefaultAsync(ct);

        if (settings == null)
        {
            await SendOkAsync(ct);
            return;
        }

        var dto = MapToDTO(settings);
        await SendOkAsync(dto, ct);
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
