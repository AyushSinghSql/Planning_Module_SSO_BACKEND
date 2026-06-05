using System.Text.Json.Serialization;

namespace PlanningAPI.Models
{
    public class BurdenTemplate
    {
        public int Id { get; set; }
        public string TemplateCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [JsonIgnore]
        public ICollection<PlPoolRate> PlPoolRates { get; set; } = new List<PlPoolRate>();

        [JsonIgnore]
        public ICollection<PlTemplatePoolRate> TemplatePoolRate { get; set; } = new List<PlTemplatePoolRate>();
        [JsonIgnore]
        public List<BurdenRate> Rates { get; set; } = new();
    }

    public class BurdenRate
    {
        public int Id { get; set; }
        public int TemplateId { get; set; }
        public string PoolName { get; set; } = string.Empty;
        public decimal RatePercentage { get; set; }
        public string Base { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime? EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }

        public BurdenTemplate Template { get; set; } = null!;
    }

    public class BurdenInput
    {
        /// <summary>
        /// Total hours worked.
        /// </summary>
        public decimal Hours { get; set; }

        public decimal DirectCost { get; set; }

        /// <summary>
        /// Base hourly rate.
        /// </summary>
        public decimal HourlyRate { get; set; }

        /// <summary>
        /// Fringe rate as a percentage (e.g., 25 for 25%).
        /// </summary>
        public decimal FringeRate { get; set; }

        /// <summary>
        /// Overhead rate as a percentage.
        /// </summary>
        public decimal OverheadRate { get; set; }

        /// <summary>
        /// G&A (General & Administrative) rate as a percentage.
        /// </summary>
        public decimal GnaRate { get; set; }

        public PlEmployee plEmployee { get; set; } = new();
    }


}
