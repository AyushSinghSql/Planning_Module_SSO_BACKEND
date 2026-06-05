using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanningAPI.Models
{
    public class ProjectPlcRate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string ProjId { get; set; }

        [Required]
        public string LaborCategoryCode { get; set; }
        public string? SBillRtTypeCd { get; set; }
        
        public decimal? CostRate { get; set; }

        public decimal? BillingRate { get; set; }

        [Required]
        public DateTime EffectiveDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(50)]
        public string? ModifiedBy { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Optional: Navigation properties
        //public PlProject? Project { get; set; }
        //public PlcCode? PlcCode { get; set; }
        //[ForeignKey(nameof(ProjId))]
        //public PlProject Project { get; set; }

        //[ForeignKey(nameof(LaborCategoryCode))] // ✅ This ensures EF links correctly
        //public PlcCode PlcCode { get; set; }
    }
}
