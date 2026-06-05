using Dapper;
using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Protocol;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using Npgsql;
using NPOI.OpenXmlFormats.Dml.Diagram;
using NPOI.POIFS.FileSystem;
using NPOI.SS.Formula.Atp;
using NPOI.SS.Formula.Functions;
using PlanningAPI.Models;
using QuestPDF.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Security.Cryptography.Xml;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebApi.DTO;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models;
using WebApi.Services;
using YourNamespace.Models;
using static NPOI.HSSF.Util.HSSFColor;
using static QuestPDF.Helpers.Colors;

public interface IProjPlanRepository
{

    Task<IEnumerable<PlProjectPlan>> GetProjectPlans(string projectID);
    //Task<IEnumerable<PlProjectPlan>> GetProjectPlans(int UserId, string Role, string projectID, string? status);
    Task<IEnumerable<PlProjectPlan>> GetProjectPlans(int UserId, string Role, string projectID, string? status, string fetchNewBussiness);
    Task<IEnumerable<PlProjectPlan>> GetProjectPlansV1(int UserId, string Role, string projectID, string? status, string type);
    Task<PagedResponse<PlProjectPlan>> GetProjectPlansPaged(int userId, string role, string projectID, string? status, string? planstatus, string? planType, string fetchNewBussiness, int pageNumber, int pageSize);
    //Task<PagedResult<PlProjectPlan>> GetProjectPlans(int userId, string role, string? projectId, int pageNumber, int pageSize);
    Task<IEnumerable<forecast>> GetForecastByPlanID(int planID);

    Task<IEnumerable<forecast>> GetEmployeeForecastByPlanID(int planID, int year);
    Task<IEnumerable<forecast>> GetEmployeeForecastByPlanID(int planID, int? year, string? emplid, int pageNumber, int pageSize);
    Task<PlProjectPlan> AddProjectPlanAsync(PlProjectPlan newPlan, string type);

    Task<PlProjectPlan> AddPBudgetFromNewBussinessAsync(NewBusinessBudgetDTO newPlan, string SourceProject);
    Task<bool> UpdateProjectPlanAsync(PlProjectPlan updatedPlan);
    Task<bool> BulkUpdateProjectPlansAsync(List<PlProjectPlan> updatedPlans);
    Task<bool> DeleteProjectPlanAsync(int planId);
    Task<IEnumerable<forecast>> GetEACDataByPlanId(int planID);
    Task<IEnumerable<Account>> GetAccountsByProjectId(string projId);
    Task<IEnumerable<PlEmployee>> GetEmployeesByProjectId(string projId);
    Task<IEnumerable<DirectCostforecast>> GetDirectCostForecastByPlanID(int planID, int year);
    Task<IEnumerable<DirectCostforecast>> GetDirectCostForecastByPlanID(int planID, int? year, string? emplid, int pageNumber, int pageSize);
    Task<IEnumerable<PlProjectPlan>> GetAllNewBussiness(string nbId);
    Task<IEnumerable<DirectCostforecast>> GetDirectCostEACDataByPlanId(int planId, int year);
}

public class ProjPlanRepository : IProjPlanRepository
{
    //private DataContext _context;
    private readonly MydatabaseContext _context;
    private readonly IConfiguration _configuration;
    IOrgService _orgService;
    IEmplService _emplService;
    IPl_ForecastService _pl_ForecastService;
    ScheduleHelper sheduleHelper = new ScheduleHelper();

    public ProjPlanRepository(MydatabaseContext context, IOrgService orgService, IEmplService emplService, IPl_ForecastService pl_ForecastService, IConfiguration configuration)
    {
        _context = context;
        _orgService = orgService;
        _emplService = emplService;
        _pl_ForecastService = pl_ForecastService;
        _configuration = configuration;
    }

