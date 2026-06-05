using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PlanningAPI.Helpers;
using WebApi.DTO;
using YourNamespace.Models;

namespace PlanningAPI.Models;

public partial class MydatabaseContext : DbContext
{
    public MydatabaseContext(DbContextOptions<MydatabaseContext> options)
        : base(options)
    {
    }

    public DbSet<ProjVendorEmployeeLabcat> ProjVendorEmployeeLabcats { get; set; }
    public DbSet<ProjEmployeeLabcat> ProjEmployeeLabcats { get; set; }
    public DbSet<pl_EmployeeBurdenCalculated> EmployeeBurdenCalculated { get; set; }
    public DbSet<ProjectFee> ProjectFees { get; set; }
    public DbSet<BurdenTemplate> BurdenTemplates { get; set; }
    public DbSet<BurdenRate> BurdenRates { get; set; }
    public virtual DbSet<Holidaycalender> Holidaycalenders { get; set; }

    public virtual DbSet<PlConfigValue> PlConfigValues { get; set; }

    public virtual DbSet<PlEmployee> PlEmployees { get; set; }

    public virtual DbSet<PlEmployeeProjectMapping> PlEmployeeProjectMappings { get; set; }

    public virtual DbSet<PlForecast> PlForecasts { get; set; }

    //public virtual DbSet<PlOrgnization> PlOrgnizations { get; set; }

    public virtual DbSet<PlProject> PlProjects { get; set; }

    public virtual DbSet<PlProjectPlan> PlProjectPlans { get; set; }

    public virtual DbSet<PlcCode> PlcCodes { get; set; }

    public DbSet<Pools> AccountGroups { get; set; }
    public DbSet<OrgAccount> OrgAccounts { get; set; }
    //public DbSet<BurdenTemplate> PlBurdenTemplates { get; set; }
    public DbSet<PlPoolRate> PlPoolRates { get; set; }
    public DbSet<ProjectPlcRate> ProjectPlcRates { get; set; }
    public DbSet<PlOrgAcctPoolMapping> PlOrgAcctPoolMappings { get; set; }
    public DbSet<PlTemplatePoolRate> PlTemplatePoolRates { get; set; }
    public DbSet<PlTemplatePoolMapping> PlTemplatePoolMappings { get; set; }
    public DbSet<PlDct> PlDcts { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Organization> Organizations { get; set; }
    public DbSet<PlCeilHrCat> PlCeilHrCats { get; set; }
    public DbSet<PlCeilHrEmpl> PlCeilHrEmpls { get; set; }
    public DbSet<PlCeilDirCst> PlCeilDirCsts { get; set; }
    public DbSet<PlCeilBurden> PlCeilBurdens { get; set; }
    public DbSet<PlCeilProjTotal> PlCeilProjTotals { get; set; }
    public DbSet<AccountGroupSetup> AccountGroupSetup { get; set; }

    public DbSet<ProspectiveEntity> ProspectiveEntities { get; set; }
    public DbSet<EmployeeDetails> EmployeeDetails { get; set; }
    public DbSet<VendorDetails> VendorDetails { get; set; }
    public DbSet<VendorEmployeeDetails> VendorEmployeeDetails { get; set; }
    public DbSet<PLCDetails> PLCDetails { get; set; }
    public DbSet<GenericStaffDetails> GenericStaffDetails { get; set; }
    public DbSet<NewBusinessBudget> NewBusinessBudgets { get; set; }
    public DbSet<LabHours> LabHours { get; set; }
    public DbSet<PSRFinalData> PSRFinalData { get; set; }
    public DbSet<PsrHeader> PsrHeader { get; set; }
    public DbSet<EmployeeMaster> EmployeeMasters { get; set; }
    public DbSet<PlFinancialTransaction> PlFinancialTransactions { get; set; }

    public DbSet<ProjectWithPlanDto> ProjectWithPlanDto { get; set; }
    public DbSet<EmployeeDTOs> EmployeeDTOs { get; set; }
    public DbSet<VendorEmployeeDTOs> VendorEmployeeDTOs { get; set; }
    public DbSet<PlWarning> PLWarnings { get; set; }



    public DbSet<Empl_Master> Empl_Master { get; set; }
    public DbSet<Empl_Master_Dto> Empl_Master_Dto { get; set; }

    public DbSet<PlEmployeee> PlEmployeees { get; set; }
    public DbSet<ProjBgtRevSetup> ProjBgtRevSetups { get; set; }
    public DbSet<ProjEmplRt> ProjEmplRts { get; set; }
    public DbSet<ProjVendRt> ProjVendRts { get; set; }
    public DbSet<RevFormula> RevFormulas { get; set; }
    public DbSet<ProjRevWrkPd> ProjRevWrkPds { get; set; }
    public DbSet<VendorEmployee> VendorEmployees { get; set; }
    public DbSet<MonthlyCostRevenue> MonthlyCostRevenue { get; set; }
    public DbSet<ProjForecastSummary> ProjForecastSummary { get; set; }
    public DbSet<Charts_Of_Accounts> Charts_Of_Accounts { get; set; }
    public DbSet<Schedule> Schedules { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<PoolBaseAccount> PoolBaseAccounts { get; set; }
    public DbSet<PoolCostAccount> PoolCostAccounts { get; set; }
    public DbSet<PsrPoolCostSummary> PsrPoolCostSummaries { get; set; }
    public DbSet<ViewPsrData> ViewPsrData { get; set; }
    public DbSet<PoolRatesCostpoint> PoolRatesCostpoint { get; set; }
    public DbSet<ForecastView> ForecastView { get; set; }
    public DbSet<ChartOfAccount> ChartOfAccounts { get; set; } = null!;
    public DbSet<AnalgsRt> AnalgsRts { get; set; } = null!;

    public DbSet<PoolAllocationRawDto> PoolAllocationRaw { get; set; }
    public DbSet<UserProjectMap> UserProjectMaps { get; set; }
    public DbSet<OrgGroupUserMapping> OrgGroupUserMappings { get; set; }
    public DbSet<OrgGroup> OrgGroup { get; set; }

    public DbSet<OrgGroupOrgMapping> OrgGroupOrgMappings { get; set; }

    public DbSet<Fs> Fs { get; set; }
    public DbSet<FsLn> FsLns { get; set; }
    public DbSet<FsLnAcct> FsLnAccts { get; set; }
    public DbSet<View_Is_Report> View_Is_Report { get; set; }
    public DbSet<GlPostDetail> GlPostDetails { get; set; }
    public DbSet<ProjectModification> ProjectModifications { get; set; }
    public DbSet<ProjRevDefinition> ProjRevDefinitions { get; set; }
    public DbSet<ProjBillHs> ProjBillHs { get; set; }
    //public DbSet<UserAccess> UsersAccess => Set<UserAccess>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RoleScreenPermission> RoleScreenPermissions => Set<RoleScreenPermission>();
    public DbSet<RoleFieldPermission> RoleFieldPermissions => Set<RoleFieldPermission>();
    public DbSet<UserScreenPermission> UserScreenPermissions => Set<UserScreenPermission>();
    public DbSet<UserFieldPermission> UserFieldPermissions => Set<UserFieldPermission>();
    public DbSet<VersionCode> VersionCodes { get; set; }
    public DbSet<LaborProjectAccount> LaborProjectAccount { get; set; }
    public DbSet<NbiPrmtrcRt> NbiPrmtrcRts { get; set; }
    public DbSet<ParametricView> ParametricViews { get; set; } = null!;
    public DbSet<IsRpt> IsRpt { get; set; }
    public DbSet<TrndBsRpt> TrndBsRpts { get; set; }
    public DbSet<ForecastReportData> ForecastReportDatas { get; set; } = null!;

    //public DbSet<UserOrgMapping> UserOrgMappings { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.Entity<NbiPrmtrcRt>()
            .HasKey(p => p.ParaId); // Primary key

        modelBuilder.Entity<ParametricView>(entity =>
        {
            entity.HasNoKey(); // Views don't have primary keys
            entity.ToView("nbi_parametric_view"); // Database view name
        });

        modelBuilder.Entity<ForecastReportData>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("unanet_vw_psr_combined_data");
        });

        modelBuilder.Entity<TrndBsRpt>(entity =>
        {
            entity.ToTable("trnd_bs_rpt");
            entity.HasNoKey();
        });

        modelBuilder
         .Entity<View_Is_Report>()
         .HasNoKey()
         .ToView("view_is_report");

        modelBuilder
         .Entity<LaborProjectAccount>()
         .HasNoKey()
         .ToView("labor_project_accounts");

        modelBuilder.Entity<VersionCode>(entity =>
        {
            entity.HasKey(e => e.Id)
                  .HasName("version_code_pkey");

            entity.HasIndex(e => e.VersionCodeValue)
                  .IsUnique()
                  .HasDatabaseName("version_code_key");

            entity.Property(e => e.Id)
                  .HasColumnName("id")
                  .HasDefaultValueSql("nextval('roles_role_id_seq'::regclass)");

            entity.Property(e => e.VersionCodeValue)
                  .HasColumnName("version_code")
                  .HasMaxLength(50)
                  .IsRequired();

            entity.Property(e => e.CreatedAt)
                  .HasColumnName("created_at")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });


        // -----------------------------
        // ROLE
        // -----------------------------
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("roles");

            entity.HasKey(e => e.RoleId);

            entity.Property(e => e.RoleId)
                .HasColumnName("role_id");

