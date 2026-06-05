using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

[Table("proj_employee_labcat", Schema = "public")]
public class ProjEmployeeLabcat
{
    [Key, Column("proj_id", Order = 0)]
    [Required]
    [MaxLength(50)]
    public string ProjId { get; set; } = null!;

    [Key, Column("empl_id", Order = 1)]
    [Required]
    [MaxLength(50)]
    public string EmplId { get; set; } = null!;

    [Key, Column("bill_lab_cat_cd", Order = 2)]   // 👈 explicit mapping
    [Required]
    [MaxLength(50)]
    public string BillLabCatCd { get; set; } = null!;

    [Column("dflt_fl")]
    public string DfltFl { get; set; } = "N";

    [Column("company_id")]
    [MaxLength(50)]
    public string? CompanyId { get; set; }

    [Column("start_dt")]
    public DateTime? StartDt { get; set; }

    [Column("end_dt")]
    public DateTime? EndDt { get; set; }
}