    public Task<IEnumerable<PlProjectPlan>> GetProjectPlans(string projectID)
    {
        //var result = _context.ProjectWithPlanDto
        //                .FromSqlRaw(@"
        //                        SELECT
        //                    p.proj_id AS ProjId,
        //                    p.acct_grp_cd AS AcctGrpCd,
        //                 p.proj_name AS ProjName,
        //                 p.proj_end_dt AS ProjEndDt,
        //                 p.org_id AS OrgId,
        //                    p.notes AS Description,
        //                 p.proj_start_dt AS ProjStartDt,
        //                 p.proj_f_tot_amt AS proj_f_tot_amt,
        //                 p.proj_f_cst_amt AS proj_f_cst_amt,
        //                 p.proj_f_fee_amt AS proj_f_fee_amt,
        //                    p.proj_type_dc AS ProjType,
        //                    pp.pl_id as PlId,
        //                    pp.pl_type as PlType,
        //                    pp.version as Version,
        //                    pp.version_code as VersionCode,
        //                    pp.final_version as FinalVersion,
        //                    pp.is_completed as IsCompleted,
        //                    pp.is_approved as IsApproved,
        //                    pp.status as Status,
        //                    pp.closed_period as ClosedPeriod,
        //                    pp.created_at as CreatedAt,
        //                    pp.updated_at as UpdatedAt,
        //                    pp.modified_by as ModifiedBy,
        //                    pp.approved_by as ApprovedBy,
        //                    pp.created_by as CreatedBy,
        //                    pp.source as Source,
        //                    pp.type as Type,
        //                    pp.burden_template_id as BurdenTemplateId
        //                FROM
        //                    public.pl_project p

        //                LEFT JOIN
        //                    public.pl_project_plan pp ON p.proj_id = pp.proj_id
        //                 WHERE p.proj_id LIKE {0}", projectID + "%")
        //            .ToList();

        //var result = _context.ProjectWithPlanDto
        //    .FromSqlRaw(@"
        //SELECT
        //    COALESCE(p.proj_id, nb.business_budget_id, pp.proj_id) AS ProjId,
        //    p.acct_grp_cd AS AcctGrpCd,
        //    p.proj_name AS ProjName,
        //    COALESCE(p.proj_end_dt, nb.end_date) AS ProjEndDt,
        //    p.org_id AS OrgId,
        //    p.notes AS Description,
        //    COALESCE(p.proj_start_dt, nb.start_date) AS ProjStartDt,
        //    COALESCE(p.proj_f_tot_amt, 0) AS proj_f_tot_amt,
        //    COALESCE(p.proj_f_cst_amt, 0) AS proj_f_cst_amt,
        //    COALESCE(p.proj_f_fee_amt, 0) AS proj_f_fee_amt,
        //    p.proj_type_dc AS ProjType,

        //    pp.pl_id AS PlId,
        //    pp.pl_type AS PlType,
        //    pp.version AS Version,
        //    pp.version_code AS VersionCode,
        //    pp.final_version AS FinalVersion,
        //    pp.is_completed AS IsCompleted,
        //    pp.is_approved AS IsApproved,
        //    pp.status AS Status,
        //    pp.closed_period AS ClosedPeriod,
        //    pp.created_at AS CreatedAt,
        //    pp.updated_at AS UpdatedAt,
        //    pp.modified_by AS ModifiedBy,
        //    pp.approved_by AS ApprovedBy,
        //    pp.created_by AS CreatedBy,
        //    pp.source AS Source,
        //    pp.type AS Type,
        //    pp.burden_template_id AS BurdenTemplateId
        //FROM public.pl_project_plan pp
        //LEFT JOIN public.pl_project p 
        //    ON pp.proj_id = p.proj_id
        //LEFT JOIN public.pl_new_business_budget nb
        //    ON pp.proj_id = nb.business_budget_id
        //WHERE pp.proj_id LIKE {0}", projectID + "%")
        //    .ToList();



        var result = _context.ProjectWithPlanDto
            .FromSqlRaw(@"
        SELECT
            p.proj_id AS ProjId,
            p.proj_type_dc AS ProjType,
            pp.pl_id AS PlId,
            pp.pl_type AS PlType,
            pp.version AS Version,
            pp.version_code AS VersionCode,
            pp.final_version AS FinalVersion,
            pp.is_completed AS IsCompleted,
            pp.is_approved AS IsApproved,
            pp.status AS Status,
            pp.closed_period AS ClosedPeriod,
            pp.created_at AS CreatedAt,
            pp.updated_at AS UpdatedAt,
            pp.modified_by AS ModifiedBy,
            pp.approved_by AS ApprovedBy,
            pp.created_by AS CreatedBy,
            pp.source AS Source,
            pp.type AS Type,
            pp.burden_template_id AS BurdenTemplateId,
            COALESCE(p.proj_f_fee_amt,0) AS proj_f_fee_amt,
            COALESCE(p.proj_f_cst_amt,0) AS proj_f_cst_amt,
            COALESCE(p.proj_f_tot_amt,0) AS proj_f_tot_amt,
            p.proj_name AS ProjName,
            p.org_id AS OrgId,
            p.proj_end_dt AS ProjEndDt,
            p.proj_start_dt AS ProjStartDt,
            p.acct_grp_cd AS AcctGrpCd
        FROM pl_project p
        LEFT JOIN pl_project_plan pp ON pp.proj_id = p.proj_id
        WHERE p.proj_id LIKE @p0

        UNION ALL

        SELECT
            nb.business_budget_id AS ProjId,
            NULL AS ProjType,
            pp.pl_id AS PlId,
            pp.pl_type AS PlType,
            pp.version AS Version,
            pp.version_code AS VersionCode,
            pp.final_version AS FinalVersion,
            pp.is_completed AS IsCompleted,
            pp.is_approved AS IsApproved,
            pp.status AS Status,
            pp.closed_period AS ClosedPeriod,
            pp.created_at AS CreatedAt,
            pp.updated_at AS UpdatedAt,
            pp.modified_by AS ModifiedBy,
            pp.approved_by AS ApprovedBy,
            pp.created_by AS CreatedBy,
            pp.source AS Source,
            pp.type AS Type,
            pp.burden_template_id AS BurdenTemplateId,
            0 AS proj_f_fee_amt,
            0 AS proj_f_cst_amt,
            0 AS proj_f_tot_amt,
            nb.business_budget_id AS ProjName,
            NULL AS OrgId,
            nb.end_date AS ProjEndDt,
            nb.start_date AS ProjStartDt,
            NULL AS AcctGrpCd
        FROM pl_new_business_budget nb
        LEFT JOIN pl_project_plan pp ON pp.proj_id = nb.business_budget_id
        WHERE nb.business_budget_id LIKE @p0
          AND nb.business_budget_id NOT IN (SELECT proj_id FROM pl_project)
    ", projectID + "%")
            .ToList();

        var validDescriptions = new List<string> { "REVENUE" }; //plans.Select(p=>p.ProjId).ToList()

        var plans = (IEnumerable<PlProjectPlan>)result.Select(p => p.ToEntity()).ToList();


        var planIds = plans.Select(p => p.AcctGrpCd).Distinct().ToList();

        var accountGroupSetupDTOs = (from ags in _context.AccountGroupSetup
                                     join a in _context.Accounts
                                         on ags.AccountId equals a.AcctId
                                     where planIds.Contains(ags.AcctGroupCode)
                                        && validDescriptions.Contains(ags.AccountFunctionDescription.ToUpper())
                                     select new AccountGroupSetupDTO
                                     {
                                         AccountId = ags.AccountId,
                                         AccountFunctionDescription = ags.AcctGroupCode,
                                         AcctName = a.AcctName
                                     }).ToList();

        var accountGroupLookup = accountGroupSetupDTOs
    .GroupBy(dto => dto.AccountFunctionDescription)
    .ToDictionary(g => g.Key, g => g.First().AccountId);

        var summary = _context.ProjForecastSummary.ToList(); // Load the DbSet into memory

        // Update plans based on lookup
        foreach (var plan in plans)
        {
            plan.Revenue = summary.Where(s => s.ProjId == plan.ProjId && s.PlType == plan.PlType && s.Version == plan.Version).Sum(z => z.MonthlyRevenue);

            if (plan.AcctGrpCd != null && accountGroupLookup.TryGetValue(plan.AcctGrpCd, out var accountId))
            {
                plan.RevenueAccount = accountId;
            }
        }

        return Task.FromResult(plans);

    }

    //public async Task<IEnumerable<PlProjectPlan>> GetProjectPlans(int UserId, string Role, string projectID, string? status, string fetchNewBussiness)
    //{
    //    string sqlQuery = "";
    //    string[] projIds = Array.Empty<string>();

    //    if (Role.ToUpper() == "ADMIN")
    //    {
    //        if (fetchNewBussiness.Trim().ToUpper() == "Y")
    //        {
    //            sqlQuery = @"
    //                SELECT
    //                    p.proj_id AS ProjId,
    //                    p.proj_type_dc AS ProjType,
    //                    pp.pl_id AS PlId,
    //                    pp.pl_type AS PlType,
    //                    pp.version AS Version,
    //                    pp.version_code AS VersionCode,
    //                    pp.final_version AS FinalVersion,
    //                    pp.is_completed AS IsCompleted,
    //                    pp.is_approved AS IsApproved,
    //                    pp.status AS Status,
    //                    pp.closed_period AS ClosedPeriod,
    //                    pp.created_at AS CreatedAt,
    //                    pp.updated_at AS UpdatedAt,
    //                    pp.modified_by AS ModifiedBy,
    //                    pp.approved_by AS ApprovedBy,
    //                    pp.created_by AS CreatedBy,
    //                    pp.source AS Source,
    //                    pp.type AS Type,
    //                    pp.burden_template_id AS BurdenTemplateId,
    //                    COALESCE(p.proj_f_fee_amt,0) AS proj_f_fee_amt,
    //                    COALESCE(p.proj_f_cst_amt,0) AS proj_f_cst_amt,
    //                    COALESCE(p.proj_f_tot_amt,0) AS proj_f_tot_amt,
    //                    p.proj_name AS ProjName,
    //                    p.org_id AS OrgId,
    //                    COALESCE(pp.proj_start_dt, p.proj_start_dt) AS ProjStartDt,
    //                    COALESCE(pp.proj_end_dt,   p.proj_end_dt)   AS ProjEndDt,
    //                    p.acct_grp_cd AS AcctGrpCd,
    //                    p.active_fl AS ProjectStatus
    //                FROM pl_project p
    //                LEFT JOIN pl_project_plan pp ON pp.proj_id = p.proj_id
    //                WHERE p.proj_id ILIKE @projPrefix

    //                UNION ALL

    //                SELECT
    //                    nb.business_budget_id AS ProjId,
    //                    nb.description AS ProjType,
    //                    pp.pl_id AS PlId,
    //                    pp.pl_type AS PlType,
    //                    pp.version AS Version,
    //                    pp.version_code AS VersionCode,
    //                    pp.final_version AS FinalVersion,
    //                    pp.is_completed AS IsCompleted,
    //                    pp.is_approved AS IsApproved,
    //                    pp.status AS Status,
    //                    pp.closed_period AS ClosedPeriod,
    //                    pp.created_at AS CreatedAt,
    //                    pp.updated_at AS UpdatedAt,
    //                    pp.modified_by AS ModifiedBy,
    //                    pp.approved_by AS ApprovedBy,
    //                    pp.created_by AS CreatedBy,
    //                    pp.source AS Source,
    //                    'NBBUD' AS Type,
    //                    pp.burden_template_id AS BurdenTemplateId,
    //                    0 AS proj_f_fee_amt,
    //                    0 AS proj_f_cst_amt,
    //                    0 AS proj_f_tot_amt,
    //                    nb.description AS ProjName,
    //                    NULL AS OrgId,
    //                    COALESCE(pp.proj_start_dt, nb.start_date) AS ProjStartDt,
    //                    COALESCE(pp.proj_end_dt, nb.end_date)   AS ProjEndDt,
    //                    NULL AS AcctGrpCd,
    //                    NULL AS ProjectStatus
    //                FROM pl_new_business_budget nb
    //                LEFT JOIN pl_project_plan pp ON pp.proj_id = nb.business_budget_id
    //                WHERE nb.business_budget_id ILIKE @projPrefix
    //                  AND nb.business_budget_id NOT IN (SELECT proj_id FROM pl_project)
    //            ";
    //        }
    //        else
    //        {
    //            sqlQuery = @"
    //                SELECT
    //                    p.proj_id AS ProjId,
    //                    p.proj_type_dc AS ProjType,
    //                    pp.pl_id AS PlId,
    //                    pp.pl_type AS PlType,
    //                    pp.version AS Version,
    //                    pp.version_code AS VersionCode,
    //                    pp.final_version AS FinalVersion,
    //                    pp.is_completed AS IsCompleted,
    //                    pp.is_approved AS IsApproved,
    //                    pp.status AS Status,
    //                    pp.closed_period AS ClosedPeriod,
    //                    pp.created_at AS CreatedAt,
    //                    pp.updated_at AS UpdatedAt,
    //                    pp.modified_by AS ModifiedBy,
    //                    pp.approved_by AS ApprovedBy,
    //                    pp.created_by AS CreatedBy,
    //                    pp.source AS Source,
    //                    pp.type AS Type,
    //                    pp.burden_template_id AS BurdenTemplateId,
    //                    COALESCE(p.proj_f_fee_amt,0) AS proj_f_fee_amt,
    //                    COALESCE(p.proj_f_cst_amt,0) AS proj_f_cst_amt,
    //                    COALESCE(p.proj_f_tot_amt,0) AS proj_f_tot_amt,
    //                    p.proj_name AS ProjName,
    //                    p.org_id AS OrgId,
    //                    COALESCE(pp.proj_start_dt, p.proj_start_dt) AS ProjStartDt,
    //                    COALESCE(pp.proj_end_dt,   p.proj_end_dt)   AS ProjEndDt,
    //                    p.acct_grp_cd AS AcctGrpCd,
    //                    p.active_fl AS ProjectStatus
    //                FROM pl_project p
    //                LEFT JOIN pl_project_plan pp ON pp.proj_id = p.proj_id
    //                WHERE p.proj_id ILIKE @projPrefix ";
    //        }
    //    }
    //    else
    //    {
    //        //var projIds = _context.UserProjectMaps.Where(u => u.UserId == UserId).Select(u => u.ProjId).ToList();
    //        //var p1 = string.Join(",", projIds.Select(v => $"'{v}'"));
    //        projIds = await _context.UserProjectMaps
    //                .Where(u => u.UserId == UserId)
    //                .Select(u => u.ProjId)
    //                .ToArrayAsync();
    //        if (fetchNewBussiness.Trim().ToUpper() == "Y")
    //        {
    //            sqlQuery = @"
    //                SELECT
    //                    p.proj_id AS ProjId,
    //                    p.proj_type_dc AS ProjType,
    //                    pp.pl_id AS PlId,
    //                    pp.pl_type AS PlType,
    //                    pp.version AS Version,
    //                    pp.version_code AS VersionCode,
    //                    pp.final_version AS FinalVersion,
    //                    pp.is_completed AS IsCompleted,
    //                    pp.is_approved AS IsApproved,
    //                    pp.status AS Status,
    //                    pp.closed_period AS ClosedPeriod,
    //                    pp.created_at AS CreatedAt,
    //                    pp.updated_at AS UpdatedAt,
    //                    pp.modified_by AS ModifiedBy,
    //                    pp.approved_by AS ApprovedBy,
    //                    pp.created_by AS CreatedBy,
    //                    pp.source AS Source,
    //                    pp.type AS Type,
    //                    pp.burden_template_id AS BurdenTemplateId,
    //                    COALESCE(p.proj_f_fee_amt,0) AS proj_f_fee_amt,
    //                    COALESCE(p.proj_f_cst_amt,0) AS proj_f_cst_amt,
    //                    COALESCE(p.proj_f_tot_amt,0) AS proj_f_tot_amt,
    //                    p.proj_name AS ProjName,
    //                    p.org_id AS OrgId,
    //                    COALESCE(pp.proj_start_dt, p.proj_start_dt) AS ProjStartDt,
    //                    COALESCE(pp.proj_end_dt,   p.proj_end_dt)   AS ProjEndDt,
    //                    p.acct_grp_cd AS AcctGrpCd,
    //                    p.active_fl AS ProjectStatus
    //                FROM pl_project p
    //                LEFT JOIN pl_project_plan pp ON pp.proj_id = p.proj_id
    //                WHERE p.proj_id = ANY(@projIds)
    //                  AND p.proj_id ILIKE @projPrefix

    //                UNION ALL

    //                SELECT
    //                    nb.business_budget_id AS ProjId,
    //                    nb.description AS ProjType,
    //                    pp.pl_id AS PlId,
    //                    pp.pl_type AS PlType,
    //                    pp.version AS Version,
    //                    pp.version_code AS VersionCode,
    //                    pp.final_version AS FinalVersion,
    //                    pp.is_completed AS IsCompleted,
    //                    pp.is_approved AS IsApproved,
    //                    pp.status AS Status,
    //                    pp.closed_period AS ClosedPeriod,
    //                    pp.created_at AS CreatedAt,
    //                    pp.updated_at AS UpdatedAt,
    //                    pp.modified_by AS ModifiedBy,
    //                    pp.approved_by AS ApprovedBy,
    //                    pp.created_by AS CreatedBy,
    //                    pp.source AS Source,
    //                    pp.type AS Type,
    //                    pp.burden_template_id AS BurdenTemplateId,
    //                    0 AS proj_f_fee_amt,
    //                    0 AS proj_f_cst_amt,
    //                    0 AS proj_f_tot_amt,
    //                    nb.description AS ProjName,
    //                    NULL AS OrgId,
    //                    COALESCE(pp.proj_start_dt, nb.start_date) AS ProjStartDt,
    //                    COALESCE(pp.proj_end_dt, nb.end_date)   AS ProjEndDt,
    //                    NULL AS AcctGrpCd,
    //                    NULL AS ProjectStatus
    //                FROM pl_new_business_budget nb
    //                LEFT JOIN pl_project_plan pp ON pp.proj_id = nb.business_budget_id
    //                WHERE nb.business_budget_id ILIKE @projPrefix
    //                  AND nb.business_budget_id NOT IN (SELECT proj_id FROM pl_project)
    //                                ";
    //        }
    //        else
    //        {
    //            sqlQuery = @"
    //                SELECT
    //                    p.proj_id AS ProjId,
    //                    p.proj_type_dc AS ProjType,
    //                    pp.pl_id AS PlId,
    //                    pp.pl_type AS PlType,
    //                    pp.version AS Version,
    //                    pp.version_code AS VersionCode,
    //                    pp.final_version AS FinalVersion,
    //                    pp.is_completed AS IsCompleted,
    //                    pp.is_approved AS IsApproved,
    //                    pp.status AS Status,
    //                    pp.closed_period AS ClosedPeriod,
    //                    pp.created_at AS CreatedAt,
    //                    pp.updated_at AS UpdatedAt,
    //                    pp.modified_by AS ModifiedBy,
    //                    pp.approved_by AS ApprovedBy,
    //                    pp.created_by AS CreatedBy,
    //                    pp.source AS Source,
    //                    pp.type AS Type,
    //                    pp.burden_template_id AS BurdenTemplateId,
    //                    COALESCE(p.proj_f_fee_amt,0) AS proj_f_fee_amt,
    //                    COALESCE(p.proj_f_cst_amt,0) AS proj_f_cst_amt,
    //                    COALESCE(p.proj_f_tot_amt,0) AS proj_f_tot_amt,
    //                    p.proj_name AS ProjName,
    //                    p.org_id AS OrgId,
    //                    COALESCE(pp.proj_start_dt, p.proj_start_dt) AS ProjStartDt,
    //                    COALESCE(pp.proj_end_dt,   p.proj_end_dt)   AS ProjEndDt,
    //                    p.acct_grp_cd AS AcctGrpCd,
    //                    p.active_fl AS ProjectStatus
    //                FROM pl_project p
    //                LEFT JOIN pl_project_plan pp ON pp.proj_id = p.proj_id
    //                WHERE p.proj_id = ANY(@projIds)
    //                AND p.proj_id ILIKE @projPrefix ";
    //        }
    //    }
    //    List<ProjectWithPlanDto> result;
    //    try
    //    {

    //        result = await _context.ProjectWithPlanDto
    //                 .FromSqlRaw(
    //                             sqlQuery,
    //                             new NpgsqlParameter("@projIds", projIds),
    //                             new NpgsqlParameter("@projPrefix", projectID + "%")
    //                         )
    //                         .AsNoTracking()
    //             .ToListAsync();
    //    }
    //    catch (Exception ex)
    //    {
    //        throw ex;
    //    }

    //    if (!string.IsNullOrEmpty(status))
    //    {
    //        result = result.Where(p => p.ProjectStatus == status).ToList();
    //    }

    //    //var projectsWithoutStartEndDate = result.Where(p => !p.ProjStartDt.HasValue || !p.ProjEndDt.HasValue)
    //    //    .Select(p => p.ProjId)
    //    //    .Distinct()
    //    //    .ToList();

    //    //var allprojs = _context.ProjectModifications.Where(p => projectsWithoutStartEndDate.Contains(p.ProjId));

    //    //var definitations = _context.ProjRevDefinitions.ToList();


    //    //foreach (var proj in projectsWithoutStartEndDate)
    //    //{

    //    //    var parts = proj.Split('.', StringSplitOptions.RemoveEmptyEntries);
    //    //    var prefixes = Enumerable
    //    //        .Range(1, parts.Length - 1)
    //    //        .Select(i => string.Join('.', parts.Take(i)))
    //    //        .ToList();

    //    //    var revenuelevel = definitations
    //    //        .Select(z =>
    //    //            z.ProjectId.Length
    //    //            - z.ProjectId.Replace(".", "").Length
    //    //            + 1
    //    //        )
    //    //        .Distinct()
    //    //        .ToList();

    //    //    //if (proj == "22003.01.100300.0000")
    //    //    //{

    //    //    //}
    //    //    //var projectDetails = result.Where(p => p.ProjId.StartsWith(proj) && p.ProjStartDt != null).ToList();
    //    //    //if (projectDetails != null && projectDetails.Count > 0)
    //    //    //{
    //    //    //    var projStartDt = projectDetails.Min(p => p.ProjStartDt);
    //    //    //    var projEndDt = projectDetails.Max(p => p.ProjEndDt);
    //    //    //    foreach (var plan in result.Where(p => p.ProjId == proj))
    //    //    //    {
    //    //    //        plan.ProjStartDt = projStartDt;
    //    //    //        plan.ProjEndDt = projEndDt;
    //    //    //    }
    //    //    //}
    //    //}

    //    //projectsWithoutStartEndDate = result.Where(p => !p.ProjStartDt.HasValue || !p.ProjEndDt.HasValue)
    //    //    .Select(p => p.ProjId)
    //    //    .Distinct()
    //    //    .ToList();

    //    var validDescriptions = new List<string> { "REVENUE" }; //plans.Select(p=>p.ProjId).ToList()

    //    var plans = (IEnumerable<PlProjectPlan>)result.Select(p => p.ToEntity()).ToList();


    //    var planIds = plans.Select(p => p.AcctGrpCd).Distinct().ToList();

    //    var accountGroupSetupDTOs = (from ags in _context.AccountGroupSetup
    //                                 join a in _context.Accounts
    //                                     on ags.AccountId equals a.AcctId
    //                                 where planIds.Contains(ags.AcctGroupCode)
    //                                    && validDescriptions.Contains(ags.AccountFunctionDescription.ToUpper())
    //                                 select new AccountGroupSetupDTO
    //                                 {
    //                                     AccountId = ags.AccountId,
    //                                     AccountFunctionDescription = ags.AcctGroupCode,
    //                                     AcctName = a.AcctName
    //                                 }).ToList();

    //    var accountGroupLookup = accountGroupSetupDTOs
    //        .GroupBy(dto => dto.AccountFunctionDescription)
    //        .ToDictionary(g => g.Key, g => g.First().AccountId);

    //    //var summary = _context.ProjForecastSummary.ToList(); // Load the DbSet into memory

    //    var summary = _context.PlForecasts.Where(p => plans.Select(r => r.PlId).Contains(p.PlId)).ToList();
    //    var revenuesummary = summary.GroupBy(p => new { p.ProjId, p.PlId })
    //        .Select(g => new
    //        {
    //            g.Key.ProjId,
    //            g.Key.PlId,
    //            MonthlyRevenue = g.Sum(x => x.Revenue)
    //        }).ToList();




    //    // Update plans based on lookup
    //    foreach (var plan in plans)
    //    {
    //        try
    //        {
    //            plan.Revenue = revenuesummary.FirstOrDefault(s => s.ProjId == plan.ProjId && s.PlId == plan.PlId)?.MonthlyRevenue ?? 0;
    //        }
    //        catch (Exception ex)
    //        {
    //            plan.Revenue = 0;
    //        }

    //        //try
    //        //{
    //        //    if(plan.ProjStartDt == null)
    //        //    {

    //        //        var parts = plan.ProjId.Split('.', StringSplitOptions.RemoveEmptyEntries);

    //        //        var prefixes = Enumerable
    //        //            .Range(1, parts.Length)
    //        //            .Select(i => string.Join('.', parts.Take(i)))
    //        //            .ToList();

    //        //        var proj = allprojs
    //        //            .Where(p => prefixes.Contains(p.ProjId))
    //        //            .OrderByDescending(p => p.ProjId.Length)
    //        //            .FirstOrDefault();

    //        //        if(proj != null)
    //        //        {
    //        //            plan.ProjStartDt = DateOnly.FromDateTime(proj.ProjStartDt.GetValueOrDefault());
    //        //            plan.ProjEndDt = DateOnly.FromDateTime(proj.ProjEndDt.GetValueOrDefault());
    //        //        }
    //        //    }
    //        //}
    //        //catch (Exception ex)
    //        //{
    //        //    //plan.Revenue = 0;
    //        //}

    //        //plan.Revenue = summary.Where(s => s.ProjId == plan.ProjId && s.PlType == plan.PlType && s.Version == plan.Version).Sum(z => z.MonthlyRevenue);

    //        if (plan.AcctGrpCd != null && accountGroupLookup.TryGetValue(plan.AcctGrpCd, out var accountId))
    //        {
    //            plan.RevenueAccount = accountId;
    //        }
    //    }

    //    return plans;

    //}

    public async Task<IEnumerable<PlProjectPlan>> GetProjectPlans(
    int userId,
    string role,
    string projectID,
    string? status,
    string fetchNewBussiness)
    {

        //try
        //{
            var isAdmin = role.Equals("ADMIN", StringComparison.OrdinalIgnoreCase);
            var includeNewBusiness = fetchNewBussiness.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase);

            string sqlQuery;
            string[] projIds = Array.Empty<string>();

            if (!isAdmin)
            {
                projIds = await _context.UserProjectMaps
                    .Where(u => u.UserId == userId)
                    .Select(u => u.ProjId)
                    .ToArrayAsync();
            }

            sqlQuery = includeNewBusiness
                ? BuildQueryWithNewBusiness(isAdmin)
                : BuildStandardQuery(isAdmin);

            var parameters = new List<object>
    {
        new NpgsqlParameter("@projPrefix", projectID + "%")
    };

            if (!isAdmin)
                parameters.Add(new NpgsqlParameter("@projIds", projIds));

            var result = await _context.ProjectWithPlanDto
                .FromSqlRaw(sqlQuery, parameters.ToArray())
                .AsNoTracking()
                .ToListAsync();

            if (!string.IsNullOrEmpty(status))
                result = result.Where(p => p.ProjectStatus == status).ToList();

            var plans = result.Select(p => p.ToEntity()).ToList();

            // ---- Revenue Summary (Optimized lookup dictionary) ----
            var planIdList = plans.Select(p => p.PlId).Distinct().ToList();

            var revenueLookup = await _context.PlForecasts
                .Where(p => planIdList.Contains(p.PlId))
                .GroupBy(p => new { p.ProjId, p.PlId })
                .Select(g => new
                {
                    g.Key.ProjId,
                    g.Key.PlId,
                    Revenue = g.Sum(x => x.Revenue)
                })
                .ToDictionaryAsync(x => (x.ProjId, x.PlId), x => x.Revenue);

            // ---- Account Group Lookup (Optimized dictionary) ----
            var acctGrpCodes = plans
                .Where(p => p.AcctGrpCd != null)
                .Select(p => p.AcctGrpCd!)
                .Distinct()
                .ToList();

            var accountLookup = await (
                from ags in _context.AccountGroupSetup
                join a in _context.Accounts on ags.AccountId equals a.AcctId
                where acctGrpCodes.Contains(ags.AcctGroupCode)
                      && ags.AccountFunctionDescription.ToUpper() == "REVENUE"
                select new { ags.AcctGroupCode, ags.AccountId }
            ).ToDictionaryAsync(x => x.AcctGroupCode, x => x.AccountId);

            // ---- Final Projection ----
            foreach (var plan in plans)
            {
                if (revenueLookup.TryGetValue((plan.ProjId, plan.PlId.GetValueOrDefault()), out var revenue))
                    plan.Revenue = revenue;
                else
                    plan.Revenue = 0;

                if (plan.AcctGrpCd != null &&
                    accountLookup.TryGetValue(plan.AcctGrpCd, out var accountId))
                {
                    plan.RevenueAccount = accountId;
                }
            }

            // ---- Free memory of large temp collections ----
            result.Clear();
            revenueLookup.Clear();
            accountLookup.Clear();
            projIds = Array.Empty<string>();

            return plans;
        //}
        //catch(Exception ex)
        //{

        //}
    }

    public async Task<IEnumerable<PlProjectPlan>> GetProjectPlansV1(
int userId,
string role,
string projectID,
string? status,
string type)
    {
        var isAdmin = role.Equals("ADMIN", StringComparison.OrdinalIgnoreCase);
        var includeNewBusiness = type.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase);

