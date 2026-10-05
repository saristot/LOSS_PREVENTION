using MongoDB.Bson;

namespace LossPrevention.Application.DTO.Data
{
    public sealed class DistanceResultDto
    {
        public string Id { get; set; }
        public string Score { get; set; }
        public string FieldMatch { get; set; }
        public string DistanceMatch { get; set; }
        public string FieldsUsed { get; set; }
        public KeyField KeyField { get; set; } 
        public Dictionary<string, object> ComparedFields { get; set; }
    }

}
