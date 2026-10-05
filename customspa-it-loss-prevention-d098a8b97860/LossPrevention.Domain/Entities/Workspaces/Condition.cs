namespace LossPrevention.Domain.Entities.Workspaces
{
    public sealed class Condition
    {
        public string Field { get; set; }
        public string Operator { get; set; }
        public object Value { get; set; }
    }
}
