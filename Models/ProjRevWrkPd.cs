using System.ComponentModel.DataAnnotations.Schema;

namespace PlanningAPI.Models
{
    public class ProjRevWrkPd
    {
        public int Id { get; set; }
        public int Pl_Id { get; set; }

        public string ProjId { get; set; }

        public int? VersionNo { get; set; }
        [NotMapped]
        public int? Fy_Cd { get; set; }
        public int? Period { get; set; }

        public DateOnly? EndDate { get; set; }

        public int RowVersion { get; set; } = 0;

        public string ModifiedBy { get; set; } = "";

        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

        public string? CreatedBy { get; set; } = "";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public decimal? RevAmt { get; set; } = 0;

        public decimal? RevAdj { get; set; } = 0;
        public decimal? RevAdj1 { get; set; } = 0;
        
        public string? RevDesc { get; set; }
        public string? BgtType { get; set; }

        public decimal ActualFeeRateOnCost { get; set; } = 0m;
        public decimal TargetFeeRateOnCost { get; set; } = 0m;

        public decimal ActualFeeAmountOnCost { get; set; } = 0m;
        public decimal TargetFeeAmountOnCost { get; set; } = 0m;


    }

    public class NB_Revenue
    {
        public int? Fy_Cd { get; set; }
        public int? Period { get; set; }
        public decimal? RevAmt { get; set; } = 0;

    }

}
