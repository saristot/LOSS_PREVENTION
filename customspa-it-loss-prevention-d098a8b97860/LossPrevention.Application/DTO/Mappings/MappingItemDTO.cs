namespace LossPrevention.Application.DTO.Mappings
{
    public sealed class MappingItemDTO
    {
        public string Id { get; set; }
        public string Name { get; set; } = default!;
        public string Alias { get; set; } = default!;
        public string DataType { get; set; } = "String";
        public bool IsVisible { get; set; } = true;
        public bool IsArray { get; set; }
        public bool IsCalculated { get; set; }
        public bool IsLookup { get; set; }
        public string CollectionName { get; set; } = default!;
        public required int LongestLength { get; set; }
    }

}
