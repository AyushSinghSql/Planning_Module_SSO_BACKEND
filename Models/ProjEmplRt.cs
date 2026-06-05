using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PlanningAPI.Models;

[Table("proj_empl_rt")]
public class ProjEmplRt
{
    [Key]
    [Column("proj_empl_rt_key")]
    public int ProjEmplRtKey { get; set; }

    [Column("proj_id")]
    public string ProjId { get; set; }

    [Column("empl_id")]
    public string EmplId { get; set; }

    [Column("bill_lab_cat_cd")]
    public string BillLabCatCd { get; set; }

    [NotMapped]
    public string? plcDescription { get; set; }

    [Column("bill_rt_amt", TypeName = "numeric(19,4)")]
    public decimal? BillRtAmt { get; set; }

    [Column("s_bill_rt_type_cd")]
    [MaxLength(10)]
    public string? SBillRtTypeCd { get; set; }

    [Column("start_dt")]
    public DateTime? StartDt { get; set; }

    [Column("end_dt")]
    public DateTime? EndDt { get; set; }

    [Column("modified_by")]
    [MaxLength(50)]
    public string? ModifiedBy { get; set; } = string.Empty;

    [Column("time_stamp")]
    public DateTime? TimeStamp { get; set; } = DateTime.UtcNow;

    [Column("rowversion")]
    public int? RowVersion { get; set; } = 0;

    [Column("company_id")]
    [MaxLength(10)]
    public string? CompanyId { get; set; }

    [Column("type")]
    [MaxLength(50)]
    public string? Type { get; set; }

    [Column("bill_disc_rt", TypeName = "numeric(19,4)")]
    public decimal? BillDiscRt { get; set; }

    public PlcCode? plc { get; set; }
    [NotMapped]
    public string? EmplName { get; set; }

}