        string sqlQuery;
        string[] projIds = Array.Empty<string>();

        if (!isAdmin)
        {
            projIds = await _context.UserProjectMaps
                .Where(u => u.UserId == userId)
                .Select(u => u.ProjId)
                .ToArrayAsync();
        }

        //sqlQuery = includeNewBusiness != "PROJECT"
        //    ? BuildQueryWithNewBusinessV1(isAdmin)
        //    : BuildStandardQueryV1(isAdmin);

        if (type == "PROJECT")
        {
            sqlQuery = BuildStandardQueryV1(isAdmin);
        }
        else
        {
            sqlQuery = BuildQueryWithNewBusinessV1(isAdmin);
        }

        var parameters = new List<object>
        {
            new NpgsqlParameter("@projPrefix", projectID + "%"),new NpgsqlParameter("@type", type)
        };

        if (!isAdmin)
            parameters.Add(new NpgsqlParameter("@projIds", projIds));

        var result = await _context.ProjectWithPlanDto
            .FromSqlRaw(sqlQuery, parameters.ToArray())
            .AsNoTracking()
            .ToListAsync();

        if (!string.IsNullOrEmpty(status))
            result = result.Where(p => p.ProjectStatus == status).ToList();

        var plans = result.Select(p => p.ToEntity()).ToList();

        // ---- Revenue Summary (Optimized lookup dictionary) ----
        var planIdList = plans.Select(p => p.PlId).Distinct().ToList();

        var revenueLookup = await _context.PlForecasts
            .Where(p => planIdList.Contains(p.PlId))
            .GroupBy(p => new { p.ProjId, p.PlId })
            .Select(g => new
            {
                g.Key.ProjId,
                g.Key.PlId,
                Revenue = g.Sum(x => x.Revenue)
            })
            .ToDictionaryAsync(x => (x.ProjId, x.PlId), x => x.Revenue);

        // ---- Account Group Lookup (Optimized dictionary) ----
        var acctGrpCodes = plans
            .Where(p => p.AcctGrpCd != null)
            .Select(p => p.AcctGrpCd!)
            .Distinct()
            .ToList();

        try
        {
            //var accountLookup = await (
            //    from ags in _context.AccountGroupSetup
            //    join a in _context.Accounts on ags.AccountId equals a.AcctId
            //    where acctGrpCodes.Contains(ags.AcctGroupCode)
            //          && ags.AccountFunctionDescription.ToUpper() == "REVENUE"
            //    select new { ags.AcctGroupCode, ags.AccountId }
            //).ToDictionaryAsync(x => x.AcctGroupCode, x => x.AccountId);
            var accountLookup = await (
                from ags in _context.AccountGroupSetup
                join a in _context.Accounts on ags.AccountId equals a.AcctId
                where acctGrpCodes.Contains(ags.AcctGroupCode)
                      && ags.AccountFunctionDescription.ToUpper() == "REVENUE"
                select new { ags.AcctGroupCode, ags.AccountId }
            )
            .GroupBy(x => x.AcctGroupCode)
            .ToDictionaryAsync(g => g.Key, g => g.First().AccountId);

            // ---- Final Projection ----
            foreach (var plan in plans)
            {
                if (revenueLookup.TryGetValue((plan.ProjId, plan.PlId.GetValueOrDefault()), out var revenue))
                    plan.Revenue = revenue;
                else
                    plan.Revenue = 0;

                if (plan.AcctGrpCd != null &&
                    accountLookup.TryGetValue(plan.AcctGrpCd, out var accountId))
                {
                    plan.RevenueAccount = accountId;
                }
            }

            // ---- Free memory of large temp collections ----
            result.Clear();
            revenueLookup.Clear();
            accountLookup.Clear();
            projIds = Array.Empty<string>();

        }
        catch (Exception ex)
        {

        }

