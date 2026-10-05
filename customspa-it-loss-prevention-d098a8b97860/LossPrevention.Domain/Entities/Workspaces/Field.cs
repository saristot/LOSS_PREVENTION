namespace LossPrevention.Domain.Entities.Workspaces
{
    public sealed class Field
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public string DataType { get; set; }
        public bool GroupBy { get; set; }
        public string Aggregation { get; set; }
        public bool IsCalculated { get; set; }
        public string Expression { get; set; }
        public string Prefix { get; set; }
        public string Suffix { get; set; }
        public bool Visible { get; set; }
    }
}
