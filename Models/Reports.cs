using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanningAPI.Models
{
    [Table("fs", Schema = "public")]
    public class Fs
    {
        [Key]
        [Column("fs_cd")]
        [StringLength(6)]
        public string FsCd { get; set; } = null!;

        [Column("prim_fs_fl")]
        [StringLength(1)]
        public string PrimFsFl { get; set; } = null!;

        [Column("s_fs_type")]
        [StringLength(1)]
        public string SFsType { get; set; } = null!;

        [Column("fs_desc")]
        [StringLength(30)]
        public string FsDesc { get; set; } = null!;

        [Column("modified_by")]
        [StringLength(20)]
        public string ModifiedBy { get; set; } = null!;

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("company_id")]
        [StringLength(10)]
        public string CompanyId { get; set; } = null!;

        [Column("rowversion")]
        public int? RowVersion { get; set; }

        [Column("fs_isoci_amt_fl")]
        [StringLength(1)]
        public string? FsIsociAmtFl { get; set; }

        [Column("incstmt_cd")]
        [StringLength(6)]
        public string? IncstmtCd { get; set; }
    }

    [Table("fs_ln", Schema = "public")]
    public class FsLn
    {
        [Column("fs_cd")]
        [StringLength(6)]
        public string FsCd { get; set; } = null!;

        [Key]
        [Column("fs_ln_key")]
        public int FsLnKey { get; set; }

        [Column("fs_major_no")]
        public int FsMajorNo { get; set; }

        [Column("fs_grp_no")]
        public int FsGrpNo { get; set; }

        [Column("fs_ln_no")]
        public int FsLnNo { get; set; }

        [Column("fs_major_desc")]
        [StringLength(30)]
        public string FsMajorDesc { get; set; } = null!;

        [Column("fs_grp_desc")]
        [StringLength(30)]
        public string FsGrpDesc { get; set; } = null!;

        [Column("fs_ln_desc")]
        [StringLength(30)]
        public string FsLnDesc { get; set; } = null!;

        [Column("fs_rvrs_sgn_fl")]
        [StringLength(1)]
        public string FsRvrsSgnFl { get; set; } = null!;

        [Column("modified_by")]
        [StringLength(20)]
        public string ModifiedBy { get; set; } = null!;

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("company_id")]
        [StringLength(10)]
        public string CompanyId { get; set; } = null!;

        [Column("rev_fl")]
        [StringLength(1)]
        public string RevFl { get; set; } = null!;

        [Column("fs_recl_cr_acct_id")]
        [StringLength(15)]
        public string? FsReclCrAcctId { get; set; }

        [Column("fs_recl_dr_acct_id")]
        [StringLength(15)]
        public string? FsReclDrAcctId { get; set; }

        [Column("cf_srce_fs_cd")]
        [StringLength(6)]
        public string? CfSrceFsCd { get; set; }

        [Column("cf_srce_fs_ln_key")]
        public int? CfSrceFsLnKey { get; set; }

        [Column("s_cf_actvty_cd")]
        [StringLength(1)]
        public string? SCfActvtyCd { get; set; }

        [Column("s_cf_acct_typ_cd")]
        [StringLength(6)]
        public string? SCfAcctTypCd { get; set; }

        [Column("assign_lvl_cd")]
        [StringLength(1)]
        public string? AssignLvlCd { get; set; }

        [Column("rowversion")]
        public int? RowVersion { get; set; }

        [Column("elim_acct_id")]
        [StringLength(15)]
        public string? ElimAcctId { get; set; }

        [Column("fs_cioci_amt_fl")]
        [StringLength(1)]
        public string? FsCiociAmtFl { get; set; }
    }

    [Table("fs_ln_acct", Schema = "public")]
    public class FsLnAcct
    {
        [Column("fs_cd")]
        [StringLength(6)]
        public string FsCd { get; set; } = null!;

        [Column("fs_ln_key")]
        public int FsLnKey { get; set; }

        [Column("acct_id")]
        [StringLength(15)]
        public string AcctId { get; set; } = null!;

        [Column("modified_by")]
        [StringLength(20)]
        public string ModifiedBy { get; set; } = null!;

        [Column("time_stamp")]
        public DateTime TimeStamp { get; set; }

        [Column("company_id")]
        [StringLength(10)]
        public string CompanyId { get; set; } = null!;

        [Column("s_cf_acct_typ_cd")]
        [StringLength(6)]
        public string? SCfAcctTypCd { get; set; }

        [Column("s_cf_actvty_cd")]
        [StringLength(1)]
        public string? SCfActvtyCd { get; set; }

        [Column("rowversion")]
        public int? RowVersion { get; set; }
    }




    [Keyless]
    [Table("view_is_report")] // 🔁 replace with your VIEW name
    public class View_Is_Report
    {
        [Column("pd_no")]
        public int? PdNo { get; set; }

        [Column("effect_bill_dt")]
        public DateTime? EffectBillDt { get; set; }

        [Column("amt1", TypeName = "numeric")]
        public decimal? Amt1 { get; set; }

        [Column("hrs1", TypeName = "numeric")]
        public decimal? Hrs1 { get; set; }

        [Column("sub_pd_no")]
        public int? SubPdNo { get; set; }

        [Column("id")]
        public string? Id { get; set; }

        [Column("s_id_type")]
        public string? SIdType { get; set; }

        [Column("name")]
        public string? Name { get; set; }

        [Column("bill_lab_cat_cd")]
        public string? BillLabCatCd { get; set; }

        [Column("fs_major_desc")]
        public string? FsMajorDesc { get; set; }

        [Column("fs_grp_desc")]
        public string? FsGrpDesc { get; set; }

        [Column("acct_id")]
        public string? AcctId { get; set; }

        [Column("fs_ln_desc")]
        public string? FsLnDesc { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("proj_id")]
        public string? ProjId { get; set; }

        [Column("fy_cd")]
        public string? FyCd { get; set; }

        [Column("company_id")]
        public string? CompanyId { get; set; }
    }


    //[Table("gl_post_details", Schema = "public")]
    //public class GlPostDetail
    //{
    //    [Column("acct_id")]
    //    public string? AcctId { get; set; }

    //    [Column("org_id")]
    //    public string? OrgId { get; set; }

    //    [Column("proj_id")]
    //    public string? ProjId { get; set; }

    //    [Column("fy_cd")]
    //    public string? FyCd { get; set; }

    //    [Column("pd_no")]
    //    public int? PdNo { get; set; }

    //    [Column("sub_pd_no")]
    //    public int? SubPdNo { get; set; }

    //    [Column("company_id")]
    //    public string? CompanyId { get; set; }

    //    [Column("id")]
    //    public string? Id { get; set; }

    //    [Column("s_id_type")]
    //    public string? SIdType { get; set; }

    //    [Column("amt1")]
    //    public decimal? Amt1 { get; set; }

    //    [Column("hrs1")]
    //    public decimal? Hrs1 { get; set; }

    //    [Column("name")]
    //    public string? Name { get; set; }

    //    [Column("bill_lab_cat_cd")]
    //    public string? BillLabCatCd { get; set; }

    //    [Column("effect_bill_dt")]
    //    public DateTime? EffectBillDt { get; set; }
    //}


    //public class DbInfoViewModel
    //{
    //    public List<TableInfoViewModel> Tables { get; set; } = new();
    //}

    //public class TableInfoViewModel
    //{
    //    public string TableName { get; set; } = "";
    //    public int RowCount { get; set; }
    //    public List<ColumnInfoViewModel> Columns { get; set; } = new();
    //}

    //public class ColumnInfoViewModel
    //{
    //    public string ColumnName { get; set; } = "";
    //    public string ColumnType { get; set; } = "";
    //    public bool IsNullable { get; set; }
    //    public string? DefaultValue { get; set; }
    //}

    public class DbInfoViewModel
    {
        public List<TableInfoViewModel> Tables { get; set; } = new();
    }

    public class TableInfoViewModel
    {
        public string TableName { get; set; } = "";
        public string? Schema { get; set; }
        public int RowCount { get; set; }

        public List<ColumnInfoViewModel> Columns { get; set; } = new();
        public List<string> PrimaryKeys { get; set; } = new();
        public List<ForeignKeyInfoViewModel> ForeignKeys { get; set; } = new();
        public List<IndexInfoViewModel> Indexes { get; set; } = new();
    }

    public class ColumnInfoViewModel
    {
        public string ColumnName { get; set; } = "";
        public string ColumnType { get; set; } = "";
        public bool IsNullable { get; set; }
        public string? DefaultValue { get; set; }
        public bool IsPrimaryKey { get; set; }
        public string? Description { get; set; }
        public string? Comment { get; set; }
    }

    public class ForeignKeyInfoViewModel
    {
        public string Name { get; set; } = "";
        public List<string> Columns { get; set; } = new();
        public string PrincipalTable { get; set; } = "";
        public List<string> PrincipalColumns { get; set; } = new();
    }

    public class IndexInfoViewModel
    {
        public string Name { get; set; } = "";
        public List<string> Columns { get; set; } = new();
        public bool IsUnique { get; set; }
    }
    public class ParametricView
    {
        [Column("proj_id")]
        public string? ProjId { get; set; }

        [Column("acct_id")]
        public string? AcctId { get; set; }

        [Column("org_id")]
        public string? OrgId { get; set; }

        [Column("fy_cd")]
        public string? FyCd { get; set; }

        [Column("pd_no")]
        public int? PdNo { get; set; }

        [Column("sub_pd_no")]
        public int? SubPdNo { get; set; }

        [Column("pool_no")]
        public string? PoolNo { get; set; }

        [Column("pool_name")]
        public string? PoolName { get; set; }

        [Column("sub_tot_type_no")]
        public int? SubTotTypeNo { get; set; }

        [Column("rate_type")]
        public string? RateType { get; set; }

        [Column("py_incur_amt")]
        public decimal? PyIncurAmt { get; set; }

        [Column("sub_incur_amt")]
        public decimal? SubIncurAmt { get; set; }

        [Column("ptd_incur_amt")]
        public decimal? PtdIncurAmt { get; set; }

        [Column("ytd_incur_amt")]
        public decimal? YtdIncurAmt { get; set; }

        [Column("cur_burd_rt")]
        public decimal? CurBurdRt { get; set; }

        [Column("ytd_burd_rt")]
        public decimal? YtdBurdRt { get; set; }

        [Column("proj_type_dc")]
        public string? ProjTypeDc { get; set; }

        [Column("proj_name")]
        public string? ProjName { get; set; }

        [Column("active_fl")]
        public string? ActiveFl { get; set; }

        [Column("account_name")]
        public string? AccountName { get; set; }

        [Column("cost_type")]
        public string? CostType { get; set; }

        [Column("account_type")]
        public string? AccountType { get; set; }

        [Column("budget_sheet")]
        public string? BudgetSheet { get; set; }

        [Column("fs_major_desc")]
        public string? FsMajorDesc { get; set; }

        [Column("fs_grp_desc")]
        public string? FsGrpDesc { get; set; }

        [Column("fs_ln_desc")]
        public string? FsLnDesc { get; set; }
    }

    [Table("is_rpt")]
    public class IsRpt
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("acct_id")]
        [StringLength(50)]
        public string? AcctId { get; set; }

        [Column("descp")]
        [StringLength(100)]
        public string? Descp { get; set; }

        [Column("fy_cd")]
        public int? FyCd { get; set; }

        [Column("pd_no")]
        public int? PdNo { get; set; }

        [Column("amt", TypeName = "numeric(18,2)")]
        public decimal? Amt { get; set; }

        [Column("created_ts")]
        public DateTime? CreatedTs { get; set; }
    }

    // DTO for POST request
    public class ProjIdsRequest
    {
        public List<string> ProjIds { get; set; } = new List<string>();
        public int? Take { get; set; }
    }
}
