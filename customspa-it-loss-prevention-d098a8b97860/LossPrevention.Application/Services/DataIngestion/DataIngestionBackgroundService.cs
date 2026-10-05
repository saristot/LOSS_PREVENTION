using LossPrevention.Application.Interfaces.DataIngestion;
using LossPrevention.Application.Services.DataIngestion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LossPrevention.Infrastructure.Services
{
    public class DataIngestionBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DataIngestionBackgroundService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

        public DataIngestionBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<DataIngestionBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Data Ingestion Background Service is starting");

            // Wait a bit on startup to let the app initialize
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndRunScheduledIngestionAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Data Ingestion Background Service");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Data Ingestion Background Service is stopping");
        }

        private async Task CheckAndRunScheduledIngestionAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var configService = scope.ServiceProvider.GetRequiredService<IDataIngestionService>();
            var coordinator = scope.ServiceProvider.GetRequiredService<IFileProcessingCoordinator>();

            var config = await configService.GetConfigurationAsync();

            if (config == null || config.ScheduleType == "one-time")
            {
                // No recurring schedule set
                return;
            }

            if (!ShouldRunNow(config))
            {
                return;
            }

            _logger.LogInformation("Scheduled ingestion triggered");

            try
            {
                var result = await coordinator.RunIngestionAsync(cancellationToken);

                if (result.Success)
                {
                    _logger.LogInformation(
                        "Scheduled ingestion completed: {Files} files, {Records} records",
                        result.FilesProcessed, result.RecordsInserted);
                }
                else
                {
                    _logger.LogWarning(
                        "Scheduled ingestion completed with errors: {Errors}",
                        string.Join(", ", result.Errors));
                }

                // Update last run time
                config.LastRunAt = DateTime.UtcNow;
                // Note: You might want to save this back to the database
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Scheduled ingestion failed");
            }
        }

        private bool ShouldRunNow(LossPrevention.Domain.Entities.DataIngestion.DataIngestionConfiguration config)
        {
            var now = DateTime.Now;

            // Check if we've run recently (within the last 50 seconds to avoid double-runs)
            if (config.LastRunAt.HasValue &&
                (now - config.LastRunAt.Value).TotalSeconds < 50)
            {
                return false;
            }

            // Parse schedule time
            if (string.IsNullOrWhiteSpace(config.ScheduleTime))
            {
                return false;
            }

            if (!TimeSpan.TryParse(config.ScheduleTime, out var scheduledTime))
            {
                return false;
            }

            var scheduledDateTime = now.Date.Add(scheduledTime);
            var timeDifference = Math.Abs((now - scheduledDateTime).TotalMinutes);

            // If we're within 1 minute of the scheduled time
            if (timeDifference > 1)
            {
                return false;
            }

            // Check recurrence pattern
            switch (config.Recurrence?.ToLowerInvariant())
            {
                case "daily":
                    return true;

                case "weekly":
                    if (config.SelectedDaysOfWeek == null || !config.SelectedDaysOfWeek.Any())
                    {
                        return false;
                    }
                    var currentDay = now.DayOfWeek.ToString();
                    return config.SelectedDaysOfWeek.Contains(currentDay, StringComparer.OrdinalIgnoreCase);

                case "monthly":
                    // Run on the first day of the month
                    return now.Day == 1;

                default:
                    return false;
            }
        }
    }
}