        return plans;
    }

    private string BuildStandardQuery(bool isAdmin)
    {
        var baseWhere = isAdmin
            ? "WHERE p.proj_id ILIKE @projPrefix"
            : "WHERE p.proj_id = ANY(@projIds) AND p.proj_id ILIKE @projPrefix%";

        return $@"
        SELECT
            p.proj_id AS ProjId,
            p.proj_type_dc AS ProjType,
            pp.pl_id AS PlId,
            pp.pl_type AS PlType,
            pp.version AS Version,
            pp.version_code AS VersionCode,
            pp.final_version AS FinalVersion,
            pp.is_completed AS IsCompleted,
            pp.is_approved AS IsApproved,
            pp.status AS Status,
            pp.closed_period AS ClosedPeriod,
            pp.created_at AS CreatedAt,
            pp.updated_at AS UpdatedAt,
            pp.modified_by AS ModifiedBy,
            pp.approved_by AS ApprovedBy,
            pp.created_by AS CreatedBy,
            pp.source AS Source,
            pp.type AS Type,
            pp.burden_template_id AS BurdenTemplateId,
            COALESCE(p.proj_f_fee_amt,0) AS proj_f_fee_amt,
            COALESCE(p.proj_f_cst_amt,0) AS proj_f_cst_amt,
            COALESCE(p.proj_f_tot_amt,0) AS proj_f_tot_amt,
            p.proj_name AS ProjName,
            p.org_id AS OrgId,
            COALESCE(pp.proj_start_dt, p.proj_start_dt) AS ProjStartDt,
            COALESCE(pp.proj_end_dt, p.proj_end_dt) AS ProjEndDt,
            p.acct_grp_cd AS AcctGrpCd,
            p.active_fl AS ProjectStatus
        FROM pl_project p
        LEFT JOIN pl_project_plan pp ON pp.proj_id = p.proj_id
        {baseWhere}";
    }

    private string BuildQueryWithNewBusiness(bool isAdmin)
    {
        var standardQuery = BuildStandardQuery(isAdmin);

        return standardQuery + @"

        UNION ALL

        SELECT
            nb.business_budget_id AS ProjId,
            nb.description AS ProjType,
            pp.pl_id AS PlId,
            pp.pl_type AS PlType,
            pp.version AS Version,
            pp.version_code AS VersionCode,
            pp.final_version AS FinalVersion,
            pp.is_completed AS IsCompleted,
            pp.is_approved AS IsApproved,
            pp.status AS Status,
            pp.closed_period AS ClosedPeriod,
            pp.created_at AS CreatedAt,
            pp.updated_at AS UpdatedAt,
            pp.modified_by AS ModifiedBy,
            pp.approved_by AS ApprovedBy,
            pp.created_by AS CreatedBy,
            pp.source AS Source,
            'NBBUD' AS Type,
            pp.burden_template_id AS BurdenTemplateId,
            0 AS proj_f_fee_amt,
            0 AS proj_f_cst_amt,
            0 AS proj_f_tot_amt,
            nb.description AS ProjName,
            NULL AS OrgId,
            COALESCE(pp.proj_start_dt, nb.start_date) AS ProjStartDt,
            COALESCE(pp.proj_end_dt, nb.end_date) AS ProjEndDt,
            NULL AS AcctGrpCd,
            NULL AS ProjectStatus
        FROM pl_new_business_budget nb
        LEFT JOIN pl_project_plan pp ON pp.proj_id = nb.business_budget_id
        WHERE nb.business_budget_id ILIKE @projPrefix
          AND nb.business_budget_id NOT IN (SELECT proj_id FROM pl_project)";
    }

    private string BuildStandardQueryV1(bool isAdmin)
    {
        var baseWhere = isAdmin
            ? "WHERE p.proj_id ILIKE @projPrefix and (pp.pl_type IN ('BUD','EAC') OR pp.pl_type IS NULL)"
            : "WHERE p.proj_id = ANY(@projIds) AND p.proj_id ILIKE @projPrefix";

        return $@"
        SELECT
            p.proj_id AS ProjId,
            p.proj_type_dc AS ProjType,
            pp.pl_id AS PlId,
            pp.pl_type AS PlType,
            pp.version AS Version,
            pp.version_code AS VersionCode,
            pp.final_version AS FinalVersion,
            pp.is_completed AS IsCompleted,
            pp.is_approved AS IsApproved,
            pp.status AS Status,
            pp.closed_period AS ClosedPeriod,
            pp.created_at AS CreatedAt,
            pp.updated_at AS UpdatedAt,
            pp.modified_by AS ModifiedBy,
            pp.approved_by AS ApprovedBy,
            pp.created_by AS CreatedBy,
            pp.source AS Source,
            pp.type AS Type,
            pp.burden_template_id AS BurdenTemplateId,
            COALESCE(p.proj_f_fee_amt,0) AS proj_f_fee_amt,
            COALESCE(p.proj_f_cst_amt,0) AS proj_f_cst_amt,
            COALESCE(p.proj_f_tot_amt,0) AS proj_f_tot_amt,
            p.proj_name AS ProjName,
            p.org_id AS OrgId,
            COALESCE(pp.proj_start_dt, p.proj_start_dt) AS ProjStartDt,
            COALESCE(pp.proj_end_dt, p.proj_end_dt) AS ProjEndDt,
            p.acct_grp_cd AS AcctGrpCd,
            p.active_fl AS ProjectStatus
        FROM pl_project p
        LEFT JOIN pl_project_plan pp ON pp.proj_id = p.proj_id
        {baseWhere}";
    }

    private string BuildQueryWithNewBusinessV1(bool isAdmin)
    {
        var standardQuery = @"
        SELECT
            nb.business_budget_id AS ProjId,
            nb.description AS ProjType,
            pp.pl_id AS PlId,
            pp.pl_type AS PlType,
            pp.version AS Version,
            pp.version_code AS VersionCode,
            pp.final_version AS FinalVersion,
            pp.is_completed AS IsCompleted,
            pp.is_approved AS IsApproved,
            pp.status AS Status,
            pp.closed_period AS ClosedPeriod,
            pp.created_at AS CreatedAt,
            pp.updated_at AS UpdatedAt,
            pp.modified_by AS ModifiedBy,
            pp.approved_by AS ApprovedBy,
            pp.created_by AS CreatedBy,
            pp.source AS Source,
            pp.type AS Type,
            pp.burden_template_id AS BurdenTemplateId,
            0 AS proj_f_fee_amt,
            0 AS proj_f_cst_amt,
            0 AS proj_f_tot_amt,
            nb.description AS ProjName,
            NULL AS OrgId,
            COALESCE(pp.proj_start_dt, nb.start_date) AS ProjStartDt,
            COALESCE(pp.proj_end_dt, nb.end_date) AS ProjEndDt,
            nb.account_group AS AcctGrpCd,
            NULL AS ProjectStatus
        FROM pl_new_business_budget nb
        LEFT JOIN pl_project_plan pp ON pp.proj_id = nb.business_budget_id
        WHERE nb.business_budget_id ILIKE @projPrefix and nb.nb_type = @type";

        return standardQuery;
    }


    public async Task<PagedResponse<PlProjectPlan>> GetProjectPlansPaged(
    int userId,
    string role,
    string projectID,
    string? status,
    string? planstatus,
    string? planType,
    string fetchNewBussiness,
    int pageNumber,
    int pageSize)
    {
        var isAdmin = role.Equals("ADMIN", StringComparison.OrdinalIgnoreCase);
        var includeNewBusiness = fetchNewBussiness.Trim().Equals("Y", StringComparison.OrdinalIgnoreCase);
        var offset = (pageNumber - 1) * pageSize;

        var sql = $@"
    WITH base_projects AS (

        -- STANDARD PROJECTS
        SELECT
            p.proj_id AS ""ProjId"",
            p.proj_type_dc AS ""ProjType"",
            pp.pl_id AS ""PlId"",
            pp.pl_type AS ""PlType"",
            pp.version AS ""Version"",
            pp.version_code AS ""VersionCode"",
            pp.final_version AS ""FinalVersion"",
            pp.is_completed AS ""IsCompleted"",
            pp.is_approved AS ""IsApproved"",
            pp.status AS ""Status"",
            pp.closed_period AS ""ClosedPeriod"",
            pp.created_at AS ""CreatedAt"",
            pp.updated_at AS ""UpdatedAt"",
            pp.modified_by AS ""ModifiedBy"",
            pp.approved_by AS ""ApprovedBy"",
            pp.created_by AS ""CreatedBy"",
            pp.source AS ""Source"",
            pp.type AS ""Type"",
            pp.burden_template_id AS ""BurdenTemplateId"",
            COALESCE(p.proj_f_fee_amt,0) AS ""proj_f_fee_amt"",
            COALESCE(p.proj_f_cst_amt,0) AS ""proj_f_cst_amt"",
            COALESCE(p.proj_f_tot_amt,0) AS ""proj_f_tot_amt"",
            p.proj_name AS ""ProjName"",
            p.org_id AS ""OrgId"",
            COALESCE(pp.proj_start_dt, p.proj_start_dt) AS ""ProjStartDt"",
            COALESCE(pp.proj_end_dt, p.proj_end_dt) AS ""ProjEndDt"",
            p.acct_grp_cd AS ""AcctGrpCd"",
            p.active_fl AS ""ProjectStatus""
        FROM pl_project p
        LEFT JOIN pl_project_plan pp ON pp.proj_id = p.proj_id
        WHERE p.proj_id ILIKE @projPrefix

        {(includeNewBusiness ? @"

        UNION ALL

        SELECT
            nb.business_budget_id,
            nb.description,
            pp.pl_id,
            pp.pl_type,
            pp.version,
            pp.version_code,
            pp.final_version,
            pp.is_completed,
            pp.is_approved,
            pp.status,
            pp.closed_period,
            pp.created_at,
            pp.updated_at,
            pp.modified_by,
            pp.approved_by,
            pp.created_by,
            pp.source,
            'NBBUD',
            pp.burden_template_id,
            0,0,0,
            nb.description,
            NULL,
            COALESCE(pp.proj_start_dt, nb.start_date),
            COALESCE(pp.proj_end_dt, nb.end_date),
            NULL,
            NULL
        FROM pl_new_business_budget nb
        LEFT JOIN pl_project_plan pp ON pp.proj_id = nb.business_budget_id
        WHERE nb.business_budget_id ILIKE @projPrefix
        AND nb.business_budget_id NOT IN (SELECT proj_id FROM pl_project)
        " : "")}

    ),
    revenue_summary AS (
        SELECT proj_id, pl_id, SUM(revenue) AS revenue
        FROM pl_forecast
        GROUP BY proj_id, pl_id
    ),
    revenue_account AS (
        SELECT ags.acct_grp_cd, ags.acct_id
        FROM account_group_setup ags
        WHERE UPPER(ags.s_acct_func_dc) = 'REVENUE'
    )

    SELECT 
        bp.*,
        COALESCE(rs.revenue, 0) AS ""Revenue"",
        ra.acct_id AS ""RevenueAccount""
    FROM base_projects bp
    LEFT JOIN revenue_summary rs 
        ON rs.proj_id = bp.""ProjId"" AND rs.pl_id = bp.""PlId""
    LEFT JOIN revenue_account ra
        ON ra.acct_grp_cd = bp.""AcctGrpCd""
    WHERE bp.""ProjectStatus"" = COALESCE(@status, bp.""ProjectStatus"")
    ORDER BY bp.""ProjId""
    OFFSET @offset LIMIT @pageSize
    ";

        var countSql = @"
    SELECT COUNT(*) AS ""Value""
    FROM pl_project p
    WHERE p.proj_id ILIKE @projPrefix
    AND (@isAdmin = TRUE OR p.proj_id = ANY(
        SELECT upm.proj_id FROM user_project_map upm WHERE upm.user_id = @userId
    ))
";


        var parameters = new[]
        {
        new Npgsql.NpgsqlParameter("@projPrefix", projectID + "%"),
        new Npgsql.NpgsqlParameter("@status", (object?)status ?? DBNull.Value),
        new Npgsql.NpgsqlParameter("@isAdmin", isAdmin),
        new Npgsql.NpgsqlParameter("@userId", userId),
        new Npgsql.NpgsqlParameter("@offset", offset),
        new Npgsql.NpgsqlParameter("@pageSize", pageSize)
    };

        var totalRecords = await _context.Database
            .SqlQueryRaw<int>(countSql, parameters)
            .FirstAsync();

        var data = await _context.ProjectWithPlanDto
            .FromSqlRaw(sql, parameters)
            .AsNoTracking()
            .ToListAsync();

        if (!string.IsNullOrEmpty(planstatus))
        {
            switch (planstatus.ToUpper())
            {
                case "INPROGRESS":
                    data = data.Where(p => p.Status?.ToUpper() == "IN PROGRESS").ToList();
                    break;
                case "SUBMITTED":
                    data = data.Where(p => p.Status?.ToUpper() == "SUBMITTED").ToList();
                    break;
                case "APPROVED":
                    data = data.Where(p => p.Status?.ToUpper() == "APPROVED").ToList();
                    break;
                case "CONCLUDED":
                    data = data.Where(p => p.Status?.ToUpper() == "CONCLUDED").ToList();
                    break;
                default:
                    break;
            }
            totalRecords = data.Count;
            //data = data.Where(p => p.ProjectStatus == planstatus).ToList();
        }

        if (!string.IsNullOrEmpty(planType))
        {
            switch (planType.ToUpper())
            {
                case "BUD":
                    data = data.Where(p => p.PlType == "BUD").ToList();
                    break;
                case "NBBUD":
                    data = data.Where(p => p.PlType == "NBBUD").ToList();
                    break;
                case "EAC":
                    data = data.Where(p => p.PlType == "EAC").ToList();
                    break;
                case "BUD/EAC":
                    data = data.Where(p => p.PlType == "EAC" || p.PlType == "BUD").ToList();
                    break;
                default:
                    break;
            }
            totalRecords = data.Count;

        }
        //totalRecords = data.Count;
        //var plans = data.Select(p => p.ToEntity()).Where(p=>p.PlType == planType).ToList();

        List<PlProjectPlan> plans = new List<PlProjectPlan>();

        if (role.ToUpper() != "ADMIN")
        {
            var projIds = await _context.UserProjectMaps
                .Where(u => u.UserId == userId)
                .Select(u => u.ProjId)
                .ToArrayAsync();
            plans = data.Where(x => projIds.Contains(x.ProjId)).Select(p => p.ToEntity()).ToList();
        }
        else
        {
            plans = data.Select(p => p.ToEntity()).ToList();
        }

        return new PagedResponse<PlProjectPlan>
        {
            Data = plans,
            TotalRecords = totalRecords,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }






    //public async Task<PagedResult<PlProjectPlan>> GetProjectPlans(
    //int UserId,
    //string Role,
    //string projectID,
    //int pageNumber,
    //int pageSize)
    //{
    //    string sqlQuery = "";
    //    string[] projIds = Array.Empty<string>();

    //    if (Role.ToUpper() == "ADMIN")
    //    {
    //        sqlQuery = @"
    //                SELECT
    //                    p.proj_id AS ProjId,
    //                    p.proj_type_dc AS ProjType,
    //                    pp.pl_id AS PlId,
    //                    pp.pl_type AS PlType,
    //                    pp.version AS Version,
    //                    pp.version_code AS VersionCode,
    //                    pp.final_version AS FinalVersion,
    //                    pp.is_completed AS IsCompleted,
    //                    pp.is_approved AS IsApproved,
    //                    pp.status AS Status,
    //                    pp.closed_period AS ClosedPeriod,
    //                    pp.created_at AS CreatedAt,
    //                    pp.updated_at AS UpdatedAt,
    //                    pp.modified_by AS ModifiedBy,
    //                    pp.approved_by AS ApprovedBy,
    //                    pp.created_by AS CreatedBy,
    //                    pp.source AS Source,
    //                    pp.type AS Type,
    //                    pp.burden_template_id AS BurdenTemplateId,
    //                    COALESCE(p.proj_f_fee_amt,0) AS proj_f_fee_amt,
    //                    COALESCE(p.proj_f_cst_amt,0) AS proj_f_cst_amt,
    //                    COALESCE(p.proj_f_tot_amt,0) AS proj_f_tot_amt,
    //                    p.proj_name AS ProjName,
    //                    p.org_id AS OrgId,
    //                    p.proj_end_dt AS ProjEndDt,
    //                    p.proj_start_dt AS ProjStartDt,
    //                    p.acct_grp_cd AS AcctGrpCd
    //                FROM pl_project p
    //                LEFT JOIN pl_project_plan pp ON pp.proj_id = p.proj_id
    //                WHERE p.proj_id LIKE @projPrefix

    //                UNION ALL

    //                SELECT
    //                    nb.business_budget_id AS ProjId,
    //                    NULL AS ProjType,
    //                    pp.pl_id AS PlId,
    //                    pp.pl_type AS PlType,
    //                    pp.version AS Version,
    //                    pp.version_code AS VersionCode,
    //                    pp.final_version AS FinalVersion,
    //                    pp.is_completed AS IsCompleted,
    //                    pp.is_approved AS IsApproved,
    //                    pp.status AS Status,
    //                    pp.closed_period AS ClosedPeriod,
    //                    pp.created_at AS CreatedAt,
    //                    pp.updated_at AS UpdatedAt,
    //                    pp.modified_by AS ModifiedBy,
    //                    pp.approved_by AS ApprovedBy,
    //                    pp.created_by AS CreatedBy,
    //                    pp.source AS Source,
    //                    pp.type AS Type,
    //                    pp.burden_template_id AS BurdenTemplateId,
    //                    0 AS proj_f_fee_amt,
    //                    0 AS proj_f_cst_amt,
    //                    0 AS proj_f_tot_amt,
    //                    nb.business_budget_id AS ProjName,
    //                    NULL AS OrgId,
    //                    nb.end_date AS ProjEndDt,
    //                    nb.start_date AS ProjStartDt,
    //                    NULL AS AcctGrpCd
    //                FROM pl_new_business_budget nb
    //                LEFT JOIN pl_project_plan pp ON pp.proj_id = nb.business_budget_id
    //                WHERE nb.business_budget_id LIKE @projPrefix
    //                  AND nb.business_budget_id NOT IN (SELECT proj_id FROM pl_project)
    //            ";
    //    }
    //    else
    //    {
    //        //var projIds = _context.UserProjectMaps.Where(u => u.UserId == UserId).Select(u => u.ProjId).ToList();
    //        //var p1 = string.Join(",", projIds.Select(v => $"'{v}'"));
    //        projIds = await _context.UserProjectMaps
    //                .Where(u => u.UserId == UserId)
    //                .Select(u => u.ProjId)
    //                .ToArrayAsync();

    //        sqlQuery = @"
    //                SELECT
    //                    p.proj_id AS ProjId,
    //                    p.proj_type_dc AS ProjType,
    //                    pp.pl_id AS PlId,
    //                    pp.pl_type AS PlType,
    //                    pp.version AS Version,
    //                    pp.version_code AS VersionCode,
    //                    pp.final_version AS FinalVersion,
    //                    pp.is_completed AS IsCompleted,
    //                    pp.is_approved AS IsApproved,
    //                    pp.status AS Status,
    //                    pp.closed_period AS ClosedPeriod,
    //                    pp.created_at AS CreatedAt,
    //                    pp.updated_at AS UpdatedAt,
    //                    pp.modified_by AS ModifiedBy,
    //                    pp.approved_by AS ApprovedBy,
    //                    pp.created_by AS CreatedBy,
    //                    pp.source AS Source,
    //                    pp.type AS Type,
    //                    pp.burden_template_id AS BurdenTemplateId,
    //                    COALESCE(p.proj_f_fee_amt,0) AS proj_f_fee_amt,
    //                    COALESCE(p.proj_f_cst_amt,0) AS proj_f_cst_amt,
    //                    COALESCE(p.proj_f_tot_amt,0) AS proj_f_tot_amt,
    //                    p.proj_name AS ProjName,
    //                    p.org_id AS OrgId,
    //                    p.proj_end_dt AS ProjEndDt,
    //                    p.proj_start_dt AS ProjStartDt,
    //                    p.acct_grp_cd AS AcctGrpCd
    //                FROM pl_project p
    //                LEFT JOIN pl_project_plan pp ON pp.proj_id = p.proj_id
    //                WHERE p.proj_id = ANY(@projIds)
    //                  AND p.proj_id LIKE @projPrefix

    //                UNION ALL

    //                SELECT
    //                    nb.business_budget_id AS ProjId,
    //                    NULL AS ProjType,
    //                    pp.pl_id AS PlId,
    //                    pp.pl_type AS PlType,
    //                    pp.version AS Version,
    //                    pp.version_code AS VersionCode,
    //                    pp.final_version AS FinalVersion,
    //                    pp.is_completed AS IsCompleted,
    //                    pp.is_approved AS IsApproved,
    //                    pp.status AS Status,
    //                    pp.closed_period AS ClosedPeriod,
    //                    pp.created_at AS CreatedAt,
    //                    pp.updated_at AS UpdatedAt,
    //                    pp.modified_by AS ModifiedBy,
    //                    pp.approved_by AS ApprovedBy,
    //                    pp.created_by AS CreatedBy,
    //                    pp.source AS Source,
    //                    pp.type AS Type,
    //                    pp.burden_template_id AS BurdenTemplateId,
    //                    0 AS proj_f_fee_amt,
    //                    0 AS proj_f_cst_amt,
    //                    0 AS proj_f_tot_amt,
    //                    nb.business_budget_id AS ProjName,
    //                    NULL AS OrgId,
    //                    nb.end_date AS ProjEndDt,
    //                    nb.start_date AS ProjStartDt,
    //                    NULL AS AcctGrpCd
    //                FROM pl_new_business_budget nb
    //                LEFT JOIN pl_project_plan pp ON pp.proj_id = nb.business_budget_id
    //                WHERE nb.business_budget_id LIKE @projPrefix
    //                  AND nb.business_budget_id NOT IN (SELECT proj_id FROM pl_project)
    //                                ";
    //    }

    //    var result = _context.ProjectWithPlanDto
    //                 .FromSqlRaw(
    //                             sqlQuery,
    //                             new NpgsqlParameter("@projIds", projIds),
    //                             new NpgsqlParameter("@projPrefix", projectID + "%")
    //                         )
    //                         .AsNoTracking();


    //    // 🔢 Total count (for paging)
    //    var totalRecords = await result.CountAsync();

    //    // 📄 Paged data
    //    var pagedDtos = await result
    //        .OrderByDescending(x => x.CreatedAt)
    //        .Skip((pageNumber - 1) * pageSize)
    //        .Take(pageSize)
    //        .ToListAsync();

    //    var plans = pagedDtos
    //        .Select(p => p.ToEntity())
    //        .ToList();

    //    if (!plans.Any())
    //        return new PagedResult<PlProjectPlan>(plans, totalRecords, pageNumber, pageSize);






    //    ///////////////////////////////////////////////////

    //    var validDescriptions = new List<string> { "REVENUE" }; //plans.Select(p=>p.ProjId).ToList()

    //    var plans = (IEnumerable<PlProjectPlan>)result.Select(p => p.ToEntity()).ToList();


    //    var planIds = plans.Select(p => p.AcctGrpCd).Distinct().ToList();

    //    var accountGroupSetupDTOs = (from ags in _context.AccountGroupSetup
    //                                 join a in _context.Accounts
    //                                     on ags.AccountId equals a.AcctId
    //                                 where planIds.Contains(ags.AcctGroupCode)
    //                                    && validDescriptions.Contains(ags.AccountFunctionDescription.ToUpper())
    //                                 select new AccountGroupSetupDTO
    //                                 {
    //                                     AccountId = ags.AccountId,
    //                                     AccountFunctionDescription = ags.AcctGroupCode,
    //                                     AcctName = a.AcctName
    //                                 }).ToList();

    //    var accountGroupLookup = accountGroupSetupDTOs
    //.GroupBy(dto => dto.AccountFunctionDescription)
    //.ToDictionary(g => g.Key, g => g.First().AccountId);

    //    var summary = _context.ProjForecastSummary.ToList(); // Load the DbSet into memory

    //    // Update plans based on lookup
    //    foreach (var plan in plans)
    //    {
    //        plan.Revenue = summary.Where(s => s.ProjId == plan.ProjId && s.PlType == plan.PlType && s.Version == plan.Version).Sum(z => z.MonthlyRevenue);

    //        if (plan.AcctGrpCd != null && accountGroupLookup.TryGetValue(plan.AcctGrpCd, out var accountId))
    //        {
    //            plan.RevenueAccount = accountId;
    //        }
    //    }

    //    return plans;

    //}

    public async Task<PlProjectPlan> AddProjectPlanAsync(PlProjectPlan newPlan, string type)
    {
        List<PlEmployeee> actualEmployees = new List<PlEmployeee>();
        List<PlDct> actualAmountsData = new List<PlDct>();
        try
        {
            int existing_PlanId = newPlan.PlId.GetValueOrDefault();
            Helper helper = new Helper(_context, _configuration);

            if (newPlan.PlType.ToUpper() != "NBBUD")
            {
                var response = helper.CanWeCreateBudget(newPlan.ProjId);
                if (!response.IsSuccess)
                    throw new Exception(response.Message);
            }

            //return await helper.GetForecastActulData(newPlan, type);

            ScheduleHelper scheduleHelper = new ScheduleHelper();
            var existing_empls = _context.PlEmployeees.Where(p => p.PlId == existing_PlanId).Select(p => new { p.EmplId, p.Type }).ToList();
            var existing_Other_Cost_empls = _context.PlDcts.Where(p => p.PlId == existing_PlanId).Select(p => new { p.Id, p.Type }).ToList();
            int SourceVersion = newPlan.Version.GetValueOrDefault();
            string revenueFormula = string.Empty;

            newPlan.CreatedAt = DateTime.Now;
            //var configValue = _context.PlConfigValues
            //    .FirstOrDefault(r => r.Name.ToLower() == "closing_period")?.Value;

            //if (string.IsNullOrEmpty(configValue))
            //    throw new Exception("closing_period config missing");

            //newPlan.ClosedPeriod = DateOnly.FromDateTime(
            //    DateTime.ParseExact(configValue, "dd-MM-yyyy", CultureInfo.InvariantCulture)
            //);
            newPlan.ClosedPeriod = DateOnly.FromDateTime(DateTime.Parse(_context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "closing_period").Value));
            newPlan.Source = Convert.ToString(newPlan.Source);
            newPlan.Type = newPlan.Type;
            newPlan.Status = "In Progress";
            var temp = _context.PlProjectPlans.Where(p => p.ProjId == newPlan.ProjId && p.PlType == newPlan.PlType).ToList();
            int? lastVersion = await _context.PlProjectPlans.Where(p => p.ProjId == newPlan.ProjId && p.PlType == newPlan.PlType).OrderByDescending(p => p.Version).Select(p => (int?)p.Version).FirstOrDefaultAsync();
            newPlan.Version = (lastVersion ?? 0) + 1;
            List<PlForecast> plForecasts = new List<PlForecast>();

            if (type.Trim().ToLower() == "blank")
            {
                try
                {
                    newPlan.PlId = null;
                    var blankEntry = await _context.PlProjectPlans.AddAsync(newPlan);
                    await _context.SaveChangesAsync();

                    var existingSetUp = helper.GetRevenuDefinitionFromCP(blankEntry.Entity);
                    _context.ProjBgtRevSetups.Add(existingSetUp);
                    await _context.SaveChangesAsync();
                    return blankEntry.Entity;

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"DB Update Error: {ex.StackTrace}");
                    throw new Exception($"Error creating project plan: {ex.StackTrace}", ex);
                }
            }

            if (newPlan.Version == 1 && type.Trim().ToLower() == "actual" && newPlan.PlType?.ToUpper() == "BUD" && !newPlan.CopyFromExistingProject)
            {
                return await helper.GetForecastActulData(newPlan, type);
            }

            List<PlForecast> forecastDetailsToCopy = new List<PlForecast>();
            if (newPlan.CopyFromExistingProject)
            {
                var startDate = newPlan.SourceProjStartDt.GetValueOrDefault();
                var endDate = newPlan.SourceProjEndDt.GetValueOrDefault();

                forecastDetailsToCopy = await _context.PlForecasts.AsNoTracking()
                    .Include(p => p.Emple)
                    .Include(p => p.DirectCost)
                    .Include(p => p.Proj)
                    .Where(f => f.PlId == existing_PlanId &&
                               (f.Year > startDate.Year ||
                               (f.Year == startDate.Year && f.Month >= startDate.Month)) &&
                               (f.Year < endDate.Year ||
                               (f.Year == endDate.Year && f.Month <= endDate.Month)))
                    .ToListAsync();


                foreach (var forecast in forecastDetailsToCopy)
                {
                    forecast.ProjId = newPlan.ProjId;
                }

            }
            else
            {
                forecastDetailsToCopy = await _pl_ForecastService.GetByPlanAndProjectAsync(existing_PlanId, newPlan.ProjId);
            }
            if (newPlan.PlType != "EAC")
            {
                foreach (var forecast in forecastDetailsToCopy)
                {
                    if(newPlan.Source.Contains("E"))
                        plForecasts.Add(PlForecast.CloneWithoutIdFromEACToBUD(forecast));
                    else
                        plForecasts.Add(PlForecast.CloneWithoutId(forecast));
                }
            }
            else
            {
                foreach (var forecast in forecastDetailsToCopy)
                {
                    DateOnly forecastDay = new DateOnly(forecast.Year, forecast.Month, DateTime.DaysInMonth(forecast.Year, forecast.Month));
                    if (forecast.DirectCost != null)
                        forecast.EmplId = forecast.DirectCost.Id;
                    else
                    {
                        if (forecast.Emple == null)
                            continue;
                        forecast.EmplId = forecast.Emple.EmplId;
                    }
                    if (forecastDay <= newPlan.ClosedPeriod)
                    {
                        forecast.Actualhours = 0;
                        forecast.Actualamt = 0;
                        forecast.Cost = 0;
                        forecast.Revenue = 0;
                        forecast.Fringe = 0;
                        forecast.Cost = 0;
                        forecast.Overhead = 0;
                        forecast.Gna = 0;
                        forecast.Materials = 0;
                        forecast.Hr = 0;
                        forecast.YtdGna = 0;
                        forecast.YtdCost = 0;
                        forecast.YtdFringe = 0;
                        forecast.YtdOverhead = 0;
                        forecast.YtdMaterials = 0;
                        forecast.Burden = 0;
                    }
                    else
                    {
                        forecast.Actualhours = forecast.Forecastedhours;
                        forecast.Actualamt = forecast.Forecastedamt;
                    }
                    plForecasts.Add(PlForecast.CloneWithoutIdForEAC(forecast));

                }
                actualEmployees = helper.GetEmployeeActulHoursData(newPlan, forecastDetailsToCopy);
                actualAmountsData = helper.GetEmployeeActulAmountData(newPlan, forecastDetailsToCopy);
            }
            var hours = plForecasts.Where(p => p.DirectCost == null).ToList();
            var tempamounts = plForecasts.Where(p => p.Emple == null && p.DirectCost != null).Select(p => p.DirectCost).Distinct().ToList();
            var amounts = tempamounts
                    .DistinctBy(p => new
                    {
                        p.Id,
                        p.OrgId,
                        p.AcctId,
                        p.PlcGlc
                    })
                    .ToList();
            foreach(var item in amounts)
            {
                item.PlForecasts = new List<PlForecast>();
                item.PlForecasts = plForecasts.Where(p => p.EmplId == item.Id && p.OrgId == item.OrgId && p.AcctId == item.AcctId && p.Plc == item.PlcGlc).ToList();
            }

            var tempemphours = plForecasts.Where(p => p.DirectCost == null && p.Emple != null).Select(p => p.Emple).Distinct().ToList();

            var emphours = tempemphours
                    .DistinctBy(p => new
                    {
                        p.EmplId,
                        p.OrgId,
                        p.AccId,
                        p.PlcGlcCode
                    })
                    .ToList();

            foreach (var item in emphours)
            {
                item.PlForecasts = new List<PlForecast>();
                item.PlForecasts = plForecasts.Where(p => p.EmplId == item.EmplId && p.OrgId == item.OrgId && p.AcctId == item.AccId && p.Plc == item.PlcGlcCode).ToList();
            }


            List<PlEmployeee> onlyInList2 = emphours
                        .Where(l2 => !actualEmployees.Any(l1 => l1.EmplId == l2.EmplId && l1.AccId == l2.AccId && l1.OrgId == l2.OrgId && l1.PlcGlcCode == l2.PlcGlcCode))
                        .ToList();

            List<PlDct> DirectCostNotExist = amounts
                            .Where(l2 => !actualAmountsData.Any(l1 => l1.Id == l2.Id && l1.AcctId == l2.AcctId && l1.OrgId == l2.OrgId && l1.PlcGlc == l2.PlcGlc))
                            .ToList();

            if (onlyInList2.Count() > 0)
                actualEmployees.AddRange(onlyInList2);

            if (DirectCostNotExist.Count() > 0)
                actualAmountsData.AddRange(DirectCostNotExist);


            newPlan.PlId = null;
            var entry = await _context.PlProjectPlans.AddAsync(newPlan);
            await _context.SaveChangesAsync();

            if (newPlan.PlType.ToUpper() == "EAC")
            {
                var existingSetUp = helper.GetRevenuDefinitionFromCP(entry.Entity);

                revenueFormula = existingSetUp?.RevType?.ToUpper() ?? string.Empty;

                _context.ProjBgtRevSetups.Add(existingSetUp);
            }
            else
            {
                var existingSetUp = _context.ProjBgtRevSetups.Where(p => p.PlId == existing_PlanId).Select(p =>
                            new ProjBgtRevSetup
                            {
                                ProjId = p.ProjId,
                                RevType = p.RevType,
                                RevAcctId = p.RevAcctId,
                                DfltFeeRt = p.DfltFeeRt,

                                LabCostFl = p.LabCostFl,
                                LabBurdFl = p.LabBurdFl,
                                LabFeeCostFl = p.LabFeeCostFl,
                                LabFeeHrsFl = p.LabFeeHrsFl,
                                LabFeeRt = p.LabFeeRt,
                                LabTmFl = p.LabTmFl,

                                NonLabCostFl = p.NonLabCostFl,
                                NonLabBurdFl = p.NonLabBurdFl,
                                NonLabFeeCostFl = p.NonLabFeeCostFl,
                                NonLabFeeHrsFl = p.NonLabFeeHrsFl,
                                NonLabFeeRt = p.NonLabFeeRt,
                                NonLabTmFl = p.NonLabTmFl,

                                UseBillBurdenRates = p.UseBillBurdenRates,

                                OverrideFundingCeilingFl = p.OverrideFundingCeilingFl,
                                OverrideRevAmtFl = p.OverrideRevAmtFl,
                                OverrideRevAdjFl = p.OverrideRevAdjFl,
                                OverrideRevSettingFl = p.OverrideRevSettingFl,

                                AtRiskAmt = p.AtRiskAmt,

                                VersionNo = entry.Entity.Version,
                                BgtType = newPlan.PlType,
                                CompanyId = p.CompanyId,

                            });

                if (existingSetUp.Any())
                {
                    revenueFormula = existingSetUp.FirstOrDefault().RevType.ToUpper();
                    var existing = existingSetUp.FirstOrDefault();
                    existing.PlId = entry.Entity.PlId;
                    _context.ProjBgtRevSetups.Add(existing);
                }
                else
                {


                    var setupFromCP = helper.GetRevenuDefinitionFromCP(entry.Entity);
                    if (setupFromCP != null && setupFromCP.Id != 0)
                    {
                        revenueFormula = setupFromCP.RevType.ToUpper();
                        _context.ProjBgtRevSetups.Add(setupFromCP);
                    }
                    else
                    {

                        if (newPlan.PlType.ToUpper() == "NBBUD")
                        {
                            _context.ProjBgtRevSetups.Add(new ProjBgtRevSetup
                            {
                                ProjId = newPlan.ProjId,
                                VersionNo = entry.Entity.Version,
                                BgtType = newPlan.PlType,
                                AtRiskAmt = 100000000,
                                //LabBurdFl = true,
                                NonLabBurdFl = false,
                                NonLabCostFl = false,
                                //RevType = ProjRevDef.RevenueFormulaCd,
                                RevType = "UNIT",
                                PlId = entry.Entity.PlId,
                                CompanyId = "1",
                                DfltFeeRt = 0,
                                //LabCostFl = true,
                                LabFeeCostFl = false,
                                //LabFeeHrsFl = true,
                                LabTmFl = false,
                                NonLabFeeCostFl = false,
                                NonLabFeeRt = 0,
                                //NonLabTmFl = false,
                                RevAcctId = "40-0000-000",
                                //UseBillBurdenRates = true,
                                //RevAcctId = p.RevAcctId,
                            });
                        }
                        else
                        {
                            existingSetUp = _context.ProjBgtRevSetups.Where(p => p.BgtType == newPlan.PlType && p.ProjId == newPlan.ProjId && p.VersionNo == newPlan.Version - 1).Select(p =>
                                new ProjBgtRevSetup
                                {
                                    ProjId = p.ProjId,
                                    RevType = p.RevType,
                                    RevAcctId = p.RevAcctId,
                                    DfltFeeRt = p.DfltFeeRt,

                                    LabCostFl = p.LabCostFl,
                                    LabBurdFl = p.LabBurdFl,
                                    LabFeeCostFl = p.LabFeeCostFl,
                                    LabFeeHrsFl = p.LabFeeHrsFl,
                                    LabFeeRt = p.LabFeeRt,
                                    LabTmFl = p.LabTmFl,

                                    NonLabCostFl = p.NonLabCostFl,
                                    NonLabBurdFl = p.NonLabBurdFl,
                                    NonLabFeeCostFl = p.NonLabFeeCostFl,
                                    NonLabFeeHrsFl = p.NonLabFeeHrsFl,
                                    NonLabFeeRt = p.NonLabFeeRt,
                                    NonLabTmFl = p.NonLabTmFl,

                                    UseBillBurdenRates = p.UseBillBurdenRates,

                                    OverrideFundingCeilingFl = p.OverrideFundingCeilingFl,
                                    OverrideRevAmtFl = p.OverrideRevAmtFl,
                                    OverrideRevAdjFl = p.OverrideRevAdjFl,
                                    OverrideRevSettingFl = p.OverrideRevSettingFl,

                                    AtRiskAmt = p.AtRiskAmt,

                                    VersionNo = entry.Entity.Version,
                                    BgtType = newPlan.PlType,
                                    CompanyId = p.CompanyId,

                                });
                        }

                        if (existingSetUp.Any())
                        {
                            var existing = existingSetUp.FirstOrDefault();
                            existing.PlId = entry.Entity.PlId;
                            _context.ProjBgtRevSetups.Add(existingSetUp.FirstOrDefault());
                        }


                    }
                }


            }

            await _context.SaveChangesAsync();


            List<PlDct> plDctForecasts = helper.GetForecastDirectCostData(actualAmountsData, entry.Entity, type);
            List<PlEmployeee> plEmpForecasts = helper.GetForecasEmployeeHoursData(actualEmployees, entry.Entity, type);



            var uniqueList = plDctForecasts
               .GroupBy(x => new { x.Id, x.AcctId, x.OrgId, x.PlId, x.PlcGlc })
               .Select(g => g.First())
               .ToList();

            //plEmpForecasts = plEmpForecasts
            //   .GroupBy(x => new { x.Id, x.AccId, x.OrgId, x.PlId, x.PlcGlcCode })
            //   .Select(g => g.OrderByDescending(c => c.PerHourRate).First())
            //   .ToList();

            foreach (var emp in plEmpForecasts)
            {
                if (!string.IsNullOrEmpty(emp.Type))
                { continue; }
                emp.Type = existing_empls.FirstOrDefault(p => p.EmplId == emp.EmplId)?.Type;
                if (string.IsNullOrEmpty(emp.Type))
                    emp.Type = "Other";
            }

            foreach (var emp in uniqueList)
            {
                if (!string.IsNullOrEmpty(emp.Type))
                { continue; }
                emp.Type = existing_Other_Cost_empls.FirstOrDefault(p => p.Id == emp.Id)?.Type;
                if (string.IsNullOrEmpty(emp.Type))
                    emp.Type = "Other";
            }


            await PlanningBulkInsertHelper.BulkInsertPlanningDataParallelWithTransactionsAsync(
                _context,
                _configuration,
                plEmpForecasts,
                uniqueList,
                emp => emp.PlForecasts,
                dct => dct.PlForecasts,
                maxParallelTasks: 4
            );


            if (revenueFormula == "UNIT")
                helper.GetRevenueDataForUnit(entry.Entity.PlId.GetValueOrDefault(), newPlan.ProjId, newPlan.Version, newPlan.PlType);
            else
                helper.GetAdjustmentData(entry.Entity.PlId.GetValueOrDefault(), newPlan.ProjId, newPlan.Version, newPlan.PlType);

            //helper.GetAdjustmentData(entry.Entity.PlId.GetValueOrDefault(), newPlan.ProjId, revenueFormula, newPlan.Version, newPlan.PlType);
            if (revenueFormula.ToUpper().Equals("CPFC"))
            {
                helper.isBudgetIsCreatedOnLowerLevel(newPlan.ProjId, entry.Entity.PlId.GetValueOrDefault());
            }
            return entry.Entity;
        }
        catch (DbUpdateException ex)
        {
            // This usually means a database update failed (constraint violation, FK, etc.)
            var inner = ex.InnerException?.Message;
            Console.WriteLine($"DB Update Error: {inner ?? ex.StackTrace}");
            throw new Exception($"Error creating project plan: {ex.StackTrace}", ex); // or handle gracefully
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Update Error: {ex.StackTrace}");
            throw new Exception($"Error creating project plan: {ex.StackTrace}", ex);
        }

    }

    public async Task<PlProjectPlan> AddPBudgetFromNewBussinessAsync(NewBusinessBudgetDTO newBusiness, string SourceProject)
    {

        var exists = await _context.PlProjects
            .AsNoTracking()
            .AnyAsync(p => p.ProjId == SourceProject);

        if (!exists)
        {
            throw new KeyNotFoundException(
                  $"Source Project not found. ProjectId: {SourceProject}");
        }

        var firstPart = SourceProject.Split('.', StringSplitOptions.RemoveEmptyEntries);

        var existingBudgets = _context.PlProjectPlans.FirstOrDefault(p => p.ProjId.StartsWith(firstPart[0]));

        if (existingBudgets != null)
        {
            if (firstPart.Length != existingBudgets.ProjId.Split('.', StringSplitOptions.RemoveEmptyEntries).Length)
            {
                throw new KeyNotFoundException(
                      $"Project Budget already exists at level {existingBudgets.ProjId.Split('.', StringSplitOptions.RemoveEmptyEntries).Length} for ProjectId: {SourceProject}");
            }
        }


        List<PlEmployeee> actualEmployees = new List<PlEmployeee>();
        List<PlDct> actualAmountsData = new List<PlDct>();
        PlProjectPlan newPlan = new PlProjectPlan();
        string businessBudgetId = "";
        string type = "BUD";

        var NB_Budget = _context.PlProjectPlans.FirstOrDefault(p => p.ProjId == newBusiness.BusinessBudgetId && p.PlType == "NBBUD" && p.FinalVersion == true);




        if (NB_Budget == null)
        {
            throw new KeyNotFoundException(
                  $"Bussiness Budget not found. New Bussiness Id: {newBusiness.BusinessBudgetId}");
        }
        var existingNB = _context.NewBusinessBudgets.FirstOrDefault(p => p.BusinessBudgetId == newBusiness.BusinessBudgetId);
        if (string.Equals(existingNB.Status, "TRANSFERRED", StringComparison.OrdinalIgnoreCase))
        {
            throw new KeyNotFoundException(
                $"New Business Budget is already transferred: {newBusiness.BusinessBudgetId}");
        }

        businessBudgetId = newBusiness.BusinessBudgetId;
        int existing_PlanId = 0;
        Helper helper = new Helper(_context, _configuration);
        ScheduleHelper scheduleHelper = new ScheduleHelper();
        var existing_empls = _context.PlEmployeees.Where(p => p.PlId == NB_Budget.PlId).Select(p => new { p.EmplId, p.Type }).ToList();
        var existing_Other_Cost_empls = _context.PlDcts.Where(p => p.PlId == NB_Budget.PlId).Select(p => new { p.Id, p.Type }).ToList();
        int SourceVersion = newPlan.Version.GetValueOrDefault();
        try
        {
            newPlan.CreatedAt = DateTime.SpecifyKind(
    DateTime.UtcNow,
    DateTimeKind.Unspecified);
            newPlan.ClosedPeriod = DateOnly.FromDateTime(DateTime.Parse(_context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "closing_period").Value));
            newPlan.Source = businessBudgetId;
            newPlan.Type = newPlan.Type;
            newPlan.Status = "In Progress";
            newPlan.Version = 1;
            newPlan.PlType = "BUD";
            int? lastVersion = await _context.PlProjectPlans.Where(p => p.ProjId == SourceProject && p.PlType == newPlan.PlType).OrderByDescending(p => p.Version).Select(p => (int?)p.Version).FirstOrDefaultAsync();

            if (lastVersion >= 1)
            {
                throw new Exception(
                      $"Project Budget already exists for ProjectId: {SourceProject}");
            }
            newPlan.Version = (lastVersion ?? 0) + 1;
            var ids = businessBudgetId
                .Split('.', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToArray();

            var selectedId = ids.Length > 1
                    ? string.Join(".", ids.Skip(1))
                    : ids[0];

            //newPlan.ProjId = $"{SourceProject}.{selectedId}";

            newPlan.ProjId = SourceProject;

            newPlan.TemplateId = NB_Budget.TemplateId;

            //return helper.GetForecastActulData(newPlan, type);
            List<PlForecast> plForecasts = new List<PlForecast>();

            var forecastDetailsToCopy = await _pl_ForecastService.GetByPlanAndProjectAsync(NB_Budget.PlId.GetValueOrDefault(), NB_Budget.ProjId);
            foreach (var forecast in forecastDetailsToCopy)
            {
                plForecasts.Add(PlForecast.CloneWithoutId(forecast));
            }

            //var hours = plForecasts.Where(p => p.DirectCost == null).ToList();
            //var amounts = plForecasts.Where(p => p.Emple == null && p.DirectCost != null).Select(p => p.DirectCost).Distinct().ToList();
            //var emphours = plForecasts.Where(p => p.DirectCost == null && p.Emple != null).Select(p => p.Emple).Distinct().ToList();

            /////////////////////////////////////////////////////////


            var hours = plForecasts.Where(p => p.DirectCost == null).ToList();
            var tempamounts = plForecasts.Where(p => p.Emple == null && p.DirectCost != null).Select(p => p.DirectCost).Distinct().ToList();
            var amounts = tempamounts
                    .DistinctBy(p => new
                    {
                        p.Id,
                        p.OrgId,
                        p.AcctId,
                        p.PlcGlc
                    })
                    .ToList();
            foreach (var item in amounts)
            {
                item.PlForecasts = new List<PlForecast>();
                item.PlForecasts = plForecasts.Where(p => p.EmplId == item.Id && p.OrgId == item.OrgId && p.AcctId == item.AcctId && p.Plc == item.PlcGlc).ToList();
            }

            var tempemphours = plForecasts.Where(p => p.DirectCost == null && p.Emple != null).Select(p => p.Emple).Distinct().ToList();

            var emphours = tempemphours
                    .DistinctBy(p => new
                    {
                        p.EmplId,
                        p.OrgId,
                        p.AccId,
                        p.PlcGlcCode
                    })
                    .ToList();

            foreach (var item in emphours)
            {
                item.PlForecasts = new List<PlForecast>();
                item.PlForecasts = plForecasts.Where(p => p.EmplId == item.EmplId && p.OrgId == item.OrgId && p.AcctId == item.AccId && p.Plc == item.PlcGlcCode).ToList();
            }



            ///////////////////////////////////////////////////////////
            List<PlEmployeee> onlyInList2 = emphours
                        .Where(l2 => !actualEmployees.Any(l1 => l1.EmplId == l2.EmplId && l1.AccId == l2.AccId && l1.OrgId == l2.OrgId && l1.PlcGlcCode == l2.PlcGlcCode))
                        .ToList();

            List<PlDct> DirectCostNotExist = amounts
                            .Where(l2 => !actualAmountsData.Any(l1 => l1.Id == l2.Id && l1.AcctId == l2.AcctId && l1.OrgId == l2.OrgId))
                            .ToList();

            if (onlyInList2.Count() > 0)
                actualEmployees.AddRange(onlyInList2);

            if (DirectCostNotExist.Count() > 0)
                actualAmountsData.AddRange(DirectCostNotExist);


            var entry = await _context.PlProjectPlans.AddAsync(newPlan);
            //var existingSetUp = _context.ProjBgtRevSetups.Where(p => p.BgtType == newPlan.PlType && p.ProjId == newPlan.ProjId && p.VersionNo == SourceVersion).Select(p =>
            var existingSetUp = _context.ProjBgtRevSetups.Where(p => p.PlId == NB_Budget.PlId).Select(p =>
                        new ProjBgtRevSetup
                        {
                            ProjId = p.ProjId,
                            RevType = p.RevType,
                            RevAcctId = p.RevAcctId,
                            DfltFeeRt = p.DfltFeeRt,

                            LabCostFl = p.LabCostFl,
                            LabBurdFl = p.LabBurdFl,
                            LabFeeCostFl = p.LabFeeCostFl,
                            LabFeeHrsFl = p.LabFeeHrsFl,
                            LabFeeRt = p.LabFeeRt,
                            LabTmFl = p.LabTmFl,

                            NonLabCostFl = p.NonLabCostFl,
                            NonLabBurdFl = p.NonLabBurdFl,
                            NonLabFeeCostFl = p.NonLabFeeCostFl,
                            NonLabFeeHrsFl = p.NonLabFeeHrsFl,
                            NonLabFeeRt = p.NonLabFeeRt,
                            NonLabTmFl = p.NonLabTmFl,

                            UseBillBurdenRates = p.UseBillBurdenRates,

                            OverrideFundingCeilingFl = p.OverrideFundingCeilingFl,
                            OverrideRevAmtFl = p.OverrideRevAmtFl,
                            OverrideRevAdjFl = p.OverrideRevAdjFl,
                            OverrideRevSettingFl = p.OverrideRevSettingFl,

                            AtRiskAmt = p.AtRiskAmt,

                            VersionNo = entry.Entity.Version,
                            BgtType = newPlan.PlType,
                            CompanyId = p.CompanyId,

                        });

            if (existingSetUp.Any())
                _context.ProjBgtRevSetups.Add(existingSetUp.FirstOrDefault());
            await _context.SaveChangesAsync();


            List<PlDct> plDctForecasts = helper.GetForecastDirectCostData(actualAmountsData, entry.Entity, type);
            List<PlEmployeee> plEmpForecasts = helper.GetForecasEmployeeHoursData(actualEmployees, entry.Entity, type);



            var uniqueList = plDctForecasts
               .GroupBy(x => new { x.Id, x.AcctId, x.OrgId, x.PlId })
               .Select(g => g.First())
               .ToList();

            foreach (var emp in plEmpForecasts)
            {
                emp.Type = existing_empls.FirstOrDefault(p => p.EmplId == emp.EmplId)?.Type;
                if (string.IsNullOrEmpty(emp.Type))
                    emp.Type = "Other";
            }

            foreach (var emp in uniqueList)
            {
                emp.Type = existing_Other_Cost_empls.FirstOrDefault(p => p.Id == emp.Id)?.Type;
                if (string.IsNullOrEmpty(emp.Type))
                    emp.Type = "Other";
            }


            await PlanningBulkInsertHelper.BulkInsertPlanningDataParallelWithTransactionsAsync(
                _context,
                _configuration,
                plEmpForecasts,
                uniqueList,
                emp => emp.PlForecasts,
                dct => dct.PlForecasts,
                maxParallelTasks: 4
            );
            NB_Budget.Status = "TRANSFERRED";

            if (existingNB != null)
            {
                existingNB.Status = "TRANSFERRED";
                existingNB.Trf_ProjId = SourceProject + "-" + entry.Entity.PlType + "-" + entry.Entity.Version;

            }
            await _context.SaveChangesAsync();
            return entry.Entity;
        }
        catch (DbUpdateException ex)
        {
            // This usually means a database update failed (constraint violation, FK, etc.)
            var inner = ex.InnerException?.Message;
            Console.WriteLine($"DB Update Error: {inner ?? ex.Message}");
            throw; // or handle gracefully
        }
        catch (Exception ex)
        {
            throw ex;
        }

    }

    // ✅ UPDATE

    public async Task<bool> UpdateProjectPlanAsync(PlProjectPlan updatedPlan)
    {
        try
        {
            var existing = await _context.PlProjectPlans.FindAsync(updatedPlan.PlId);
            if (existing == null)
                return false;

            // 🔒 Validation: Only one FinalVersion per project
            if (updatedPlan.FinalVersion == true)
            {
                bool alreadyExists = await _context.PlProjectPlans
                    .AnyAsync(x =>
                        x.ProjId == existing.ProjId &&   // same project
                        x.PlId != existing.PlId &&    
                        x.PlType == existing.PlType && // same plan type
                        x.FinalVersion == true);

                if (alreadyExists)
                    throw new Exception("Another final version already exists for this project.");
            }

            // ✅ Update fields
            existing.VersionCode = updatedPlan.VersionCode;
            existing.TemplateId = updatedPlan.TemplateId;
            existing.FinalVersion = updatedPlan.FinalVersion;
            existing.IsCompleted = updatedPlan.IsCompleted;
            existing.IsApproved = updatedPlan.IsApproved;
            existing.Status = updatedPlan.Status;
            existing.ModifiedBy = updatedPlan.ModifiedBy;
            existing.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
            //return false;
        }
    }

    //public async Task<bool> UpdateProjectPlanAsync(PlProjectPlan updatedPlan)
    //{
    //    try
    //    {

    //        var existing = await _context.PlProjectPlans.FindAsync(updatedPlan.PlId);
    //        if (existing == null)
    //            return false;

    //        // Update only allowed fields
    //        //existing.PlType = updatedPlan.PlType;
    //        //existing.Version = updatedPlan.Version;
    //        existing.VersionCode = updatedPlan.VersionCode;
    //        existing.TemplateId = updatedPlan.TemplateId;
    //        existing.FinalVersion = updatedPlan.FinalVersion;
    //        existing.IsCompleted = updatedPlan.IsCompleted;
    //        existing.IsApproved = updatedPlan.IsApproved;
    //        existing.Status = updatedPlan.Status;
    //        existing.ModifiedBy = updatedPlan.ModifiedBy;
    //        existing.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

    //        await _context.SaveChangesAsync();
    //        return true;
    //    }
    //    catch (Exception ex)
    //    {
    //        return false;
    //    }
    //}

    // ✅ Bulk UPDATE
    public async Task<bool> BulkUpdateProjectPlansAsync(
    List<PlProjectPlan> updatedPlans)
    {
        if (updatedPlans == null || !updatedPlans.Any())
            return false;

        var planIds = updatedPlans.Select(x => x.PlId).ToList();

        var existingPlans = await _context.PlProjectPlans
            .Where(x => planIds.Contains(x.PlId))
            .ToListAsync();

        if (!existingPlans.Any())
            return false;

        foreach (var existing in existingPlans)
        {
            var updated = updatedPlans
                .First(x => x.PlId == existing.PlId);

            // ✅ Update only allowed fields
            existing.VersionCode = updated.VersionCode;
            existing.TemplateId = updated.TemplateId;
            existing.FinalVersion = updated.FinalVersion;
            existing.IsCompleted = updated.IsCompleted;
            existing.IsApproved = updated.IsApproved;
            existing.Status = updated.Status;
            existing.ModifiedBy = updated.ModifiedBy;
            existing.UpdatedAt = DateTime.SpecifyKind(
                DateTime.UtcNow, DateTimeKind.Unspecified);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    // ✅ DELETE
    public async Task<bool> DeleteProjectPlanAsync(int planId)
    {
        try
        {

            var existing_Forecasts = _context.PlForecasts.Where(p => p.PlId == planId).ToArray();

            if (existing_Forecasts != null && existing_Forecasts.Length > 0)
                _context.PlForecasts.RemoveRange(existing_Forecasts);

            var existing_Cost_calculated = _context.EmployeeBurdenCalculated.Where(p => p.Pl_Id == planId).ToList();
            if (existing_Cost_calculated != null && existing_Cost_calculated.Count > 0)
                _context.EmployeeBurdenCalculated.RemoveRange(existing_Cost_calculated);

            var existing_Direct_Cost = _context.PlDcts.Where(p => p.PlId == planId).ToList();
            if (existing_Direct_Cost != null && existing_Direct_Cost.Count > 0)
                _context.PlDcts.RemoveRange(existing_Direct_Cost);

            var existing_PlEmployeees = _context.PlEmployeees.Where(p => p.PlId == planId).ToList();
            if (existing_PlEmployeees != null && existing_PlEmployeees.Count > 0)
                _context.PlEmployeees.RemoveRange(existing_PlEmployeees);


            var plan = await _context.PlProjectPlans.FindAsync(planId);
            if (plan == null)
                return false;

            var existingSetUp = _context.ProjBgtRevSetups.Where(p => p.PlId == planId).ToList();
            if (existingSetUp != null && existingSetUp.Count > 0)
                _context.ProjBgtRevSetups.RemoveRange(existingSetUp);

            var existingAdjustments = _context.ProjRevWrkPds.Where(p => p.Pl_Id == planId).ToList();
            if (existingAdjustments != null && existingAdjustments.Count > 0)
                _context.ProjRevWrkPds.RemoveRange(existingAdjustments);

            _context.PlProjectPlans.Remove(plan);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public Task<IEnumerable<forecast>> GetForecastByPlanID(int planID)
    {

        List<forecast> employeeForecast = new List<forecast>();

        List<PlForecast> forecasts = _context.PlForecasts.Include(p => p.Empl)
               .Include(p => p.Proj).Where(p => p.PlId == planID).ToList();

        var distinctEmplProj = forecasts.Where(p => p.Empl != null)
                   .Select(f => new { f.EmplId, f.PlId })
                   .Distinct()
                   .ToList();

        if (forecasts.Count() == 0)
            return Task.FromResult((IEnumerable<forecast>)employeeForecast.OrderBy(p => p.Empl_Id));

        DateOnly projStartDate = forecasts.FirstOrDefault().Proj.ProjStartDt.GetValueOrDefault();
        DateOnly projEndDate = forecasts.FirstOrDefault().Proj.ProjEndDt.GetValueOrDefault();
        var schedules = sheduleHelper.GetWorkingDaysForDuration(projStartDate, projEndDate, _orgService);

        // Output result
        foreach (var empl in distinctEmplProj)
        {
            forecast forecastData = new forecast();

            forecastData.Empl_Id = empl.EmplId;
            forecastData.Pl_ID = empl.PlId;
            forecastData.Empl = forecasts.Where(p => p.EmplId == empl.EmplId).Select(p => p.Empl).FirstOrDefault();


            //if (forecastData.Empl.PlcGlcCode != null)
            //    forecastData.Empl.PlcGlcCode = forecastData.Empl.PlcGlcCode + "-(" + _context.PlcCodes.FirstOrDefault(p => p.PlcCode1 == forecastData.Empl.PlcGlcCode).Description + ")";


            List<forecostHours> filtered = forecasts
                .Where(f => f.EmplId == empl.EmplId && f.PlId == empl.PlId)
                .Select(f => new forecostHours
                {

                    Month = f.Month,
                    Year = f.Year,
                    ForecastedHours = f.Forecastedhours,
                    ForecastId = f.Forecastid,
                    ForecastedAmount = f.Forecastedamt

                })
                .ToList();

            foreach (var forecast in forecastData.Empl.PlForecasts)
            {
                int daysInMonth = DateTime.DaysInMonth(forecast.Year, forecast.Month);

                var schedule = schedules.FirstOrDefault(p => p.MonthNo == forecast.Month && p.Year == forecast.Year);
                forecast.DisplayText = "1/" + forecast.Month.ToString() + " - " + daysInMonth.ToString() + "/" + forecast.Month.ToString() + "(" + schedule?.WorkingHours + ")";
            }
            employeeForecast.Add(forecastData);
        }
        employeeForecast = employeeForecast.OrderBy(p => p.Empl_Id).ToList();
        return Task.FromResult((IEnumerable<forecast>)employeeForecast.OrderBy(p => p.Empl_Id));

    }

    public Task<IEnumerable<forecast>> GetEmployeeForecastByPlanID(int planID, int year)
    {

        List<MonthlyEmployeeHours> actualHours = new List<MonthlyEmployeeHours>();
        DateTime currentMonth = DateTime.Now;

        var plan = _context.PlProjectPlans.Find(planID);
        if (plan == null)
            throw new InvalidOperationException($"Plan with ID '{planID}' not found.");

        if (plan.PlType.ToUpper() == "EAC")
        {
            Helper helper = new Helper(_context, _configuration);
            //actualHours = helper.getActualDataByProjectId(plan.ProjId);
            var closingPeriodConfig = _context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "closing_period");

            if (closingPeriodConfig != null && DateTime.TryParse(closingPeriodConfig.Value, out currentMonth))
            {
                // currentMonth is now safely parsed
            }
            else
            {
                // Handle the missing or invalid value here
                throw new Exception("Invalid or missing 'closing_period' configuration.");
            }
        }

        List<forecast> hoursForecast = new List<forecast>();
        List<PlForecast> forecasts = new List<PlForecast>();

        if (year == 0)
        {
            forecasts = _context.PlForecasts.Include(p => p.Emple)
               .Include(p => p.Proj).Where(p => p.PlId == planID && p.ProjId == plan.ProjId).ToList();
        }
        else
        {
            forecasts = _context.PlForecasts.Include(p => p.Emple)
                .Include(p => p.Proj).Where(p => p.PlId == planID && p.ProjId == plan.ProjId && p.Year == year).ToList();
        }

        var distinctEmployeeProj = forecasts.Where(p => p.Emple != null)
                   .Select(f => new { f.empleId, f.PlId, f.AcctId, f.Plc, f.OrgId })
                   .Distinct()
                   .ToList();

        //DateOnly projStartDate = forecasts.FirstOrDefault().Proj.ProjStartDt.GetValueOrDefault();
        //DateOnly projEndDate = forecasts.FirstOrDefault().Proj.ProjEndDt.GetValueOrDefault();

        DateOnly projStartDate = plan.ProjStartDt.GetValueOrDefault();
        DateOnly projEndDate = plan.ProjEndDt.GetValueOrDefault();
        List<Schedule> schedules = new List<Schedule>();
        if (year == 0)
        {
            schedules = sheduleHelper.GetWorkingDaysForDuration(projStartDate, projEndDate, _orgService);
        }
        else
        {
            DateOnly startDate = new DateOnly(year, 1, 1);
            DateOnly endDate = new DateOnly(year, 12, 31);
            schedules = sheduleHelper.GetWorkingDaysForDuration(startDate, endDate, _orgService);
        }



        // Output result
        foreach (var directCost in distinctEmployeeProj)
        {
            forecast forecastData = new forecast();

            forecastData.Emple_Id = directCost.empleId.GetValueOrDefault();
            forecastData.Pl_ID = directCost.PlId;
            forecastData.Emple = forecasts.Where(p => p.empleId == directCost.empleId).Select(p => p.Emple).FirstOrDefault();
            if (forecastData.Emple != null && !string.IsNullOrEmpty(forecastData.Emple.PlcGlcCode))
                forecastData.Emple.PlcGlcCode = forecastData.Emple.PlcGlcCode + "-(" + _context.PlcCodes.FirstOrDefault(p => p.LaborCategoryCode == forecastData.Emple.PlcGlcCode)?.Description + ")";


            foreach (var forecast in forecastData.Emple.PlForecasts)
            {
                try
                {
                    int daysInMonth = DateTime.DaysInMonth(forecast.Year, forecast.Month);

                    var schedule = schedules.FirstOrDefault(p => p.MonthNo == forecast.Month && p.Year == forecast.Year);
                    forecast.DisplayText = "1/" + forecast.Month.ToString() + " - " + daysInMonth.ToString() + "/" + forecast.Month.ToString() + "(" + schedule?.WorkingHours + ")";
                    if (forecast.Month == 8 && forecast.Year == 2025 && forecast.EmplId == "000659")
                    {

                    }

                    if (plan.PlType.ToUpper() == "EAC")
                    {
                        DateTime forecastDay = new DateTime(forecast.Year, forecast.Month, DateTime.DaysInMonth(forecast.Year, forecast.Month));

                        if (forecastDay < currentMonth)
                        {
                            var actualHour = (decimal)(actualHours
                                                .FirstOrDefault(p => p.Month == forecast.Month &&
                                                                     p.Year == forecast.Year &&
                                                                     p.EmployeeId == forecast.EmplId &&
                                                                     p.AccId == forecast.AcctId &&
                                                                     p.OrgId == forecast.OrgId &&
                                                                     p.Plc == forecast.Plc)
                                                ?.ActualHours ?? 0m);

                            if (forecast.Actualhours != actualHour)
                            {
                                //forecast.Actualhours = actualHour;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                }

            }
            hoursForecast.Add(forecastData);
        }
        hoursForecast = hoursForecast.OrderBy(p => p.Emple_Id).ToList();


        CeilingHelper ceilingHelper = new CeilingHelper(_context);
        var warnings = ceilingHelper.GetWarningsByPlId(planID).Select(p => p.EmplId).ToList();

        foreach (var forecastItem in hoursForecast)
        {
            if (warnings.Contains(forecastItem.Emple.EmplId))
            {
                forecastItem.Emple.isWarning = true;
            }
        }

        return Task.FromResult((IEnumerable<forecast>)hoursForecast);

    }

    Task<IEnumerable<forecast>> IProjPlanRepository.GetEmployeeForecastByPlanID(int planID, int? year, string? emplid, int pageNumber, int pageSize)
    {
        List<MonthlyEmployeeHours> actualHours = new List<MonthlyEmployeeHours>();
        DateTime currentMonth = DateTime.Now;


        var plan = _context.PlProjectPlans.Find(planID);
        if (plan == null)
            throw new InvalidOperationException($"Plan with ID '{planID}' not found.");

        if (plan.PlType.ToUpper() == "EAC")
        {
            Helper helper = new Helper(_context, _configuration);
            //actualHours = helper.getActualDataByProjectId(plan.ProjId);
            var closingPeriodConfig = _context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "closing_period");

            if (closingPeriodConfig != null && DateTime.TryParse(closingPeriodConfig.Value, out currentMonth))
            {
                // currentMonth is now safely parsed
            }
            else
            {
                // Handle the missing or invalid value here
                throw new Exception("Invalid or missing 'closing_period' configuration.");
            }
        }

        List<forecast> hoursForecast = new List<forecast>();
        List<PlForecast> forecasts = new List<PlForecast>();

        //if (year == 0)
        //{
        //    forecasts = _context.PlForecasts.Include(p => p.Emple)
        //       .Include(p => p.Proj).Where(p => p.PlId == planID).ToList();
        //}
        //else
        //{
        //    forecasts = _context.PlForecasts.Include(p => p.Emple)
        //        .Include(p => p.Proj).Where(p => p.PlId == planID && p.Year == year).ToList();
        //}

        IQueryable<PlForecast> query = _context.PlForecasts
            .AsNoTracking()
            .Include(p => p.Emple)
            .Include(p => p.Proj)
            .Where(p => p.PlId == planID);

        if (year.HasValue && year.Value > 0)
        {
            query = query.Where(p => p.Year == year.Value);
        }

        // Optional: filter by employee
        if (!string.IsNullOrEmpty(emplid))
        {
            query = query.Where(p => p.EmplId.StartsWith(emplid));
        }

        forecasts = query.ToList();


        var distinctEmployeeProj = forecasts.Where(p => p.Emple != null)
                   .Select(f => new { f.empleId, f.PlId, f.AcctId, f.Plc, f.OrgId })
                   .Distinct()
                   .Take(pageSize)
                   .ToList();



        // Apply year filter only if provided




        //DateOnly projStartDate = forecasts.FirstOrDefault().Proj.ProjStartDt.GetValueOrDefault();
        //DateOnly projEndDate = forecasts.FirstOrDefault().Proj.ProjEndDt.GetValueOrDefault();

        DateOnly projStartDate = plan.ProjStartDt.GetValueOrDefault();
        DateOnly projEndDate = plan.ProjEndDt.GetValueOrDefault();
        List<Schedule> schedules = new List<Schedule>();
        if (year == 0)
        {
            schedules = sheduleHelper.GetWorkingDaysForDuration(projStartDate, projEndDate, _orgService);
        }
        else
        {
            DateOnly startDate = new DateOnly(year.GetValueOrDefault(), 1, 1);
            DateOnly endDate = new DateOnly(year.GetValueOrDefault(), 12, 31);
            schedules = sheduleHelper.GetWorkingDaysForDuration(startDate, endDate, _orgService);
        }



        // Output result
        foreach (var directCost in distinctEmployeeProj)
        {
            forecast forecastData = new forecast();

            forecastData.Emple_Id = directCost.empleId.GetValueOrDefault();
            forecastData.Pl_ID = directCost.PlId;
            forecastData.Emple = forecasts.Where(p => p.empleId == directCost.empleId).Select(p => p.Emple).FirstOrDefault();
            if (forecastData.Emple != null && !string.IsNullOrEmpty(forecastData.Emple.PlcGlcCode))
                forecastData.Emple.PlcGlcCode = forecastData.Emple.PlcGlcCode + "-(" + _context.PlcCodes.FirstOrDefault(p => p.LaborCategoryCode == forecastData.Emple.PlcGlcCode)?.Description + ")";


            foreach (var forecast in forecastData.Emple.PlForecasts)
            {
                try
                {
                    int daysInMonth = DateTime.DaysInMonth(forecast.Year, forecast.Month);

                    var schedule = schedules.FirstOrDefault(p => p.MonthNo == forecast.Month && p.Year == forecast.Year);
                    forecast.DisplayText = "1/" + forecast.Month.ToString() + " - " + daysInMonth.ToString() + "/" + forecast.Month.ToString() + "(" + schedule?.WorkingHours + ")";
                    if (forecast.Month == 8 && forecast.Year == 2025 && forecast.EmplId == "000659")
                    {

                    }

                    if (plan.PlType.ToUpper() == "EAC")
                    {
                        DateTime forecastDay = new DateTime(forecast.Year, forecast.Month, DateTime.DaysInMonth(forecast.Year, forecast.Month));

                        if (forecastDay < currentMonth)
                        {
                            var actualHour = (decimal)(actualHours
                                                .FirstOrDefault(p => p.Month == forecast.Month &&
                                                                     p.Year == forecast.Year &&
                                                                     p.EmployeeId == forecast.EmplId &&
                                                                     p.AccId == forecast.AcctId &&
                                                                     p.OrgId == forecast.OrgId &&
                                                                     p.Plc == forecast.Plc)
                                                ?.ActualHours ?? 0m);

                            if (forecast.Actualhours != actualHour)
                            {
                                //forecast.Actualhours = actualHour;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                }

            }
            hoursForecast.Add(forecastData);
        }
        hoursForecast = hoursForecast.OrderBy(p => p.Emple_Id).ToList();


        CeilingHelper ceilingHelper = new CeilingHelper(_context);
        var warnings = ceilingHelper.GetWarningsByPlId(planID).Select(p => p.EmplId).ToList();

        foreach (var forecastItem in hoursForecast)
        {
            if (warnings.Contains(forecastItem.Emple.EmplId))
            {
                forecastItem.Emple.isWarning = true;
            }
        }

        return Task.FromResult((IEnumerable<forecast>)hoursForecast);
    }
    public Task<IEnumerable<forecast>> GetEmployeeForecastByPlanID1(int planID, int year)
    {

        List<MonthlyEmployeeHours> actualHours = new List<MonthlyEmployeeHours>();
        DateTime currentMonth = DateTime.Now;

        var plan = _context.PlProjectPlans.Find(planID);
        if (plan == null)
            throw new InvalidOperationException($"Plan with ID '{planID}' not found.");

        if (plan.PlType.ToUpper() == "EAC")
        {
            Helper helper = new Helper(_context, _configuration);
            actualHours = helper.getActualDataByProjectId(plan.ProjId);
            var closingPeriodConfig = _context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "closing_period");

            if (closingPeriodConfig != null && DateTime.TryParse(closingPeriodConfig.Value, out currentMonth))
            {
                // currentMonth is now safely parsed
            }
            else
            {
                // Handle the missing or invalid value here
                throw new Exception("Invalid or missing 'closing_period' configuration.");
            }
        }

        List<forecast> hoursForecast = new List<forecast>();
        List<PlForecast> forecasts = new List<PlForecast>();

        if (year == 0)
        {
            forecasts = _context.PlForecasts.Include(p => p.Emple)
               .Include(p => p.Proj).Where(p => p.PlId == planID).ToList();
        }
        else
        {
            forecasts = _context.PlForecasts.Include(p => p.Emple)
                .Include(p => p.Proj).Where(p => p.PlId == planID && p.Year == year).ToList();
        }

        var distinctEmployeeProj = forecasts.Where(p => p.Emple != null)
                   .Select(f => new { f.empleId, f.PlId })
                   .Distinct()
                   .ToList();

        DateOnly projStartDate = forecasts.FirstOrDefault().Proj.ProjStartDt.GetValueOrDefault();
        DateOnly projEndDate = forecasts.FirstOrDefault().Proj.ProjEndDt.GetValueOrDefault();
        List<Schedule> schedules = new List<Schedule>();
        if (year == 0)
        {
            schedules = sheduleHelper.GetWorkingDaysForDuration(projStartDate, projEndDate, _orgService);
        }
        else
        {
            DateOnly startDate = new DateOnly(year, 1, 1);
            DateOnly endDate = new DateOnly(year, 12, 31);
            schedules = sheduleHelper.GetWorkingDaysForDuration(startDate, endDate, _orgService);
        }



        // Output result
        foreach (var directCost in distinctEmployeeProj)
        {
            forecast forecastData = new forecast();

            forecastData.Emple_Id = directCost.empleId.GetValueOrDefault();
            forecastData.Pl_ID = directCost.PlId;
            forecastData.Emple = forecasts.Where(p => p.empleId == directCost.empleId).Select(p => p.Emple).FirstOrDefault();
            if (forecastData.Emple != null && !string.IsNullOrEmpty(forecastData.Emple.PlcGlcCode))
                forecastData.Emple.PlcGlcCode = forecastData.Emple.PlcGlcCode + "-(" + _context.PlcCodes.FirstOrDefault(p => p.LaborCategoryCode == forecastData.Emple.PlcGlcCode).Description + ")";


            foreach (var forecast in forecastData.Emple.PlForecasts)
            {
                int daysInMonth = DateTime.DaysInMonth(forecast.Year, forecast.Month);

                var schedule = schedules.FirstOrDefault(p => p.MonthNo == forecast.Month && p.Year == forecast.Year);
                forecast.DisplayText = "1/" + forecast.Month.ToString() + " - " + daysInMonth.ToString() + "/" + forecast.Month.ToString() + "(" + schedule?.WorkingHours + ")";

                if (plan.PlType.ToUpper() == "EAC")
                {
                    DateTime forecastDay = new DateTime(forecast.Year, forecast.Month, DateTime.DaysInMonth(forecast.Year, forecast.Month));

                    if (forecastDay < currentMonth)
                    {
                        var actualHour = (decimal)(actualHours
                                            .FirstOrDefault(p => p.Month == forecast.Month &&
                                                                 p.Year == forecast.Year &&
                                                                 p.EmployeeId == forecast.EmplId &&
                                                                 p.AccId == forecast.AcctId &&
                                                                 p.OrgId == forecast.OrgId &&
                                                                 p.Plc == forecast.Plc)
                                            ?.ActualHours ?? 0m);

                        if (forecast.Actualhours != actualHour)
                        {
                            forecast.Actualhours = actualHour;
                        }
                    }
                }

            }
            hoursForecast.Add(forecastData);
        }
        hoursForecast = hoursForecast.OrderBy(p => p.Emple_Id).ToList();
        return Task.FromResult((IEnumerable<forecast>)hoursForecast);

    }

    public Task<IEnumerable<DirectCostforecast>> GetDirectCostForecastByPlanID(int planID, int year)
    {

        List<DirectCostforecast> directCostForecast = new List<DirectCostforecast>();

        List<PlForecast> forecasts = new List<PlForecast>();

        var plan = _context.PlProjectPlans.Find(planID);
        if (plan == null)
            throw new InvalidOperationException($"Plan with ID '{planID}' not found.");

        if (year == 0)
            forecasts = _context.PlForecasts.Include(p => p.DirectCost)
                        .Include(p => p.Proj).Where(p => p.PlId == planID && p.ProjId == plan.ProjId && p.DirectCost != null).ToList();
        else
            forecasts = _context.PlForecasts.Include(p => p.DirectCost)
                        .Include(p => p.Proj).Where(p => p.PlId == planID && p.ProjId == plan.ProjId && p.DirectCost != null && p.Year == year).ToList();


        var distinctDirectCostProj = forecasts.Where(p => p.DirectCost != null)
                   .Select(f => new { f.DctId, f.PlId })
                   .Distinct()
                   .ToList();

        if (distinctDirectCostProj.Count == 0)
            return Task.FromResult((IEnumerable<DirectCostforecast>)directCostForecast);

        //DateOnly projStartDate = forecasts.FirstOrDefault().Proj.ProjStartDt.GetValueOrDefault();
        //DateOnly projEndDate = forecasts.FirstOrDefault().Proj.ProjEndDt.GetValueOrDefault();

        DateOnly projStartDate = plan.ProjStartDt.GetValueOrDefault();
        DateOnly projEndDate = plan.ProjEndDt.GetValueOrDefault();

        List<Schedule> schedules = new List<Schedule>();
        if (year == 0)
        {
            schedules = sheduleHelper.GetWorkingDaysForDuration(projStartDate, projEndDate, _orgService);
        }
        else
        {
            DateOnly startDate = new DateOnly(year, 1, 1);
            DateOnly endDate = new DateOnly(year, 12, 31);
            schedules = sheduleHelper.GetWorkingDaysForDuration(startDate, endDate, _orgService);
        }

        // Output result
        foreach (var directCost in distinctDirectCostProj)
        {
            try
            {
                DirectCostforecast forecastData = new DirectCostforecast();

                forecastData.DctId = directCost.DctId.GetValueOrDefault();
                forecastData.Pl_ID = directCost.PlId;
                forecastData.Empl = forecasts.Where(p => p.DctId == directCost.DctId).Select(p => p.DirectCost).FirstOrDefault();
                //if (forecastData.Empl.PlcGlc != null)
                //    forecastData.Empl.PlcGlc = forecastData.Empl.PlcGlc + "-(" + _context.PlcCodes.FirstOrDefault(p => p.PlcCode1 == forecastData.Empl.PlcGlc).Description + ")";
                List<forecostHours> filtered = forecasts
                    .Where(f => f.DctId == directCost.DctId && f.PlId == directCost.PlId)
                    .Select(f => new forecostHours
                    {
                        Month = f.Month,
                        Year = f.Year,
                        ForecastedHours = f.Forecastedhours,
                        ForecastId = f.Forecastid,
                        ForecastedAmount = f.Forecastedamt

                    })
                    .ToList();

                foreach (var forecast in forecastData.Empl.PlForecasts)
                {
                    int daysInMonth = DateTime.DaysInMonth(forecast.Year, forecast.Month);

                    var schedule = schedules.FirstOrDefault(p => p.MonthNo == forecast.Month && p.Year == forecast.Year);
                    forecast.DisplayText = "1/" + forecast.Month.ToString() + " - " + daysInMonth.ToString() + "/" + forecast.Month.ToString() + "(" + schedule?.WorkingHours + ")";
                }
                directCostForecast.Add(forecastData);
            }
            catch (Exception ex)
            {
            }
        }
        directCostForecast = directCostForecast.OrderBy(p => p.DctId).ToList();
        return Task.FromResult((IEnumerable<DirectCostforecast>)directCostForecast);

    }

    Task<IEnumerable<forecast>> IProjPlanRepository.GetEACDataByPlanId(int planID)
    {
        List<forecast> employeeForecast = new List<forecast>();
        List<PlForecast> employeeForecastUpdateActual = new List<PlForecast>();

        List<MonthlyEmployeeHours> actualHours = new List<MonthlyEmployeeHours>();

        List<PlForecast> forecasts = _context.PlForecasts.Include(p => p.Empl)
               .Include(p => p.Proj).Where(p => p.PlId == planID && p.Empl != null).ToList();

        DateOnly projStartDate = forecasts.FirstOrDefault().Proj.ProjStartDt.GetValueOrDefault();
        DateOnly projEndDate = forecasts.FirstOrDefault().Proj.ProjEndDt.GetValueOrDefault();


        var distinctEmplProj = forecasts
                   .Select(f => new { f.EmplId, f.PlId })
                   .Distinct()
                   .ToList();


        Helper helper = new Helper(_context, _configuration);

        var schedules = sheduleHelper.GetWorkingDaysForDuration(projStartDate, projEndDate, _orgService);
        actualHours = helper.getActualDataByProjectId(forecasts.FirstOrDefault().Proj.ProjId);

        // Output result
        foreach (var empl in distinctEmplProj)
        {
            forecast forecastData = new forecast();

            forecastData.Empl_Id = empl.EmplId ?? string.Empty;
            forecastData.Pl_ID = empl.PlId;
            forecastData.Empl = forecasts.Where(p => p.EmplId == empl.EmplId).Select(p => p.Empl).FirstOrDefault();
            forecastData.Empl.PlcGlcCode = forecastData.Empl.PlcGlcCode;
            //actualHours = helper.getActualData(empl.EmplId, forecasts.FirstOrDefault().Proj.ProjId);

            foreach (var forecast in forecastData.Empl.PlForecasts.OrderBy(p => p.Year).OrderBy(p => p.Month))
            {
                int daysInMonth = DateTime.DaysInMonth(forecast.Year, forecast.Month);

                DateTime forecastDay = new DateTime(forecast.Year, forecast.Month, DateTime.DaysInMonth(forecast.Year, forecast.Month));
                var closingPeriodConfig = _context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "closing_period");

                DateTime currentMonth;

                if (closingPeriodConfig != null && DateTime.TryParse(closingPeriodConfig.Value, out currentMonth))
                {
                    // currentMonth is now safely parsed
                }
                else
                {
                    // Handle the missing or invalid value here
                    throw new Exception("Invalid or missing 'closing_period' configuration.");
                }

                if (forecastDay < currentMonth)
                {
                    var actualHour = (decimal)(actualHours
                                        .FirstOrDefault(p => p.Month == forecast.Month &&
                                                             p.Year == forecast.Year &&
                                                             p.EmployeeId == forecast.EmplId &&
                                                             p.AccId == forecast.AcctId &&
                                                             p.OrgId == forecast.OrgId &&
                                                             p.Plc == forecast.Plc)

                                        ?.ActualHours ?? 0m);

                    //var actualHour = (decimal)(actualHours
                    //                    .FirstOrDefault(p => p.Month == forecast.Month &&
                    //                                         p.Year == forecast.Year &&
                    //                                         p.EmployeeId == forecast.EmplId)

                    //                    ?.ActualHours ?? 0m);
                    if (actualHour > 0)
                    {

                    }
                    if (forecast.Forecastedhours != actualHour)
                    {
                        forecast.Forecastedhours = actualHour;
                        employeeForecastUpdateActual.Add(forecast);
                    }
                }
                var schedule = schedules.FirstOrDefault(p => p.MonthNo == forecast.Month && p.Year == forecast.Year);
                forecast.DisplayText = "1/" + forecast.Month.ToString() + " - " + daysInMonth.ToString() + "/" + forecast.Month.ToString() + "(" + schedule?.WorkingHours + ")";
            }
            employeeForecast.Add(forecastData);
        }
        if (employeeForecastUpdateActual.Count > 0)
        {
            _context.PlForecasts.UpdateRange(employeeForecastUpdateActual);
            _context.SaveChanges();
        }

        foreach (var forecast in employeeForecast)
        {
            forecast.Empl.PlcGlcCode = forecast.Empl.PlcGlcCode + "-(" + _context.PlcCodes.FirstOrDefault(p => p.LaborCategoryCode == forecast.Empl.PlcGlcCode)?.Description + ")";
        }


        return Task.FromResult((IEnumerable<forecast>)employeeForecast.OrderBy(p => p.Empl_Id));
    }

    public Task<IEnumerable<Account>> GetAccountsByProjectId(string projId)
    {
        var acctGroupCode = _context.PlProjects.FirstOrDefault(p => p.ProjId == projId)?.AcctGrpCd;

        var validFunctions = new[]
                                {
                                    "LABOR", "NON-LABOR", "UNALLOW-LABOR",
                                    "UNALLOW-NONLABR", "COST GOODS SOLD", "UNITS"
                                };

        var res = _context.AccountGroupSetup
            .Where(p => p.AcctGroupCode == acctGroupCode &&
                        validFunctions.Contains(p.AccountFunctionDescription))
            .Include(p => p.Account).Select(p => p.Account).OrderBy(p => p.AcctId)
            .ToList();

        return Task.FromResult((IEnumerable<Account>)res);
    }

    public Task<IEnumerable<PlEmployee>> GetEmployeesByProjectId(string projId)
    {
        var project = _context.PlEmployeeProjectMappings.Where(p => p.ProjId == projId).Include(p => p.EmplMaster).Select(p => p.EmplMaster).ToList();


        return Task.FromResult((IEnumerable<PlEmployee>)project);
    }

    public Task<IEnumerable<PlProjectPlan>> GetAllNewBussiness(string nbId)
    {

        var projects = _context.PlProjectPlans.Where(p => p.ProjId == nbId && p.Type == "NB").ToList();
        return Task.FromResult((IEnumerable<PlProjectPlan>)projects);
    }

    public Task<IEnumerable<DirectCostforecast>> GetDirectCostEACDataByPlanId(int planID, int year)
    {
        List<DirectCostforecast> employeeForecast = new List<DirectCostforecast>();
        List<PlForecast> employeeForecastUpdateActual = new List<PlForecast>();

        List<MonthlyEmployeeHours> actualAmounts = new List<MonthlyEmployeeHours>();

        List<PlForecast> forecasts = new List<PlForecast>();

        if (year == 0)
            forecasts = _context.PlForecasts.Include(p => p.DirectCost)
                        .Include(p => p.Proj).Where(p => p.PlId == planID && p.DirectCost != null).ToList();
        else
            forecasts = _context.PlForecasts.Include(p => p.DirectCost)
                        .Include(p => p.Proj).Where(p => p.PlId == planID && p.DirectCost != null && p.Year == year).ToList();

        DateOnly projStartDate = forecasts.FirstOrDefault().Proj.ProjStartDt.GetValueOrDefault();
        DateOnly projEndDate = forecasts.FirstOrDefault().Proj.ProjEndDt.GetValueOrDefault();


        var distinctEmplProj = forecasts
                   .Select(f => new { f.DctId, f.PlId })
                   .Distinct()
                   .ToList();


        Helper helper = new Helper(_context, _configuration);

        List<Schedule> schedules = new List<Schedule>();
        if (year == 0)
        {
            schedules = sheduleHelper.GetWorkingDaysForDuration(projStartDate, projEndDate, _orgService);
        }
        else
        {
            DateOnly startDate = new DateOnly(year, 1, 1);
            DateOnly endDate = new DateOnly(year, 12, 31);
            schedules = sheduleHelper.GetWorkingDaysForDuration(startDate, endDate, _orgService);
        }
        actualAmounts = helper.getActualDirectDataByProjectId(forecasts.FirstOrDefault().Proj.ProjId);

        // Output result
        foreach (var directCost in distinctEmplProj)
        {
            DirectCostforecast forecastData = new DirectCostforecast();

            forecastData.DctId = directCost.DctId.GetValueOrDefault();
            forecastData.Pl_ID = directCost.PlId;
            forecastData.Empl = forecasts.Where(p => p.DctId == directCost.DctId).Select(p => p.DirectCost).FirstOrDefault();
            //forecastData.Empl.PlcGlc = forecastData.Empl.PlcGlc + "-(" + _context.PlcCodes.FirstOrDefault(p => p.PlcCode1 == forecastData.Empl.PlcGlc).Description + ")";

            //actualHours = helper.getActualData(empl.EmplId, forecasts.FirstOrDefault().Proj.ProjId);

            foreach (var forecast in forecastData.Empl.PlForecasts.Where(p => p.Year > 2024).OrderBy(p => p.Year).OrderBy(p => p.Month))
            {
                int daysInMonth = DateTime.DaysInMonth(forecast.Year, forecast.Month);

                DateTime forecastDay = new DateTime(forecast.Year, forecast.Month, DateTime.DaysInMonth(forecast.Year, forecast.Month));
                var closingPeriodConfig = _context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "closing_period");

                DateTime currentMonth;

                if (closingPeriodConfig != null && DateTime.TryParse(closingPeriodConfig.Value, out currentMonth))
                {
                    // currentMonth is now safely parsed
                }
                else
                {
                    // Handle the missing or invalid value here
                    throw new Exception("Invalid or missing 'closing_period' configuration.");
                }

                if (forecastDay < currentMonth)
                {
                    //var actualAmount = (decimal)(actualAmounts
                    //                    .FirstOrDefault(p => p.Month == forecast.Month &&
                    //                                         p.Year == forecast.Year &&
                    //                                         p.EmployeeId == forecast.DirectCost.Id &&
                    //                                         p.AccId == forecast.AcctId &&
                    //                                         p.OrgId == forecast.OrgId &&
                    //                                         p.Plc == forecast.Plc)
                    //                    ?.ActualHours ?? 0m);
                    var actualAmount = actualAmounts
                                        .Where(p => p.Month == forecast.Month &&
                                                    p.Year == forecast.Year &&
                                                    p.EmployeeId == forecast.DirectCost.Id &&
                                                    p.AccId == forecast.AcctId &&
                                                    p.OrgId == forecast.OrgId)
                                        .Sum(p => (decimal?)p.ActualHours) ?? 0m;


                    if (forecast.Forecastedamt != actualAmount)
                    {
                        forecast.Forecastedamt = actualAmount;
                        employeeForecastUpdateActual.Add(forecast);
                    }
                }
                var schedule = schedules.FirstOrDefault(p => p.MonthNo == forecast.Month && p.Year == forecast.Year);
                forecast.DisplayText = "1/" + forecast.Month.ToString() + " - " + daysInMonth.ToString() + "/" + forecast.Month.ToString() + "(" + schedule?.WorkingHours + ")";
            }
            employeeForecast.Add(forecastData);
        }
        if (employeeForecastUpdateActual.Count > 0)
        {
            _context.PlForecasts.UpdateRange(employeeForecastUpdateActual);
            _context.SaveChanges();
        }

        foreach (var forecast in employeeForecast)
        {
            forecast.Empl.PlcGlc = forecast.Empl.PlcGlc + "-(" + _context.PlcCodes.FirstOrDefault(p => p.LaborCategoryCode == forecast.Empl.PlcGlc)?.Description + ")";
        }


        return Task.FromResult((IEnumerable<DirectCostforecast>)employeeForecast);
    }

    public Task<IEnumerable<DirectCostforecast>> GetDirectCostForecastByPlanID(int planID, int? year, string? emplid, int pageNumber, int pageSize)
    {
        List<DirectCostforecast> directCostForecast = new List<DirectCostforecast>();

        List<PlForecast> forecasts = new List<PlForecast>();

        var plan = _context.PlProjectPlans.Find(planID);
        if (plan == null)
            throw new InvalidOperationException($"Plan with ID '{planID}' not found.");

        //if (year == 0)
        //    forecasts = _context.PlForecasts.Include(p => p.DirectCost)
        //                .Include(p => p.Proj).Where(p => p.PlId == planID && p.DirectCost != null).ToList();
        //else
        //    forecasts = _context.PlForecasts.Include(p => p.DirectCost)
        //                .Include(p => p.Proj).Where(p => p.PlId == planID && p.DirectCost != null && p.Year == year).ToList();

        //var distinctDirectCostProj = _context.PlForecasts
        //    .AsNoTracking()
        //    .Where(p => p.PlId == planID)
        //    .Where(p => p.DirectCost != null)
        //    .Where(p => !year.HasValue || year.Value <= 0 || p.Year == year.Value)
        //    .Where(p => string.IsNullOrEmpty(emplid) || p.EmplId.StartsWith(emplid))
        //    .Select(p => new
        //    {
        //        p.DctId,
        //        p.PlId
        //    })
        //    .Distinct()
        //    .OrderBy(x => x.DctId) // REQUIRED for stable paging
        //    .Skip((pageNumber - 1) * pageSize)
        //    .Take(pageSize)
        //    .ToList();



        IQueryable<PlForecast> query = _context.PlForecasts
          .AsNoTracking()
          .Include(p => p.DirectCost)
          .Include(p => p.Proj)
          .Where(p => p.PlId == planID && p.DirectCost != null);

        if (year.HasValue && year.Value > 0)
        {
            query = query.Where(p => p.Year == year.Value);
        }

        // Optional: filter by employee
        if (!string.IsNullOrEmpty(emplid))
        {
            query = query.Where(p => p.EmplId.StartsWith(emplid));
        }

        forecasts = query.ToList();



        var distinctDirectCostProj = forecasts.Where(p => p.DirectCost != null)
                   .Select(f => new { f.DctId, f.PlId })
                   .Distinct()
                   .Take(pageSize)
                   .ToList();

        if (distinctDirectCostProj.Count == 0)
            return Task.FromResult((IEnumerable<DirectCostforecast>)directCostForecast);

        //DateOnly projStartDate = forecasts.FirstOrDefault().Proj.ProjStartDt.GetValueOrDefault();
        //DateOnly projEndDate = forecasts.FirstOrDefault().Proj.ProjEndDt.GetValueOrDefault();

        DateOnly projStartDate = plan.ProjStartDt.GetValueOrDefault();
        DateOnly projEndDate = plan.ProjEndDt.GetValueOrDefault();

        List<Schedule> schedules = new List<Schedule>();
        if (year == 0)
        {
            schedules = sheduleHelper.GetWorkingDaysForDuration(projStartDate, projEndDate, _orgService);
        }
        else
        {
            DateOnly startDate = new DateOnly(year.GetValueOrDefault(), 1, 1);
            DateOnly endDate = new DateOnly(year.GetValueOrDefault(), 12, 31);
            schedules = sheduleHelper.GetWorkingDaysForDuration(startDate, endDate, _orgService);
        }

        // Output result
        foreach (var directCost in distinctDirectCostProj)
        {
            try
            {
                DirectCostforecast forecastData = new DirectCostforecast();

                forecastData.DctId = directCost.DctId.GetValueOrDefault();
                forecastData.Pl_ID = directCost.PlId;
                forecastData.Empl = forecasts.Where(p => p.DctId == directCost.DctId).Select(p => p.DirectCost).FirstOrDefault();
                //if (forecastData.Empl.PlcGlc != null)
                //    forecastData.Empl.PlcGlc = forecastData.Empl.PlcGlc + "-(" + _context.PlcCodes.FirstOrDefault(p => p.PlcCode1 == forecastData.Empl.PlcGlc).Description + ")";
                List<forecostHours> filtered = forecasts
                    .Where(f => f.DctId == directCost.DctId && f.PlId == directCost.PlId)
                    .Select(f => new forecostHours
                    {
                        Month = f.Month,
                        Year = f.Year,
                        ForecastedHours = f.Forecastedhours,
                        ForecastId = f.Forecastid,
                        ForecastedAmount = f.Forecastedamt

                    })
                    .ToList();

                foreach (var forecast in forecastData.Empl.PlForecasts)
                {
                    int daysInMonth = DateTime.DaysInMonth(forecast.Year, forecast.Month);

                    var schedule = schedules.FirstOrDefault(p => p.MonthNo == forecast.Month && p.Year == forecast.Year);
                    forecast.DisplayText = "1/" + forecast.Month.ToString() + " - " + daysInMonth.ToString() + "/" + forecast.Month.ToString() + "(" + schedule?.WorkingHours + ")";
                }
                directCostForecast.Add(forecastData);
            }
            catch (Exception ex)
            {
            }
        }
        directCostForecast = directCostForecast.OrderBy(p => p.DctId).ToList();
        return Task.FromResult((IEnumerable<DirectCostforecast>)directCostForecast);
    }
}