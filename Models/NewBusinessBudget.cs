using System.Text.Json.Serialization;

namespace PlanningAPI.Models
{
    public class NewBusinessBudget
    {
        public string BusinessBudgetId { get; set; }
        public string Description { get; set; }
        public int Level { get; set; }
        public bool IsActive { get; set; }
        public int Version { get; set; }
        public string VersionCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal EscalationRate { get; set; }

        public string OrgId { get; set; }
        public string AccountGroup { get; set; }
        public string? NBType { get; set; }
        
        public int? BurdenTemplateId { get; set; }
        [JsonIgnore]
        public BurdenTemplate BurdenTemplate { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string ModifiedBy { get; set; }
        public string? Trf_ProjId { get; set; }
        
        public string? Status { get; set; }


        /* ✅ Newly added columns */

        public string? Stage { get; set; }
        public string? Customer { get; set; }

        // ⚠ Recommended rename in DB later (Type is risky)
        public string? Type { get; set; }

        public string? OurRole { get; set; }
        public decimal? Workshare { get; set; }

        public string? ContractValue { get; set; }
        public string? ContractTypes { get; set; }

        public decimal? PgoCalculation { get; set; }
        public decimal? PwinValue { get; set; }
    }


    public class NewBusinessBudgetDTO
    {
        public string BusinessBudgetId { get; set; }
        public string Description { get; set; }
        public int Level { get; set; }
        public bool IsActive { get; set; }
        public int Version { get; set; }
        public string VersionCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal EscalationRate { get; set; }

        public string OrgId { get; set; }
        public string AccountGroup { get; set; }

        public int? BurdenTemplateId { get; set; }
    }

}
