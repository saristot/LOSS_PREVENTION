using LossPrevention.Application.Interfaces.DataIngestion;
using LossPrevention.Application.DTO.DataIngestion;
using LossPrevention.Domain.Entities.DataIngestion;
using LossPrevention.Infrastructure.Repositories;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LossPrevention.Application.Services.DataIngestion;

public sealed class DataIngestionService : IDataIngestionService
{
    private readonly IMongoRepository<DataIngestionConfiguration> _configurationRepository;
    private const string DEFAULT_CONFIG_ID = "default";

    public DataIngestionService(IMongoRepository<DataIngestionConfiguration> configurationRepository)
    {
        _configurationRepository = configurationRepository;
    }

    public async Task<DataIngestionConfiguration?> GetConfigurationAsync()
    {
        // For simplicity, we'll use a single default configuration
        var filter = Builders<DataIngestionConfiguration>.Filter.Empty;
        var configurations = await _configurationRepository.FindManyAsync(filter);
        return configurations.FirstOrDefault();
    }

    public async Task<DataIngestionConfiguration> SaveConfigurationAsync(DataIngestionConfigurationDTO dto)
    {
        var existing = await GetConfigurationAsync();

        if (existing != null)
        {
            // Update existing configuration
            existing.SelectedSources = dto.SelectedSources;
            existing.SelectedFileType = dto.SelectedFileType;
            existing.SftpHost = dto.SftpHost;
            existing.SftpPort = dto.SftpPort;
            existing.SftpUsername = dto.SftpUsername;
            existing.SftpPassword = dto.SftpPassword;
            existing.SftpRemoteDirectory = dto.SftpRemoteDirectory;
            existing.FileSystemPath = dto.FileSystemPath;
            existing.ManualLoad = dto.ManualLoad; // NEW: Include ManualLoad
            existing.ScheduleType = dto.ScheduleType;
            existing.ScheduleDate = dto.ScheduleDate;
            existing.ScheduleTime = dto.ScheduleTime;
            existing.Recurrence = dto.Recurrence;
            existing.SelectedDaysOfWeek = dto.SelectedDaysOfWeek;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UseMappings = dto.UseMappings;

            var filter = Builders<DataIngestionConfiguration>.Filter.Eq(x => x.Id, existing.Id);
            var update = Builders<DataIngestionConfiguration>.Update
                .Set(x => x.SelectedSources, existing.SelectedSources)
                .Set(x => x.SelectedFileType, existing.SelectedFileType)
                .Set(x => x.SftpHost, existing.SftpHost)
                .Set(x => x.SftpPort, existing.SftpPort)
                .Set(x => x.SftpUsername, existing.SftpUsername)
                .Set(x => x.SftpPassword, existing.SftpPassword)
                .Set(x => x.SftpRemoteDirectory, existing.SftpRemoteDirectory)
                .Set(x => x.FileSystemPath, existing.FileSystemPath)
                .Set(x => x.ManualLoad, existing.ManualLoad) // NEW: Include ManualLoad
                .Set(x => x.ScheduleType, existing.ScheduleType)
                .Set(x => x.ScheduleDate, existing.ScheduleDate)
                .Set(x => x.ScheduleTime, existing.ScheduleTime)
                .Set(x => x.Recurrence, existing.Recurrence)
                .Set(x => x.SelectedDaysOfWeek, existing.SelectedDaysOfWeek)
                .Set(x => x.UpdatedAt, existing.UpdatedAt)
                .Set(x => x.UseMappings, existing.UseMappings);

            await _configurationRepository.UpdateOneAsync(filter, update);
            return existing;
        }
        else
        {
            // Create new configuration
            var newConfig = new DataIngestionConfiguration
            {
                SelectedSources = dto.SelectedSources,
                SelectedFileType = dto.SelectedFileType,
                SftpHost = dto.SftpHost,
                SftpPort = dto.SftpPort,
                SftpUsername = dto.SftpUsername,
                SftpPassword = dto.SftpPassword,
                SftpRemoteDirectory = dto.SftpRemoteDirectory,
                FileSystemPath = dto.FileSystemPath,
                ManualLoad = dto.ManualLoad, // NEW: Include ManualLoad
                ScheduleType = dto.ScheduleType,
                ScheduleDate = dto.ScheduleDate,
                ScheduleTime = dto.ScheduleTime,
                Recurrence = dto.Recurrence,
                SelectedDaysOfWeek = dto.SelectedDaysOfWeek,
                UseMappings = dto.UseMappings
            };

            await _configurationRepository.InsertOneAsync(newConfig);
            return newConfig;
        }
    }

