
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace PlanningAPI.Models
{
    [Table("proj_bgt_rev_setup")]
    public class ProjBgtRevSetup
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("plid")]
        public int? PlId { get; set; }

        [Column("proj_id")]
        [MaxLength(50)]
        public string ProjId { get; set; }

        [Column("rev_type")]
        [MaxLength(50)]
        public string RevType { get; set; }

        [Column("rev_acct_id")]
        [MaxLength(20)]
        public string? RevAcctId { get; set; }

        [Column("dflt_fee_rt", TypeName = "numeric(19,4)")]
        public decimal? DfltFeeRt { get; set; }

        [Column("lab_cost_fl")]
        public bool? LabCostFl { get; set; }

        [Column("lab_burd_fl")]
        public bool? LabBurdFl { get; set; }

        [Column("lab_fee_cost_fl")]
        public bool? LabFeeCostFl { get; set; }

        [Column("lab_fee_hrs_fl")]
        public bool? LabFeeHrsFl { get; set; }

        [Column("lab_fee_rt", TypeName = "numeric(19,4)")]
        public decimal? LabFeeRt { get; set; }

        [Column("lab_tm_fl")]
        public bool? LabTmFl { get; set; }

        [Column("non_lab_cost_fl")]
        public bool? NonLabCostFl { get; set; }

        [Column("non_lab_burd_fl")]
        public bool? NonLabBurdFl { get; set; }

        [Column("non_lab_fee_cost_fl")]
        public bool? NonLabFeeCostFl { get; set; }

        [Column("non_lab_fee_hrs_fl")]
        public bool? NonLabFeeHrsFl { get; set; }

        [Column("non_lab_fee_rt", TypeName = "numeric(19,4)")]
        public decimal? NonLabFeeRt { get; set; }

        [Column("non_lab_tm_fl")]
        public bool? NonLabTmFl { get; set; }

        [Column("use_bill_burden_rates")]
        public bool? UseBillBurdenRates { get; set; }

        [Column("override_funding_ceiling_fl")]
        public bool OverrideFundingCeilingFl { get; set; }

        [Column("override_rev_amt_fl")]
        public bool OverrideRevAmtFl { get; set; }

        [Column("override_rev_adj_fl")]
        public bool OverrideRevAdjFl { get; set; }

        [Column("override_rev_setting_fl")]
        public bool OverrideRevSettingFl { get; set; }

        [Column("rowversion")]
        public int RowVersion { get; set; } = 0;

        [Column("modified_by")]
        [MaxLength(50)]
        public string ModifiedBy { get; set; } = string.Empty;

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; } = DateTime.UtcNow;

        [Column("company_id")]
        [MaxLength(10)]
        public string? CompanyId { get; set; }

        [Column("at_risk_amt", TypeName = "numeric(17,2)")]
        public decimal AtRiskAmt { get; set; } = 0;

        [Column("version_no")]
        public int? VersionNo { get; set; }

        [Column("bgt_type")]
        [MaxLength(10)]
        public string BgtType { get; set; }
    }


    [Table("proj_rev_definition")]
    public class ProjRevDefinition
    {
        [Column("project_id")]
        [MaxLength(50)]
        public string? ProjectId { get; set; }

        [Column("revenue_formula_cd")]
        [MaxLength(50)]
        public string? RevenueFormulaCd { get; set; }

        [Column("revenue_formula_desc")]
        [MaxLength(255)]
        public string? RevenueFormulaDesc { get; set; }

        [Column("company_id")]
        public int? CompanyId { get; set; }

        [Column("non_labor_goal_multiplier_rate", TypeName = "numeric(10,4)")]
        public decimal? NonLaborGoalMultiplierRate { get; set; }

        [Column("at_risk")]
        [MaxLength(1)]
        public string? AtRisk { get; set; }   // Y / N

        [Column("revenue_to_own_org")]
        [MaxLength(1)]
        public string? RevenueToOwnOrg { get; set; }   // Y / N

        [Column("units")]
        [MaxLength(1)]
        public string? Units { get; set; }   // Y / N

        [Column("discount_type_cd")]
        [MaxLength(50)]
        public string? DiscountTypeCd { get; set; }

        [Column("at_risk_amount", TypeName = "numeric(15,2)")]
        public decimal? AtRiskAmount { get; set; }

        [Column("contract_loss_value", TypeName = "numeric(15,2)")]
        public decimal? ContractLossValue { get; set; }

        [Column("revenue_adjustment_amount", TypeName = "numeric(15,2)")]
        public decimal? RevenueAdjustmentAmount { get; set; }

        [Column("revenue_calculation_amount", TypeName = "numeric(15,6)")]
        public decimal? RevenueCalculationAmount { get; set; }

        [Column("revenue_calculation_1_amount", TypeName = "numeric(15,6)")]
        public decimal? RevenueCalculation1Amount { get; set; }

        [Column("revenue_calculation_2_amount", TypeName = "numeric(15,6)")]
        public decimal? RevenueCalculation2Amount { get; set; }

        [Column("modified_by")]
        [MaxLength(100)]
        public string? ModifiedBy { get; set; }

        [Column("time_stamp")]
        public DateTime? TimeStamp { get; set; }
    }

    
[Keyless]
    public class ProjectRevenueAdjustment
    {
        public string ProjId { get; set; }
        public string FyCd { get; set; }
        public int PdNo { get; set; }
        public int? SubPdNo { get; set; }
        public decimal? RevAdjAmt { get; set; }
        public string RevAdjDesc { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime TimeStamp { get; set; }
        public decimal? RevAwdAdjAmt { get; set; }
    }


}
