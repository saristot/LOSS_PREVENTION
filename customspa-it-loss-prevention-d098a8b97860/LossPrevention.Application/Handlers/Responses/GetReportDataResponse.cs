namespace LossPrevention.API.Handlers.Responses
{
    public sealed class GetReportDataResponse
    {
        public List<Dictionary<string, object>> Data { get; set; } = new();
        public long Total { get; set; }
    }
}
