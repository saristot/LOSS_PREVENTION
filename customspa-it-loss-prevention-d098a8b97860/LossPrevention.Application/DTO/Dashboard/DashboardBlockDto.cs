namespace LossPrevention.Application.Dashboards.DTO
{
    public sealed class DashboardBlockDto
    {
        public string i { get; set; } = "";
        public int x { get; set; }
        public int y { get; set; }
        public int w { get; set; }
        public int h { get; set; }
        public string type { get; set; } = "text";
        public Dictionary<string, object>? data { get; set; }
    }
}
