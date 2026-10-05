namespace LossPrevention.API.Handlers.Requests.Data
{
    public sealed class DistanceRequest
    {
        public string Id {get; set; }
        public string StartDateField { get; set; }
        public string EndDateField { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string KeyField { get; set; }
        public List<string> Fields { get; set; } = new();
    }

}
