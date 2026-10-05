using MongoDB.Bson;

namespace Handlers.Requests.Data
{
    public class GetReportDataRequest
    {
        public List<object> QueryPipeline { get; set; } = new();
        public int Take { get; set; }
        public int Skip { get; set; }
    }

}