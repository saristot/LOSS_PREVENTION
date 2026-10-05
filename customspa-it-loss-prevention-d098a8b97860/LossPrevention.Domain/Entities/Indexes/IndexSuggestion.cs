namespace LossPrevention.Domain.Entities.Indexes
{
    public sealed class IndexSuggestion
    {
        public required string FieldName { get; set; }
        public required string Reason { get; set; }
    }
}
