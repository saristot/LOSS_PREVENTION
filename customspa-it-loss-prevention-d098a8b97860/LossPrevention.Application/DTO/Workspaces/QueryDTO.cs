namespace LossPrevention.Application.DTO.Workspaces
{
    public class QueryDTO
    {
        public string Type { get; set; }
        public List<ConditionDTO> Conditions { get; set; }
        public string Id { get; set; }
    }

}
