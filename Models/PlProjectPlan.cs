using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PlanningAPI.Models;

public partial class PlProjectPlan
{
    public int? PlId { get; set; }

    public string? ProjId { get; set; } = null!;

    [NotMapped]
    public string? ProjType { get; set; } = null!;

    public string? PlType { get; set; } = null!;

    public int? Version { get; set; }
    public int? TemplateId { get; set; }


    public string? VersionCode { get; set; }

    public bool? FinalVersion { get; set; }

    public bool? IsCompleted { get; set; }

    public bool? IsApproved { get; set; }

    public string? Status { get; set; }
    public string? Source { get; set; } = null!;
    public string? Type { get; set; } = null!;

    public DateOnly? ClosedPeriod { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? ModifiedBy { get; set; }

    public string? ApprovedBy { get; set; }

    public string? CreatedBy { get; set; }
    //[NotMapped]
    public DateOnly? ProjEndDt { get; set; }
    //[NotMapped]
    public DateOnly? ProjStartDt { get; set; }


    [NotMapped]
    public bool CopyFromExistingProject { get; set; }
    [NotMapped]
    public DateOnly? SourceProjEndDt { get; set; }
    [NotMapped]
    public DateOnly? SourceProjStartDt { get; set; }
    [NotMapped]
    public string? SourceProjId { get; set; }
    [NotMapped]
    public decimal? proj_f_fee_amt { get; set; } = 0m;
    [NotMapped]
    public decimal? proj_f_cst_amt { get; set; } = 0m;
    [NotMapped]
    public decimal? proj_f_tot_amt { get; set; } = 0m;
    [NotMapped]
    public string? ProjName { get; set; } = null!;
    [NotMapped]
    public string? OrgId { get; set; } = null!;
    [NotMapped]
    public string? AcctGrpCd { get; set; } = null!;
    [NotMapped]
    public string? RevenueAccount { get; set; } = null!;
    [NotMapped]

    public decimal? Revenue { get; set; } = null!;
    [NotMapped]
    public string? ProjectStatus { get; set; } = null!;


    [JsonIgnore]
    public virtual ICollection<PlForecast> PlForecasts { get; set; } = new List<PlForecast>();
    [JsonIgnore]
    public virtual PlProject? Proj { get; set; } = null!;
    [JsonIgnore]
    public virtual ICollection<PlDct>? PlDct { get; set; } = new List<PlDct>();
}

public class ProjectWithPlanDto
{
    public string ProjId { get; set; }
    public string? ProjType { get; set; }
    public int? PlId { get; set; }
    public string? PlType { get; set; }
    public int? Version { get; set; }
    public string? VersionCode { get; set; }
    public bool? FinalVersion { get; set; }
    public bool? IsCompleted { get; set; }
    public bool? IsApproved { get; set; }
    public string? Status { get; set; }
    public DateTime? ClosedPeriod { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public string? ApprovedBy { get; set; }
    public string? CreatedBy { get; set; }
    public string? Source { get; set; }
    public string? Type { get; set; }
    public int? BurdenTemplateId { get; set; }
    public decimal? proj_f_fee_amt { get; set; } = 0m;
    public decimal? proj_f_cst_amt { get; set; } = 0m;
    public decimal? proj_f_tot_amt { get; set; } = 0m;
    public string? ProjName { get; set; } = null!;
    public string? OrgId { get; set; } = null!;
    public DateOnly? ProjEndDt { get; set; }

    public DateOnly? ProjStartDt { get; set; }

    public string? AcctGrpCd { get; set; } = null!;
    public string? ProjectStatus { get; set; } = null!;

    public PlProjectPlan ToEntity()
    {
        var plPlan = new PlProjectPlan
        {
            PlId = this.PlId,
            ProjId = this.ProjId,
            ProjType = this.ProjType,
            PlType = this.PlType,
            Version = this.Version,
            VersionCode = this.VersionCode,
            FinalVersion = this.FinalVersion,
            IsCompleted = this.IsCompleted,
            IsApproved = this.IsApproved,
            Status = this.Status,
            ClosedPeriod = this.ClosedPeriod.HasValue
                ? DateOnly.FromDateTime(this.ClosedPeriod.Value)
                : null,
            CreatedAt = this.CreatedAt,
            UpdatedAt = this.UpdatedAt,
            ModifiedBy = this.ModifiedBy,
            ApprovedBy = this.ApprovedBy,
            CreatedBy = this.CreatedBy,
            Source = this.Source,
            Type = this.Type,
            OrgId = this.OrgId,
            ProjEndDt = this.ProjEndDt,
            ProjStartDt = this.ProjStartDt,
            ProjName = this.ProjName,
            proj_f_cst_amt = this.proj_f_cst_amt,
            proj_f_fee_amt = this.proj_f_fee_amt,
            proj_f_tot_amt = this.proj_f_tot_amt,
            TemplateId = this.BurdenTemplateId,
            AcctGrpCd = this.AcctGrpCd
        };
        return plPlan;
    }
}