            entity.Property(e => e.RoleName)
                .HasColumnName("role_name")
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(e => e.RoleName)
                .IsUnique()
                .HasDatabaseName("ux_roles_role_name");
        });

        // -----------------------------
        // USER
        // -----------------------------
        //modelBuilder.Entity<UserAccess>(entity =>
        //{
        //    entity.ToTable("user_access");

        //    entity.HasKey(e => e.UserId);

        //    entity.Property(e => e.UserId)
        //        .HasColumnName("user_id");

        //    entity.Property(e => e.Username)
        //        .HasColumnName("username")
        //        .HasMaxLength(100)
        //        .IsRequired();

        //    entity.Property(e => e.IsActive)
        //        .HasColumnName("is_active")
        //        .HasDefaultValue(true);

        //    entity.Property(e => e.RoleId)
        //        .HasColumnName("role_id");

        //    //entity.HasOne(e => e.Role)
        //    //    .WithMany(r => r.Users)
        //    //    .HasForeignKey(e => e.RoleId)
        //    //    .OnDelete(DeleteBehavior.Restrict);

        //    // ⭐⭐⭐ ADD THESE TWO BLOCKS ⭐⭐⭐

        //    //entity.HasMany(e => e.ScreenOverrides)
        //    //    .WithOne(e => e.User)
        //    //    .HasForeignKey(e => e.UserId)
        //    //    .HasPrincipalKey(e => e.UserId)
        //    //    .OnDelete(DeleteBehavior.Cascade);

        //    //entity.HasMany(e => e.FieldOverrides)
        //    //    .WithOne(e => e.User)
        //    //    .HasForeignKey(e => e.UserId)
        //    //    .HasPrincipalKey(e => e.UserId)
        //    //    .OnDelete(DeleteBehavior.Cascade);
        //});
        // -----------------------------
        // ROLE → SCREEN PERMISSIONS
        // -----------------------------
        modelBuilder.Entity<RoleScreenPermission>(entity =>
        {
            entity.ToTable("role_screen_permissions");

            entity.HasKey(e => new { e.RoleId, e.ScreenCode });

            entity.Property(e => e.RoleId)
                .HasColumnName("role_id");

            entity.Property(e => e.ScreenCode)
                .HasColumnName("screen_code")
                .HasMaxLength(100);

            entity.Property(e => e.CanView)
                .HasColumnName("can_view")
                .HasDefaultValue(false);

            entity.Property(e => e.CanEdit)
                .HasColumnName("can_edit")
                .HasDefaultValue(false);

            entity.HasOne(e => e.Role)
                .WithMany(r => r.ScreenPermissions)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // -----------------------------
        // ROLE → FIELD PERMISSIONS
        // -----------------------------
        modelBuilder.Entity<RoleFieldPermission>(entity =>
        {
            entity.ToTable("role_field_permissions");

            entity.HasKey(e => new { e.RoleId, e.FieldCode });

            entity.Property(e => e.RoleId)
                .HasColumnName("role_id");

            entity.Property(e => e.FieldCode)
                .HasColumnName("field_code")
                .HasMaxLength(100);

            entity.Property(e => e.CanView)
                .HasColumnName("can_view")
                .HasDefaultValue(false);

            entity.Property(e => e.CanEdit)
                .HasColumnName("can_edit")
                .HasDefaultValue(false);

            entity.HasOne(e => e.Role)
                .WithMany(r => r.FieldPermissions)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // -----------------------------
        // USER → SCREEN OVERRIDES
        // -----------------------------
        modelBuilder.Entity<UserScreenPermission>(entity =>
        {
            entity.ToTable("user_screen_permissions");

            entity.HasKey(e => new { e.UserId, e.ScreenCode });

            entity.Property(e => e.UserId)
                .HasColumnName("user_id");

            entity.Property(e => e.ScreenCode)
                .HasColumnName("screen_code")
                .HasMaxLength(100);

            entity.Property(e => e.CanView)
                .HasColumnName("can_view");

            entity.Property(e => e.CanEdit)
                .HasColumnName("can_edit");

            entity.HasOne(e => e.User)
                .WithMany(u => u.ScreenOverrides)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // -----------------------------
        // USER → FIELD OVERRIDES
        // -----------------------------
        modelBuilder.Entity<UserFieldPermission>(entity =>
        {
            entity.ToTable("user_field_permissions");

            entity.HasKey(e => new { e.UserId, e.FieldCode });

            entity.Property(e => e.UserId)
                .HasColumnName("user_id");

            entity.Property(e => e.FieldCode)
                .HasColumnName("field_code")
                .HasMaxLength(100);

            entity.Property(e => e.CanView)
                .HasColumnName("can_view");

            entity.Property(e => e.CanEdit)
                .HasColumnName("can_edit");

            entity.HasOne(e => e.User)
                .WithMany(u => u.FieldOverrides)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProjBillHs>(entity =>
        {
            // Table
            entity.ToTable("proj_bill_hs");

            // Primary Key (composite example)
            entity.HasKey(e => new { e.ProjId, e.FyCd, e.PdNo, e.SubPdNo });

            // Columns mapping
            entity.Property(e => e.ProjId)
                  .HasColumnName("proj_id")
                  .HasMaxLength(50);

            entity.Property(e => e.FyCd)
                  .HasColumnName("fy_cd")
                  .HasMaxLength(10);

            entity.Property(e => e.PdNo)
                  .HasColumnName("pd_no");

            entity.Property(e => e.SubPdNo)
                  .HasColumnName("sub_pd_no");

            entity.Property(e => e.BilledAmt)
                  .HasColumnName("billed_amt")
                  .HasColumnType("numeric(18,2)");

            entity.Property(e => e.RtngeAmt)
                  .HasColumnName("rtnge_amt")
                  .HasColumnType("numeric(18,2)");

            entity.Property(e => e.ModifiedBy)
                  .HasColumnName("modified_by")
                  .HasMaxLength(50);

            entity.Property(e => e.TimeStamp)
                  .HasColumnName("time_stamp")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.DelAmt)
                  .HasColumnName("del_amt")
                  .HasColumnType("numeric(18,2)");

            entity.Property(e => e.RowVersion)
                  .HasColumnName("rowversion")
                  .IsConcurrencyToken();

            entity.Property(e => e.BillWhAmt)
                  .HasColumnName("bill_wh_amt")
                  .HasColumnType("numeric(18,2)");

            entity.Property(e => e.BillWhRelAmt)
                  .HasColumnName("bill_wh_rel_amt")
                  .HasColumnType("numeric(18,2)");

            entity.Property(e => e.MuBilledAmt)
                  .HasColumnName("mu_billed_amt")
                  .HasColumnType("numeric(18,2)");

            entity.Property(e => e.MuRtngeAmt)
                  .HasColumnName("mu_rtnge_amt")
                  .HasColumnType("numeric(18,2)");

            entity.Property(e => e.MuBillWhAmt)
                  .HasColumnName("mu_bill_wh_amt")
                  .HasColumnType("numeric(18,2)");

            entity.Property(e => e.MuBillWhRelAmt)
                  .HasColumnName("mu_bill_wh_rel_amt")
                  .HasColumnType("numeric(18,2)");
        });

        modelBuilder.Entity<ProjectRevenueAdjustment>(entity =>
        {
            entity.HasNoKey();
            entity.ToTable("project_revenue_adjustments");

            entity.Property(e => e.ProjId).HasColumnName("proj_id");
            entity.Property(e => e.FyCd).HasColumnName("fy_cd");
            entity.Property(e => e.PdNo).HasColumnName("pd_no");
            entity.Property(e => e.SubPdNo).HasColumnName("sub_pd_no");
            entity.Property(e => e.RevAdjAmt).HasColumnName("rev_adj_amt");
            entity.Property(e => e.RevAdjDesc).HasColumnName("rev_adj_desc");
            entity.Property(e => e.ModifiedBy).HasColumnName("modified_by");
            entity.Property(e => e.TimeStamp).HasColumnName("time_stamp");
            entity.Property(e => e.RevAwdAdjAmt).HasColumnName("rev_awd_adj_amt");
        });

        modelBuilder.Entity<ProjectModification>(entity =>
        {
            entity.ToTable("project_modifications");
            entity.HasNoKey(); // REQUIRED
        });

        modelBuilder.Entity<ProjRevDefinition>(entity =>
        {
            entity.ToTable("proj_rev_definition");

            // If table has NO primary key
            entity.HasNoKey();

            // Optional: indexes for performance
            entity.HasIndex(e => e.ProjectId);
            entity.HasIndex(e => e.CompanyId);
        });

        modelBuilder.Entity<FsLnAcct>(entity =>
        {
            entity.ToTable("fs_ln_acct", "public");

            entity.HasKey(e => new { e.FsCd, e.FsLnKey, e.AcctId });

            entity.Property(e => e.FsCd)
                  .HasMaxLength(6)
                  .IsRequired();

            entity.Property(e => e.AcctId)
                  .HasMaxLength(15)
                  .IsRequired();

            entity.Property(e => e.ModifiedBy)
                  .HasMaxLength(20)
                  .IsRequired();

            entity.Property(e => e.CompanyId)
                  .HasMaxLength(10)
                  .IsRequired();

            entity.Property(e => e.SCfAcctTypCd)
                  .HasMaxLength(6);

            entity.Property(e => e.SCfActvtyCd)
                  .HasMaxLength(1);
        });

        modelBuilder.Entity<FsLn>(entity =>
        {
            entity.ToTable("fs_ln", "public");

            entity.HasKey(e => e.FsLnKey);

            entity.Property(e => e.FsCd)
                  .HasMaxLength(6)
                  .IsRequired();

            entity.Property(e => e.FsMajorDesc)
                  .HasMaxLength(30)
                  .IsRequired();

            entity.Property(e => e.FsGrpDesc)
                  .HasMaxLength(30)
                  .IsRequired();

            entity.Property(e => e.FsLnDesc)
                  .HasMaxLength(30)
                  .IsRequired();

            entity.Property(e => e.FsRvrsSgnFl)
                  .HasMaxLength(1)
                  .IsRequired();

            entity.Property(e => e.ModifiedBy)
                  .HasMaxLength(20)
                  .IsRequired();

            entity.Property(e => e.CompanyId)
                  .HasMaxLength(10)
                  .IsRequired();

            entity.Property(e => e.RevFl)
                  .HasMaxLength(1)
                  .IsRequired();

            entity.Property(e => e.FsReclCrAcctId)
                  .HasMaxLength(15);

            entity.Property(e => e.FsReclDrAcctId)
                  .HasMaxLength(15);

            entity.Property(e => e.CfSrceFsCd)
                  .HasMaxLength(6);

            entity.Property(e => e.SCfActvtyCd)
                  .HasMaxLength(1);

            entity.Property(e => e.SCfAcctTypCd)
                  .HasMaxLength(6);

            entity.Property(e => e.AssignLvlCd)
                  .HasMaxLength(1);

            entity.Property(e => e.ElimAcctId)
                  .HasMaxLength(15);

            entity.Property(e => e.FsCiociAmtFl)
                  .HasMaxLength(1);
        });

    modelBuilder.Entity<Fs>(entity =>
        {
            entity.ToTable("fs", "public");

            entity.HasKey(e => e.FsCd);

            entity.Property(e => e.FsCd)
                  .HasMaxLength(6)
                  .IsRequired();

            entity.Property(e => e.PrimFsFl)
                  .HasMaxLength(1)
                  .IsRequired();

            entity.Property(e => e.SFsType)
                  .HasMaxLength(1)
                  .IsRequired();

            entity.Property(e => e.FsDesc)
                  .HasMaxLength(30)
                  .IsRequired();

            entity.Property(e => e.ModifiedBy)
                  .HasMaxLength(20)
                  .IsRequired();

            entity.Property(e => e.TimeStamp)
                  .IsRequired();

            entity.Property(e => e.CompanyId)
                  .HasMaxLength(10)
                  .IsRequired();

            entity.Property(e => e.RowVersion);

            entity.Property(e => e.FsIsociAmtFl)
                  .HasMaxLength(1);

            entity.Property(e => e.IncstmtCd)
                  .HasMaxLength(6);
        });

        modelBuilder.Entity<OrgGroupOrgMapping>(entity =>
        {
            entity.ToTable("org_group_org_mapping");

            entity.HasKey(e => new { e.OrgGroupId, e.OrgId });

            entity.Property(e => e.OrgGroupId)
                .HasColumnName("org_group_id");

            entity.Property(e => e.OrgId)
                .HasColumnName("org_id")
                .IsRequired()
                .HasMaxLength(50);

            entity.HasOne(e => e.OrgGroup)
                .WithMany(g => g.OrgMappings)
                .HasForeignKey(e => e.OrgGroupId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Organization)
                .WithMany(o => o.OrgGroupMappings)
                .HasForeignKey(e => e.OrgId)
                .HasPrincipalKey(o => o.OrgId)
                .OnDelete(DeleteBehavior.Restrict);
        });


        modelBuilder.Entity<OrgGroup>(entity =>
        {
            entity.ToTable("org_groups");

            entity.HasKey(e => e.OrgGroupId);

            entity.Property(e => e.OrgGroupId)
                .HasColumnName("org_group_id");

            entity.Property(e => e.OrgGroupCode)
                .HasColumnName("org_group_code")
                .HasMaxLength(50);

            entity.Property(e => e.OrgGroupName)
                .HasColumnName("org_group_name")
                .HasMaxLength(150);

            entity.Property(e => e.Description)
                .HasColumnName("description");

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at");

            entity.Property(e => e.CreatedBy)
                .HasColumnName("created_by");

            entity.Property(e => e.ModifiedAt)
                .HasColumnName("modified_at");

            entity.Property(e => e.ModifiedBy)
                .HasColumnName("modified_by");
        });



        modelBuilder.Entity<PoolAllocationRawDto>()
                .HasNoKey()
                .ToView(null);

        modelBuilder.Entity<OrgGroupUserMapping>(entity =>
        {
            entity.ToTable("org_group_user_mapping");

            entity.HasKey(e => new { e.OrgGroupId, e.UserId });

            entity.Property(e => e.OrgGroupId)
                .HasColumnName("org_group_id");

            entity.Property(e => e.UserId)
                .HasColumnName("user_id");

            entity.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            entity.Property(e => e.AssignedAt)
                .HasColumnName("assigned_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.AssignedBy)
                .HasColumnName("assigned_by")
                .HasMaxLength(50);

            entity.HasOne(e => e.OrgGroup)
                .WithMany(g => g.UserMappings)
                .HasForeignKey(e => e.OrgGroupId);

            entity.HasOne(e => e.User)
                .WithMany(u => u.OrgGroupMappings)
                .HasForeignKey(e => e.UserId);
        });


        // USER ↔ PROJECT MAPPING
        modelBuilder.Entity<UserProjectMap>(entity =>
        {
            entity.ToTable("user_project_map");

            // Composite PK
            entity.HasKey(e => new { e.UserId, e.ProjId });

            entity.Property(e => e.UserId)
                  .HasColumnName("user_id");

            entity.Property(e => e.ProjId)
                  .HasColumnName("proj_id")
                  .HasMaxLength(30);

            entity.Property(e => e.AssignedAt)
                  .HasColumnName("assigned_at")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // FK → Users
            entity.HasOne(e => e.User)
                  .WithMany(u => u.UserProjects)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // FK → Projects
            entity.HasOne(e => e.Project)
                  .WithMany(p => p.UserProjects)
                  .HasForeignKey(e => e.ProjId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AnalgsRt>(entity =>
        {
            entity.ToTable("analgs_rt");

            entity.HasKey(e => e.AnalgId);

            entity.Property(e => e.AnalgId)
                  .HasColumnName("analg_id");

            entity.Property(e => e.ClsPd)
                  .HasColumnName("cls_pd")
                  .IsRequired();

            entity.Property(e => e.ActualAmt)
                  .HasColumnName("actual_amt")
                  .HasDefaultValue(false);

            entity.Property(e => e.TimeStamp)
                  .HasColumnName("time_stamp")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<ChartOfAccount>(entity =>
        {
            entity.ToTable("chart_of_accounts");

            entity.HasKey(e => e.AccountId);

            entity.Property(e => e.AccountId)
                .HasColumnName("account_id")
                .HasMaxLength(50);

            entity.Property(e => e.AccountName)
                .HasColumnName("account_name")
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(e => e.CostType)
                .HasColumnName("cost_type")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.AccountType)
                .HasColumnName("account_type")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.BudgetSheet)
                .HasColumnName("budget_sheet")
                .HasMaxLength(50);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });



        modelBuilder.Entity<ForecastView>(entity =>
            {
                entity.HasNoKey();
                entity.ToView("forecast_view");

                entity.Property(e => e.ForecastedAmt).HasColumnName("forecastedamt");
                entity.Property(e => e.ForecastId).HasColumnName("forecastid");
                entity.Property(e => e.ProjId).HasColumnName("proj_id");
                entity.Property(e => e.PlId).HasColumnName("pl_id");
                entity.Property(e => e.EmplId).HasColumnName("empl_id");
                entity.Property(e => e.Month).HasColumnName("month");
                entity.Property(e => e.Year).HasColumnName("year");
                entity.Property(e => e.ForecastedHours).HasColumnName("forecastedhours");
                entity.Property(e => e.CreatedAt).HasColumnName("createdat");
                entity.Property(e => e.UpdatedAt).HasColumnName("updatedat");
                entity.Property(e => e.DctId).HasColumnName("dct_id");
                entity.Property(e => e.AcctId).HasColumnName("acct_id");
                entity.Property(e => e.OrgId).HasColumnName("org_id");
                entity.Property(e => e.PlcRateId).HasColumnName("plc_rate_id");
                entity.Property(e => e.HrlyRate).HasColumnName("hrly_rate");
                entity.Property(e => e.EffectDt).HasColumnName("effect_dt");
                entity.Property(e => e.Plc).HasColumnName("plc");
                entity.Property(e => e.Burden).HasColumnName("burden");
                entity.Property(e => e.Cost).HasColumnName("cost");
                entity.Property(e => e.Revenue).HasColumnName("revenue");
                entity.Property(e => e.ActualAmt).HasColumnName("actualamt");
                entity.Property(e => e.ActualHours).HasColumnName("actualhours");
                entity.Property(e => e.ForecastedCost).HasColumnName("forecastedcost");
                entity.Property(e => e.Fringe).HasColumnName("fringe");
                entity.Property(e => e.Overhead).HasColumnName("overhead");
                entity.Property(e => e.Gna).HasColumnName("gna");
                entity.Property(e => e.Mnh).HasColumnName("mnh");
                entity.Property(e => e.Fees).HasColumnName("fees");

                entity.Property(e => e.PlType).HasColumnName("pl_type");
                entity.Property(e => e.Version).HasColumnName("version");
                entity.Property(e => e.VersionCode).HasColumnName("version_code");
                entity.Property(e => e.FinalVersion).HasColumnName("final_version");
                entity.Property(e => e.IsCompleted).HasColumnName("is_completed");
                entity.Property(e => e.IsApproved).HasColumnName("is_approved");
                entity.Property(e => e.Status).HasColumnName("status");
                entity.Property(e => e.ClosedPeriod).HasColumnName("closed_period");

                entity.Property(e => e.PCreatedAt).HasColumnName("created_at");
                entity.Property(e => e.PUpdatedAt).HasColumnName("updated_at");

                entity.Property(e => e.ModifiedBy).HasColumnName("modified_by");
                entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
                entity.Property(e => e.CreatedBy).HasColumnName("created_by");
                entity.Property(e => e.Source).HasColumnName("source");
                entity.Property(e => e.Type).HasColumnName("type");
                entity.Property(e => e.BurdenTemplateId).HasColumnName("burden_template_id");
                entity.Property(e => e.BudEacYear).HasColumnName("bud_eac_year");
            });


        modelBuilder.Entity<PoolRatesCostpoint>(entity =>
        {
            entity.ToTable("poolrates_costpoint");

            // No primary key (if this table does not have one)
            entity.HasKey(x => new { x.FyCd, x.PdNo, x.PoolNo, x.OrgId, x.AcctId });
            entity.Property(e => e.FyCd).HasColumnName("fy_cd");
            entity.Property(e => e.PdNo).HasColumnName("pd_no");
            entity.Property(e => e.AllocGrpNo).HasColumnName("alloc_grp_no");
            entity.Property(e => e.PoolNo).HasColumnName("pool_no");
            entity.Property(e => e.ProcSeqNo).HasColumnName("proc_seq_no");
            entity.Property(e => e.SAcctTypeCd).HasColumnName("s_acct_type_cd");
            entity.Property(e => e.AcctId).HasColumnName("acct_id");
            entity.Property(e => e.OrgId).HasColumnName("org_id");
            entity.Property(e => e.CurAmt).HasColumnName("cur_amt");
            entity.Property(e => e.YtdAmt).HasColumnName("ytd_amt");
            entity.Property(e => e.CurAllocAmt).HasColumnName("cur_alloc_amt");
            entity.Property(e => e.YtdAllocAmt).HasColumnName("ytd_alloc_amt");
            entity.Property(e => e.CurRt).HasColumnName("cur_rt");
            entity.Property(e => e.YtdRt).HasColumnName("ytd_rt");
            entity.Property(e => e.CurBudAmt).HasColumnName("cur_bud_amt");
            entity.Property(e => e.YtdBudAmt).HasColumnName("ytd_bud_amt");
            entity.Property(e => e.CurBudAllocAmt).HasColumnName("cur_bud_alloc_amt");
            entity.Property(e => e.YtdBudAllocAmt).HasColumnName("ytd_bud_alloc_amt");
            entity.Property(e => e.CurBudRt).HasColumnName("cur_bud_rt");
            entity.Property(e => e.YtdBudRt).HasColumnName("ytd_bud_rt");
            entity.Property(e => e.ModifiedBy).HasColumnName("modified_by");
            entity.Property(e => e.TimeStamp).HasColumnName("time_stamp");
            entity.Property(e => e.CurBaseAmt).HasColumnName("cur_base_amt");
            entity.Property(e => e.YtdBaseAmt).HasColumnName("ytd_base_amt");
            entity.Property(e => e.ProjId).HasColumnName("proj_id");
            entity.Property(e => e.Rowversion).HasColumnName("rowversion");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
        });
        modelBuilder.Entity<ViewPsrData>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("view_psr_data");

            entity.Property(e => e.ProjId).HasColumnName("proj_id");
            entity.Property(e => e.AcctId).HasColumnName("acct_id");
            entity.Property(e => e.OrgId).HasColumnName("org_id");
            entity.Property(e => e.FyCd).HasColumnName("fy_cd");
            entity.Property(e => e.PdNo).HasColumnName("pd_no");
            entity.Property(e => e.SubPdNo).HasColumnName("sub_pd_no");
            entity.Property(e => e.PoolName).HasColumnName("pool_name");
            entity.Property(e => e.SubTotTypeNo).HasColumnName("sub_tot_type_no");
            entity.Property(e => e.RateType).HasColumnName("rate_type");

            entity.Property(e => e.ProjName).HasColumnName("proj_name");
            entity.Property(e => e.ProjectOrgId).HasColumnName("project_org_id");
            entity.Property(e => e.ProjStartDt).HasColumnName("proj_start_dt");
            entity.Property(e => e.ProjEndDt).HasColumnName("proj_end_dt");

            entity.Property(e => e.L1AcctName).HasColumnName("l1_acct_name");

            entity.Property(e => e.PyIncurAmt).HasColumnName("py_incur_amt");
            entity.Property(e => e.SubIncurAmt).HasColumnName("sub_incur_amt");
            entity.Property(e => e.PtdIncurAmt).HasColumnName("ptd_incur_amt");
            entity.Property(e => e.YtdIncurAmt).HasColumnName("ytd_incur_amt");
        });


        //modelBuilder.Entity<PsrPoolCostSummary>(entity =>
        //{
        //    entity.HasNoKey();
        //    entity.ToView("psr_final_data");


        //    entity.Property(e => e.ProjId).HasColumnName("proj_id");
        //    entity.Property(e => e.AcctId).HasColumnName("acct_id");
        //    entity.Property(e => e.OrgId).HasColumnName("org_id");
        //    entity.Property(e => e.FyCd).HasColumnName("fy_cd");
        //    entity.Property(e => e.PdNo).HasColumnName("pd_no");
        //    entity.Property(e => e.SubPdNo).HasColumnName("sub_pd_no");
        //    entity.Property(e => e.PoolName).HasColumnName("pool_name");
        //    entity.Property(e => e.SubTotTypeNo).HasColumnName("sub_tot_type_no");
        //    entity.Property(e => e.RateType).HasColumnName("rate_type");

        //    entity.Property(e => e.ProjName).HasColumnName("proj_name");
        //    entity.Property(e => e.ProjectOrgId).HasColumnName("project_org_id");
        //    entity.Property(e => e.ProjStartDt).HasColumnName("proj_start_dt");
        //    entity.Property(e => e.ProjEndDt).HasColumnName("proj_end_dt");

        //    entity.Property(e => e.L1AcctName).HasColumnName("l1_acct_name");

        //    entity.Property(e => e.PyIncurAmt).HasColumnName("py_incur_amt");
        //    entity.Property(e => e.SubIncurAmt).HasColumnName("sub_incur_amt");
        //    entity.Property(e => e.PtdIncurAmt).HasColumnName("ptd_incur_amt");
        //    entity.Property(e => e.YtdIncurAmt).HasColumnName("ytd_incur_amt");
        //});

        modelBuilder.Entity<PlWarning>(entity =>
        {
            entity.HasKey(e => e.PlId);
            entity.HasIndex(e => new { e.PlId, e.ProjId, e.EmplId, e.Year, e.Month, e.MultipleProjects })
                  .IsUnique()
                  .HasDatabaseName("unique");
            entity.HasKey(e => new { e.PlId, e.ProjId, e.EmplId, e.Year, e.Month, e.MultipleProjects });
            entity.ToTable("pl_warnings");
        });

        modelBuilder.Entity<PoolBaseAccount>(entity =>
        {
            entity.ToTable("pool_base_account");

            entity.HasKey(e => new
            {
                e.AllocGrpNo,
                e.FyCd,
                e.PoolNo,
                e.OrgId,
                e.AcctId
            });

            entity.Property(e => e.AllocGrpNo).HasColumnName("alloc_grp_no");
            entity.Property(e => e.FyCd).HasColumnName("fy_cd");
            entity.Property(e => e.PoolNo).HasColumnName("pool_no");

            entity.Property(e => e.OrgId).HasColumnName("org_id").HasMaxLength(20);
            entity.Property(e => e.AcctId).HasColumnName("acct_id").HasMaxLength(20);

            entity.Property(e => e.AllocOrgId).HasColumnName("alloc_org_id").HasMaxLength(20);
            entity.Property(e => e.AllocAcctId).HasColumnName("alloc_acct_id").HasMaxLength(20);

            entity.Property(e => e.CompanyId).HasColumnName("company_id");
        });

        // =============================
        // POOL COST ACCOUNT
        // =============================
        modelBuilder.Entity<PoolCostAccount>(entity =>
        {
            entity.ToTable("pool_cost_account");

            entity.HasKey(e => new
            {
                e.AllocGrpNo,
                e.FyCd,
                e.PoolNo,
                e.OrgId,
                e.AcctId
            });

            entity.Property(e => e.AllocGrpNo).HasColumnName("alloc_grp_no");
            entity.Property(e => e.FyCd).HasColumnName("fy_cd");
            entity.Property(e => e.PoolNo).HasColumnName("pool_no");

            entity.Property(e => e.OrgId).HasColumnName("org_id").HasMaxLength(20);
            entity.Property(e => e.AcctId).HasColumnName("acct_id").HasMaxLength(20);

            //entity.Property(e => e.AllocOrgId).HasColumnName("alloc_org_id").HasMaxLength(20);
            //entity.Property(e => e.AllocAcctId).HasColumnName("alloc_acct_id").HasMaxLength(20);

            entity.Property(e => e.CompanyId).HasColumnName("company_id");
        });

        // USERS
        modelBuilder.Entity<User>().ToTable("users").HasKey(u => u.UserId);
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.UserId).HasColumnName("user_id");
            entity.Property(u => u.Username).HasColumnName("username");
            entity.Property(u => u.FullName).HasColumnName("full_name");
            entity.Property(u => u.Email).HasColumnName("email");
            entity.Property(u => u.IsActive).HasColumnName("is_active");
            entity.Property(u => u.PasswordHash).HasColumnName("password_hash");
            entity.Property(u => u.Role).HasColumnName("role");
            entity.Property(u => u.RoleId).HasColumnName("role_id");
            entity.Property(u => u.CreatedAt).HasColumnName("created_at");


            entity.HasOne(e => e.UserRole)
                .WithMany(r => r.Users)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        var dateOnlyConverter = new ValueConverter<DateOnly, DateTime>(
                d => d.ToDateTime(TimeOnly.MinValue),      // store as DateTime
                d => DateOnly.FromDateTime(d));

        modelBuilder.Entity<ProjVendorEmployeeLabcat>()
            .HasKey(p => new { p.ProjId, p.VendId, p.VendEmplId, p.BillLabCatCd });

        // Composite Primary Key
        modelBuilder.Entity<ProjEmployeeLabcat>()
                .HasKey(p => new { p.ProjId, p.EmplId, p.BillLabCatCd });
        modelBuilder.Entity<ProjForecastSummary>()
            .HasKey(p => new { p.ProjId, p.PlType, p.Version, p.Month, p.Year });

        modelBuilder.Entity<VendorEmployeeDTOs>().HasNoKey().ToView(null);
        //modelBuilder.Entity<PlWarning>().HasNoKey().ToView(null);


        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.ToTable("schedule", "public");  // maps to public.schedule

            // Composite Key
            entity.HasKey(s => new { s.Year, s.MonthNo });

            // Column mappings
            entity.Property(s => s.Year)
                  .HasColumnName("year")
                  .IsRequired();

            entity.Property(s => s.MonthNo)
                  .HasColumnName("month_no")
                  .IsRequired();

            entity.Property(s => s.WorkingHours)
                  .HasColumnName("working_hours")
                  .HasColumnType("numeric(10,2)"); // Adjust precision if needed

            entity.Property(s => s.WorkingDays)
                  .HasColumnName("working_days");

            // Ignore NotMapped property
            entity.Ignore(s => s.Month);
        });

        modelBuilder.Entity<MonthlyCostRevenue>(entity =>
        {
            entity.ToTable("monthlycostrevenue");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                  .HasColumnName("id")
                  .ValueGeneratedOnAdd();

            entity.Property(e => e.ForecastId).HasColumnName("forecastid").IsRequired();
            entity.Property(e => e.Year).HasColumnName("year").IsRequired();
            entity.Property(e => e.Month).HasColumnName("month").IsRequired();
            entity.Property(e => e.Empl_id).HasColumnName("empl_id").IsRequired();


            entity.Property(e => e.Cost).HasColumnName("cost").HasColumnType("decimal(18,2)").HasDefaultValue(0.00m);
            entity.Property(e => e.Burden).HasColumnName("burden").HasColumnType("decimal(18,2)").HasDefaultValue(0.00m);

            // EF doesn't map generated columns, but include the column to avoid mismatches
            entity.Property(e => e.BurdenedCost)
                  .HasColumnName("burdenedcost")
                  .HasColumnType("decimal(18,2)")
                  .ValueGeneratedOnAddOrUpdate()
                  .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

            entity.Property(e => e.Revenue).HasColumnName("revenue").HasColumnType("decimal(18,2)").HasDefaultValue(0.00m);

            entity.Property(e => e.CreatedAt).HasColumnName("createdat").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedat");

            entity.HasOne<PlForecast>()
                  .WithMany()
                  .HasForeignKey(e => e.ForecastId)
                  .HasConstraintName("fk_forecast")
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<VendorEmployee>(entity =>
        {
            entity.ToTable("vendor_employee");

            // Composite Primary Key
            entity.HasKey(e => new { e.VendEmplId, e.VendId });

            // Column Mappings
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.DfGenlLabCatCd).HasColumnName("df_genl_lab_cat_cd");
            entity.Property(e => e.ModifiedBy).HasColumnName("modified_by");
            entity.Property(e => e.Rowversion).HasColumnName("rowversion");
            entity.Property(e => e.TimeStamp).HasColumnName("time_stamp");
            entity.Property(e => e.VendEmplId).HasColumnName("vend_empl_id");
            entity.Property(e => e.VendEmplName).HasColumnName("vend_empl_name");
            entity.Property(e => e.VendId).HasColumnName("vend_id");
            entity.Property(e => e.DfBillLabCatCd).HasColumnName("df_bill_lab_cat_cd");
            entity.Property(e => e.LastName).HasColumnName("last_name");
            entity.Property(e => e.FirstName).HasColumnName("first_name");
            entity.Property(e => e.MidName).HasColumnName("mid_name");
            entity.Property(e => e.VendEmplStatus).HasColumnName("vend_empl_status");
            entity.Property(e => e.SubctrId).HasColumnName("subctr_id");
            entity.Property(e => e.TeEmplId).HasColumnName("te_empl_id");
            entity.Property(e => e.VendEmplAprvrId).HasColumnName("vend_empl_aprvr_id");
            entity.Property(e => e.VendEmplAprvlDt).HasColumnName("vend_empl_aprvl_dt");
            entity.Property(e => e.VendEmplAprvlCd).HasColumnName("vend_empl_aprvl_cd");
            entity.Property(e => e.IntEmail).HasColumnName("int_email");
            entity.Property(e => e.ExtEmail).HasColumnName("ext_email");
            entity.Property(e => e.IntPhone).HasColumnName("int_phone");
            entity.Property(e => e.ExtPhone).HasColumnName("ext_phone");
            entity.Property(e => e.CellPhone).HasColumnName("cell_phone");
            entity.Property(e => e.Cont1Name).HasColumnName("cont_1_name");
            entity.Property(e => e.Cont1Rel).HasColumnName("cont_1_rel");
            entity.Property(e => e.Cont1Phone1).HasColumnName("cont_1_phone_1");
            entity.Property(e => e.Cont1Phone2).HasColumnName("cont_1_phone_2");
            entity.Property(e => e.Cont1Phone3).HasColumnName("cont_1_phone_3");
            entity.Property(e => e.Cont2Name).HasColumnName("cont_2_name");
            entity.Property(e => e.Cont2Rel).HasColumnName("cont_2_rel");
            entity.Property(e => e.Cont2Phone1).HasColumnName("cont_2_phone_1");
            entity.Property(e => e.Cont2Phone2).HasColumnName("cont_2_phone_2");
            entity.Property(e => e.Cont2Phone3).HasColumnName("cont_2_phone_3");
            entity.Property(e => e.UsCitizenFl).HasColumnName("us_citizen_fl");
            entity.Property(e => e.ItarStatus).HasColumnName("itar_status");
        });

        modelBuilder.Entity<EmployeeDTOs>().HasNoKey().ToView(null);
        modelBuilder.Entity<ProjRevWrkPd>(entity =>
        {
            entity.ToTable("proj_rev_wrk_pds");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Pl_Id).HasColumnName("pl_id");

            entity.Property(e => e.ProjId).HasColumnName("proj_id").HasMaxLength(50);
            entity.Property(e => e.VersionNo).HasColumnName("version_no");
            entity.Property(e => e.BgtType).HasColumnName("bgt_type");
            entity.Property(e => e.Period).HasColumnName("period");
            entity.Property(e => e.EndDate).HasColumnName("end_date");

            entity.Property(e => e.RowVersion).HasColumnName("rowversion").HasDefaultValue(0);
            entity.Property(e => e.ModifiedBy).HasColumnName("modified_by").HasMaxLength(50).HasDefaultValue("");
            entity.Property(e => e.TimeStamp).HasColumnName("time_stamp").HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.CreatedBy).HasColumnName("created_by").HasMaxLength(50).HasDefaultValue("");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.RevAmt).HasColumnName("rev_amt").HasColumnType("numeric(20,4)");
            entity.Property(e => e.RevAdj).HasColumnName("rev_adj").HasColumnType("numeric(20,4)");
            entity.Property(e => e.RevAdj1).HasColumnName("rev_adj_1").HasColumnType("numeric(10,2)");

            entity.Property(e => e.ActualFeeRateOnCost).HasColumnName("actual_fee_rate_on_cost").HasColumnType("numeric(20,6)");
            entity.Property(e => e.TargetFeeRateOnCost).HasColumnName("target_fee_rate_on_cost").HasColumnType("numeric(10,6)");
            entity.Property(e => e.ActualFeeAmountOnCost).HasColumnName("actual_fee_amt_on_cost").HasColumnType("numeric(20,6)");
            entity.Property(e => e.TargetFeeAmountOnCost).HasColumnName("target_fee_amt_on_cost").HasColumnType("numeric(20,6)");

            entity.Property(e => e.RevDesc).HasColumnName("rev_desc").HasMaxLength(254);
        });

        modelBuilder.Entity<RevFormula>(entity =>
        {
            entity.ToTable("rev_formulas");

            entity.HasKey(e => e.FormulaCd)
                  .HasName("rev_formulas_pkey");

            entity.Property(e => e.FormulaCd)
                  .HasColumnName("formula_cd")
                  .HasMaxLength(15);

            entity.Property(e => e.FormulaDesc)
                  .HasColumnName("formula_desc")
                  .HasMaxLength(100);

            entity.Property(e => e.AwardFeeFl)
                  .HasColumnName("award_fee_fl")
                  .HasDefaultValue(false);

            entity.Property(e => e.ModifiedBy)
                  .HasColumnName("modified_by")
                  .HasMaxLength(20);

            entity.Property(e => e.Timestamp)
                  .HasColumnName("timestamp")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<ProjVendRt>(entity =>
        {
            entity.ToTable("proj_vend_rt");

            entity.HasKey(e => e.ProjVendRtKey).HasName("pk_proj_vend_rt");

            entity.Property(e => e.ProjVendRtKey)
                  .HasColumnName("proj_vend_rt_key");

            entity.Property(e => e.ProjId)
                  .HasColumnName("proj_id")
                  .HasMaxLength(50);

            entity.Property(e => e.VendId)
                  .HasColumnName("vend_id")
                  .HasMaxLength(50);

            entity.Property(e => e.VendEmplId)
                  .HasColumnName("vend_empl_id")
                  .HasMaxLength(50);

            entity.Property(e => e.BillLabCatCd)
                  .HasColumnName("bill_lab_cat_cd")
                  .HasMaxLength(20);

            entity.Property(e => e.BillRtAmt)
                  .HasColumnName("bill_rt_amt")
                  .HasColumnType("numeric(19,4)");

            entity.Property(e => e.SBillRtTypeCd)
                  .HasColumnName("s_bill_rt_type_cd")
                  .HasMaxLength(10);

            entity.Property(e => e.StartDt)
                  .HasColumnName("start_dt");

            entity.Property(e => e.EndDt)
                  .HasColumnName("end_dt");

            entity.Property(e => e.ModifiedBy)
                  .HasColumnName("modified_by")
                  .HasMaxLength(50)
                  .HasDefaultValue("");

            entity.Property(e => e.TimeStamp)
                  .HasColumnName("time_stamp")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.RowVersion)
                  .HasColumnName("rowversion")
                  .HasDefaultValue(0);

            entity.Property(e => e.CompanyId)
                  .HasColumnName("company_id")
                  .HasMaxLength(10);

            entity.Property(e => e.Type)
                .HasColumnName("type")
                .HasMaxLength(50);

            entity.Property(e => e.BillDiscRt)
                  .HasColumnName("bill_disc_rt")
                  .HasColumnType("numeric(19,4)");
        });

        modelBuilder.Entity<ProjEmplRt>(entity =>
        {
            entity.ToTable("proj_empl_rt");

            entity.HasKey(e => e.ProjEmplRtKey).HasName("pk_proj_empl_rt");

            entity.Property(e => e.ProjEmplRtKey).HasColumnName("proj_empl_rt_key");

            entity.Property(e => e.ProjId)
                .HasColumnName("proj_id")
                .HasMaxLength(50);

            entity.Property(e => e.EmplId)
                .HasColumnName("empl_id")
                .HasMaxLength(50);

            entity.Property(e => e.BillLabCatCd)
                .HasColumnName("bill_lab_cat_cd")
                .HasMaxLength(20);

            entity.Property(e => e.BillRtAmt)
                .HasColumnName("bill_rt_amt")
                .HasColumnType("numeric(19,4)");

            entity.Property(e => e.SBillRtTypeCd)
                .HasColumnName("s_bill_rt_type_cd")
                .HasMaxLength(10);

            entity.Property(e => e.StartDt)
                .HasColumnName("start_dt");

            entity.Property(e => e.EndDt)
                .HasColumnName("end_dt");

            entity.Property(e => e.ModifiedBy)
                .HasColumnName("modified_by")
                .HasMaxLength(50)
                .HasDefaultValue("");

            entity.Property(e => e.TimeStamp)
                .HasColumnName("time_stamp")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.Property(e => e.RowVersion)
                .HasColumnName("rowversion")
                .HasDefaultValue(0);

            entity.Property(e => e.CompanyId)
                .HasColumnName("company_id")
                .HasMaxLength(10);

            entity.Property(e => e.Type)
                .HasColumnName("type")
                .HasMaxLength(50);

            entity.Property(e => e.BillDiscRt)
                .HasColumnName("bill_disc_rt")
                .HasColumnType("numeric(19,4)");

            entity.HasOne(e => e.plc) // Navigation property in ProjEmplRate
                .WithMany() // Or .WithMany(p => p.ProjEmplRates) if reverse nav exists
                .HasForeignKey(e => e.BillLabCatCd) // FK in ProjEmplRate
                .HasPrincipalKey(p => p.LaborCategoryCode) // PK in PlcCode (mapped to PLC_CODE)
                .HasConstraintName("proj_empl_rt_bill_lab_cat_cd_fkey")
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<ProjBgtRevSetup>(entity =>
        {
            entity.ToTable("proj_bgt_rev_setup");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.ProjId).HasColumnName("proj_id").HasMaxLength(50);
            entity.Property(e => e.RevType).HasColumnName("rev_type").HasMaxLength(50);
            entity.Property(e => e.RevAcctId).HasColumnName("rev_acct_id").HasMaxLength(20);
            entity.Property(e => e.DfltFeeRt).HasColumnName("dflt_fee_rt").HasColumnType("numeric(19,4)");
            entity.Property(e => e.LabCostFl).HasColumnName("lab_cost_fl");
            entity.Property(e => e.LabBurdFl).HasColumnName("lab_burd_fl");
            entity.Property(e => e.LabFeeCostFl).HasColumnName("lab_fee_cost_fl");
            entity.Property(e => e.LabFeeHrsFl).HasColumnName("lab_fee_hrs_fl");
            entity.Property(e => e.LabFeeRt).HasColumnName("lab_fee_rt").HasColumnType("numeric(19,6)");
            entity.Property(e => e.LabTmFl).HasColumnName("lab_tm_fl");
            entity.Property(e => e.UseBillBurdenRates).HasColumnName("use_bill_burden_rates");
            entity.Property(e => e.OverrideFundingCeilingFl).HasColumnName("override_funding_ceiling_fl").IsRequired();
            entity.Property(e => e.OverrideRevAmtFl).HasColumnName("override_rev_amt_fl").IsRequired();
            entity.Property(e => e.OverrideRevAdjFl).HasColumnName("override_rev_adj_fl").IsRequired();
            entity.Property(e => e.OverrideRevSettingFl).HasColumnName("override_rev_setting_fl").IsRequired();

            // NEW FIELDS
            entity.Property(e => e.NonLabCostFl).HasColumnName("non_lab_cost_fl");
            entity.Property(e => e.PlId).HasColumnName("plid");
            entity.Property(e => e.NonLabBurdFl).HasColumnName("non_lab_burd_fl");
            entity.Property(e => e.NonLabFeeCostFl).HasColumnName("non_lab_fee_cost_fl");
            entity.Property(e => e.NonLabFeeHrsFl).HasColumnName("non_lab_fee_hrs_fl");
            entity.Property(e => e.NonLabFeeRt).HasColumnName("non_lab_fee_rt").HasColumnType("numeric(19,6)");
            entity.Property(e => e.NonLabTmFl).HasColumnName("non_lab_tm_fl");

            entity.Property(e => e.RowVersion).HasColumnName("rowversion").HasDefaultValue(0);
            entity.Property(e => e.ModifiedBy).HasColumnName("modified_by").HasMaxLength(50).HasDefaultValue("");
            entity.Property(e => e.TimeStamp).HasColumnName("time_stamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.CompanyId).HasColumnName("company_id").HasMaxLength(10);
            entity.Property(e => e.AtRiskAmt).HasColumnName("at_risk_amt").HasColumnType("numeric(17,2)").HasDefaultValue(0);
            entity.Property(e => e.VersionNo).HasColumnName("version_no");
            entity.Property(e => e.BgtType).HasColumnName("bgt_type").HasMaxLength(10);
        });


        modelBuilder.Entity<PlEmployeee>(entity =>
        {
            entity.ToTable("pl_employeee", "public");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.HireDate).HasColumnName("hire_date");
            entity.Property(e => e.EffectiveDate).HasColumnName("effective_date");
            entity.Property(e => e.EmplId)
                .HasColumnName("empl_id")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.OrgId)
                .HasColumnName("org_id")
                .HasMaxLength(20);

            entity.Property(e => e.FirstName)
                .HasColumnName("first_name")
                .HasMaxLength(50);

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasMaxLength(10);

            entity.Property(e => e.LastName)
                .HasColumnName("last_name")
                .HasMaxLength(50);

            entity.Property(e => e.PlcGlcCode)
                .HasColumnName("plc_glc_code")
                .HasMaxLength(10);

            entity.Property(e => e.PerHourRate)
                .HasColumnName("per_hour_rate")
                .HasColumnType("numeric(10,2)");

            entity.Property(e => e.Esc_Percent)
                .HasColumnName("esc_percent")
                .HasColumnType("numeric(10,2)");

            entity.Property(e => e.Salary)
                .HasColumnName("salary")
                .HasColumnType("numeric(15,2)");

            entity.Property(e => e.AccId)
                .HasColumnName("acc_id")
                .HasMaxLength(20);

            entity.Property(e => e.IsRev).HasColumnName("is_rev");
            entity.Property(e => e.IsBrd).HasColumnName("is_brd");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.Property(e => e.Type)
                .HasColumnName("type")
                .HasMaxLength(20);

            entity.Property(e => e.PlId).HasColumnName("pl_id");

            entity.HasIndex(e => new { e.PlId, e.EmplId, e.OrgId, e.AccId, e.PlcGlcCode })
                .IsUnique()
                .HasDatabaseName("uq_employee_composite");

            entity.HasOne(e => e.Organization)
                .WithMany()
                .HasForeignKey(e => e.OrgId)
                .HasConstraintName("fk_org_id")
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.PlProjectPlan)
                .WithMany()
                .HasForeignKey(e => e.PlId)
                .HasConstraintName("fk_pl_id")
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ProjectWithPlanDto>().HasNoKey().ToView(null);
        modelBuilder.Entity<Empl_Master>().HasNoKey().ToView(null);
        modelBuilder.Entity<Empl_Master_Dto>().HasNoKey().ToView(null);

        modelBuilder.Entity<PsrHeader>(entity =>
        {
            entity.ToTable("psr_header", "public");

            entity.HasKey(e => e.ProjId);

            entity.Property(e => e.ProjId)
                .HasColumnName("proj_id")
                .HasMaxLength(50);

            entity.Property(e => e.ProjName).HasColumnName("proj_name").HasMaxLength(200);
            entity.Property(e => e.CustName).HasColumnName("cust_name").HasMaxLength(200);
            entity.Property(e => e.PrimeContrId).HasColumnName("prime_contr_id").HasMaxLength(50);

            entity.Property(e => e.ProjStartDt).HasColumnName("proj_start_dt");
            entity.Property(e => e.ProjEndDt).HasColumnName("proj_end_dt");

            entity.Property(e => e.ActiveFl).HasColumnName("active_fl").HasMaxLength(1);
            entity.Property(e => e.SProjRptDc).HasColumnName("s_proj_rpt_dc").HasMaxLength(20);
            entity.Property(e => e.ProjTypeDc).HasColumnName("proj_type_dc").HasMaxLength(50);

            entity.Property(e => e.ProjVCstAmt).HasColumnName("proj_v_cst_amt").HasPrecision(14, 2);
            entity.Property(e => e.ProjVFeeAmt).HasColumnName("proj_v_fee_amt").HasPrecision(14, 2);
            entity.Property(e => e.ProjVTotAmt).HasColumnName("proj_v_tot_amt").HasPrecision(14, 2);

            entity.Property(e => e.PyIncurHrs).HasColumnName("py_incur_hrs").HasPrecision(10, 2);
            entity.Property(e => e.SubIncurHrs).HasColumnName("sub_incur_hrs").HasPrecision(10, 2);
            entity.Property(e => e.PtdIncurHrs).HasColumnName("ptd_incur_hrs").HasPrecision(10, 2);

            entity.Property(e => e.ModifiedBy).HasColumnName("modified_by").HasMaxLength(100);
            entity.Property(e => e.TimeStamp).HasColumnName("time_stamp");

            entity.Property(e => e.FyCd).HasColumnName("fy_cd").HasMaxLength(10);
            entity.Property(e => e.PdNo).HasColumnName("pd_no");
            entity.Property(e => e.SubPdNo).HasColumnName("sub_pd_no");

            entity.Property(e => e.BilledAmt).HasColumnName("billed_amt").HasPrecision(14, 2);
            entity.Property(e => e.BalDueAmt).HasColumnName("bal_due_amt").HasPrecision(14, 2);

            entity.Property(e => e.BillingLevelFl).HasColumnName("billing_level_fl").HasMaxLength(1);

            entity.Property(e => e.ProjFCstAmt).HasColumnName("proj_f_cst_amt").HasPrecision(14, 2);
            entity.Property(e => e.ProjFFeeAmt).HasColumnName("proj_f_fee_amt").HasPrecision(14, 2);
            entity.Property(e => e.ProjFTotAmt).HasColumnName("proj_f_tot_amt").HasPrecision(14, 2);

            entity.Property(e => e.Rowversion).HasColumnName("rowversion");
            entity.Property(e => e.CompanyId).HasColumnName("company_id").HasMaxLength(50);
        });

        modelBuilder.Entity<GlPostDetail>(entity =>
        {
            entity.ToTable("gl_post_details", "public");

            // ✅ No Primary Key
            entity.HasNoKey();

            entity.Property(e => e.AcctId).HasMaxLength(50);
            entity.Property(e => e.OrgId).HasMaxLength(50);
            entity.Property(e => e.ProjId).HasMaxLength(50);
            entity.Property(e => e.FyCd).HasMaxLength(10);
            entity.Property(e => e.CompanyId).HasMaxLength(50);
            entity.Property(e => e.Id).HasMaxLength(50);
            entity.Property(e => e.SIdType).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.BillLabCatCd).HasMaxLength(50);
            entity.Property(e => e.S_JNL_CD).HasMaxLength(20);

            entity.Property(e => e.Amt1)
                  .HasPrecision(18, 2);

            entity.Property(e => e.Hrs1)
                  .HasPrecision(18, 2);
        });
        modelBuilder.Entity<PlFinancialTransaction>(entity =>
        {
            entity.ToTable("gl_post_details");

            //entity.Property(e => e.SrceKey).HasColumnName("srce_key");
            //entity.Property(e => e.Lvl1Key).HasColumnName("lvl1_key");
            //entity.Property(e => e.Lvl2Key).HasColumnName("lvl2_key");
            //entity.Property(e => e.Lvl3Key).HasColumnName("lvl3_key");
            entity.Property(e => e.AcctId).HasColumnName("acct_id");
            entity.Property(e => e.OrgId).HasColumnName("org_id");
            entity.Property(e => e.ProjId).HasColumnName("proj_id");
            entity.Property(e => e.FyCd).HasColumnName("fy_cd");
            entity.Property(e => e.PdNo).HasColumnName("pd_no");
            entity.Property(e => e.SubPdNo).HasColumnName("sub_pd_no");
            //entity.Property(e => e.SJnlCd).HasColumnName("s_jnl_cd");
            //entity.Property(e => e.PostSeqNo).HasColumnName("post_seq_no");
            //entity.Property(e => e.Amt).HasColumnName("amt");
            //entity.Property(e => e.Hrs).HasColumnName("hrs");
            //entity.Property(e => e.ModifiedBy).HasColumnName("modified_by");
            //entity.Property(e => e.TimeStamp).HasColumnName("time_stamp");
            //entity.Property(e => e.Rowversion).HasColumnName("rowversion");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");

            //entity.Property(e => e.SrceKey1).HasColumnName("srce_key1");
            //entity.Property(e => e.Lvl1Key1).HasColumnName("lvl1_key1");
            //entity.Property(e => e.Lvl2Key1).HasColumnName("lvl2_key1");
            //entity.Property(e => e.Lvl3Key1).HasColumnName("lvl3_key1");
            //entity.Property(e => e.GlpsumSrceKey).HasColumnName("glpsum_srce_key");
            //entity.Property(e => e.GlpsumLvl1Key).HasColumnName("glpsum_lvl1_key");
            //entity.Property(e => e.GlpsumLvl2Key).HasColumnName("glpsum_lvl2_key");
            //entity.Property(e => e.GlpsumLvl3Key).HasColumnName("glpsum_lvl3_key");

            entity.Property(e => e.Id).HasColumnName("id");
            //entity.Property(e => e.VchrNo).HasColumnName("vchr_no");
            entity.Property(e => e.SIdType).HasColumnName("s_id_type");
            //entity.Property(e => e.PoId).HasColumnName("po_id");
            //entity.Property(e => e.InvcId).HasColumnName("invc_id");
            //entity.Property(e => e.Ref1Id).HasColumnName("ref1_id");
            //entity.Property(e => e.GenlLabCatCd).HasColumnName("genl_lab_cat_cd");
            //entity.Property(e => e.Ref2Id).HasColumnName("ref2_id");

            entity.Property(e => e.Amt1).HasColumnName("amt1");
            entity.Property(e => e.Hrs1).HasColumnName("hrs1");
            //entity.Property(e => e.ModifiedBy1).HasColumnName("modified_by1");
            //entity.Property(e => e.TimeStamp1).HasColumnName("time_stamp1");

            entity.Property(e => e.Name).HasColumnName("name");
            //entity.Property(e => e.TrnDesc).HasColumnName("trn_desc");
            //entity.Property(e => e.PostUserId).HasColumnName("post_user_id");
            //entity.Property(e => e.EntrUserId).HasColumnName("entr_user_id");
            //entity.Property(e => e.JeNo).HasColumnName("je_no");
            //entity.Property(e => e.ChkNo).HasColumnName("chk_no");
            entity.Property(e => e.BillLabCatCd).HasColumnName("bill_lab_cat_cd");
            //entity.Property(e => e.CashRecptNo).HasColumnName("cash_recpt_no");
            //entity.Property(e => e.BillNoId).HasColumnName("bill_no_id");
            //entity.Property(e => e.ItemId).HasColumnName("item_id");
            //entity.Property(e => e.ItemRvsnId).HasColumnName("item_rvsn_id");
            //entity.Property(e => e.UnitsQty).HasColumnName("units_qty");
            //entity.Property(e => e.CommentsNt).HasColumnName("comments_nt");

            //entity.Property(e => e.TsDt).HasColumnName("ts_dt");
            //entity.Property(e => e.CashRecptDt).HasColumnName("cash_recpt_dt");
            //entity.Property(e => e.UnitsUsageDt).HasColumnName("units_usage_dt");
            entity.Property(e => e.EffectBillDt).HasColumnName("effect_bill_dt");
            entity.Property(e => e.SJnlCd).HasColumnName("s_jnl_cd").HasMaxLength(20);
            //entity.Property(e => e.TrnCrncyCd).HasColumnName("trn_crncy_cd");
            //entity.Property(e => e.TrnAmt).HasColumnName("trn_amt");
            //entity.Property(e => e.Rowversion1).HasColumnName("rowversion1");
            //entity.Property(e => e.CompanyId1).HasColumnName("company_id1");

            //entity.Property(e => e.VchrInvcCrncyCd).HasColumnName("vchr_invc_crncy_cd");
            //entity.Property(e => e.VchrInvcAmt).HasColumnName("vchr_invc_amt");
            //entity.Property(e => e.CrChkId).HasColumnName("cr_chk_id");
            //entity.Property(e => e.SettlNo).HasColumnName("settl_no");
            //entity.Property(e => e.BsrTrnCrncy).HasColumnName("bsr_trn_crncy");
        });

        modelBuilder.Entity<EmployeeMaster>(entity =>
        {
            entity.ToTable("empl");
            entity.HasKey(e => e.EmplId);
            entity.Property(e => e.EmplId).HasColumnName("empl_id");
            entity.Property(e => e.LvPdCd).HasColumnName("lv_pd_cd");
            entity.Property(e => e.TaxbleEntityId).HasColumnName("taxble_entity_id");
            entity.Property(e => e.SsnId).HasColumnName("ssn_id");
            entity.Property(e => e.OrigHireDt).HasColumnName("orig_hire_dt");
            entity.Property(e => e.AdjHireDt).HasColumnName("adj_hire_dt");
            entity.Property(e => e.TermDt).HasColumnName("term_dt");
            entity.Property(e => e.SEmplStatusCd).HasColumnName("s_empl_status_cd");
            entity.Property(e => e.SpvsrName).HasColumnName("spvsr_name");
            entity.Property(e => e.LastName).HasColumnName("last_name");
            entity.Property(e => e.FirstName).HasColumnName("first_name");
            entity.Property(e => e.MidName).HasColumnName("mid_name");
            entity.Property(e => e.PrefName).HasColumnName("pref_name");
            entity.Property(e => e.NamePrfxCd).HasColumnName("name_prfx_cd");
            entity.Property(e => e.NameSfxCd).HasColumnName("name_sfx_cd");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.TsPdCd).HasColumnName("ts_pd_cd");
            entity.Property(e => e.BirthDt).HasColumnName("birth_dt");
            entity.Property(e => e.CityName).HasColumnName("city_name");
            entity.Property(e => e.CountryCd).HasColumnName("country_cd");
            entity.Property(e => e.LastFirstName).HasColumnName("last_first_name");
            entity.Property(e => e.Ln1Adr).HasColumnName("ln_1_adr");
            entity.Property(e => e.Ln2Adr).HasColumnName("ln_2_adr");
            entity.Property(e => e.Ln3Adr).HasColumnName("ln_3_adr");
            entity.Property(e => e.MailStateDc).HasColumnName("mail_state_dc");
            entity.Property(e => e.PostalCd).HasColumnName("postal_cd");
            entity.Property(e => e.ModifiedBy).HasColumnName("modified_by");
            entity.Property(e => e.TimeStamp).HasColumnName("time_stamp");
            entity.Property(e => e.LocatorCd).HasColumnName("locator_cd");
            entity.Property(e => e.PrirName).HasColumnName("prir_name");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.LastReviewDt).HasColumnName("last_review_dt");
            entity.Property(e => e.NextReviewDt).HasColumnName("next_review_dt");
            entity.Property(e => e.SexCd).HasColumnName("sex_cd");
            entity.Property(e => e.MaritalCd).HasColumnName("marital_cd");
            entity.Property(e => e.EligAutoPayFl).HasColumnName("elig_auto_pay_fl");
            entity.Property(e => e.EmailId).HasColumnName("email_id");
            entity.Property(e => e.MgrEmplId).HasColumnName("mgr_empl_id");
            entity.Property(e => e.SRaceCd).HasColumnName("s_race_cd");
            entity.Property(e => e.PrServEmplId).HasColumnName("pr_serv_empl_id");
            entity.Property(e => e.CountyName).HasColumnName("county_name");
            entity.Property(e => e.TsPdRegHrsNo).HasColumnName("ts_pd_reg_hrs_no");
            entity.Property(e => e.PayPdRegHrsNo).HasColumnName("pay_pd_reg_hrs_no");
            entity.Property(e => e.DisabledFl).HasColumnName("disabled_fl");
            entity.Property(e => e.MosReviewNo).HasColumnName("mos_review_no");
            entity.Property(e => e.ContName1).HasColumnName("cont_name_1");
            entity.Property(e => e.ContName2).HasColumnName("cont_name_2");
            entity.Property(e => e.ContPhone1).HasColumnName("cont_phone_1");
            entity.Property(e => e.ContPhone2).HasColumnName("cont_phone_2");
            entity.Property(e => e.ContRel1).HasColumnName("cont_rel_1");
            entity.Property(e => e.ContRel2).HasColumnName("cont_rel_2");
            entity.Property(e => e.UnionEmplFl).HasColumnName("union_empl_fl");
            entity.Property(e => e.VisaTypeCd).HasColumnName("visa_type_cd");
            entity.Property(e => e.VetStatusS).HasColumnName("vet_status_s");
            entity.Property(e => e.VetStatusV).HasColumnName("vet_status_v");
            entity.Property(e => e.VetStatusO).HasColumnName("vet_status_o");
            entity.Property(e => e.VetStatusR).HasColumnName("vet_status_r");
            entity.Property(e => e.EssPinId).HasColumnName("ess_pin_id");
            entity.Property(e => e.PinUpdatedFl).HasColumnName("pin_updated_fl");
            entity.Property(e => e.SEssCosCd).HasColumnName("s_ess_cos_cd");
            entity.Property(e => e.HomeEmailId).HasColumnName("home_email_id");
            entity.Property(e => e.Rowversion).HasColumnName("rowversion");
            entity.Property(e => e.VetReleaseDt).HasColumnName("vet_release_dt");
            entity.Property(e => e.ContractorFl).HasColumnName("contractor_fl");
            entity.Property(e => e.BlindFl).HasColumnName("blind_fl");
            entity.Property(e => e.VisaDt).HasColumnName("visa_dt");
            entity.Property(e => e.VetStatusD).HasColumnName("vet_status_d");
            entity.Property(e => e.VetStatusA).HasColumnName("vet_status_a");
            entity.Property(e => e.TimeEntryType).HasColumnName("time_entry_type");
            entity.Property(e => e.BadgeGroup).HasColumnName("badge_group");
            entity.Property(e => e.BadgeId).HasColumnName("badge_id");
            entity.Property(e => e.LoginId).HasColumnName("login_id");
            entity.Property(e => e.SftFl).HasColumnName("sft_fl");
            entity.Property(e => e.MesFl).HasColumnName("mes_fl");
            entity.Property(e => e.ClockFl).HasColumnName("clock_fl");
            entity.Property(e => e.PlantId).HasColumnName("plant_id");
            entity.Property(e => e.EmplSourceCd).HasColumnName("empl_source_cd");
            entity.Property(e => e.SrExportDt).HasColumnName("sr_export_dt");
            entity.Property(e => e.HrsmartExportDt).HasColumnName("hrsmart_export_dt");
            entity.Property(e => e.VetStatusP).HasColumnName("vet_status_p");
            entity.Property(e => e.BirthCityName).HasColumnName("birth_city_name");
            entity.Property(e => e.BirthMailStateDc).HasColumnName("birth_mail_state_dc");
            entity.Property(e => e.BirthCountryCd).HasColumnName("birth_country_cd");
            entity.Property(e => e.UserLoginId).HasColumnName("user_login_id");
            entity.Property(e => e.EmplAuthMthd).HasColumnName("empl_auth_mthd");
            entity.Property(e => e.EssUserFl).HasColumnName("ess_user_fl");
            entity.Property(e => e.LastDayDt).HasColumnName("last_day_dt");
            entity.Property(e => e.GovwiniqLoginId).HasColumnName("govwiniq_login_id");
            entity.Property(e => e.HuaId).HasColumnName("hua_id");
            entity.Property(e => e.HuaActvMapFl).HasColumnName("hua_actv_map_fl");
            entity.Property(e => e.VetStatusNp).HasColumnName("vet_status_np");
            entity.Property(e => e.VetStatusDeclined).HasColumnName("vet_status_declined");
            entity.Property(e => e.VetStatusRs).HasColumnName("vet_status_rs");
        });
        modelBuilder.Entity<Holidaycalender>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("holidaycalender_pkey");

            entity.ToTable("holidaycalender");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.Ispublicholiday).HasColumnName("ispublicholiday");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.State)
                .HasMaxLength(50)
                .HasColumnName("state");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");
        });

        modelBuilder.Entity<PlConfigValue>(entity =>
        {
            entity.ToTable("pl_config_values");

            //entity.HasKey(e => e.Name)
            //      .HasName("pl_config_values_pkey");

            entity.Property(e => e.Id)
                  .HasColumnName("id")
                  .ValueGeneratedOnAdd();

            entity.Property(e => e.Name)
                  .HasColumnName("name")
                  .HasMaxLength(20)
                  .IsRequired();

            entity.Property(e => e.Value)
                  .HasColumnName("value")
                  .HasMaxLength(20);

            entity.Property(e => e.CreatedAt)
                  .HasColumnType("timestamp without time zone")
                  .HasColumnName("created_at");

            entity.Property(e => e.ProjId)
                  .HasColumnName("proj_id")
                  .HasMaxLength(20);

            //entity.HasIndex(e => e.Id)
            //      .IsUnique()
            //      .HasDatabaseName("unique_conf_key")
            //      .IncludeProperties("proj_id", "name");

            //entity.HasOne<PlProject>()  // Optional: only if PlProject entity exists
            //      .WithMany()
            //      .HasForeignKey(e => e.ProjId)
            //      .HasConstraintName("fk_proj_id")
            //      .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<PlEmployee>(entity =>
        {
            entity.HasKey(e => e.EmplId).HasName("empl_pkey");

            entity.ToTable("pl_employee");

            entity.HasIndex(e => e.Email, "empl_email_key").IsUnique();

            entity.Property(e => e.EmplId)
                .HasMaxLength(20)
                .HasColumnName("empl_id");
            entity.Property(e => e.AccId)
                .HasMaxLength(20)
                .HasColumnName("acc_id");
            entity.Property(e => e.CountyName)
                .HasMaxLength(10)
                .HasColumnName("county_name");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasColumnName("first_name");
            entity.Property(e => e.Gender)
                .HasMaxLength(1)
                .HasColumnName("gender");
            entity.Property(e => e.HireDate).HasColumnName("hire_date");
            entity.Property(e => e.IsBrd)
                .HasDefaultValue(false)
                .HasColumnName("is_brd");
            entity.Property(e => e.IsRev)
                .HasDefaultValue(false)
                .HasColumnName("is_rev");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .HasColumnName("last_name");
            entity.Property(e => e.Ln1Adr)
                .HasMaxLength(30)
                .HasColumnName("ln_1_adr");
            entity.Property(e => e.Ln2Adr)
                .HasMaxLength(30)
                .HasColumnName("ln_2_adr");
            entity.Property(e => e.Ln3Adr)
                .HasMaxLength(30)
                .HasColumnName("ln_3_adr");
            entity.Property(e => e.MailStateDc)
                .HasMaxLength(15)
                .HasColumnName("mail_state_dc");
            entity.Property(e => e.MaritalCd)
                .HasMaxLength(1)
                .HasColumnName("marital_cd");
            entity.Property(e => e.MidName)
                .HasMaxLength(50)
                .HasColumnName("mid_name");
            entity.Property(e => e.OrgId)
                .HasMaxLength(20)
                .HasColumnName("org_id");
            entity.Property(e => e.PerHourRate)
                .HasPrecision(10, 2)
                .HasColumnName("per_hour_rate");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(15)
                .HasColumnName("phone_number");
            entity.Property(e => e.PlcGlcCode)
                .HasMaxLength(10)
                .HasColumnName("plc_glc_code");
            entity.Property(e => e.PostalCd)
                .HasMaxLength(10)
                .HasColumnName("postal_cd");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasColumnName("role");
            entity.Property(e => e.Salary)
                .HasPrecision(15, 2)
                .HasColumnName("salary");
            entity.Property(e => e.Type)
                .HasMaxLength(10)
                .HasColumnName("type");

            entity.HasOne(d => d.Org).WithMany(p => p.PlEmployees)
                .HasForeignKey(d => d.OrgId)
                .HasConstraintName("empl_org_id_fkey");
        });

        modelBuilder.Entity<PlEmployeeProjectMapping>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("pl_employee_project_mapping");

            entity.Property(e => e.EmplId)
                .HasMaxLength(20)
                .HasColumnName("empl_empl_id");
            entity.Property(e => e.ProjId)
                .HasMaxLength(30)
                .HasColumnName("proj_proj_id");

            entity.HasOne(d => d.Empl).WithMany()
                .HasForeignKey(d => d.EmplId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pl_employee_project_mapping_empl_empl_id_fkey");

            entity.HasOne(d => d.EmplMaster).WithMany()
                .HasForeignKey(d => d.EmplId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pl_employee_project_mapping_empl_master_empl_id_fkey");

            entity.HasOne(d => d.Proj).WithMany()
                .HasForeignKey(d => d.ProjId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pl_employee_project_mapping_proj_proj_id_fkey");
        });

        modelBuilder.Entity<PlForecast>(entity =>
        {
            entity.HasKey(e => e.Forecastid).HasName("empl_forecast_pkey");

            entity.ToTable("pl_forecast");

            entity.Property(e => e.Forecastid).HasColumnName("forecastid");
            entity.Property(e => e.DctId).HasColumnName("dct_id");
            entity.Property(e => e.AcctId).HasColumnName("acct_id");
            entity.Property(e => e.OrgId).HasColumnName("org_id");
            entity.Property(e => e.empleId).HasColumnName("plc_rate_id");
            entity.Property(e => e.HrlyRate).HasColumnName("hrly_rate");
            entity.Property(e => e.Fringe).HasColumnName("fringe");
            entity.Property(e => e.Overhead).HasColumnName("overhead");
            entity.Property(e => e.Gna).HasColumnName("gna");
            entity.Property(e => e.Fees).HasColumnName("fees");
            entity.Property(e => e.Materials).HasColumnName("mnh");
            entity.Property(e => e.Hr).HasColumnName("hr");
            entity.Property(e => e.YtdFringe)
           .HasColumnName("ytd_fringe")
           .HasColumnType("numeric(10,2)")
           .HasDefaultValue(0);

            entity.Property(e => e.YtdOverhead)
                .HasColumnName("ytd_overhead")
                .HasColumnType("numeric(10,2)")
                .HasDefaultValue(0);

            entity.Property(e => e.YtdGna)
                .HasColumnName("ytd_gna")
                .HasColumnType("numeric(10,2)")
                .HasDefaultValue(0);

            entity.Property(e => e.YtdMaterials)
                .HasColumnName("ytd_materials")
                .HasColumnType("numeric(10,2)")
                .HasDefaultValue(0);

            entity.Property(e => e.YtdCost)
                .HasColumnName("ytd_cost")
                .HasColumnType("numeric(10,2)")
                .HasDefaultValue(0);

            entity.Property(e => e.EffectDt).HasColumnName("effect_dt").HasConversion(dateOnlyConverter).HasColumnType("date");
            //.HasColumnType("timestamp without time zone");
            entity.Property(e => e.Plc).HasColumnName("plc");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.EmplId)
                .HasMaxLength(20)
                .HasColumnName("empl_id");
            entity.Property(e => e.Forecastedamt).HasColumnName("forecastedamt");
            entity.Property(e => e.Forecastedhours)
                .HasPrecision(10, 2)
                .HasColumnName("forecastedhours");
            entity.Property(e => e.Actualamt).HasColumnName("actualamt");
            entity.Property(e => e.Actualhours)
                .HasPrecision(10, 2)
                .HasColumnName("actualhours");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.Burden).HasColumnName("burden");
            entity.Property(e => e.Cost).HasColumnName("cost");
            entity.Property(e => e.ForecastedCost).HasColumnName("forecastedcost");
            entity.Property(e => e.Revenue).HasColumnName("revenue");
            entity.Property(e => e.PlId).HasColumnName("pl_id");
            entity.Property(e => e.ProjId)
                .HasMaxLength(30)
                .HasColumnName("proj_id");
            entity.Property(e => e.Updatedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedat");
            entity.Property(e => e.Year).HasColumnName("year");


            entity.HasOne(d => d.DirectCost).WithMany(p => p.PlForecasts)
                .HasForeignKey(d => d.DctId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("dct_forecast_dct_id_fkey");

            //entity.HasOne(d => d.Empl).WithMany(p => p.PlForecasts)
            //    .HasForeignKey(d => d.EmplId)
            //    .OnDelete(DeleteBehavior.ClientSetNull)
            //    .HasConstraintName("empl_forecast_empl_id_fkey");

            entity.HasOne(d => d.Emple).WithMany(p => p.PlForecasts)
                .HasForeignKey(d => d.empleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_pl_forecast_emple_id_fkey");

            entity.HasOne(d => d.Pl).WithMany(p => p.PlForecasts)
                .HasForeignKey(d => d.PlId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("empl_forecast_bud_id_fkey");

            entity.HasOne(d => d.Proj).WithMany(p => p.PlForecasts)
                .HasForeignKey(d => d.ProjId)
                .HasConstraintName("empl_forecast_proj_id_fkey");

            //entity.HasOne(e => e.PlcRate)
            //          .WithMany()
            //          .HasForeignKey(e => e.PlcRateId)
            //          .HasConstraintName("fk_pl_forecast_plc_rate_id");
        });

        modelBuilder.Entity<PlProject>(entity =>
        {
            entity.HasKey(e => e.ProjId).HasName("proj_pkey");

            entity.ToTable("pl_project");

            entity.Property(e => e.proj_v_tot_amt).HasColumnName("proj_v_tot_amt").HasColumnType("numeric(20,4)");
            entity.Property(e => e.proj_f_tot_amt).HasColumnName("proj_f_tot_amt").HasColumnType("numeric(20,4)");
            entity.Property(e => e.proj_v_fee_amt).HasColumnName("proj_v_fee_amt").HasColumnType("numeric(20,4)");
            entity.Property(e => e.proj_v_cst_amt).HasColumnName("proj_v_cst_amt").HasColumnType("numeric(20,4)");
            entity.Property(e => e.proj_f_fee_amt).HasColumnName("proj_f_fee_amt").HasColumnType("numeric(20,4)");
            entity.Property(e => e.proj_f_cst_amt).HasColumnName("proj_f_cst_amt").HasColumnType("numeric(20,4)");

            entity.Property(e => e.ProjId)
                .HasMaxLength(30)
                .HasColumnName("proj_id");
            entity.Property(e => e.AcctGrpCd)
                .HasMaxLength(3)
                .HasColumnName("acct_grp_cd");
            entity.Property(e => e.AcctGrpFl)
                .HasMaxLength(1)
                .HasColumnName("acct_grp_fl");
            entity.Property(e => e.ActiveFl)
                .HasMaxLength(1)
                .HasColumnName("active_fl");
            entity.Property(e => e.CompanyId)
                .HasMaxLength(10)
                .HasColumnName("company_id");
            entity.Property(e => e.CustId)
                .HasMaxLength(12)
                .HasColumnName("cust_id");
            entity.Property(e => e.InactiveDt).HasColumnName("inactive_dt");
            entity.Property(e => e.Notes)
                .HasMaxLength(254)
                .HasColumnName("notes");
            entity.Property(e => e.OrgId)
                .HasMaxLength(20)
                .HasColumnName("org_id");
            entity.Property(e => e.ProjEndDt).HasColumnName("proj_end_dt");
            entity.Property(e => e.ProjLongName)
                .HasMaxLength(120)
                .HasColumnName("proj_long_name");
            entity.Property(e => e.ProjMgrName)
                .HasMaxLength(25)
                .HasColumnName("proj_mgr_name");
            entity.Property(e => e.ProjName)
                .HasMaxLength(25)
                .HasColumnName("proj_name");
            entity.Property(e => e.ProjStartDt).HasColumnName("proj_start_dt");
            entity.Property(e => e.ProjTypeDc)
                .HasMaxLength(15)
                .HasColumnName("proj_type_dc");

            entity.HasOne(d => d.Org).WithMany(p => p.PlProjects)
                .HasForeignKey(d => d.OrgId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("proj_org_id_fkey");
        });

        modelBuilder.Entity<PlProjectPlan>(entity =>
        {
            entity.HasKey(e => e.PlId).HasName("pl_project_plan_pkey");

            entity.ToTable("pl_project_plan");

            entity.Property(e => e.PlId).HasColumnName("pl_id");
            entity.Property(e => e.ApprovedBy)
                .HasMaxLength(20)
                .HasColumnName("approved_by");
            entity.Property(e => e.ClosedPeriod).HasColumnName("closed_period");
            entity.Property(e => e.ProjStartDt).HasColumnName("proj_start_dt");
            entity.Property(e => e.ProjEndDt).HasColumnName("proj_end_dt");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedBy)
                .HasMaxLength(20)
                .HasColumnName("created_by");
            entity.Property(e => e.FinalVersion)
                .HasDefaultValue(false)
                .HasColumnName("final_version");
            entity.Property(e => e.IsApproved)
                .HasDefaultValue(false)
                .HasColumnName("is_approved");
            entity.Property(e => e.IsCompleted)
                .HasDefaultValue(false)
                .HasColumnName("is_completed");
            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(20)
                .HasColumnName("modified_by");
            entity.Property(e => e.Source)
                .HasMaxLength(20)
                .HasColumnName("source");
            entity.Property(e => e.Type)
                .HasMaxLength(20)
                .HasColumnName("type");
            entity.Property(e => e.PlType)
                .HasMaxLength(10)
                .HasColumnName("pl_type");
            entity.Property(e => e.ProjId)
                .HasMaxLength(30)
                .HasColumnName("proj_id");
            entity.Property(e => e.Status)
                .HasMaxLength(15)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.Version).HasColumnName("version");
            entity.Property(e => e.TemplateId).HasColumnName("burden_template_id");
            entity.Property(e => e.VersionCode)
                .HasMaxLength(10)
                .HasColumnName("version_code");

            entity.HasOne(d => d.Proj).WithMany(p => p.PlProjectPlans)
                .HasForeignKey(d => d.ProjId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("project_budget_proj_id_fkey");
        });

        modelBuilder.Entity<PlcCode>(entity =>
        {
            entity.HasKey(e => e.LaborCategoryCode).HasName("PLC_CODES_pkey");

            entity.ToTable("plc_codes");

            entity.Property(e => e.LaborCategoryCode)
                .HasMaxLength(10)
                .HasColumnName("plc_code");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
        });

        modelBuilder.Entity<BurdenTemplate>(entity =>
        {
            entity.ToTable("pl_burden_templates");

            entity.HasKey(t => t.Id);

            entity.Property(t => t.Id).HasColumnName("id");
            entity.Property(t => t.TemplateCode)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("template_code");

            entity.Property(t => t.Description)
                .HasColumnName("description");
        });

        modelBuilder.Entity<BurdenRate>(entity =>
        {
            entity.ToTable("pl_burden_rates");

            entity.HasKey(r => r.Id);

            entity.Property(r => r.Id).HasColumnName("id");
            entity.Property(r => r.TemplateId).HasColumnName("template_id");
            entity.Property(r => r.PoolName)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("pool_name");

            entity.Property(r => r.Base)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnName("base");

            entity.Property(r => r.RatePercentage)
                .HasColumnType("numeric(5,4)")
                .HasColumnName("rate_percentage");

            entity.Property(r => r.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");

            entity.Property(r => r.EffectiveDate).HasColumnName("effective_date");
            entity.Property(r => r.ExpirationDate).HasColumnName("expiration_date");

            entity.HasOne(r => r.Template)
                .WithMany(t => t.Rates)
                .HasForeignKey(r => r.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<pl_EmployeeBurdenCalculated>(entity =>
        {
            entity.ToTable("pl_employeeburdencalculated");

            entity.HasKey(e => e.Id).HasName("pl_employeeburdencalculated_pkey");

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.Empl_Id)
                .IsRequired()
                .HasMaxLength(12)
                .HasColumnName("empl_id");

            entity.Property(e => e.BurdenDate)
                .HasColumnType("date");

            entity.Property(e => e.Proj_Id)
                .HasMaxLength(20)
                .HasColumnName("proj_id");

            entity.Property(e => e.Pl_Id)
                .HasColumnName("pl_id");

            entity.Property(e => e.Plc_Code)
                .HasMaxLength(10)
                .HasColumnName("plc_code");

            entity.Property(e => e.ForecastedCost)
                .HasColumnType("numeric(18,4)");

            entity.Property(e => e.ForecastedHours)
                .HasColumnType("numeric(18,4)");

            entity.Property(e => e.BurdenCost)
                .HasColumnType("numeric(18,4)");

            entity.Property(e => e.Template_ID)
                .HasColumnName("template_id");

            entity.Property(e => e.Notes);

            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamptz").HasDefaultValueSql("CURRENT_TIMESTAMP"); ;

            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamptz");

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(50);

            entity.Property(e => e.UpdatedBy)
                .HasMaxLength(50);

            // Relationships
            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.Empl_Id)
                .HasConstraintName("fk_employee");

            entity.HasOne(e => e.Project)
                .WithMany()
                .HasForeignKey(e => e.Proj_Id)
                .HasConstraintName("fk_project");

            entity.HasOne(e => e.LabCategory)
                .WithMany()
                .HasForeignKey(e => e.Plc_Code)
                .HasConstraintName("fk_labcategory");

            entity.HasOne(e => e.ProjectPlan)
                .WithMany()
                .HasForeignKey(e => e.Pl_Id)
                .HasConstraintName("fk_employee_burden_pl");
        });

        // AccountGroup table
        modelBuilder.Entity<Pools>(entity =>
        {
            entity.ToTable("pools");

            entity.HasKey(e => e.Code);
            entity.Property(e => e.Code).HasColumnName("code");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Sequence).HasColumnName("sequence");
            entity.Property(e => e.PoolNo).HasColumnName("poolno");
            entity.Property(e => e.Name).HasColumnName("name");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.ModifiedBy).HasColumnName("modified_by");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        // OrgAccount table
        modelBuilder.Entity<OrgAccount>(entity =>
        {
            entity.ToTable("org_account");

            entity.HasKey(e => new { e.OrgId, e.AcctId });

            entity.Property(e => e.OrgId).HasColumnName("org_id");
            entity.Property(e => e.AcctId).HasColumnName("acct_id");
            entity.Property(e => e.AccType).HasColumnName("acc_type");
            entity.Property(e => e.ActiveFl).HasColumnName("active_fl");
            entity.Property(e => e.ModifiedBy).HasColumnName("modified_by");
            entity.Property(e => e.TimeStamp).HasColumnName("time_stamp");

            // Foreign key to Account
            entity.HasOne(e => e.Account)
                  .WithMany() // or .WithMany(a => a.SomeTables) if you have a navigation collection
                  .HasForeignKey(e => e.AcctId)
                  .HasConstraintName("fk_account")
                  .OnDelete(DeleteBehavior.Restrict); // maps to NO ACTION

            // Foreign key to Organization
            entity.HasOne(e => e.Organization)
                  .WithMany()
                  .HasForeignKey(e => e.OrgId)
                  .HasConstraintName("fk_org")
                  .OnDelete(DeleteBehavior.Restrict); // maps to NO ACTION
        });

        modelBuilder.Entity<PlPoolRate>(entity =>
        {
            entity.ToTable("pl_pool_rates");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.OrgId).HasColumnName("org_id");
            entity.Property(e => e.AccountId).HasColumnName("account_id");
            entity.Property(e => e.AccountGroupCode).HasColumnName("account_group_code");
            entity.Property(e => e.AccountType).HasColumnName("account_type");
            entity.Property(e => e.Year).HasColumnName("year");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.ActualRate).HasColumnName("actual_rate");
            entity.Property(e => e.TargetRate).HasColumnName("target_rate");
            entity.Property(e => e.ModifiedBy).HasColumnName("modified_by");
            entity.Property(e => e.CreatedAt)
                                    .HasColumnName("created_at")
                                    .HasConversion(
                                        v => v.ToUniversalTime(),
                                        v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
                                    );
            entity.Property(e => e.UpdatedAt)
                        .HasColumnName("updated_at")
                        .HasConversion(
                            v => v.ToUniversalTime(),
                            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
                        );

            entity.Property(e => e.BurdenTemplateId).HasColumnName("burden_template_id");

            // Unique constraint
            entity.HasIndex(p => new { p.OrgId, p.AccountId, p.Year, p.Month, p.AccountGroupCode })
                  .IsUnique()
                  .HasDatabaseName("uq_pool_rate");

            // Foreign keys
            entity.HasOne(p => p.OrgAccount)
                  .WithMany(o => o.PlPoolRates)
                  .HasForeignKey(p => new { p.OrgId, p.AccountId })
                  .HasConstraintName("fk_account_orgacct");

            entity.HasOne(p => p.AccountGroup)
                  .WithMany(g => g.PlPoolRates)
                  .HasForeignKey(p => p.AccountGroupCode)
                  .HasConstraintName("fk_account_group_code");

            entity.HasOne(p => p.BurdenTemplate)
                  .WithMany(b => b.PlPoolRates)
                  .HasForeignKey(p => p.BurdenTemplateId)
                  .HasConstraintName("fk_burden_template");
        });

        modelBuilder.Entity<ProjectFee>(entity =>
        {
            entity.ToTable("project_fees");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ProjId).HasColumnName("proj_id").HasMaxLength(30).IsRequired();
            entity.Property(e => e.FeeCode).HasColumnName("fee_code").HasMaxLength(10).IsRequired();
            entity.Property(e => e.FeeType).HasColumnName("fee_type").HasMaxLength(20).IsRequired();
            entity.Property(e => e.FeeBase).HasColumnName("fee_base").HasMaxLength(50);
            entity.Property(e => e.FeePercent).HasColumnName("fee_percent").HasColumnType("numeric(6,3)");
            entity.Property(e => e.FixedAmount).HasColumnName("fixed_amount").HasColumnType("numeric(12,2)");
            entity.Property(e => e.EffectiveDate).HasColumnName("effective_date").IsRequired();
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.ModifiedBy).HasColumnName("modified_by").HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(e => new { e.ProjId, e.FeeCode, e.EffectiveDate })
                  .HasDatabaseName("IX_ProjectFees_ProjCodeDate");

            // Foreign key to project table
            entity.HasOne<PlProject>() // or your actual Project model
                  .WithMany()
                  .HasForeignKey(e => e.ProjId)
                  .HasConstraintName("fk_project")
                  .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<ProjectPlcRate>(entity =>
        {
            entity.ToTable("project_plc_rates");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ProjId).HasColumnName("proj_id").HasMaxLength(30).IsRequired();
            entity.Property(e => e.LaborCategoryCode).HasColumnName("labor_category_code").HasMaxLength(10).IsRequired();
            entity.Property(e => e.CostRate).HasColumnName("cost_rate").HasColumnType("numeric(12,4)");
            entity.Property(e => e.BillingRate).HasColumnName("billing_rate").HasColumnType("numeric(12,4)");
            entity.Property(e => e.EffectiveDate).HasColumnName("effective_date").IsRequired();
            entity.Property(e => e.EndDate).HasColumnName("end_date");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.ModifiedBy).HasColumnName("modified_by").HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.SBillRtTypeCd).HasColumnName("s_bill_rt_type_cd");

            entity.HasIndex(e => new { e.ProjId, e.LaborCategoryCode, e.EffectiveDate }).IsUnique()
                  .HasDatabaseName("uq_proj_plc_rate");

            entity.HasOne<PlProject>() // Assumes you have a Project entity
                  .WithMany()
                  .HasForeignKey(e => e.ProjId)
                  .HasConstraintName("fk_proj")
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<PlcCode>() // Assumes you have a PlcCode entity
                  .WithMany()
                  .HasForeignKey(e => e.LaborCategoryCode)
                  .HasConstraintName("fk_plc_code")
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PlDct>(entity =>
        {
            // Explicitly map the entity to the 'pl_dct' table.
            // This is redundant if [Table("pl_dct")] is used, but included for demonstration.
            entity.ToTable("pl_dct");

            entity.Property(p => p.DctId)
                    .ValueGeneratedOnAdd();

            // Configure the primary key
            entity.HasKey(e => e.DctId);

            // Configure properties and their column mappings
            // Note: [Column("column_name")] attributes already handle mapping,
            // but these fluent API calls demonstrate an alternative way to do it.

            entity.Property(e => e.DctId)
                  .HasColumnName("dct_id")
                  .IsRequired(); // Matches NOT NULL in SQL

            entity.Property(e => e.PlId)
                  .HasColumnName("pl_id")
                  .IsRequired(); // Matches NOT NULL in SQL

            entity.Property(e => e.Notes)
                  .HasColumnName("notes")
                  .HasMaxLength(255);

            entity.Property(e => e.Category)
                  .HasColumnName("category")
                  .HasMaxLength(50);

            entity.Property(e => e.AmountType)
                  .HasColumnName("amount_type")
                  .HasMaxLength(30);

            entity.Property(e => e.AcctId)
                  .HasColumnName("acctid")
                  .HasMaxLength(30);

            entity.Property(e => e.OrgId)
                  .HasColumnName("orgid")
                  .HasMaxLength(30);

            entity.Property(e => e.Type)
                  .HasColumnName("id_type")
                  .HasMaxLength(30);

            entity.Property(e => e.Id)
                  .HasColumnName("id")
                  .HasMaxLength(50);

            entity.Property(e => e.IsRev)
                  .HasColumnName("is_rev")
                  .HasDefaultValue(false); // Matches BOOLEAN DEFAULT FALSE

            entity.Property(e => e.IsBrd)
                  .HasColumnName("is_brd")
                  .HasDefaultValue(false); // Matches BOOLEAN DEFAULT FALSE

            entity.Property(e => e.PlcGlc) // New column mapping
                  .HasColumnName("plc_glc")
                  .HasMaxLength(20);

            entity.Property(e => e.CreatedBy)
                  .HasColumnName("created_by")
                  .HasMaxLength(50);

            entity.Property(e => e.CreatedDate)
                  .HasColumnName("created_date")
                  //.HasColumnType("timestamp without time zone")
                  .HasDefaultValueSql("CURRENT_DATE"); // Use SQL function for default date

            entity.Property(e => e.LastModifiedBy)
                  .HasColumnName("last_modified_by")
                  .HasMaxLength(50);

            entity.Property(e => e.LastModifiedDate)
                  //.HasColumnType("timestamp without time zone")
                  .HasDefaultValueSql("CURRENT_DATE")
                  .HasColumnName("last_modified_date");

            entity.HasOne(d => d.Pl).WithMany(p => p.PlDct)
                    .HasForeignKey(d => d.PlId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("pl_dct_pkey");
        });

        // --- Table: pl_org_acct_pool_mapping ---
        modelBuilder.Entity<PlOrgAcctPoolMapping>()
            .ToTable("pl_org_acct_pool_mapping", "public");

        modelBuilder.Entity<PlOrgAcctPoolMapping>()
            .HasKey(p => p.Id);

        modelBuilder.Entity<PlOrgAcctPoolMapping>()
            .Property(p => p.Id)
            .ValueGeneratedOnAdd()
            .HasColumnName("id");

        modelBuilder.Entity<PlOrgAcctPoolMapping>()
            .Property(p => p.OrgId)
            .HasColumnName("org_id");

        modelBuilder.Entity<PlOrgAcctPoolMapping>()
            .Property(p => p.AccountId)
            .HasColumnName("account_id");

        modelBuilder.Entity<PlOrgAcctPoolMapping>()
            .Property(p => p.PoolId)
            .HasColumnName("pool_id");

        modelBuilder.Entity<PlOrgAcctPoolMapping>()
            .Property(p => p.ModifiedBy)
            .HasColumnName("modified_by");

        modelBuilder.Entity<PlOrgAcctPoolMapping>()
            .Property(p => p.Year)
            .HasColumnName("year");

        modelBuilder.Entity<PlOrgAcctPoolMapping>()
            .Property(p => p.CreatedAt)
            .HasColumnName("created_at");

        modelBuilder.Entity<PlOrgAcctPoolMapping>()
            .Property(p => p.UpdatedAt)
            .HasColumnName("updated_at");

        modelBuilder.Entity<PlOrgAcctPoolMapping>()
            .HasIndex(p => new { p.OrgId, p.AccountId, p.PoolId, p.Year })
            .IsUnique();

        modelBuilder.Entity<PlOrgAcctPoolMapping>()
            .HasOne(p => p.OrgAccount)
            .WithMany()
            .HasForeignKey(p => new { p.OrgId, p.AccountId })
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PlOrgAcctPoolMapping>()
            .HasOne(p => p.Pool)
            .WithMany()
            .HasForeignKey(p => p.PoolId)
            .OnDelete(DeleteBehavior.SetNull);


        // --- Table: pl_template_pool_rates ---
        modelBuilder.Entity<PlTemplatePoolRate>(entity =>
        {
            entity.ToTable("pl_template_pool_rates");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TemplateId).HasColumnName("template_id").IsRequired();
            entity.Property(e => e.PoolId).HasColumnName("pool_id").HasMaxLength(20);
            entity.Property(e => e.Year).HasColumnName("year").IsRequired();
            entity.Property(e => e.Month).HasColumnName("month").IsRequired();
            entity.Property(e => e.ActualRate).HasColumnName("actual_rate");
            entity.Property(e => e.TargetRate).HasColumnName("target_rate");
            entity.Property(e => e.ModifiedBy).HasColumnName("modified_by").HasMaxLength(30);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

            entity.HasIndex(x => new { x.TemplateId, x.PoolId, x.Year, x.Month })
            .IsUnique()
            .HasDatabaseName("ux_template_pool_year_month");

            // Configure the foreign key relationship to AccountGroup
            entity.HasOne(e => e.Pool)
                .WithMany(p => p.TemplatePoolRate)
                .HasForeignKey(e => e.PoolId)
                .HasPrincipalKey(p => p.Code)
                .OnDelete(DeleteBehavior.SetNull); // Or Cascade, whatever fits your case
        });

        modelBuilder.Entity<PlTemplatePoolMapping>(entity =>
        {
            entity.ToTable("pl_template_pool_mapping");

            // Composite Key
            entity.HasKey(e => new { e.TemplateId, e.PoolId });

            entity.Property(e => e.TemplateId).HasColumnName("template_id");
            entity.Property(e => e.PoolId).HasColumnName("pool_id").HasMaxLength(20);
            entity.Property(e => e.ModifiedBy).HasColumnName("modified_by").HasMaxLength(30);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Foreign Keys
            entity.HasOne(e => e.Template)
                .WithMany()
                .HasForeignKey(e => e.TemplateId)
                .HasConstraintName("fk_template_id")
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.Pool)
                .WithMany()
                .HasForeignKey(e => e.PoolId)
                .HasConstraintName("fk_pool_id")
                .OnDelete(DeleteBehavior.NoAction);
        });
        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("account");

            entity.HasKey(e => e.AcctId);

            entity.Property(e => e.AcctId).HasColumnName("acct_id").HasMaxLength(30);
            entity.Property(e => e.ActiveFlag).HasColumnName("active_fl").HasMaxLength(1);
            entity.Property(e => e.L1AcctName).HasColumnName("l1_acct_name").HasMaxLength(100);
            entity.Property(e => e.AcctName).HasColumnName("acct_name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.L1AcctName).HasColumnName("l1_acct_name").HasMaxLength(100);
            entity.Property(e => e.L2AcctName).HasColumnName("l2_acct_name").HasMaxLength(100);
            entity.Property(e => e.L3AcctName).HasColumnName("l3_acct_name").HasMaxLength(100);
            entity.Property(e => e.L4AcctName).HasColumnName("l4_acct_name").HasMaxLength(100);
            entity.Property(e => e.L5AcctName).HasColumnName("l5_acct_name").HasMaxLength(100);
            entity.Property(e => e.L6AcctName).HasColumnName("l6_acct_name").HasMaxLength(100);
            entity.Property(e => e.L7AcctName).HasColumnName("l7_acct_name").HasMaxLength(100);
            //entity.Property(e => e.ModifiedBy).HasColumnName("modified_by").HasMaxLength(100);
            entity.Property(e => e.LvlNo).HasColumnName("lvl_no");
            entity.Property(e => e.SAcctTypeCd).HasColumnName("s_acct_type_cd").HasMaxLength(10);
            //entity.Property(e => e.Createdat).HasColumnName("created_at");
            //entity.Property(e => e.Updatedat).HasColumnName("modified_at");
        });
        modelBuilder.Entity<Organization>(entity =>
        {
            entity.ToTable("organization");

            entity.HasKey(e => e.OrgId);

            entity.Property(e => e.OrgId).HasColumnName("org_id").HasMaxLength(30);
            entity.Property(e => e.OrgName).HasColumnName("org_name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.LvlNo).HasColumnName("lvl_no");

            entity.Property(e => e.L1OrgName).HasColumnName("l1_org_name").HasMaxLength(100);
            entity.Property(e => e.L2OrgName).HasColumnName("l2_org_name").HasMaxLength(100);
            entity.Property(e => e.L3OrgName).HasColumnName("l3_org_name").HasMaxLength(100);
            entity.Property(e => e.L4OrgName).HasColumnName("l4_org_name").HasMaxLength(100);
            entity.Property(e => e.L5OrgName).HasColumnName("l5_org_name").HasMaxLength(100);
            entity.Property(e => e.L6OrgName).HasColumnName("l6_org_name").HasMaxLength(100);
            entity.Property(e => e.L7OrgName).HasColumnName("l7_org_name").HasMaxLength(100);
            entity.Property(e => e.L8OrgName).HasColumnName("l8_org_name").HasMaxLength(100);
            entity.Property(e => e.L9OrgName).HasColumnName("l9_org_name").HasMaxLength(100);
        });


        // Prospective Entity
        modelBuilder.Entity<ProspectiveEntity>(entity =>
        {
            entity.ToTable("pl_prospective_entity");
            entity.HasKey(e => e.ProspectiveId);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50).HasColumnName("type");
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_date");
            entity.Property(e => e.IsActive).HasDefaultValue(true).HasColumnName("is_active");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");
            entity.Property(e => e.ModifiedBy).HasMaxLength(100).HasColumnName("modified_by");
            entity.Property(e => e.ProspectiveId).HasColumnName("prospective_id");
        });

        // Employee
        modelBuilder.Entity<EmployeeDetails>(entity =>
        {
            entity.ToTable("pl_employee_details");
            entity.HasKey(e => e.ProspectiveId);
            entity.Property(e => e.HrlyRate).HasColumnType("numeric(10,2)").HasColumnName("hrly_rate");
            entity.Property(e => e.PLC).HasMaxLength(50).HasColumnName("plc");
            entity.Property(e => e.Salary).HasColumnType("numeric(12,2)").HasColumnName("salary");
            entity.Property(e => e.HomeOrg).HasMaxLength(100).HasColumnName("home_org");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");
            entity.Property(e => e.ModifiedBy).HasMaxLength(100).HasColumnName("modified_by");
            entity.Property(e => e.ProspectiveId).HasColumnName("prospective_id");
        });

        // Vendor
        modelBuilder.Entity<VendorDetails>(entity =>
        {
            entity.ToTable("pl_vendor_details");
            entity.HasKey(e => e.ProspectiveId);
            entity.Property(e => e.VendorId).HasMaxLength(50).HasColumnName("vendor_id");
            entity.Property(e => e.VendorName).HasMaxLength(100).HasColumnName("vendor_name");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");
            entity.Property(e => e.ModifiedBy).HasMaxLength(100).HasColumnName("modified_by");
            entity.Property(e => e.ProspectiveId).HasColumnName("prospective_id");
        });

        // Vendor Employee
        modelBuilder.Entity<VendorEmployeeDetails>(entity =>
        {
            entity.ToTable("pl_vendor_employee_details");
            entity.HasKey(e => e.ProspectiveId);
            entity.Property(e => e.HrlyRate).HasColumnType("numeric(10,2)").HasColumnName("hrly_rate");
            entity.Property(e => e.PLC).HasMaxLength(50).HasColumnName("plc");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");
            entity.Property(e => e.ModifiedBy).HasMaxLength(100).HasColumnName("modified_by");
            entity.Property(e => e.ProspectiveId).HasColumnName("prospective_id");
        });

        // PLC
        modelBuilder.Entity<PLCDetails>(entity =>
        {
            entity.ToTable("pl_plc_details");
            entity.HasKey(e => e.ProspectiveId);
            entity.Property(e => e.Category).HasMaxLength(100).HasColumnName("category");
            entity.Property(e => e.HrlyRate).HasColumnType("numeric(10,2)").HasColumnName("hrly_rate");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");
            entity.Property(e => e.ModifiedBy).HasMaxLength(100).HasColumnName("modified_by");
            entity.Property(e => e.ProspectiveId).HasColumnName("prospective_id");
        });

        modelBuilder.Entity<NewBusinessBudget>(entity =>
        {
            entity.ToTable("pl_new_business_budget");

            entity.HasKey(e => e.BusinessBudgetId);

            entity.Property(e => e.BusinessBudgetId).HasColumnName("business_budget_id");
            entity.Property(e => e.Description).HasColumnName("description").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Level).HasColumnName("level").HasColumnType("numeric(10)");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.Version).HasColumnName("version").IsRequired();
            entity.Property(e => e.VersionCode).HasColumnName("version_code").HasMaxLength(50);
            entity.Property(e => e.StartDate).HasColumnName("start_date").IsRequired();
            entity.Property(e => e.EndDate).HasColumnName("end_date").IsRequired();
            entity.Property(e => e.EscalationRate).HasColumnName("escalation_rate").HasColumnType("numeric(5,2)");
            entity.Property(e => e.OrgId).HasColumnName("org_id").IsRequired();
            entity.Property(e => e.AccountGroup).HasColumnName("account_group").HasMaxLength(100);
            entity.Property(e => e.BurdenTemplateId).HasColumnName("burden_template");
            entity.Property(e => e.Status).HasColumnName("status"); 
            entity.Property(e => e.Trf_ProjId).HasColumnName("trf_projid");

            entity.Property(e => e.Stage).HasColumnName("stage");
            entity.Property(e => e.Customer).HasColumnName("customer");
            entity.Property(e => e.Type).HasColumnName("type"); 
            entity.Property(e => e.NBType).HasColumnName("nb_type"); 
            entity.Property(e => e.OurRole).HasColumnName("our_role");
            entity.Property(e => e.Workshare).HasColumnName("workshare");
            entity.Property(e => e.ContractValue).HasColumnName("contract_value");
            entity.Property(e => e.ContractTypes).HasColumnName("contract_types");
            entity.Property(e => e.PgoCalculation).HasColumnName("pgo_calculation");
            entity.Property(e => e.PwinValue).HasColumnName("pwin_value");

            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.ModifiedBy).HasColumnName("modified_by").HasMaxLength(100);

            entity.HasOne(e => e.BurdenTemplate)
                .WithMany()
                .HasForeignKey(e => e.BurdenTemplateId)
                .OnDelete(DeleteBehavior.Restrict);
        });


        // Generic Staff
        modelBuilder.Entity<GenericStaffDetails>(entity =>
        {
            entity.ToTable("pl_generic_staff_details");
            entity.HasKey(e => e.ProspectiveId);
            entity.Property(e => e.HrlyRate).HasColumnType("numeric(10,2)").HasColumnName("hrly_rate");
            entity.Property(e => e.PLC).HasMaxLength(50).HasColumnName("plc");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP").HasColumnName("updated_at");
            entity.Property(e => e.ModifiedBy).HasMaxLength(100).HasColumnName("modified_by");
            entity.Property(e => e.ProspectiveId).HasColumnName("prospective_id");
        });

        // Relationships
        modelBuilder.Entity<ProspectiveEntity>()
            .HasOne(p => p.EmployeeDetails)
            .WithOne(e => e.ProspectiveEntity)
            .HasForeignKey<EmployeeDetails>(e => e.ProspectiveId);

        modelBuilder.Entity<ProspectiveEntity>()
            .HasOne(p => p.VendorDetails)
            .WithOne(v => v.ProspectiveEntity)
            .HasForeignKey<VendorDetails>(v => v.ProspectiveId);

        modelBuilder.Entity<ProspectiveEntity>()
            .HasOne(p => p.VendorEmployeeDetails)
            .WithOne(v => v.ProspectiveEntity)
            .HasForeignKey<VendorEmployeeDetails>(v => v.ProspectiveId);

        modelBuilder.Entity<ProspectiveEntity>()
            .HasOne(p => p.PLCDetails)
            .WithOne(plc => plc.ProspectiveEntity)
            .HasForeignKey<PLCDetails>(plc => plc.ProspectiveId);

        modelBuilder.Entity<ProspectiveEntity>()
            .HasOne(p => p.GenericStaffDetails)
            .WithOne(g => g.ProspectiveEntity)
            .HasForeignKey<GenericStaffDetails>(g => g.ProspectiveId);


        modelBuilder.HasSequence("holidaycalender_id_seq");

        modelBuilder.Entity<AccountGroupSetup>(entity =>
        {
            entity.ToTable("account_group_setup");

            entity.HasKey(e => e.AcctGroupCode); // You may configure composite keys here if needed

            entity.Property(e => e.AcctGroupCode).HasColumnName("acct_grp_cd").HasMaxLength(50);
            entity.Property(e => e.AccountId).HasColumnName("acct_id").HasMaxLength(50);
            entity.Property(e => e.AccountFunctionDescription).HasColumnName("s_acct_func_dc").HasMaxLength(255);
            entity.Property(e => e.ModifiedBy).HasColumnName("modified_by").HasMaxLength(100);
            entity.Property(e => e.TimeStamp).HasColumnName("time_stamp").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.CompanyId).HasColumnName("company_id").HasMaxLength(50);
            entity.Property(e => e.ProjectAccountAbbreviation).HasColumnName("proj_acct_abbrv_cd").HasMaxLength(50);
            entity.Property(e => e.ActiveFlag).HasColumnName("active_fl").HasDefaultValue(true);
            entity.Property(e => e.RevenueMappedAccount).HasColumnName("rev_map_acct").HasMaxLength(50);
            entity.Property(e => e.SalaryCapMappedAccount).HasColumnName("salcap_map_acct").HasMaxLength(50);
            entity.HasOne(e => e.Account)
                  .WithOne(a => a.AccountGroupSetup)
                  .HasForeignKey<AccountGroupSetup>(e => e.AccountId)
                  .HasConstraintName("fk_accounts")
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PlCeilHrCat>(entity =>
        {
            entity.ToTable("pl_ceil_hr_cat");

            entity.HasKey(e => new { e.ProjectId, e.LaborCategoryId });

            entity.Property(e => e.ProjectId)
                .HasColumnName("project_id")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.LaborCategoryId)
                .HasColumnName("labor_category_id")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.HoursCeiling)
                .HasColumnName("hours_ceiling")
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(e => e.ApplyToRbaCode)
                .HasColumnName("apply_to_rba_code")
                .HasColumnType("char(1)");

            //// Optional: enforce valid RBA codes in code
            //entity.HasCheckConstraint("CK_PL_CEIL_HR_CAT_RBA",
            //    "\"Apply_to_RBA_Code\" IN ('R', 'B', 'A', 'N')");
        });

        modelBuilder.Entity<PlCeilHrEmpl>(entity =>
        {
            entity.ToTable("pl_ceil_hr_empl");

            entity.HasKey(e => new { e.ProjectId, e.EmployeeId, e.LaborCategoryId });

            entity.Property(e => e.ProjectId)
                .HasColumnName("project_id")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.EmployeeId)
                .HasColumnName("employee_id")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.LaborCategoryId)
                .HasColumnName("labor_category_id")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.HoursCeiling)
                .HasColumnName("hours_ceiling")
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(e => e.ApplyToRbaCode)
                .HasColumnName("apply_to_rba_code")
                .HasColumnType("char(1)");

            entity.HasCheckConstraint("ck_pl_ceil_hr_empl_rba",
                "\"apply_to_rba_code\" IN ('R', 'B', 'A', 'N')");
        });

        modelBuilder.Entity<PlCeilDirCst>(entity =>
        {
            entity.ToTable("pl_ceil_dir_cst");

            entity.HasKey(e => new { e.ProjectId, e.AccountId });

            entity.Property(e => e.ProjectId)
                .HasColumnName("project_id")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.AccountId)
                .HasColumnName("account_id")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.CeilingAmountFunc)
                .HasColumnName("ceiling_amount_func")
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(e => e.CeilingAmountBilling)
                .HasColumnName("ceiling_amount_billing")
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.ApplyToRbaCode)
                .HasColumnName("apply_to_rba_code")
                .HasColumnType("char(1)");

            entity.HasCheckConstraint("ck_pl_ceil_dir_cst_rba",
                "\"apply_to_rba_code\" IN ('R', 'B', 'A', 'N')");
        });

        modelBuilder.Entity<PlCeilBurden>(entity =>
        {
            entity.ToTable("pl_ceil_burden");

            entity.HasKey(e => new { e.ProjectId, e.FiscalYear, e.AccountId, e.PoolCode });

            entity.Property(e => e.ProjectId)
                .HasColumnName("project_id")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.FiscalYear)
                .HasColumnName("fiscal_year")
                .HasMaxLength(10)
                .IsRequired();

            entity.Property(e => e.AccountId)
                .HasColumnName("account_id")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.PoolCode)
                .HasColumnName("pool_code")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.RateCeiling)
                .HasColumnName("rate_ceiling")
                .HasColumnType("decimal(8,4)");

            entity.Property(e => e.RateFormat)
                .HasColumnName("rate_format")
                .HasMaxLength(10);

            entity.Property(e => e.ComCeiling)
                .HasColumnName("com_ceiling")
                .HasColumnType("decimal(8,4)");

            entity.Property(e => e.ComFormat)
                .HasColumnName("com_format")
                .HasMaxLength(10);

            entity.Property(e => e.CeilingMethodCode)
                .HasColumnName("ceiling_method_code")
                .HasColumnType("char(1)");

            entity.Property(e => e.ApplyToRbaCode)
                .HasColumnName("apply_to_rba_code")
                .HasColumnType("char(1)");

            entity.HasCheckConstraint("ck_pl_ceil_burden_method",
                "\"ceiling_method_code\" IN ('C', 'O', 'F')");

            entity.HasCheckConstraint("ck_pl_ceil_burden_rba",
                "\"apply_to_rba_code\" IN ('R', 'B', 'A', 'N')");
        });

        modelBuilder.Entity<PlCeilProjTotal>(entity =>
        {
            entity.ToTable("pl_ceil_proj_total");

            entity.HasKey(e => e.ProjectId);

            entity.Property(e => e.ProjectId)
                .HasColumnName("project_id")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(e => e.CostCeiling)
                .HasColumnName("cost_ceiling")
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.FeeCeiling)
                .HasColumnName("fee_ceiling")
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.TotalValueCeiling)
                .HasColumnName("total_value_ceiling")
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.CeilingCode)
                .HasColumnName("ceiling_code")
                .HasColumnType("char(1)");

            entity.HasCheckConstraint("ck_pl_ceil_proj_total_code",
                "\"ceiling_code\" IN ('R', 'B', 'A', 'N')");
        });

        modelBuilder.Entity<LabHours>(entity =>
        {
            entity.ToTable("lab_hours");

            entity.HasKey(e => e.LabHsKey);
            entity.Property(e => e.LabHsKey).HasColumnName("lab_hs_key");

            entity.Property(e => e.ProjId).HasColumnName("proj_id");
            entity.Property(e => e.OrgId).HasColumnName("org_id");
            entity.Property(e => e.AcctId).HasColumnName("acct_id");
            entity.Property(e => e.FyCd).HasColumnName("fy_cd");
            entity.Property(e => e.PdNo).HasColumnName("pd_no");
            entity.Property(e => e.SubPdNo).HasColumnName("sub_pd_no");
            entity.Property(e => e.BillLabCatCd).HasColumnName("bill_lab_cat_cd");
            entity.Property(e => e.ActHrs).HasColumnName("act_hrs");
            entity.Property(e => e.AllowRevHrs).HasColumnName("allow_rev_hrs");
            entity.Property(e => e.RevRtAmt).HasColumnName("rev_rt_amt");
            entity.Property(e => e.GenlLabCatCd).HasColumnName("genl_lab_cat_cd");
            entity.Property(e => e.ModifiedBy).HasColumnName("modified_by");
            entity.Property(e => e.TimeStamp).HasColumnName("time_stamp");
            entity.Property(e => e.ActAmt).HasColumnName("act_amt");
            entity.Property(e => e.EmplId).HasColumnName("empl_id");
            entity.Property(e => e.VendId).HasColumnName("vend_id");
            entity.Property(e => e.RecalcRevFl).HasColumnName("recalc_rev_fl");
            entity.Property(e => e.VendEmplId).HasColumnName("vend_empl_id");
            entity.Property(e => e.SBillRtTypeCd).HasColumnName("s_bill_rt_type_cd");
            entity.Property(e => e.CurMultRt).HasColumnName("cur_mult_rt");
            entity.Property(e => e.YtdMultRt).HasColumnName("ytd_mult_rt");
            entity.Property(e => e.EffectBillDt).HasColumnName("effect_bill_dt");
            entity.Property(e => e.RowVersion).HasColumnName("rowversion");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
            entity.Property(e => e.OrigRevRtAmt).HasColumnName("orig_rev_rt_amt");
            //entity.Property(e => e.PerHrRt).HasColumnName("perhrrate");
        });

        modelBuilder.Entity<PSRFinalData>(entity =>
        {
            entity.ToTable("psr_final_data"); // ← renamed here
            entity.HasKey(e => new { e.ProjId, e.AcctId, e.FyCd, e.PdNo });
            entity.Property(e => e.ProjId).HasColumnName("proj_id");
            entity.Property(e => e.AcctId).HasColumnName("acct_id");
            entity.Property(e => e.OrgId).HasColumnName("org_id");
            entity.Property(e => e.FyCd).HasColumnName("fy_cd");
            entity.Property(e => e.PdNo).HasColumnName("pd_no");
            entity.Property(e => e.SubPdNo).HasColumnName("sub_pd_no");
            entity.Property(e => e.PoolNo).HasColumnName("pool_no");
            entity.Property(e => e.PoolName).HasColumnName("pool_name");
            entity.Property(e => e.SubTotTypeNo).HasColumnName("sub_tot_type_no");
            entity.Property(e => e.RateType).HasColumnName("rate_type");
            entity.Property(e => e.PyIncurAmt).HasColumnName("py_incur_amt");
            entity.Property(e => e.SubIncurAmt).HasColumnName("sub_incur_amt");
            entity.Property(e => e.PtdIncurAmt).HasColumnName("ptd_incur_amt");
            entity.Property(e => e.YtdIncurAmt).HasColumnName("ytd_incur_amt");
            entity.Property(e => e.CurBurdRt).HasColumnName("cur_burd_rt");
            entity.Property(e => e.YtdBurdRt).HasColumnName("ytd_burd_rt");
            entity.Property(e => e.ModifiedBy).HasColumnName("modified_by");
            entity.Property(e => e.TimeStamp).HasColumnName("time_stamp");
            entity.Property(e => e.Rowversion).HasColumnName("rowversion");
            entity.Property(e => e.CompanyId).HasColumnName("company_id");
        });


        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
