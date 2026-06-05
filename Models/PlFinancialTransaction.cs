namespace PlanningAPI.Models
{

    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("gl_post_details", Schema = "public")]
    public class GlPostDetail
    {
        [Column("acct_id")]
        public string? AcctId { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("proj_id")]
        public string? ProjId { get; set; }

        [Column("fy_cd")]
        public string? FyCd { get; set; }

        [Column("pd_no")]
        public int? PdNo { get; set; }

        [Column("sub_pd_no")]
        public int? SubPdNo { get; set; }

        [Column("company_id")]
        public string? CompanyId { get; set; }

        [Column("id")]
        public string? Id { get; set; }

        [Column("s_id_type")]
        public string? SIdType { get; set; }

        [Column("amt1")]
        public decimal? Amt1 { get; set; }

        [Column("hrs1")]
        public decimal? Hrs1 { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("bill_lab_cat_cd")]
        public string? BillLabCatCd { get; set; }

        [Column("effect_bill_dt")]
        public DateTime? EffectBillDt { get; set; }
        [Column("s_jnl_cd")]
        public string? S_JNL_CD { get; set; }
    }

    public class PlFinancialTransaction
    {
        public string? SrceKey { get; set; }
        public string? Lvl1Key { get; set; }
        public string? Lvl2Key { get; set; }
        public string? Lvl3Key { get; set; }
        public string? AcctId { get; set; }
        public string? OrgId { get; set; }
        public string? ProjId { get; set; }
        public string? FyCd { get; set; }
        public int? PdNo { get; set; }
        public int? SubPdNo { get; set; }
        public string? SJnlCd { get; set; }
        public int? PostSeqNo { get; set; }
        public decimal? Amt { get; set; }
        public decimal? Hrs { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? TimeStamp { get; set; }
        public int? Rowversion { get; set; }
        public string? CompanyId { get; set; }

        public string? SrceKey1 { get; set; }
        public string? Lvl1Key1 { get; set; }
        public string? Lvl2Key1 { get; set; }
        public string? Lvl3Key1 { get; set; }
        public string? GlpsumSrceKey { get; set; }
        public string? GlpsumLvl1Key { get; set; }
        public string? GlpsumLvl2Key { get; set; }
        public string? GlpsumLvl3Key { get; set; }

        public string? Id { get; set; }
        public string? VchrNo { get; set; }
        public string? SIdType { get; set; }
        public string? PoId { get; set; }
        public string? InvcId { get; set; }
        public string? Ref1Id { get; set; }
        public string? GenlLabCatCd { get; set; }
        public string? Ref2Id { get; set; }

        public decimal? Amt1 { get; set; }
        public decimal? Hrs1 { get; set; }
        public string? ModifiedBy1 { get; set; }
        public DateTime? TimeStamp1 { get; set; }

        public string? Name { get; set; }
        public string? TrnDesc { get; set; }
        public string? PostUserId { get; set; }
        public string? EntrUserId { get; set; }
        public string? JeNo { get; set; }
        public string? ChkNo { get; set; }
        public string? BillLabCatCd { get; set; }
        public string? CashRecptNo { get; set; }
        public string? BillNoId { get; set; }
        public string? ItemId { get; set; }
        public string? ItemRvsnId { get; set; }
        public decimal? UnitsQty { get; set; }
        public string? CommentsNt { get; set; }

        public DateOnly? TsDt { get; set; }
        public DateOnly? CashRecptDt { get; set; }
        public DateOnly? UnitsUsageDt { get; set; }
        public DateOnly? EffectBillDt { get; set; }

        public string? TrnCrncyCd { get; set; }
        public decimal? TrnAmt { get; set; }
        public int? Rowversion1 { get; set; }
        public string? CompanyId1 { get; set; }

        public string? VchrInvcCrncyCd { get; set; }
        public decimal? VchrInvcAmt { get; set; }
        public string? CrChkId { get; set; }
        public string? SettlNo { get; set; }
        public string? BsrTrnCrncy { get; set; }
    }

    public class PoolOrgFinancialDetail
    {
        public List<PoolOrgCostFinancialDetail> poolOrgCostFinancialDetail { get; set; }
        public List<PoolOrgBaseFinancialDetail> poolOrgBaseFinancialDetail { get; set; }
        public decimal? TotalYTDCostActualAmt { get; set; } = 0m;
        public decimal? TotalYTDBaseActualAmt { get; set; } = 0m;
        public decimal? TotalYTDBaseAllocationActualAmt { get; set; } = 0m;
        public decimal? Rate { get; set; } = 0m;

        public int? PoolNo { get; set; }
        public string? PoolName { get; set; }

    }
    public class PoolOrgCostFinancialDetail
    {
        public string? OrgId { get; set; }
        public string? AcctId { get; set; }
        public string? AccountName { get; set; }
        public decimal? YTDActualAmt { get; set; }
        public decimal? YTDBudgetedAmt { get; set; }
        public List<PeriodCostFinancialDetail>? PeriodDetails { get; set; }
    }
    public class PeriodCostFinancialDetail
    {
        public decimal? Actualamt { get; set; }
        public decimal? BudgetedAmt { get; set; }
        public decimal? Ytdamt { get; set; }

        public decimal? Rate { get; set; }
        public decimal? YtdRate { get; set; }
        public decimal? Period { get; set; }
    }
    public class PoolOrgBaseFinancialDetail
    {
        public string? OrgId { get; set; }
        public string? AcctId { get; set; }
        public string? AllocationOrgId { get; set; }
        public string? AllocationAcctId { get; set; }
        public string? AccountName { get; set; }
        public decimal? YTDActualAmt { get; set; }
        public decimal? YTDAllocationAmt { get; set; }
        public decimal? YTDBudgetedAmt { get; set; }
        public List<PeriodbaseFinancialDetail>? PeriodDetails { get; set; }
    }

    public class PeriodbaseFinancialDetail
    {
        public decimal? baseAmt { get; set; }
        public decimal? costAmt { get; set; }
        public decimal? YtdcostAmt { get; set; }
        public decimal? AllocationAmt { get; set; } = 0m;
        public decimal? YtdAllocationAmt { get; set; } = 0m;
        public decimal? BudgetAmt { get; set; } = 0m;
        public decimal? YtdBudgetAmt { get; set; } = 0m;

        public decimal? Period { get; set; }
        public decimal? Rate { get; set; }
        public decimal? YtdRate { get; set; }
    }


    public class PoolRateDetailsByPeriod
    {
        public string? PoolName { get; set; }
        public int? PoolNo { get; set; }
        public List<PeriodbaseFinancialDetail>? PeriodDetails { get; set; }
    }
}
