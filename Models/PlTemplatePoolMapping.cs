using System;
using System.ComponentModel.DataAnnotations;

namespace PlanningAPI.Models
{
    public class PlTemplatePoolMapping
    {
        public int TemplateId { get; set; }

        [MaxLength(20)]
        public string PoolId { get; set; }

        [MaxLength(30)]
        public string? ModifiedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        // Navigation (optional)
        public BurdenTemplate? Template { get; set; }
        public Pools? Pool { get; set; }
    }
}
