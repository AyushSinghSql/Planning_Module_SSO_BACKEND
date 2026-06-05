using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

public class PSRFinalData
{
    public string ProjId { get; set; }
    public string AcctId { get; set; }
    public string OrgId { get; set; }
    public string FyCd { get; set; }
    public int PdNo { get; set; }
    public int SubPdNo { get; set; }
    public string? PoolNo { get; set; }
    public string? PoolName { get; set; }
    public int SubTotTypeNo { get; set; }
    public string RateType { get; set; }
    public decimal PyIncurAmt { get; set; }
    public decimal SubIncurAmt { get; set; }
    public decimal PtdIncurAmt { get; set; }
    public decimal YtdIncurAmt { get; set; }
    public decimal CurBurdRt { get; set; }
    public decimal YtdBurdRt { get; set; }
    public string ModifiedBy { get; set; }
    public DateTime TimeStamp { get; set; }
    public int Rowversion { get; set; }
    public string CompanyId { get; set; }
}

public class PsrHeader
{
    [Key]
    public string? ProjId { get; set; }

    public string? ProjName { get; set; }
    public string? CustName { get; set; }
    public string? PrimeContrId { get; set; }

    public DateOnly? ProjStartDt { get; set; }
    public DateOnly? ProjEndDt { get; set; }

    public string? ActiveFl { get; set; }
    public string? SProjRptDc { get; set; }
    public string? ProjTypeDc { get; set; }

    public decimal? ProjVCstAmt { get; set; }
    public decimal? ProjVFeeAmt { get; set; }
    public decimal? ProjVTotAmt { get; set; }

    public decimal? PyIncurHrs { get; set; }
    public decimal? SubIncurHrs { get; set; }
    public decimal? PtdIncurHrs { get; set; }

    public string? ModifiedBy { get; set; }

    public DateTime? TimeStamp { get; set; }

    public string? FyCd { get; set; }
    public int? PdNo { get; set; }
    public int? SubPdNo { get; set; }

    public decimal? BilledAmt { get; set; }
    public decimal? BalDueAmt { get; set; }

    public string? BillingLevelFl { get; set; }

    public decimal? ProjFCstAmt { get; set; }
    public decimal? ProjFFeeAmt { get; set; }
    public decimal? ProjFTotAmt { get; set; }

    public int? Rowversion { get; set; }
    public string? CompanyId { get; set; }
}


[Keyless]
public class PsrPoolCostSummary
{
    public string? ProjId { get; set; }
    public string? AcctId { get; set; }
    public string? OrgId { get; set; }
    public string? FyCd { get; set; }
    public int? PdNo { get; set; }
    public int? SubPdNo { get; set; }
    public string? PoolName { get; set; }
    public int? SubTotTypeNo { get; set; }
    public string? RateType { get; set; }

    public string? ProjName { get; set; }
    public string? ProjectOrgId { get; set; }
    public DateOnly? ProjStartDt { get; set; }
    public DateOnly? ProjEndDt { get; set; }

    public string? L1AcctName { get; set; }

    public decimal? PyIncurAmt { get; set; }
    public decimal? SubIncurAmt { get; set; }
    public decimal? PtdIncurAmt { get; set; }
    public decimal? YtdIncurAmt { get; set; }
}



[Keyless]
public class ViewPsrData
{
    public string? ProjId { get; set; }
    public string? AcctId { get; set; }
    public string? OrgId { get; set; }
    public string? FyCd { get; set; }
    public int? PdNo { get; set; }
    public int? SubPdNo { get; set; }
    public string? PoolName { get; set; }
    public int? SubTotTypeNo { get; set; }
    public string? RateType { get; set; }

    public string? ProjName { get; set; }
    public string? ProjectOrgId { get; set; }
    public DateOnly? ProjStartDt { get; set; }
    public DateOnly? ProjEndDt { get; set; }

    public string? L1AcctName { get; set; }

    public decimal PyIncurAmt { get; set; }
    public decimal SubIncurAmt { get; set; }
    public decimal PtdIncurAmt { get; set; }
    public decimal YtdIncurAmt { get; set; }
}


public class ForecastView
{
    public decimal? ForecastedAmt { get; set; }
    public int ForecastId { get; set; }
    public string? ProjId { get; set; }
    public int? PlId { get; set; }
    public string? EmplId { get; set; }
    public int? Month { get; set; }
    public int? Year { get; set; }
    public decimal? ForecastedHours { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? DctId { get; set; }
    public string? AcctId { get; set; }
    public string? OrgId { get; set; }
    public int? PlcRateId { get; set; }
    public decimal? HrlyRate { get; set; }
    public DateOnly? EffectDt { get; set; }
    public string? Plc { get; set; }
    public decimal? Burden { get; set; }
    public decimal? Cost { get; set; }
    public decimal? Revenue { get; set; }
    public decimal? ActualAmt { get; set; }
    public decimal? ActualHours { get; set; }
    public decimal? ForecastedCost { get; set; }
    public decimal? Fringe { get; set; }
    public decimal? Overhead { get; set; }
    public decimal? Gna { get; set; }
    public decimal? Mnh { get; set; }
    public decimal? Fees { get; set; }

    public string? PlType { get; set; }
    public int? Version { get; set; }
    public string? VersionCode { get; set; }
    public bool? FinalVersion { get; set; }
    public bool? IsCompleted { get; set; }
    public bool? IsApproved { get; set; }
    public string? Status { get; set; }
    public int? ClosedPeriod { get; set; }
    public DateTime? PCreatedAt { get; set; }
    public DateTime? PUpdatedAt { get; set; }
    public string? ModifiedBy { get; set; }
    public string? ApprovedBy { get; set; }
    public string? CreatedBy { get; set; }
    public string? Source { get; set; }
    public string? Type { get; set; }
    public int? BurdenTemplateId { get; set; }
    public int? BudEacYear { get; set; }
}




