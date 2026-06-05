namespace PlanningAPI.DTO
{
    public class PoolRateDTO
    {
        public int Id { get; set; }
        public string? OrgID { get; set; }
        public string? AcctID { get; set; }
        public int? TemplateId { get; set; }
        public string PoolID { get; set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        public decimal ActualRate { get; set; }
        public decimal TargetRate { get; set; }

    }
    public class PoolInfo
    {
        public string PoolId { get; set; }
        public int TemplateId { get; set; }
        public string GroupName { get; set; }
        public int Sequence { get; set; }
    }
}
