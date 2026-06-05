using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PlanningAPI.Models;

namespace YourNamespace.Models
{
    [Table("pl_employeeburdencalculated")]
    public class pl_EmployeeBurdenCalculated
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("empl_id")]
        [StringLength(12)]
        public string Empl_Id { get; set; } = null!;

        [Required]
        [Column("burdendate", TypeName = "date")]
        public DateTime BurdenDate { get; set; }

        [Column("proj_id")]
        [StringLength(20)]
        public string? Proj_Id { get; set; }

        [Column("pl_id")]
        public int? Pl_Id { get; set; }

        [Column("plc_code")]
        [StringLength(10)]
        public string? Plc_Code { get; set; }

        [Required]
        [Column("forecastedcost", TypeName = "numeric(18,4)")]
        public decimal ForecastedCost { get; set; }

        [Required]
        [Column("forecastedhours", TypeName = "numeric(18,4)")]
        public decimal ForecastedHours { get; set; }

        [Required]
        [Column("burdencost", TypeName = "numeric(18,4)")]
        public decimal BurdenCost { get; set; }

        [Required]
        [Column("template_id")]
        public int Template_ID { get; set; }

        [Column("notes")]
        public string? Notes { get; set; }

        [Required]
        [Column("createdat", TypeName = "timestamptz")]
        public DateTime CreatedAt { get; set; }

        [Required]
        [Column("updatedat", TypeName = "timestamptz")]
        public DateTime UpdatedAt { get; set; }

        [Column("createdby")]
        [StringLength(50)]
        public string? CreatedBy { get; set; }

        [Column("updatedby")]
        [StringLength(50)]
        public string? UpdatedBy { get; set; }

        // Navigation Properties

        public virtual PlEmployee? Employee { get; set; }

        public virtual PlProject? Project { get; set; }

        public virtual PlcCode? LabCategory { get; set; }

        public virtual PlProjectPlan? ProjectPlan { get; set; }
    }
}
