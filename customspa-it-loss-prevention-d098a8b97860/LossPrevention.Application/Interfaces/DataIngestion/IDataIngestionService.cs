using LossPrevention.Application.DTO.DataIngestion;
using LossPrevention.Domain.Entities.DataIngestion;

namespace LossPrevention.Application.Interfaces.DataIngestion;

public interface IDataIngestionService
{
    Task<DataIngestionConfiguration?> GetConfigurationAsync();
    Task<DataIngestionConfiguration> SaveConfigurationAsync(DataIngestionConfigurationDTO dto);
    Task<DataIngestionConfiguration> UpdateSourcesAsync(List<string> selectedSources);
    Task<DataIngestionConfiguration> UpdateScheduleAsync(UpdateScheduleRequest request);
    Task UpdateRecurrenceOptionsAsync(List<RecurrenceOptionDTO> recurrenceOptions);
    Task ClearConfigurationAsync();
    Task ClearScheduleAsync();
}

public interface IDataIngestionScheduleService
{
    Task<DataIngestionSchedule> CreateScheduleAsync(DataIngestionScheduleDTO dto);
    Task<DataIngestionSchedule?> GetScheduleByConfigurationIdAsync(string configurationId);
    Task<DataIngestionSchedule> UpdateScheduleAsync(string id, DataIngestionScheduleDTO dto);
    Task DeleteScheduleAsync(string id);
    Task<List<DataIngestionSchedule>> GetActiveSchedulesAsync();
}