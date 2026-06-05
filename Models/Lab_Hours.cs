using System;

public class LabHours
{
    public int? LabHsKey { get; set; }
    public string? ProjId { get; set; }
    public string? OrgId { get; set; }
    public string? AcctId { get; set; }
    public string FyCd { get; set; }
    public int PdNo { get; set; }
    public int? SubPdNo { get; set; }
    public string? BillLabCatCd { get; set; }
    public decimal? ActHrs { get; set; }
    public decimal? AllowRevHrs { get; set; }
    public decimal? RevRtAmt { get; set; }
    public string? GenlLabCatCd { get; set; }
    public string? ModifiedBy { get; set; }
    public DateTime? TimeStamp { get; set; }
    public decimal? ActAmt { get; set; }
    public string? EmplId { get; set; }
    public string? VendId { get; set; }
    public string? RecalcRevFl { get; set; }
    public string? VendEmplId { get; set; }
    public string? SBillRtTypeCd { get; set; }
    public decimal? CurMultRt { get; set; }
    public decimal? YtdMultRt { get; set; }
    public DateTime? EffectBillDt { get; set; }
    public int? RowVersion { get; set; }
    public string? CompanyId { get; set; }
    public decimal? OrigRevRtAmt { get; set; }
    //public decimal? PerHrRt { get; set; }
}
