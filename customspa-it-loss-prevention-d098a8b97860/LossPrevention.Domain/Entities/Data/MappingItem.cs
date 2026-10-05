using MongoDB.Bson;

namespace LossPrevention.Domain.Entities.Data
{
    public sealed class MappingItem
    {
        public ObjectId _id { get; set; }
        public required string Name { get; set; }
        public required string Alias { get; set; }
        public string DataType { get; set; } = string.Empty;
        public string Format { get; set; } = string.Empty;
        public bool IsVisible { get; set; }
        public bool IsCalculated { get; set; }
        public bool IsLookup { get; set; }
        public bool IsArray { get; set; }
        public int LongestLength { get; set; } = 0;
        public bool SetDefaultValue { get; set; } = false;
        public string DefaultValue { get; set; } = string.Empty;
        public string CollectionName { get; set; } = string.Empty;
       // public string Path { get; set; } = string.Empty;
    }
}
