using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanningAPI.Models
{
    public class PoolRatesCostpoint
    {
        public int? FyCd { get; set; }
        public int? PdNo { get; set; }
        public int? AllocGrpNo { get; set; }
        public int? PoolNo { get; set; }
        public int? ProcSeqNo { get; set; }
        public string? SAcctTypeCd { get; set; }
        public string? AcctId { get; set; }
        public string? OrgId { get; set; }
        public decimal? CurAmt { get; set; }
        public decimal? YtdAmt { get; set; }
        public decimal? CurAllocAmt { get; set; }
        public decimal? YtdAllocAmt { get; set; }
        public decimal? CurRt { get; set; }
        public decimal? YtdRt { get; set; }
        public decimal? CurBudAmt { get; set; }
        public decimal? YtdBudAmt { get; set; }
        public decimal? CurBudAllocAmt { get; set; }
        public decimal? YtdBudAllocAmt { get; set; }
        public decimal? CurBudRt { get; set; }
        public decimal? YtdBudRt { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? TimeStamp { get; set; }
        public decimal? CurBaseAmt { get; set; }
        public decimal? YtdBaseAmt { get; set; }
        public string? ProjId { get; set; }
        public int? Rowversion { get; set; }
        public string? CompanyId { get; set; }
    }



    [Table("analgs_rt")]
    public class AnalgsRt
    {
        [Key]
        [Column("analg_id")]
        public int AnalgId { get; set; }

        [Column("tot_rev")]
        public decimal? TotRev { get; set; }

        [Column("lab_onste")]
        public decimal? LabOnste { get; set; }

        [Column("lab_onste_non_bill")]
        public decimal? LabOnsteNonBill { get; set; }

        [Column("non_lab_trvl")]
        public decimal? NonLabTrvl { get; set; }

        [Column("sub_lab")]
        public decimal? SubLab { get; set; }

        [Column("sub_non_lab")]
        public decimal? SubNonLab { get; set; }

        [Required]
        [Column("cls_pd")]
        public DateOnly ClsPd { get; set; }

        [Column("ovrwrte_rt")]
        public Boolean? OvrwrteRt { get; set; }

        [Column("fy_cd")]
        public int? FyCd { get; set; }

        [Column("actual_amt")]
        public bool ActualAmt { get; set; } = false;

        [Column("modified_by")]
        [MaxLength(100)]
        public string? ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }
    }


}
