namespace PlanningAPI.Models
{
    public class PowerBISettings
    {
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }

    public class EmbedConfig
    {
        public string ReportId { get; set; }
        public string EmbedUrl { get; set; }
        public string EmbedToken { get; set; }
    }
}
