using MongoDB.Bson;

namespace LossPrevention.Application.Interfaces.Data
{
    public interface IReportDataservice
    {
        Task<(List<Dictionary<string, object>> Data, long TotalCount)> QueryReportDataAsync(BsonDocument[] pipeline, int skip, int take);
    }
}