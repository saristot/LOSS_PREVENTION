using LossPrevention.Application.DTO.Data;

namespace LossPrevention.Application.Services.Data
{
    public interface IDistanceDataservice
    {
        Task<List<DistanceResultDto>> GetDistanceAsync(
             string sourceDocumentId,
             string startDateField,
             string endDateField,
             DateTime startDate,
             DateTime endDate,
             string keyField,
             List<string> comparisonFields);
    }
}