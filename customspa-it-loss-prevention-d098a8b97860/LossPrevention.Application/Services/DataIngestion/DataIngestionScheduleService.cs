using LossPrevention.Application.Interfaces.DataIngestion;
using LossPrevention.Application.DTO.DataIngestion;
using LossPrevention.Domain.Entities.DataIngestion;
using LossPrevention.Infrastructure.Repositories;
using MongoDB.Driver;

namespace LossPrevention.Application.Services.DataIngestion;

public sealed class DataIngestionScheduleService : IDataIngestionScheduleService
{
    private readonly IMongoRepository<DataIngestionSchedule> _scheduleRepository;

    public DataIngestionScheduleService(IMongoRepository<DataIngestionSchedule> scheduleRepository)
    {
        _scheduleRepository = scheduleRepository;
    }

    public async Task<DataIngestionSchedule> CreateScheduleAsync(DataIngestionScheduleDTO dto)
    {
        var schedule = new DataIngestionSchedule
        {
            ConfigurationId = dto.ConfigurationId,
            ScheduleType = dto.ScheduleType,
            ScheduleDate = dto.ScheduleDate,
            ScheduleTime = dto.ScheduleTime,
            Recurrence = dto.Recurrence,
            SelectedDaysOfWeek = dto.SelectedDaysOfWeek,
            IsActive = dto.IsActive
        };

        await _scheduleRepository.InsertOneAsync(schedule);
        return schedule;
    }

    public async Task<DataIngestionSchedule?> GetScheduleByConfigurationIdAsync(string configurationId)
    {
        var filter = Builders<DataIngestionSchedule>.Filter.Eq(x => x.ConfigurationId, configurationId);
        var schedules = await _scheduleRepository.FindManyAsync(filter);
        return schedules.FirstOrDefault();
    }

    public async Task<DataIngestionSchedule> UpdateScheduleAsync(string id, DataIngestionScheduleDTO dto)
    {
        var filter = Builders<DataIngestionSchedule>.Filter.Eq(x => x.Id, id);
        var update = Builders<DataIngestionSchedule>.Update
            .Set(x => x.ScheduleType, dto.ScheduleType)
            .Set(x => x.ScheduleDate, dto.ScheduleDate)
            .Set(x => x.ScheduleTime, dto.ScheduleTime)
            .Set(x => x.Recurrence, dto.Recurrence)
            .Set(x => x.SelectedDaysOfWeek, dto.SelectedDaysOfWeek)
            .Set(x => x.IsActive, dto.IsActive)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);

        await _scheduleRepository.UpdateOneAsync(filter, update);
        
        var updatedSchedule = await _scheduleRepository.FindOneAsync(filter);
        return updatedSchedule!;
    }

    public async Task DeleteScheduleAsync(string id)
    {
        var filter = Builders<DataIngestionSchedule>.Filter.Eq(x => x.Id, id);
        await _scheduleRepository.DeleteOneAsync(filter);
    }

    public async Task<List<DataIngestionSchedule>> GetActiveSchedulesAsync()
    {
        var filter = Builders<DataIngestionSchedule>.Filter.Eq(x => x.IsActive, true);
        return await _scheduleRepository.FindManyAsync(filter);
    }
}