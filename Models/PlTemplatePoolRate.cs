using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using PlanningAPI.Models;

public class PlTemplatePoolRate
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public int TemplateId { get; set; }

    [MaxLength(20)]
    public string PoolId { get; set; }

    [Required]
    public int Year { get; set; }

    [Required]
    [Range(1, 12)]
    public int Month { get; set; }

    public decimal? ActualRate { get; set; }
    public decimal? TargetRate { get; set; }

    [MaxLength(30)]
    public string? ModifiedBy { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    [JsonIgnore]
    public virtual BurdenTemplate? Template { get; set; }
    [JsonIgnore]
    public virtual Pools? Pool { get; set; }

    public PlTemplatePoolRate CloneWithoutId()
    {
        return new PlTemplatePoolRate
        {
            PoolId = this.PoolId,
            Year = this.Year,
            Month = this.Month,
            ActualRate = this.ActualRate,
            TargetRate = this.TargetRate,
            ModifiedBy = this.ModifiedBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            TemplateId = this.TemplateId,
        };
    }
}


public class PlTemplatePoolRateDto
{
    public int TemplateId { get; set; }
    public string PoolId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal? ActualRate { get; set; }
    public decimal? TargetRate { get; set; }
}
