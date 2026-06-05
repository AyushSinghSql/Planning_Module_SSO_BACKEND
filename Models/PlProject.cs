using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanningAPI.Models;

public partial class PlProject
{
    public string ProjId { get; set; } = null!;

    public string ProjTypeDc { get; set; } = null!;

    public string? ProjName { get; set; } = null!;

    public string? OrgId { get; set; } = null!;

    public string? CustId { get; set; }

    public string? Notes { get; set; }

    public DateOnly? ProjEndDt { get; set; }

    public DateOnly? ProjStartDt { get; set; }

    public string? AcctGrpCd { get; set; } = null!;

    public string? ActiveFl { get; set; } = null!;

    public string? CompanyId { get; set; } = null!;

    public string? AcctGrpFl { get; set; } = null!;

    public string? ProjMgrName { get; set; } = null!;

    public string? ProjLongName { get; set; } = null!;
    public decimal? proj_v_tot_amt { get; set; } = 0m;
    public decimal? proj_f_tot_amt { get; set; } = 0m;
    public decimal? proj_v_fee_amt { get; set; } = 0m;
    public decimal? proj_v_cst_amt { get; set; } = 0m;
    public decimal? proj_f_fee_amt { get; set; } = 0m;
    public decimal? proj_f_cst_amt { get; set; } = 0m;

    public DateOnly? InactiveDt { get; set; }

    public virtual Organization? Org { get; set; } = null!;

    public virtual ICollection<PlForecast> PlForecasts { get; set; } = new List<PlForecast>();

    public virtual ICollection<PlProjectPlan> PlProjectPlans { get; set; } = new List<PlProjectPlan>();
    public ICollection<UserProjectMap> UserProjects { get; set; }
= new List<UserProjectMap>();
}

public partial class ProjectUpdateDateDto
{
    public string? ProjId { get; set; }
    public DateOnly? ProjEndDt { get; set; }
    public DateOnly? ProjStartDt { get; set; }


}


[Table("project_modifications")]
public class ProjectModification
{
    [Column("proj_id")]
    public string? ProjId { get; set; }

    [Column("proj_mod_desc")]
    public string? ProjModDesc { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("proj_start_dt")]
    public DateTime? ProjStartDt { get; set; }

    [Column("proj_end_dt")]
    public DateTime? ProjEndDt { get; set; }

    [Column("proj_v_cst_amt", TypeName = "numeric")]
    public decimal? ProjVCstAmt { get; set; }

    [Column("proj_v_fee_amt", TypeName = "numeric")]
    public decimal? ProjVFeeAmt { get; set; }

    [Column("proj_f_cst_amt", TypeName = "numeric")]
    public decimal? ProjFCstAmt { get; set; }

    [Column("proj_f_fee_amt", TypeName = "numeric")]
    public decimal? ProjFFeeAmt { get; set; }

    [Column("modified_by")]
    public string? ModifiedBy { get; set; }

    [Column("time_stamp")]
    public DateTime? TimeStamp { get; set; }

    [Column("proj_v_proft_rt", TypeName = "numeric")]
    public decimal? ProjVProftRt { get; set; }

    [Column("proj_f_proft_rt", TypeName = "numeric")]
    public decimal? ProjFProftRt { get; set; }

    [Column("proj_mod_id")]
    public string? ProjModId { get; set; }

    [Column("deliv_units_qty", TypeName = "numeric")]
    public decimal? DelivUnitsQty { get; set; }

    [Column("est_unit_cst_amt", TypeName = "numeric")]
    public decimal? EstUnitCstAmt { get; set; }

    [Column("unit_price_amt", TypeName = "numeric")]
    public decimal? UnitPriceAmt { get; set; }

    [Column("item_key")]
    public string? ItemKey { get; set; }

    [Column("clin_id")]
    public string? ClinId { get; set; }

    [Column("effect_dt")]
    public DateTime? EffectDt { get; set; }

    [Column("rowversion")]
    public int? RowVersion { get; set; }

    [Column("cntr_id")]
    public string? CntrId { get; set; }

    [Column("cntr_mod_id")]
    public string? CntrModId { get; set; }

    [Column("subcntr_id")]
    public string? SubCntrId { get; set; }

    [Column("subcntr_mod_id")]
    public string? SubCntrModId { get; set; }
}
