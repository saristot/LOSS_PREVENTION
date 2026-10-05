
namespace LossPrevention.Application.DTO.Workspaces
{
    public sealed class WorkspaceDTO
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public List<TabDTO> Tabs { get; set; }
    }

}
