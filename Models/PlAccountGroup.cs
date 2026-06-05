using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PlanningAPI.Models
{
    public class Pools
    {
        [Key]
        [MaxLength(20)]
        public string Code { get; set; }

        [MaxLength(100)]
        public string? Name { get; set; }
        [MaxLength(100)]
        public string? Type { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
        public int Sequence { get; set; }
        public int? PoolNo { get; set; }
        
        [MaxLength(30)]
        public string? ModifiedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
        [JsonIgnore]
        public ICollection<PlPoolRate> PlPoolRates { get; set; }
        public ICollection<PlTemplatePoolRate> TemplatePoolRate { get; set; }
    }

}