    public async Task<DataIngestionConfiguration> UpdateSourcesAsync(List<string> selectedSources)
    {
        var existing = await GetConfigurationAsync();

        if (existing != null)
        {
            existing.SelectedSources = selectedSources;
            existing.UpdatedAt = DateTime.UtcNow;

            var filter = Builders<DataIngestionConfiguration>.Filter.Eq(x => x.Id, existing.Id);
            var update = Builders<DataIngestionConfiguration>.Update
                .Set(x => x.SelectedSources, existing.SelectedSources)
                .Set(x => x.UpdatedAt, existing.UpdatedAt);

            await _configurationRepository.UpdateOneAsync(filter, update);
            return existing;
        }
        else
        {
            var newConfig = new DataIngestionConfiguration
            {
                SelectedSources = selectedSources
            };
            await _configurationRepository.InsertOneAsync(newConfig);
            return newConfig;
        }
    }

    public async Task<DataIngestionConfiguration> UpdateScheduleAsync(UpdateScheduleRequest request)
    {
        var existing = await GetConfigurationAsync();

        if (existing != null)
        {
            existing.ScheduleDate = request.ScheduleDate;
            existing.ScheduleTime = request.ScheduleTime;
            existing.Recurrence = request.Recurrence;
            existing.ScheduleType = request.ScheduleType;
            existing.SelectedDaysOfWeek = request.SelectedDaysOfWeek;
            existing.UpdatedAt = DateTime.UtcNow;

            var filter = Builders<DataIngestionConfiguration>.Filter.Eq(x => x.Id, existing.Id);
            var update = Builders<DataIngestionConfiguration>.Update
                .Set(x => x.ScheduleDate, existing.ScheduleDate)
                .Set(x => x.ScheduleTime, existing.ScheduleTime)
                .Set(x => x.Recurrence, existing.Recurrence)
                .Set(x => x.ScheduleType, existing.ScheduleType)
                .Set(x => x.SelectedDaysOfWeek, existing.SelectedDaysOfWeek)
                .Set(x => x.UpdatedAt, existing.UpdatedAt);

            await _configurationRepository.UpdateOneAsync(filter, update);
            return existing;
        }
        else
        {
            var newConfig = new DataIngestionConfiguration
            {
                ScheduleDate = request.ScheduleDate,
                ScheduleTime = request.ScheduleTime,
                Recurrence = request.Recurrence,
                ScheduleType = request.ScheduleType,
                SelectedDaysOfWeek = request.SelectedDaysOfWeek
            };
            await _configurationRepository.InsertOneAsync(newConfig);
            return newConfig;
        }
    }

    public async Task UpdateRecurrenceOptionsAsync(List<RecurrenceOptionDTO> recurrenceOptions)
    {
        // For now, this is just a placeholder as recurrence options might be stored differently
        // or might not need to be persisted at all since they're static options
        await Task.CompletedTask;
    }

    public async Task ClearConfigurationAsync()
    {
        var filter = Builders<DataIngestionConfiguration>.Filter.Empty;
        await _configurationRepository.DeleteManyAsync(filter);
    }

    public async Task ClearScheduleAsync()
    {
        var existing = await GetConfigurationAsync();

        if (existing != null)
        {
            existing.ScheduleType = "one-time";
            existing.ScheduleDate = null;
            existing.ScheduleTime = null;
            existing.Recurrence = "daily";
            existing.SelectedDaysOfWeek = new List<string>();
            existing.UpdatedAt = DateTime.UtcNow;

            var filter = Builders<DataIngestionConfiguration>.Filter.Eq(x => x.Id, existing.Id);
            var update = Builders<DataIngestionConfiguration>.Update
                .Set(x => x.ScheduleType, existing.ScheduleType)
                .Set(x => x.ScheduleDate, existing.ScheduleDate)
                .Set(x => x.ScheduleTime, existing.ScheduleTime)
                .Set(x => x.Recurrence, existing.Recurrence)
                .Set(x => x.SelectedDaysOfWeek, existing.SelectedDaysOfWeek)
                .Set(x => x.UpdatedAt, existing.UpdatedAt);

            await _configurationRepository.UpdateOneAsync(filter, update);
        }
    }
}