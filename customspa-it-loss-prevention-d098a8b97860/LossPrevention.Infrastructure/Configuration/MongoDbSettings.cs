namespace LossPrevention.Infrastructure.Models
{
    public sealed class MongoDbSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string CollectionName_Users { get; set; } = string.Empty;
        public string CollectionName_Roles { get; set; } = string.Empty;
        public string CollectionName_Permissions { get; set; } = string.Empty;
        public string CollectionName_ReportData { get; set; } = string.Empty;
        public string CollectionName_RulesConfiguration { get; set; } = string.Empty;
        public string CollectionName_MappingConfiguration { get; set; } = string.Empty;
        public string CollectionName_Workspaces { get; set; } = string.Empty;
        public string CollectionName_Dashboards { get; set; } = string.Empty;
        public string CollectionName_DataIngestionConfigurations { get; set; } = string.Empty;
        public string CollectionName_DataIngestionSchedules { get; set; } = string.Empty;
        public string CollectionName_Notifications { get; set; } = string.Empty;
        public string CollectionName_Groups { get; set; } = string.Empty;
        public string CollectionName_FraudDetectionSettings { get; set; } = string.Empty;
    }
}
