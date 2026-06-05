using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanningAPI.Models
{

    [Table("rev_formulas")]
    public class RevFormula
    {
        [Key]
        [Column("formula_cd")]
        [MaxLength(15)]
        public string FormulaCd { get; set; }

        [Column("formula_desc")]
        [MaxLength(100)]
        public string? FormulaDesc { get; set; }

        [Column("award_fee_fl")]
        public bool AwardFeeFl { get; set; } = false;

        [Column("modified_by")]
        [MaxLength(20)]
        public string? ModifiedBy { get; set; }

        [Column("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

}
