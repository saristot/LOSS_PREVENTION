using LossPrevention.Application.DTO.Mappings;
using LossPrevention.Application.Entities.Workspaces;
using Newtonsoft.Json;

namespace LossPrevention.Application.DTO.Workspaces
{
    public sealed class TabDTO
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public List<FieldDTO> SelectedFields { get; set; }

        public string GroupByField { get; set; }

        public QueryDTO Query { get; set; }

        public bool DesignMode { get; set; }
    }

}
