using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PlanningAPI.Models
{
    public class ProjectFee
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(30)]
        public string ProjId { get; set; }

        [MaxLength(10)]
        public string FeeCode { get; set; }

        [MaxLength(20)]
        public string FeeType { get; set; }

        [MaxLength(50)]
        public string FeeBase { get; set; }

        public decimal? FeePercent { get; set; }

        public decimal? FixedAmount { get; set; }

        public DateTime EffectiveDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(50)]
        public string ModifiedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        //[JsonIgnore]
        //public virtual PlProject Project { get; set; }
    }
}
