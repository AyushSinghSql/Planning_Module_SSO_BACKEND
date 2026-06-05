namespace WebApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol;
using NPOI.POIFS.FileSystem;
using NPOI.SS.Formula.Functions;
using NPOI.XSSF.UserModel;
using PlanningAPI.Helpers;
using PlanningAPI.Models;
using PlanningAPI.Repositories;
using QuestPDF.Helpers;
using SkiaSharp;
using System;
using System.Drawing.Drawing2D;
using System.Net.NetworkInformation;
using System.Numerics;
using WebApi.DTO;
using WebApi.Helpers;
using WebApi.Models;
using WebApi.Models.Users;
using WebApi.Repositories;
using WebApi.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static NPOI.HSSF.Util.HSSFColor;


[ApiController]
//[Authorize]
[Route("[controller]")]
public class ProjectController : ControllerBase
{
    //private readonly ICacheService _cache;
    private IProjService _projService;
    private IProjPlanService _projPlanService;
    private IPl_ForecastService _pl_ForecastService;
    private readonly IConfiguration _config;
    private readonly MydatabaseContext _context;
    private readonly ILogger<ProjectController> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly IProjRevWrkPdRepository _repository;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ProjectController(ILogger<ProjectController> logger, IProjRevWrkPdRepository repository, IProjService projService, IProjPlanService projPlanService, MydatabaseContext context, IServiceProvider serviceProvider, IBackgroundTaskQueue taskQueue, IPl_ForecastService pl_ForecastService, IConfiguration config, IServiceScopeFactory serviceScopeFactory)
    {
        _projPlanService = projPlanService;
        _projService = projService;
        _context = context;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _taskQueue = taskQueue;
        _pl_ForecastService = pl_ForecastService;
        _config = config;
        _repository = repository;
        _serviceScopeFactory = serviceScopeFactory;
        //_cache = cache;
    }
    [HttpGet("GetAllProjects")]
    public async Task<IActionResult> GetAllProjects()
    {
        try
        {
            //var cacheKey = "projects_all";
            //var projects = await _cache.GetAsync<List<ProjectDTO>>(cacheKey);

            //if (projects != null)
            //    return Ok(projects);

            var users = await _projService.GetAllProjects();

            //await _cache.SetAsync(cacheKey, projects, TimeSpan.FromMinutes(10));
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all projects.");
            return StatusCode(500, "Internal server error while retrieving projects.");
        }
    }
    [HttpGet("GetAccountGroupCodes")]
    public async Task<IEnumerable<string>> GetAccountGroupCodes()
    {
        return await _context.AccountGroupSetup.Select(p => p.AcctGroupCode).Distinct().ToListAsync();
    }


    [HttpGet("GetAllProjectsForSearch")]
    public async Task<IActionResult> GetAllProjectsForSearch()
    {
        try
        {
            var users = await _projService.GetAllProjects();
            var NB = _context.NewBusinessBudgets.Select(p => new { ProjectId = p.BusinessBudgetId, p.BusinessBudgetId, Type = "NBBUD" });
            users = users.Concat(NB.Select(p => new ProjectDTO { ProjectId = p.ProjectId, Name = p.BusinessBudgetId, Type = p.Type }));
            return Ok(users.Select(p => new { p.ProjectId, p.Name, p.Type }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all projects.");
            return StatusCode(500, "Internal server error while retrieving projects.");
        }
    }

    [HttpGet("GetAllProjectsForSearchV1")]
    public async Task<IActionResult> GetAllProjectsForSearchV1(int UserId, string Role)
    {
        try
        {
            var users = await _projService.GetAllProjectsForSearch(UserId, Role, null);
            var NB = _context.NewBusinessBudgets.Select(p => new { ProjectId = p.BusinessBudgetId, p.BusinessBudgetId, Type = "NBBUD" });
            users = users.Concat(NB.Select(p => new ProjectDTO { ProjectId = p.ProjectId, Name = p.BusinessBudgetId, Type = p.Type }));
            return Ok(users.Select(p => new { p.ProjectId, p.Name, p.Type }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all projects.");
            return StatusCode(500, "Internal server error while retrieving projects.");
        }
    }
    [HttpGet("GetAllProjectsForSearchV2")]
    public async Task<IActionResult> GetAllProjectsForSearchV2(int UserId, string Role, string Type)
    {
        try
        {
            IEnumerable<ProjectDTO> users = Enumerable.Empty<ProjectDTO>();
            if (Type == "PROJECT")
                users = await _projService.GetAllProjectsForSearch(UserId, Role, null);
            if (Type == "NBBUD")
            {
                var NB = _context.NewBusinessBudgets.Where(p => p.NBType == "NBBUD").Select(p => new { ProjectId = p.BusinessBudgetId, p.BusinessBudgetId, Type = "NBBUD" });
                users = users.Concat(NB.Select(p => new ProjectDTO { ProjectId = p.ProjectId, Name = p.BusinessBudgetId, Type = p.Type }));
            }
            if (Type == "OPP")
            {
                var Opp = _context.NewBusinessBudgets.Where(p => p.NBType == "OPP").Select(p => new { ProjectId = p.BusinessBudgetId, p.BusinessBudgetId, Type = "OPP" });
                users = users.Concat(Opp.Select(p => new ProjectDTO { ProjectId = p.ProjectId, Name = p.BusinessBudgetId, Type = p.Type }));
            }
            return Ok(users.Select(p => new { p.ProjectId, p.Name, p.Type }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all projects.");
            return StatusCode(500, "Internal server error while retrieving projects.");
        }
    }
    [HttpGet("GetAllProjectsByUser/{UserId}")]
    public async Task<IActionResult> GetAllProjectsByUser(int UserId)
    {
        try
        {
            var users = await _projService.GetAllProjects(UserId);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all projects.");
            return StatusCode(500, "Internal server error while retrieving projects.");
        }
    }

    [HttpGet("GetAllProjectsForSearch/{proj_id?}")]
    public async Task<IActionResult> GetAllProjectsForSearch(string? proj_id)
    {
        try
        {
            var users = await _projService.GetAllProjectsForSearch(proj_id);
            //users.Select(p => new { p.ProjectId, p.Name }).ToList();
            return Ok(users.Select(p => new { p.ProjectId, p.Name }).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all projects.");
            return StatusCode(500, "Internal server error while retrieving projects.");
        }
    }


    [HttpGet("GetAllProjectsForValidate/{proj_id?}")]
    public async Task<IActionResult> GetAllProjectsForValidate(string? proj_id)
    {
        try
        {
            var users = await _projService.GetAllProjectsForValidate(proj_id);
            //users.Select(p => new { p.ProjectId, p.Name }).ToList();
            return Ok(users.Select(p => new { p.ProjectId, p.Name }).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all projects.");
            return StatusCode(500, "Internal server error while retrieving projects.");
        }
    }

    [HttpGet("GetAllProjectByProjId/{ProjId}")]
    public async Task<IActionResult> GetAllProjectByProjId(string ProjId)
    {
        try
        {
            var users = await _projService.GetAllProjectByProjId(ProjId);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all projects.");
            return StatusCode(500, "Internal server error while retrieving projects.");
        }
    }

    [HttpGet("GetAllProjectByProjId/{ProjId}/{PlType}")]
    public async Task<IActionResult> GetAllProjectByProjId(string ProjId, string PlType)
    {
        try
        {
            var users = await _projService.GetAllProjectByProjId(ProjId, PlType);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all projects.");
            return StatusCode(500, "Internal server error while retrieving projects.");
        }
    }

    [HttpGet("GetAllProjects/{orgId}")]
    public async Task<IActionResult> GetAllProjectsByOrg(string orgId)
    {
        try
        {
            var users = await _projService.GetAllProjectsByOrg(orgId);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching projects for orgId: {OrgId}", orgId);
            return StatusCode(500, "Internal server error while retrieving projects by organization.");
        }
    }

    [HttpGet("GetAllPlcs/{plc?}")]
    public async Task<IActionResult> GetAllPlcs(string? plc)
    {
        try
        {
            var users = await _projService.GetAllPlcs(plc);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching plcs");
            return StatusCode(500, "Internal server error while retrieving plcs.");
        }
    }

    [HttpGet("GetProjectPlans/{projectId?}")]
    public async Task<IActionResult> GetProjectPlans(string? projectId)
    {
        try
        {
            var projectPlans = await _projPlanService.GetProjectPlans(projectId);
            return Ok(projectPlans);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching project plans for ProjectId: {ProjectId}", projectId);
            return StatusCode(500, "Internal server error while retrieving project plans.");
        }
    }

    [HttpGet("GetProjectPlans/{UserId}/{Role}/{projectId?}")]
    public async Task<IActionResult> GetProjectPlans(int UserId, string Role, string? projectId, string? status, string fetchNewBussiness)
    {
        try
        {
            var projectPlans = await _projPlanService.GetProjectPlans(UserId, Role, projectId, status, fetchNewBussiness);
            return Ok(projectPlans);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching project plans for ProjectId: {ProjectId}", projectId);
            return StatusCode(500, "Internal server error while retrieving project plans.");
        }
    }

  
    [HttpGet("GetProjectPlansV1/{UserId}/{Role}/{projectId?}")]
    public async Task<IActionResult> GetProjectPlansV1(int UserId, string Role, string? projectId, string? status, string type)
    {
        if (projectId != null && !string.IsNullOrEmpty(projectId))
            projectId = Uri.UnescapeDataString(projectId);
        try
        {
            var projectPlans = await _projPlanService.GetProjectPlansV1(UserId, Role, projectId, status, type);
            return Ok(projectPlans);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching project plans for ProjectId: {ProjectId}", projectId);
            return StatusCode(500, "Internal server error while retrieving project plans.");
        }
    }


    [HttpGet("GetProjectPlansPaged/{UserId}/{Role}/{projectId?}")]
    public async Task<IActionResult> GetProjectPlans(
    int UserId,
    string Role,
    string? projectId,
    string? status,
    string? planstatus,
    string? planType,
    string fetchNewBussiness,
    int pageNumber = 1,
    int pageSize = 10)
    {
        try
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize > 100) pageSize = 100; // Prevent large payloads

            var pagedResult = await _projPlanService.GetProjectPlansPaged(
                UserId,
                Role,
                projectId,
                status,
                planstatus,
                planType,
                fetchNewBussiness,
                pageNumber,
                pageSize);

            return Ok(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching project plans for ProjectId: {ProjectId}", projectId);
            return StatusCode(500, "Internal server error while retrieving project plans.");
        }
    }


    //[HttpGet("GetProjectPlansV1/{UserId}/{Role}/{projectId?}")]
    //public async Task<IActionResult> GetProjectPlans(
    //int UserId,
    //string Role,
    //string? projectId,
    //[FromQuery] int pageNumber = 1,
    //[FromQuery] int pageSize = 10)
    //{
    //    if (pageNumber <= 0 || pageSize <= 0)
    //        return BadRequest("PageNumber and PageSize must be greater than zero.");

    //    try
    //    {
    //        var pagedResult = await _projPlanService.GetProjectPlans(
    //            UserId, Role, projectId, pageNumber, pageSize);

    //        return Ok(pagedResult);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex,
    //            "Error fetching project plans for ProjectId: {ProjectId}", projectId);

    //        return StatusCode(500,
    //            "Internal server error while retrieving project plans.");
    //    }
    //}

    [HttpGet("MassUtilityGetAllPlans")]
    public async Task<IActionResult> MassUtilityGetAllPlans(int UserId, string Role,
    string? search,
    string? type,
    string? status)
    {
        if (search != null && !string.IsNullOrEmpty(search))
            search = Uri.UnescapeDataString(search);
        string[] projIds = Array.Empty<string>();
        try
        {

            projIds = await _context.UserProjectMaps
            .Where(u => u.UserId == UserId)
            .Select(u => u.ProjId)
            .ToArrayAsync();

            var query = _context.PlProjectPlans.Include(p => p.Proj).AsQueryable();

            // common search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(pp => pp.ProjId.StartsWith(search));
            }

            // role-based filter
            if (!Role.Equals("ADMIN", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(pp => projIds.Contains(pp.ProjId));
            }

            // optional filters
            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(pp => pp.PlType == type);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(pp => pp.Status == status);
            }

            //var query = _context.PlProjectPlans;

            //if (Role.ToUpper() == "ADMIN")
            //{
            //    query = query.Where(pp => pp.ProjId.StartsWith(search ?? ""));
            //}
            //else
            //{
            //    query = query.Where(pp => pp.ProjId.StartsWith(search ?? "") && projIds.Contains(pp.ProjId));
            //}

            //if (!string.IsNullOrEmpty(type))
            //{
            //    query = query.Where(x => x.PlType == type);
            //}

            //if (!string.IsNullOrEmpty(status))
            //{
            //    query = query.Where(x => x.Status == status);
            //}

            var plans = await query.ToListAsync();

            var nbs = _context.NewBusinessBudgets.Select(p => new PlProject() { ProjName = p.Description, ProjId = p.BusinessBudgetId }).ToList();

            foreach (var plan in plans)
            {
                if (plan.PlType.Equals("NBBUD"))
                {
                    var nb = nbs.FirstOrDefault(p => p.ProjId == plan.ProjId);
                    if (nb != null)
                    {
                        plan.ProjName = nb.ProjName;
                        continue;
                    }
                }
                plan.ProjName = plan.Proj?.ProjName;
            }

            return Ok(plans);
        }
        catch
        {
            return StatusCode(500, "Internal server error while retrieving project plans.");
        }
    }



    [HttpGet("GetAllPlans")]
    public async Task<IActionResult> GetAllPlans(
        string? ProjectIdStatus,
        string? ProjectPOPEnded,
        string? PlanType,
        string? Status,
        int? Version)
    {
        try
        {
            var query =
    from p in _context.PlProjectPlans
    join pp in _context.PlProjectPlans
        on p.ProjId equals pp.ProjId into planGroup
    from pp in planGroup
    where pp != null
    select new PlProjectPlan
    {
        ProjId = pp.ProjId,
        PlId = pp != null ? pp.PlId : null,
        PlType = pp != null ? pp.PlType : null,
        Version = pp != null ? pp.Version : null,
        VersionCode = pp != null ? pp.VersionCode : null,
        FinalVersion = pp != null && pp.FinalVersion.GetValueOrDefault(),
        IsCompleted = pp != null && pp.IsCompleted.GetValueOrDefault(),
        IsApproved = pp != null && pp.IsApproved.GetValueOrDefault(),
        Status = pp != null ? pp.Status : null,
        ClosedPeriod = pp != null ? pp.ClosedPeriod : null,
        CreatedAt = pp != null ? pp.CreatedAt : null,
        UpdatedAt = pp != null ? pp.UpdatedAt : null,
        ModifiedBy = pp != null ? pp.ModifiedBy : null,
        ApprovedBy = pp != null ? pp.ApprovedBy : null,
        CreatedBy = pp != null ? pp.CreatedBy : null,
        Source = pp != null ? pp.Source : null,
        Type = pp != null ? pp.Type : null,
        TemplateId = pp != null ? pp.TemplateId : null,
        ProjName = p.ProjName,
        OrgId = p.OrgId,
        ProjEndDt = p.ProjEndDt,
        ProjStartDt = p.ProjStartDt,
        AcctGrpCd = p.AcctGrpCd
    };

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            // -----------------------------
            // OPTIONAL FILTERS
            // -----------------------------

            if (!string.IsNullOrEmpty(ProjectIdStatus))
            {
                query = query.Where(x => x.ProjectStatus == ProjectIdStatus);
            }

            if (!string.IsNullOrEmpty(PlanType))
            {
                query = query.Where(x => x.PlType == PlanType);
            }

            if (!string.IsNullOrEmpty(Status))
            {
                query = query.Where(x => x.Status == Status);
            }

            if (Version != null)
            {
                query = query.Where(x => x.Version == Version);
            }

            if (!string.IsNullOrEmpty(ProjectPOPEnded))
            {
                if (ProjectPOPEnded == "Y")
                {
                    query = query.Where(x => x.ProjEndDt < today);
                }
                else if (ProjectPOPEnded == "N")
                {
                    query = query.Where(x => x.ProjEndDt < today);
                }
            }

            var plans = await query.ToListAsync();

            return Ok(plans);
        }
        catch
        {
            return StatusCode(500, "Internal server error while retrieving project plans.");
        }
    }



    //[HttpGet("GetAllPlans")]
    //public async Task<IActionResult> GetAllPlans(string? ProjectIdStatus, string? ProjectPOPEnded, string? PlanType,string? Status,string? Version)
    //{
    //    try
    //    {
    //        var result = _context.ProjectWithPlanDto
    //        .FromSqlRaw(@"
    //            SELECT
    //                p.proj_id AS ProjId,
    //                p.proj_type_dc AS ProjType,
    //                pp.pl_id AS PlId,
    //                pp.pl_type AS PlType,
    //                pp.version AS Version,
    //                pp.version_code AS VersionCode,
    //                pp.final_version AS FinalVersion,
    //                pp.is_completed AS IsCompleted,
    //                pp.is_approved AS IsApproved,
    //                pp.status AS Status,                    
    //                p.active_fl AS ProjectStatus,
    //                pp.closed_period AS ClosedPeriod,
    //                pp.created_at AS CreatedAt,
    //                pp.updated_at AS UpdatedAt,
    //                pp.modified_by AS ModifiedBy,
    //                pp.approved_by AS ApprovedBy,
    //                pp.created_by AS CreatedBy,
    //                pp.source AS Source,
    //                pp.type AS Type,
    //                pp.burden_template_id AS BurdenTemplateId,
    //                COALESCE(p.proj_f_fee_amt,0) AS proj_f_fee_amt,
    //                COALESCE(p.proj_f_cst_amt,0) AS proj_f_cst_amt,
    //                COALESCE(p.proj_f_tot_amt,0) AS proj_f_tot_amt,
    //                p.proj_name AS ProjName,
    //                p.org_id AS OrgId,
    //                p.proj_end_dt AS ProjEndDt,
    //                p.proj_start_dt AS ProjStartDt,
    //                p.acct_grp_cd AS AcctGrpCd
    //            FROM pl_project p
    //            LEFT JOIN pl_project_plan pp ON pp.proj_id = p.proj_id
    //        ", ProjectIdStatus + "%")
    //        .ToList();

    //        var plans = (IEnumerable<PlProjectPlan>)result.Select(p => p.ToEntity()).ToList();


    //        return Ok(plans);
    //    }
    //    catch (Exception ex)
    //    {
    //        return StatusCode(500, "Internal server error while retrieving project plans.");
    //    }
    //}

    [HttpGet("GetAllNewBussiness/{nbId}")]
    public async Task<IActionResult> GetAllNewBussiness(string nbId)
    {
        try
        {
            var projectPlans = await _projPlanService.GetAllNewBussiness(nbId);
            return Ok(projectPlans);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching project plans for ProjectId");
            return StatusCode(500, "Internal server error while retrieving project plans.");
        }
    }



    [HttpGet("GetForecastDataByPlanId/{planId}")]
    public async Task<IActionResult> GetForecastDataByPlanId(int planId)
    {
        try
        {
            var forecastData = await _projPlanService.GetForecastByPlanID(planId);
            return Ok(forecastData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving forecast data for planId: {PlanId}", planId);
            return StatusCode(500, "Internal server error while retrieving forecast data.");
        }
    }

    [HttpGet("GetEmployeeForecastByPlanID/{planId?}/{year?}")]
    public async Task<IActionResult> GetEmployeeForecastByPlanID(int? planId, int? year)
    {
        try
        {
            var forecastData = await _projPlanService.GetEmployeeForecastByPlanID(planId.GetValueOrDefault(), year.GetValueOrDefault());

            return Ok(forecastData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving forecast data for planId: {PlanId}", planId);
            return StatusCode(500, "Internal server error while retrieving forecast data.");
        }
    }
    [HttpGet("GetEmployeeForecastByPlanIDPaged/{planId?}/{year?}")]
    public async Task<PagedResponse<forecast>> GetEmployeeForecastByPlanIDPaged(
    int planId,
    string? emplid,
    int? year,
    int pageNumber,
    int pageSize)
    {
        var forecastData = await _projPlanService.GetEmployeeForecastByPlanID(planId, year.GetValueOrDefault());
        //var forecastData = await _projPlanService.GetEmployeeForecastByPlanID(planId, year, emplid, pageNumber, pageSize);

        List<forecast> data = new List<forecast>();
        //var totalRecords = forecastData.Count();

        string projId = await _context.PlProjectPlans
            .Where(p => p.PlId == planId)
            .Select(p => p.ProjId)
            .FirstOrDefaultAsync();
        //var data = forecastData
        //    .Skip((pageNumber - 1) * pageSize)
        //    .Take(pageSize)
        //    .ToList();

        DateOnly escalationMonth = DateOnly.MinValue;
        decimal escalationPercentage = 0;

        // -------------------------------------------------------
        // ESCALATION MONTH
        // -------------------------------------------------------

        var escalationMonthConfig = await _context.PlConfigValues
            .Where(x =>
                (x.ProjId == projId || x.ProjId == "xxxxx") &&
                x.Name == "escallation_month")
            .OrderByDescending(x => x.ProjId == projId)
            .FirstOrDefaultAsync();

        if (escalationMonthConfig != null &&
            int.TryParse(escalationMonthConfig.Value, out int escalationDay))
        {
            escalationMonth = new DateOnly(
                DateTime.UtcNow.Year,
                DateTime.UtcNow.Month,
                escalationDay);

            // -------------------------------------------------------
            // ESCALATION PERCENTAGE
            // -------------------------------------------------------

            var escalationPercentageValue = await _context.PlConfigValues
                .Where(x =>
                    (x.ProjId == projId || x.ProjId == "xxxxx") &&
                    x.Name == "escallation_percentage")
                .OrderByDescending(x => x.ProjId == projId)
                .Select(x => x.Value)
                .FirstOrDefaultAsync();

            decimal.TryParse(
                escalationPercentageValue,
                out escalationPercentage);
        }

        if (emplid == null)
        {
            data = forecastData
                //.Skip((pageNumber - 1) * pageSize)
                //.Take(pageSize)
                .ToList();
        }
        else
        {


            data = forecastData?
                    .Where(p => p?.Emple != null &&
                                (
                                    (!string.IsNullOrEmpty(p.Emple.EmplId) &&
                                    p.Emple.EmplId.StartsWith(emplid, StringComparison.OrdinalIgnoreCase))
                                    ||
                                    (!string.IsNullOrEmpty(p.Emple.FirstName) &&
                                    p.Emple.FirstName.StartsWith(emplid, StringComparison.OrdinalIgnoreCase))
                                ))
                    .ToList() ?? new List<forecast>();


            //data = forecastData.Where(p => p.Emple.EmplId.ToUpper().StartsWith(emplid.ToUpper()) || p.Emple.FirstName.ToUpper().StartsWith(emplid.ToUpper()) || p.Emple.PlcGlcCode.ToUpper().StartsWith(emplid.ToUpper()))
            //    //.Skip((pageNumber - 1) * pageSize)
            //    //.Take(pageSize)
            //    .ToList();
            //data = forecastData
            //        .Where(p =>
            //            EF.Functions.ILike(p.Emple.EmplId, $"%{emplid}%") ||
            //            EF.Functions.ILike(p.Emple.FirstName, $"%{emplid}%") || EF.Functions.ILike(p.Emple.PlcGlcCode, $"%{emplid}%"))
            //        //.Skip((pageNumber - 1) * pageSize)
            //        //.Take(pageSize)
            //        .ToList();

        }

        data.ForEach(x =>
        {
            x.Emple.EffectiveDate = escalationMonth;
        });

        var totalRecords = data.Count();

        return new PagedResponse<forecast>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            Data = data.Skip((pageNumber - 1) * pageSize).Take(pageSize)
        };
    }


    [HttpGet("GetDirectCostForecastByPlanIDPaged/{planId?}/{year?}")]
    public async Task<PagedResponse<DirectCostforecast>> GetDirectCostForecastByPlanIDPaged(
    int planId,
    string? emplid,
    int? year,
    int pageNumber,
    int pageSize)
    {
        List<DirectCostforecast> data = new List<DirectCostforecast>();
        var forecastData = await _projPlanService.GetDirectCostForecastByPlanID(planId, year.GetValueOrDefault());
        //var forecastData = await _projPlanService.GetDirectCostForecastByPlanID(planId, year, emplid, pageNumber, pageSize);


        //var totalRecords = forecastData.Count();

        if (emplid == null)
        {
            data = forecastData
                //.Skip((pageNumber - 1) * pageSize)
                //.Take(pageSize)
                .ToList();
        }
        else
        {
            data = forecastData?
                   .Where(p => p?.Empl != null &&
                               (
                                   (!string.IsNullOrEmpty(p.Empl.Id) &&
                                    p.Empl.Id.StartsWith(emplid, StringComparison.OrdinalIgnoreCase))
                                   ||
                                   (!string.IsNullOrEmpty(p.Empl.Category) &&
                                    p.Empl.Category.StartsWith(emplid, StringComparison.OrdinalIgnoreCase))
                               ))
                   .ToList() ?? new List<DirectCostforecast>();
            //data = forecastData.Where(p => p.Empl.Id.ToUpper().StartsWith(emplid.ToUpper()))
            //data = forecastData.Where(p => p.Empl.Id.ToUpper().StartsWith(emplid.ToUpper()) || p.Empl.Category.ToUpper().StartsWith(emplid.ToUpper()))

            //    //.Skip((pageNumber - 1) * pageSize)
            //    //.Take(pageSize)
            //    .ToList();
        }
        var totalRecords = data.Count();

        return new PagedResponse<DirectCostforecast>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            Data = data.Skip((pageNumber - 1) * pageSize).Take(pageSize)
        };
    }

    [HttpGet("GetEmployeeForecastByPlanIDTest/{planId?}/{year?}")]
    public async Task<IActionResult> GetEmployeeForecastByPlanIDTest(int? planId, int? year, int? take = null)
    {
        try
        {
            var forecastData = await _projPlanService.GetEmployeeForecastByPlanID(planId.GetValueOrDefault(), year.GetValueOrDefault());

            if (take.HasValue && take.Value > 0)
            {
                forecastData = forecastData.Take(take.Value);
            }
            return Ok(forecastData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving forecast data for planId: {PlanId}", planId);
            return StatusCode(500, "Internal server error while retrieving forecast data.");
        }
    }

    [HttpGet("GetDirectCostForecastDataByPlanId/{planId?}/{year?}")]
    public async Task<IActionResult> GetDirectCostForecastDataByPlanId(int? planId, int? year)
    {
        try
        {
            var forecastData = await _projPlanService.GetDirectCostForecastByPlanID(planId.GetValueOrDefault(), year.GetValueOrDefault());
            return Ok(forecastData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving direct cost forecast data for planId: {PlanId}", planId);
            return StatusCode(500, "Internal server error while retrieving direct cost forecast data.");
        }
    }

    [HttpGet("GetEACDataByPlanId/{planId}")]
    public async Task<IActionResult> GetEACDataByPlanId(int planId)
    {
        try
        {
            var forecastData = await _projPlanService.GetEACDataByPlanId(planId);
            return Ok(forecastData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving forecast data for planId: {PlanId}", planId);
            return StatusCode(500, "Internal server error while retrieving forecast data.");
        }
    }

    [HttpGet("GetDirectCostEACDataByPlanId/{planId?}/{year?}")]
    public async Task<IActionResult> GetDirectCostEACDataByPlanId(int? planId, int? year)
    {
        try
        {
            var forecastData = await _projPlanService.GetDirectCostEACDataByPlanId(planId.GetValueOrDefault(), year.GetValueOrDefault());
            return Ok(forecastData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving forecast data for planId: {PlanId}", planId);
            return StatusCode(500, "Internal server error while retrieving forecast data.");
        }
    }
    [HttpGet("GetEmployeesByProject/{projId}")]
    public List<EmployeeDTOs> GetEmployeesByProject(string projId, string type)
    {

        List<EmployeeDTOs> EmployeeDTOs = new List<EmployeeDTOs>();

        //ontext.ProjEmployeeLabcats.Where(p => p.ProjId == projId).ToList();
        string workforce = "false";
        try
        {
            workforce = _context.PlConfigValues
                        .FirstOrDefault(r => r.Name.ToLower() == "workforce" && r.ProjId == projId)?.Value
                        ?? "false";
        }
        catch (Exception ex) { }

        if (workforce.ToUpper() == "TRUE")
        {
            EmployeeDTOs = _context.EmployeeDTOs
                            .FromSqlRaw(@"SELECT distinct 
                                a.empl_id AS EmpId, 
                                a.last_first_name AS EmployeeName, 
                                b.hrly_amt AS HrRate,
                                d.bill_lab_cat_cd AS Plc,
	                            b.org_id As OrgId,
                                b.effect_dt AS EffectiveDate,
								'50-000-000' AS AcctId,
								'Direct Lbr-Onsite' AS AcctName,
								f.org_name As OrgName,
                                b.lab_grp_type As LaborGroup
                            FROM public.empl a
                            JOIN public.empl_lab_info b
                                ON a.empl_id = b.empl_id
                            JOIN public.organization f
                                ON f.org_id = b.org_id	
                            JOIN public.pl_employee_project_mapping c
                                ON a.empl_id = c.empl_empl_id
                            JOIN public.proj_employee_labcat d
                                ON c.proj_proj_id = d.proj_id
                               AND a.empl_id = d.empl_id
                             -- ND d.dflt_fl = 'Y'
                            WHERE b.end_dt = '2078-12-31'
                              AND c.proj_proj_id = '" + projId + "'")
                            .ToList();

        }
        else
        {

            EmployeeDTOs = _context.EmployeeDTOs
                    .FromSqlRaw(@"SELECT DISTINCT 
                    a.empl_id AS EmpId, 
                    a.last_first_name AS EmployeeName, 
                    b.hrly_amt AS HrRate,

                    CASE 
                        WHEN d.dflt_fl = 'Y' THEN d.bill_lab_cat_cd 
                        ELSE '' 
                    END AS Plc,

                    b.org_id AS OrgId,
                    b.effect_dt AS EffectiveDate,
                    '50-000-000' AS AcctId,
                    'Direct Lbr-Onsite' AS AcctName,
                    f.org_name AS OrgName,
                    b.lab_grp_type AS LaborGroup

                FROM public.empl a

                JOIN public.empl_lab_info b 
                    ON a.empl_id = b.empl_id

                JOIN public.organization f
                    ON f.org_id = b.org_id    

                LEFT JOIN public.proj_employee_labcat d
                    ON a.empl_id = d.empl_id 
                   AND d.dflt_fl = 'Y'   -- keep condition here

                WHERE b.end_dt = '2078-12-31';")
                    .ToList();

        }

        // Fetch project with Org
        var project = _context.PlProjects
            .AsNoTracking()
            .Include(p => p.Org)
            .FirstOrDefault(p => p.ProjId == projId);

        string pltype = string.Empty, OrgId = string.Empty;
        if (project == null)
        {
            var NBBudget = _context.NewBusinessBudgets.Where(p => p.BusinessBudgetId == projId).FirstOrDefault();

            if (NBBudget != null)
            {
                pltype = "NBBUD";
                OrgId = NBBudget.OrgId ?? string.Empty;
            }
        }

        // Fetch latest transaction per employee (ONE DB hit)
        var latestEmpTransactions = _context.PlFinancialTransactions
            .AsNoTracking()
            .Where(p => p.ProjId == projId && p.SIdType == "E")
            .GroupBy(p => p.Id)
            .Select(g => g
                .Select(x => new
                {
                    x.Id,
                    x.OrgId,
                    x.AcctId,
                    x.BillLabCatCd
                })
                .First())
            .ToDictionary(x => x.Id);

        var Orgs = _context.Organizations
            .AsNoTracking()
            .ToDictionary(o => o.OrgId, o => o.OrgName);

        var Plcs = _context.PlcCodes
            .AsNoTracking()
            .ToDictionary(p => p.LaborCategoryCode, p => p.Description);

        // Defaults
        string defaultAcctId = "";
        string defaultAcctName = "";
        if (type.ToUpper() == "HOURS")
        {
            defaultAcctId = "50-000-000";
            defaultAcctName = "Direct Lbr-Onsite";
        }
        else
        {
            defaultAcctId = "50-400-000";
            defaultAcctName = "Travel Expense";
        }


        foreach (var item in EmployeeDTOs)
        {
            if (item.EmpId == "1002645")
            {

            }
            if (latestEmpTransactions.TryGetValue(item.EmpId, out var tx))
            {
                item.OrgId = tx.OrgId;
                item.AcctId = tx.AcctId;
                item.Plc = tx.BillLabCatCd;
                if (!string.IsNullOrEmpty(tx.BillLabCatCd))
                {
                    Plcs.TryGetValue(tx.BillLabCatCd, out string? plcName);
                    item.Plc = item.Plc + " - (" + plcName + ")";
                }
                if (Orgs.TryGetValue(tx.OrgId, out string? orgName))
                {
                    item.OrgName = orgName;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(item.Plc))
                {
                    Plcs.TryGetValue(item.Plc, out string? plcName);
                    item.Plc = item.Plc + " - (" + plcName + ")";
                }
                if (project?.Org != null)
                {
                    item.OrgId = project.OrgId;
                    item.OrgName = project.Org.OrgName;
                }
                if (pltype.ToUpper() == "NBBUD")
                {
                    item.OrgId = OrgId;
                    if (Orgs.TryGetValue(OrgId, out string? orgName))
                    {
                        item.OrgName = orgName;
                    }
                }
                item.AcctId = defaultAcctId;
                item.AcctName = defaultAcctName;
            }
        }




        return EmployeeDTOs;
    }


    [HttpGet("GetEmployeesByProjectV1/{projId}")]
    public List<EmployeeDTOs> GetEmployeesByProjectV1(string projId, string type, string accountGroupCode)
    {

        List<EmployeeDTOs> EmployeeDTOs = new List<EmployeeDTOs>();

        //context.ProjEmployeeLabcats.Where(p => p.ProjId == projId).ToList();
        string workforce = "false";
        try
        {
            workforce = _context.PlConfigValues
                        .FirstOrDefault(r => r.Name.ToLower() == "workforce" && r.ProjId == projId)?.Value
                        ?? "false";
        }
        catch (Exception ex) { }

        if (workforce.ToUpper() == "TRUE")
        {
            EmployeeDTOs = _context.EmployeeDTOs
                            .FromSqlRaw(@"SELECT distinct 
                                a.empl_id AS EmpId, 
                                a.last_first_name AS EmployeeName, 
                                b.hrly_amt AS HrRate,
                                d.bill_lab_cat_cd AS Plc,
	                            b.org_id As OrgId,
                                b.effect_dt AS EffectiveDate,
								'50-000-000' AS AcctId,
								'Direct Lbr-Onsite' AS AcctName,
								f.org_name As OrgName,
                                b.lab_grp_type As LaborGroup
                            FROM public.empl a
                            JOIN public.empl_lab_info b
                                ON a.empl_id = b.empl_id
                            JOIN public.organization f
                                ON f.org_id = b.org_id	
                            JOIN public.pl_employee_project_mapping c
                                ON a.empl_id = c.empl_empl_id
                            JOIN public.proj_employee_labcat d
                                ON c.proj_proj_id = d.proj_id
                               AND a.empl_id = d.empl_id
                             -- ND d.dflt_fl = 'Y'
                            WHERE b.end_dt = '2078-12-31'
                              AND c.proj_proj_id = '" + projId + "'")
                            .ToList();

        }
        else
        {

            //    EmployeeDTOs = _context.EmployeeDTOs
            //        .FromSqlRaw(@"SELECT distinct 
            //                        a.empl_id AS EmpId, 
            //                        a.last_first_name AS EmployeeName, 
            //                        b.hrly_amt AS HrRate,
            //                        d.bill_lab_cat_cd AS Plc,
            //                     b.org_id As OrgId,
            //                        b.effect_dt AS EffectiveDate,
            // '50-000-000' AS AcctId,
            //  'Direct Lbr-Onsite' AS AcctName,
            //f.org_name As OrgName,
            //                        b.lab_grp_type As LaborGroup
            //                    FROM public.empl a
            //                    JOIN public.empl_lab_info b 
            //                        ON a.empl_id = b.empl_id
            //                    JOIN public.organization f
            //                        ON f.org_id = b.org_id	
            //                    JOIN public.proj_employee_labcat d
            //                        ON a.empl_id = d.empl_id 
            //                       AND d.dflt_fl = 'Y'
            //                    WHERE b.end_dt = '2078-12-31'")
            //        .ToList();

            EmployeeDTOs = _context.EmployeeDTOs
                    .FromSqlRaw(@"SELECT DISTINCT 
                    a.empl_id AS EmpId, 
                    a.last_first_name AS EmployeeName, 
                    b.hrly_amt AS HrRate,

                    CASE 
                        WHEN d.dflt_fl = 'Y' THEN d.bill_lab_cat_cd 
                        ELSE '' 
                    END AS Plc,

                    b.org_id AS OrgId,
                    b.effect_dt AS EffectiveDate,
                    '50-000-000' AS AcctId,
                    'Direct Lbr-Onsite' AS AcctName,
                    f.org_name AS OrgName,
                    b.lab_grp_type AS LaborGroup

                FROM public.empl a

                JOIN public.empl_lab_info b 
                    ON a.empl_id = b.empl_id

                JOIN public.organization f
                    ON f.org_id = b.org_id    

                LEFT JOIN public.proj_employee_labcat d
                    ON a.empl_id = d.empl_id 
                   AND d.dflt_fl = 'Y'   -- keep condition here

                WHERE b.end_dt = '2078-12-31';")
                    .ToList();

        }

        // Fetch project with Org
        var project = _context.PlProjects
            .AsNoTracking()
            .Include(p => p.Org)
            .FirstOrDefault(p => p.ProjId == projId);

        string pltype = string.Empty, OrgId = string.Empty;
        if (project == null)
        {
            var NBBudget = _context.NewBusinessBudgets.Where(p => p.BusinessBudgetId == projId).FirstOrDefault();

            if (NBBudget != null)
            {
                pltype = "NBBUD";
                OrgId = NBBudget.OrgId ?? string.Empty;
            }
        }

        // Fetch latest transaction per employee (ONE DB hit)
        //var latestEmpTransactions = _context.PlFinancialTransactions
        //    .AsNoTracking()
        //    .Where(p => p.ProjId == projId && p.SIdType == "E")
        //    .GroupBy(p => p.Id)
        //    .Select(g => g
        //        .Select(x => new
        //        {
        //            x.Id,
        //            x.OrgId,
        //            x.AcctId,
        //            x.BillLabCatCd
        //        })
        //        .FirstOrDefault())
        //    .ToDictionary(x => x.Id);

        var Orgs = _context.Organizations
            .AsNoTracking()
            .ToDictionary(o => o.OrgId, o => o.OrgName);

        var Plcs = _context.PlcCodes
            .AsNoTracking()
            .ToDictionary(p => p.LaborCategoryCode, p => p.Description);

        var laborProjectAccountDictionary = _context.LaborProjectAccount
            .AsNoTracking()
            .Distinct()
            .ToDictionary(
                p => (p.LaborGroup, p.ProjectAccountGroup),
                p => (p.Account, p.AccountName)
            );

        // Defaults
        string defaultAcctId = "";
        string defaultAcctName = "";
        DateOnly escllatiom_Month = DateOnly.MinValue;
        decimal escalationPercentage = 0;

        var configValues = _context.PlConfigValues.FirstOrDefault(p => p.ProjId == projId && p.Name == "escallation_month");
        if (configValues == null)
        {
            configValues = _context.PlConfigValues.FirstOrDefault(p => p.ProjId == "xxxxx" && p.Name == "escallation_month");
            if (configValues != null)
            {
                escllatiom_Month = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, int.Parse(configValues.Value));
                escalationPercentage = decimal.Parse(_context.PlConfigValues.FirstOrDefault(p => p.ProjId == projId && p.Name == "escallation_percentage")?.Value ?? "0");
            }
        }
        else
        {
            escllatiom_Month = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, int.Parse(configValues.Value));
            escalationPercentage = decimal.Parse(_context.PlConfigValues.FirstOrDefault(p => p.ProjId == projId && p.Name == "escallation_percentage")?.Value ?? "0");
        }

        foreach (var item in EmployeeDTOs)
        {
            if (item.EmpId == "1002645")
            {

            }
            item.EffectiveDate = escllatiom_Month;

            //if (latestEmpTransactions.TryGetValue(item.EmpId, out var tx))
            //{
            //    item.OrgId = tx.OrgId;
            //    item.AcctId = tx.AcctId;
            //    item.Plc = tx.BillLabCatCd;
            //    if (!string.IsNullOrEmpty(tx.BillLabCatCd))
            //    {
            //        Plcs.TryGetValue(tx.BillLabCatCd, out string? plcName);
            //        item.Plc = item.Plc + " - (" + plcName + ")";
            //    }
            //    if (Orgs.TryGetValue(tx.OrgId, out string? orgName))
            //    {
            //        item.OrgName = orgName;
            //    }
            //}
            //else
            {
                if (!string.IsNullOrEmpty(item.Plc))
                {
                    Plcs.TryGetValue(item.Plc, out string? plcName);
                    item.Plc = item.Plc + " - (" + plcName + ")";
                }
                if (project?.Org != null)
                {
                    item.OrgId = project.OrgId;
                    item.OrgName = project.Org.OrgName;
                }
                if (pltype.ToUpper() == "NBBUD")
                {
                    item.OrgId = OrgId;
                    if (Orgs.TryGetValue(OrgId, out string? orgName))
                    {
                        item.OrgName = orgName;
                    }
                }


                if (type.ToUpper() == "HOURS")
                {
                    var key = (item.LaborGroup, accountGroupCode);
                    if (laborProjectAccountDictionary.TryGetValue(key, out var value))
                    {
                        item.AcctId = value.Account;
                        item.AcctName = value.AccountName;
                    }
                    else
                    {
                        item.AcctId = "";
                        item.AcctName = "";
                    }
                }
                else
                {
                    item.AcctId = _config["DefaultEmployeeNonLaborAccountId"] ?? string.Empty;
                    item.AcctName = _config["DefaultEmployeeNonLaborAccountName"] ?? string.Empty;
                }

                //item.AcctId = defaultAcctId;
                //item.AcctName = defaultAcctName;
            }
        }




        return EmployeeDTOs;
    }

    [HttpGet("GetVenderEmployeesByProject/{projId}")]
    public List<VendorEmployeeDTOs> GetVenderEmployeesByProject(string projId, string type)
    {
        string workforce = "false";
        List<VendorEmployeeDTOs> vendorEmployeeDTOs = new List<VendorEmployeeDTOs>();
        try
        {
            workforce = _context.PlConfigValues
                        .FirstOrDefault(r => r.Name.ToLower() == "workforce" && r.ProjId == projId)?.Value
                        ?? "false";
        }
        catch (Exception ex) { }

        if (workforce.ToUpper() == "TRUE")
        {
            //return _context.VendorEmployeeDTOs
            //    .FromSqlRaw(@"
            //        SELECT 
            //        ve.vend_empl_id as EmpId, ve.vend_empl_name as EmployeeName, ve.df_bill_lab_cat_cd as Plc, ve.vend_id as VendId
            //        FROM vendor_employee ve
            //        INNER JOIN proj_vend_empl_mapping pvem
            //            ON ve.vend_id = pvem.vend_id
            //            AND ve.vend_empl_id = pvem.vend_empl_id
            //        WHERE pvem.proj_id = {0} 
            //            AND ve.vend_empl_status = 'A'  
            //            AND (pvem.end_dt IS NULL OR pvem.end_dt > CURRENT_DATE)
            //    ", projId)
            //    .ToList();

            vendorEmployeeDTOs = _context.VendorEmployeeDTOs
    .FromSqlRaw(@"SELECT 
                ve.vend_empl_id AS EmpId, 
                ve.vend_empl_name AS EmployeeName, 
                ve.vend_id AS VendId,
                NULL::varchar AS ""OrgId"",
                NULL::varchar AS ""OrgName"",
                NULL::varchar AS ""AcctId"",
                NULL::varchar AS ""AcctName"",
                pel.bill_lab_cat_cd AS Plc
            FROM vendor_employee ve
            INNER JOIN proj_vend_empl_mapping pvem
                ON ve.vend_id = pvem.vend_id
               AND ve.vend_empl_id = pvem.vend_empl_id
            LEFT JOIN proj_vendor_employee_labcat pel
                ON pvem.proj_id = pel.proj_id
               AND pvem.vend_id = pel.vend_id
               AND pvem.vend_empl_id = pel.vend_empl_id
               AND pel.dflt_fl = 'Y'
            WHERE pvem.proj_id = {0} 
              AND ve.vend_empl_status = 'A'  
              AND (pvem.end_dt IS NULL OR pvem.end_dt > CURRENT_DATE)", projId)
    .ToList();
        }
        else
        {
            //return _context.VendorEmployeeDTOs
            //    .FromSqlRaw(@"
            //                SELECT 
            //                    ve.vend_empl_id as EmpId, ve.vend_empl_name as EmployeeName, ve.df_bill_lab_cat_cd as Plc, ve.vend_id as VendId
            //                FROM vendor_employee ve")
            //    .ToList();
            vendorEmployeeDTOs = _context.VendorEmployeeDTOs
                        .FromSqlRaw(@"SELECT 
                            ve.vend_empl_id AS EmpId, 
                            ve.vend_empl_name AS EmployeeName, 
                            ve.vend_id AS VendId,
                            NULL::varchar AS ""OrgId"",
                            NULL::varchar AS ""OrgName"",
                            NULL::varchar AS ""AcctId"",
                            NULL::varchar AS ""AcctName"",
                            COALESCE(pel.bill_lab_cat_cd, ve.df_bill_lab_cat_cd) AS Plc
                        FROM vendor_employee ve
                        LEFT JOIN public.proj_vendor_employee_labcat pel
                            ON ve.vend_id = pel.vend_id
                           AND ve.vend_empl_id = pel.vend_empl_id
                           AND pel.dflt_fl = 'Y'
                           AND (pel.end_dt IS NULL OR pel.end_dt > CURRENT_DATE)
                        WHERE ve.vend_empl_status = 'A'")
                                                .ToList();
        }

        //var project = _context.PlProjects.Include(p => p.Org).FirstOrDefault(p => p.ProjId == projId);
        //foreach (var item in vendorEmployeeDTOs)
        //{
        //    try
        //    {
        //        if (project.Org != null)
        //        {
        //            item.OrgId = project.OrgId;
        //            item.OrgName = project.Org.OrgName;
        //        }
        //        item.AcctId = "51-000-000";
        //        item.AcctName = "Sub Labor Exp-Onsite";
        //    }
        //    catch (Exception ex) { }
        //}

        // Fetch project with Org
        var project = _context.PlProjects
            .AsNoTracking()
            .Include(p => p.Org)
            .FirstOrDefault(p => p.ProjId == projId);

        string pltype = string.Empty, OrgId = string.Empty;
        if (project == null)
        {
            var NBBudget = _context.NewBusinessBudgets.Where(p => p.BusinessBudgetId == projId).FirstOrDefault();

            if (NBBudget != null)
            {
                pltype = "NBBUD";
                OrgId = NBBudget.OrgId ?? string.Empty;
            }
        }

        var Orgs = _context.Organizations
        .AsNoTracking()
        .ToDictionary(o => o.OrgId, o => o.OrgName);

        var Plcs = _context.PlcCodes
            .AsNoTracking()
            .ToDictionary(p => p.LaborCategoryCode, p => p.Description);

        // Defaults
        //string defaultAcctId = "51-000-000";
        //string defaultAcctName = "Sub Labor Exp-Onsite";

        string defaultAcctId = "";
        string defaultAcctName = "";
        if (type.ToUpper() == "HOURS")
        {
            defaultAcctId = _config["DeaultVendorLaborAccountId"] ?? string.Empty; //"51-000-000";
            defaultAcctName = _config["DeaultVendorLaborAccountName"] ?? string.Empty;  //"Sub Labor Exp-Onsite";
        }
        else
        {
            defaultAcctId = _config["DeaultVendorNonLaborAccountId"] ?? string.Empty; //"51-400-000";
            defaultAcctName = _config["DeaultVendorNonLaborAccountName"] ?? string.Empty; //"Sub Travel Expense";
        }

        foreach (var item in vendorEmployeeDTOs)
        {
            if (!string.IsNullOrEmpty(item.Plc))
            {
                Plcs.TryGetValue(item.Plc, out string? plcName);
                item.Plc = item.Plc + " - (" + plcName + ")";
            }
            if (project?.Org != null)
            {
                item.OrgId = project.OrgId;
                item.OrgName = project.Org.OrgName;
            }
            if (pltype.ToUpper() == "NBBUD")
            {
                item.OrgId = OrgId;
                if (Orgs.TryGetValue(OrgId, out string? orgName))
                {
                    item.OrgName = orgName;
                }
            }
            item.AcctId = defaultAcctId;
            item.AcctName = defaultAcctName;
        }


        try
        {
            var latestEmpTransactions = _context.PlFinancialTransactions
                .AsNoTracking()
                .Where(p => p.ProjId == projId && p.SIdType != "E")
                .GroupBy(p => p.Id)
                .Select(g => g
                    .Select(x => new
                    {
                        x.Id,
                        x.OrgId,
                        x.AcctId,
                        x.BillLabCatCd
                    })
                    .First())
                .ToDictionary(x => x.Id);

            foreach (var item in vendorEmployeeDTOs)
            {
                if (item.EmpId == "1002645")
                {

                }
                if (latestEmpTransactions.TryGetValue(item.EmpId, out var tx))
                {
                    item.OrgId = tx.OrgId;
                    item.AcctId = tx.AcctId;
                    item.Plc = tx.BillLabCatCd;
                    if (!string.IsNullOrEmpty(tx.BillLabCatCd))
                    {
                        Plcs.TryGetValue(tx.BillLabCatCd, out string? plcName);
                        item.Plc = item.Plc + " - (" + plcName + ")";
                    }
                    if (Orgs.TryGetValue(tx.OrgId, out string? orgName))
                    {
                        item.OrgName = orgName;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(item.Plc))
                    {
                        Plcs.TryGetValue(item.Plc, out string? plcName);
                        item.Plc = item.Plc + " - (" + plcName + ")";
                    }
                    if (project?.Org != null)
                    {
                        item.OrgId = project.OrgId;
                        item.OrgName = project.Org.OrgName;
                    }
                    item.AcctId = defaultAcctId;
                    item.AcctName = defaultAcctName;
                }
            }


        }
        catch (Exception ex)
        {
        }

        //var Orgs = _context.Organizations
        //        .AsNoTracking()
        //        .ToDictionary(o => o.OrgId, o => o.OrgName);

        //var Plcs = _context.PlcCodes
        //    .AsNoTracking()
        //    .ToDictionary(p => p.LaborCategoryCode, p => p.Description);

        //// Defaults
        //string defaultAcctId = "51-000-000";
        //string defaultAcctName = "Sub Labor Exp-Onsite";

        //foreach (var item in vendorEmployeeDTOs)
        //{
        //    if (item.EmpId == "1002645")
        //    {

        //    }
        //    if (latestEmpTransactions.TryGetValue(item.EmpId, out var tx))
        //    {
        //        item.OrgId = tx.OrgId;
        //        item.AcctId = tx.AcctId;
        //        item.Plc = tx.BillLabCatCd;
        //        if (!string.IsNullOrEmpty(tx.BillLabCatCd))
        //        {
        //            Plcs.TryGetValue(tx.BillLabCatCd, out string? plcName);
        //            item.Plc = item.Plc + " - (" + plcName + ")";
        //        }
        //        if (Orgs.TryGetValue(tx.OrgId, out string? orgName))
        //        {
        //            item.OrgName = orgName;
        //        }
        //    }
        //    else
        //    {
        //        if (!string.IsNullOrEmpty(item.Plc))
        //        {
        //            Plcs.TryGetValue(item.Plc, out string? plcName);
        //            item.Plc = item.Plc + " - (" + plcName + ")";
        //        }
        //        if (project?.Org != null)
        //        {
        //            item.OrgId = project.OrgId;
        //            item.OrgName = project.Org.OrgName;
        //        }
        //        item.AcctId = defaultAcctId;
        //        item.AcctName = defaultAcctName;
        //    }
        //}

        return vendorEmployeeDTOs;
    }

    [HttpPost("AddProjectPlan")]
    public async Task<IActionResult> AddProjectPlan([FromBody] PlProjectPlan newPlan, string? type)
    {
        try
        {
            var result = await _projPlanService.AddProjectPlanAsync(
                newPlan,
                type ?? string.Empty);


            //PlForecastRepository plForecastRepository =
            //    new PlForecastRepository(_context, _config);

            //await _repository.GetByFilterAsync(result.ProjId, result.Version, newPlan.PlType, result.PlId);

            //await plForecastRepository.CalculateBurdenCost(
            //    result.PlId.GetValueOrDefault(),
            //    result.TemplateId.GetValueOrDefault(),
            //    type);

            // Run in background without waiting
            _ = Task.Run(async () =>
            {
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();

                    var context = scope.ServiceProvider
                        .GetRequiredService<MydatabaseContext>();

                    var config = scope.ServiceProvider
                        .GetRequiredService<IConfiguration>();

                    PlForecastRepository plForecastRepository =
                        new PlForecastRepository(context, config);

                    await _repository.GetByFilterAsync(result.ProjId, result.Version, newPlan.PlType, result.PlId);

                    await plForecastRepository.CalculateRevenueCost(
                        result.PlId.GetValueOrDefault(),
                        result.TemplateId.GetValueOrDefault(),
                        type);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error calculating revenue for PlanId {PlanId}",
                        result.PlId);
                }
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, ex.StackTrace);
        }
    }
    //public async Task<IActionResult> AddProjectPlan([FromBody] PlProjectPlan newPlan, string? type)
    //{
    //    try
    //    {
    //        var result = await _projPlanService.AddProjectPlanAsync(newPlan, type ?? string.Empty);

    //        PlForecastRepository plForecastRepository = new PlForecastRepository(_context, _config);
    //        try
    //        {
    //            //await _repository.GetByFilterAsync(result.ProjId, result.Version, newPlan.PlType, result.PlId);
    //            await plForecastRepository.CalculateRevenueCost(result.PlId.GetValueOrDefault(), result.TemplateId.GetValueOrDefault(), type);
    //        }
    //        catch (Exception ex)
    //        {
    //            //logger.LogError(ex, "Error Calculation Revenue for {PlanId}", result.PlId);
    //        }
    //        return Ok(result);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, ex.Message);
    //        return StatusCode(500, ex.StackTrace);
    //    }
    //}

    [HttpPost("AddPBudgetFromNewBussinessAsync")]
    public async Task<IActionResult> AddPBudgetFromNewBussinessAsync([FromBody] NewBusinessBudgetDTO newPlan, string SourceProject)
    {
        try
        {

            var result = await _projPlanService.AddPBudgetFromNewBussinessAsync(newPlan, SourceProject);

            _taskQueue.QueueBackgroundWorkItem(async token =>
            {
                using var scope = _serviceProvider.CreateScope();

                var db = scope.ServiceProvider.GetRequiredService<MydatabaseContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<ForecastController>>();
                PlForecastRepository plForecastRepository = new PlForecastRepository(db, _config);
                try
                {
                    await plForecastRepository.CalculateRevenueCost(result.PlId.GetValueOrDefault(), result.TemplateId.GetValueOrDefault(), SourceProject);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error Calculation Revenue for {PlanId}", result.PlId);
                }
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("BulkAddProjectPlan")]
    public async Task<IActionResult> BulkAddProjectPlan([FromBody] List<PlProjectPlan> newPlans, string? type)
    {
        List<PlProjectPlan> addedPlans = new List<PlProjectPlan>();
        foreach (var newPlan in newPlans)
        {
            try
            {
                _taskQueue.QueueBackgroundWorkItem(async token =>
                {
                    var result = await _projPlanService.AddProjectPlanAsync(newPlan, type ?? string.Empty);
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<MydatabaseContext>();
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<ForecastController>>();
                    PlForecastRepository plForecastRepository = new PlForecastRepository(db, _config);
                    try
                    {
                        await plForecastRepository.CalculateRevenueCost(result.PlId.GetValueOrDefault(), result.TemplateId.GetValueOrDefault(), type);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error Calculation Revenue for {PlanId}", result.PlId);
                    }
                });
                //addedPlans.Add(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding project plan.");
                return StatusCode(500, "Internal server error while adding project plan.");
            }
        }
        return Ok();

    }

    [HttpPut("UpdateProjectPlan")]
    public async Task<IActionResult> UpdateProjectPlan([FromBody] PlProjectPlan plan)
    {
        try
        {
            var success = await _projPlanService.UpdateProjectPlanAsync(plan);
            if (!success)
                return NotFound($"Project plan with ID {plan?.PlId} not found.");

            await _pl_ForecastService.CalculateRevenueCost(plan.PlId.GetValueOrDefault(), plan.TemplateId.GetValueOrDefault(), plan.Type);

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project plan with ID {PlId}", plan?.PlId);
            return StatusCode(500, ex.Message);
        }
    }
    [HttpPut("UpdateProjectPlanTemplateRateType")]
    public async Task<IActionResult> UpdateProjectPlanTemplateRateType([FromBody] PlProjectPlan updatedPlan)
    {
        try
        {

            var existing = await _context.PlProjectPlans.FindAsync(updatedPlan.PlId);
            if (existing == null)
                return NotFound();

            // Update only allowed fields
            existing.TemplateId = updatedPlan.TemplateId;
            existing.Type = updatedPlan.Type;
            existing.ModifiedBy = updatedPlan.ModifiedBy;
            existing.UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            await _context.SaveChangesAsync();
            await _pl_ForecastService.CalculateRevenueCost(updatedPlan.PlId.GetValueOrDefault(), updatedPlan.TemplateId.GetValueOrDefault(), updatedPlan.Type);
            return Ok();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    [HttpPut("BulkUpdateProjectPlan")]
    public async Task<IActionResult> BulkUpdateProjectPlan([FromBody] List<PlProjectPlan> plans)
    {
        try
        {
            var success = await _projPlanService.BulkUpdateProjectPlansAsync(plans);
            if (!success)
                return NotFound($"Project plan with ID's {string.Join(',', plans.Select(p => p.ProjId + p.PlType + p.Version).ToArray())} not found.");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project plan with ID's {PlId}", string.Join(',', plans.Select(p => p.ProjId + p.PlType + p.Version).ToArray()));
            return StatusCode(500, "Internal server error while updating project plan.");
        }
    }

    [HttpDelete("DeleteProjectPlan/{id}")]
    public async Task<IActionResult> DeleteProjectPlan(int id)
    {
        _logger.LogInformation("Request received to delete project plan with ID {ProjectPlanId}", id);

        try
        {
            var success = await _projPlanService.DeleteProjectPlanAsync(id);
            if (!success)
            {
                _logger.LogWarning("Project plan with ID {ProjectPlanId} not found.", id);
                return NotFound(new { message = $"Project plan with ID {id} not found." });
            }

            _logger.LogInformation("Project plan with ID {ProjectPlanId} deleted successfully.", id);
            return NoContent(); // 204 No Content on successful delete
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting project plan with ID {ProjectPlanId}", id);
            return StatusCode(500, "An internal server error occurred.");
        }
    }

    [HttpGet("GetAccountsByProjectId/{projId}")]
    public async Task<IActionResult> GetAccountsByProjectId(string projId)
    {
        try
        {
            var accountsData = await _projPlanService.GetAccountsByProjectId(projId);
            return Ok(accountsData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving forecast data for planId: {projId}", projId);
            return StatusCode(500, "Internal server error while retrieving forecast data.");
        }
    }

    [HttpGet("GetEmployeesByProjectId/{projId}")]
    public async Task<IActionResult> GetEmployeesByProjectId(string projId)
    {
        try
        {
            var accountsData = await _projPlanService.GetEmployeesByProjectId(projId);
            return Ok(accountsData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving employees data for planId: {projId}", projId);
            return StatusCode(500, "Internal server error while retrieving forecast data.");
        }
    }

    [HttpPost("CreateBurdenCeilingForProject")]
    public async Task<IActionResult> CreateBurdenCeilingForProject(PlCeilBurden plCeilingBurden, [FromQuery] string updatedBy)
    {
        try
        {
            CeilingHelper helper = new CeilingHelper(_context);
            var entry = await helper.CreateBurdenCeilingForProjectAsync(plCeilingBurden, updatedBy);
            return (Ok(entry));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Updating Burden Ceiling For Project: {projId}", plCeilingBurden.ProjectId);
            return StatusCode(500, "Internal server error while retrieving forecast data.");
        }
    }

    [HttpGet("GetAllBurdenCeilingForProject")]
    public async Task<IActionResult> GetAllBurdenCeilingForProject([FromQuery] string projId)
    {
        try
        {
            CeilingHelper helper = new CeilingHelper(_context);
            var entry = await helper.GetAllBurdenCeilingForProjectAsync(projId);
            return (Ok(entry));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Updating Burden Ceiling For Project: {projId}", projId);
            return StatusCode(500, "Internal server error while Get All Burden Ceiling For Project.");
        }
    }

    [HttpPut("UpdateBurdenCeilingForProject")]
    public async Task<IActionResult> UpdateBurdenCeilingForProject(PlCeilBurden plCeilingBurden, [FromQuery] string updatedBy)
    {
        try
        {
            CeilingHelper helper = new CeilingHelper(_context);
            var entry = await helper.UpdateBurdenCeilingForProjectAsync(plCeilingBurden, updatedBy);
            return (Ok(entry));

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Updating Burden Ceiling For Project: {projId}", plCeilingBurden.ProjectId);
            return StatusCode(500, "Internal server error while Updating Burden Ceiling For Project.");
        }
    }
    [HttpDelete("DeleteBurdenCeilingForProject/{projectId}/{fiscalYear}/{accountId}/{poolCode}")]
    public async Task<IActionResult> DeleteCeilingHrForPLC(string projectId, string fiscalYear, string accountId, string poolCode)
    {
        CeilingHelper helper = new CeilingHelper(_context);
        var result = await helper.DeleteBurdenCeilingForProjectAsync(projectId, fiscalYear, accountId, poolCode);

        if (!result.Success)
            return NotFound(result.Message);

        return Ok(result.Message);
    }

    [HttpPost("CreateCeilingHrForPLC")]
    public async Task<IActionResult> CreateCeilingHrForPLC(PlCeilHrCat plCeilingHrCat, [FromQuery] string updatedBy)
    {
        try
        {
            CeilingHelper helper = new CeilingHelper(_context);
            var entry = await helper.CreateCeilingHrForPLCAsync(plCeilingHrCat, updatedBy);
            return (Ok(entry));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Updating PLC hours Ceiling For Project: {projId}", plCeilingHrCat.ProjectId);
            return StatusCode(500, "Internal server error while retrieving PLC hours ceiling.");
        }
    }

    [HttpGet("GetAllCeilingHrForPLC")]
    public async Task<IActionResult> GetAllCeilingHrForPLC([FromQuery] string projId)
    {
        try
        {
            CeilingHelper helper = new CeilingHelper(_context);
            var entry = await helper.GetAllCeilingHrForPLCAsync(projId);
            return (Ok(entry));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Updating PLC hours Ceiling For Project: {projId}", projId);
            return StatusCode(500, "Internal server error while Get All PLC hours Ceiling For Project.");
        }
    }

    [HttpPut("UpdateCeilingHrForPLC")]
    public async Task<IActionResult> UpdateCeilingHrForPLC(PlCeilHrCat plCeilingHrCat, [FromQuery] string updatedBy)
    {
        try
        {
            CeilingHelper helper = new CeilingHelper(_context);
            var entry = await helper.UpdateCeilingHrForPLCAsync(plCeilingHrCat, updatedBy);
            return (Ok(entry));

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Creating Emp hours Ceiling For Project: {projId}", plCeilingHrCat.ProjectId);
            return StatusCode(500, "Internal server error while Creating Emp hours Ceiling For Project.");
        }
    }

    [HttpDelete("DeleteCeilingHrForPLC/{projectId}/{laborCategoryId}")]
    public async Task<IActionResult> DeleteCeilingHrForPLC(string projectId, string laborCategoryId)
    {
        CeilingHelper helper = new CeilingHelper(_context);
        var result = await helper.DeleteCeilingHrForPLCAsync(projectId, laborCategoryId);

        if (!result.Success)
            return NotFound(result.Message);

        return Ok(result.Message);
    }

    [HttpPost("CreateCeilingHrForEmp")]
    public async Task<IActionResult> CreateCeilingHrForEmp(PlCeilHrEmpl plCeilingHrCat, [FromQuery] string updatedBy)
    {
        try
        {
            CeilingHelper helper = new CeilingHelper(_context);
            var entry = await helper.CreateCeilingHrForEmpAsync(plCeilingHrCat, updatedBy);
            return (Ok(entry));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Updating Emp hours Ceiling For Project: {projId}", plCeilingHrCat.ProjectId);
            return StatusCode(500, "Internal server error while retrieving PLC hours ceiling.");
        }
    }

    [HttpGet("GetAllCeilingHrForEmp")]
    public async Task<IActionResult> GetAllCeilingHrForEmp([FromQuery] string projId)
    {
        try
        {
            CeilingHelper helper = new CeilingHelper(_context);
            var entry = await helper.GetAllCeilingHrForEmpAsync(projId);
            return (Ok(entry));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Updating Emp hours Ceiling For Project: {projId}", projId);
            return StatusCode(500, "Internal server error while Get All Emp hours Ceiling For Project.");
        }
    }

    [HttpPut("UpdateCeilingHrForEmp")]
    public async Task<IActionResult> UpdateCeilingHrForEmp(PlCeilHrEmpl plCeilingHrCat, [FromQuery] string updatedBy)
    {
        try
        {
            CeilingHelper helper = new CeilingHelper(_context);
            var entry = await helper.UpdateCeilingHrForEmpAsync(plCeilingHrCat, updatedBy);
            return (Ok(entry));

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Updating Emp hours Ceiling For Project: {projId}", plCeilingHrCat.ProjectId);
            return StatusCode(500, "Internal server error while Updating Emp hours Ceiling For Project.");
        }
    }

    [HttpDelete("DeleteCeilingHrForEmp/{projectId}/{employeeId}/{laborCategoryId}")]
    public async Task<IActionResult> DeleteCeilingHrForEmp(string projectId, string employeeId, string laborCategoryId)
    {
        CeilingHelper helper = new CeilingHelper(_context);
        var result = await helper.DeleteCeilingHrForEmpAsync(projectId, employeeId, laborCategoryId);

        if (!result.Success)
            return NotFound(result.Message);

        return Ok(result.Message);
    }

    [HttpPost("CreateCeilingAmtForDirectCost")]
    public async Task<IActionResult> CreateCeilingAmtForDirectCost(PlCeilDirCst plCeilDirCst, [FromQuery] string updatedBy)
    {
        try
        {
            CeilingHelper helper = new CeilingHelper(_context);
            var entry = await helper.CreateCeilingAmtForDirectCostAsync(plCeilDirCst, updatedBy);
            return (Ok(entry));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Ceiling Amt For DirectCost For Project: {projId}", plCeilDirCst.ProjectId);
            return StatusCode(500, "Internal server error while creating Ceiling Amt For DirectCost ceiling.");
        }
    }

    [HttpGet("GetAllCeilingAmtForDirectCost")]
    public async Task<IActionResult> GetAllCeilingAmtForDirectCost([FromQuery] string projId)
    {
        try
        {
            CeilingHelper helper = new CeilingHelper(_context);
            var entry = await helper.GetAllCeilingAmtForDirectCostAsync(projId);
            return (Ok(entry));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Ceiling Amt For DirectCost For Project: {projId}", projId);
            return StatusCode(500, "Internal server error while Get All Ceiling Amt For DirectCost For Project.");
        }
    }

    [HttpPut("UpdateCeilingAmtForDirectCost")]
    public async Task<IActionResult> UpdateCeilingAmtForDirectCost(PlCeilDirCst plCeilDirCst, [FromQuery] string updatedBy)
    {
        try
        {
            CeilingHelper helper = new CeilingHelper(_context);
            var entry = await helper.UpdateCeilingAmtForDirectCostAsync(plCeilDirCst, updatedBy);
            return (Ok(entry));

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Updating Ceiling Amt For DirectCost For Project: {projId}", plCeilDirCst.ProjectId);
            return StatusCode(500, "Internal server error while Updating Ceiling Amt For DirectCost For Project.");
        }
    }
    [HttpDelete("DeleteCeilingAmtForDirectCost/{projectId}/{accountId}")]
    public async Task<IActionResult> DeleteCeilingAmtForDirectCost(string projectId, string accountId)
    {
        CeilingHelper helper = new CeilingHelper(_context);
        var result = await helper.DeleteAllCeilingAmtForDirectCostAsync(projectId, accountId);

        if (!result.Success)
            return NotFound(result.Message);

        return Ok(result.Message);
    }

    [HttpPost("CreateCeilingAmtForTotalProjectCost")]
    public async Task<IActionResult> CreateCeilingAmtForTotalProjectCost(PlCeilProjTotal plCeilProjTotal, [FromQuery] string updatedBy)
    {
        try
        {
            CeilingHelper helper = new CeilingHelper(_context);
            var entry = await helper.CreateCeilingAmtForTotalProjectCostAsync(plCeilProjTotal, updatedBy);
            return (Ok(entry));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Ceiling Amt For For Total Project cost For Project: {projId}", plCeilProjTotal.ProjectId);
            return StatusCode(500, "Internal server error while creating Ceiling Amt For Total Project cost ceiling.");
        }
    }

    [HttpGet("GetAllCeilingAmtForTotalProjectCost")]
    public async Task<IActionResult> GetAllCeilingAmtForTotalProjectCost([FromQuery] string projId)
    {
        try
        {
            CeilingHelper helper = new CeilingHelper(_context);
            var entry = await helper.GetAllCeilingAmtForTotalProjectCostAsync(projId);
            return (Ok(entry));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Ceiling Amt For Total Project cost For Project: {projId}", projId);
            return StatusCode(500, "Internal server error while Get All Ceiling Amt For DirectCost For Project.");
        }
    }

    [HttpPut("UpdateCeilingAmtForTotalProjectCost")]
    public async Task<IActionResult> UpdateCeilingAmtForTotalProjectCost(PlCeilProjTotal plCeilProjTotal, [FromQuery] string updatedBy)
    {
        try
        {
            CeilingHelper helper = new CeilingHelper(_context);
            var entry = await helper.UpdateCeilingAmtForTotalProjectCostAsync(plCeilProjTotal, updatedBy);
            return (Ok(entry));

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Updating Ceiling Amt For Total Project cost For Project: {projId}", plCeilProjTotal.ProjectId);
            return StatusCode(500, "Internal server error while Updating Ceiling For Total Project cost For Project.");
        }
    }

    [HttpDelete("DeleteCeilingAmtForTotalProjectCostAsync/{proj_id}")]
    public async Task<IActionResult> DeleteCeilingAmtForTotalProjectCostAsync(string proj_id)
    {
        CeilingHelper helper = new CeilingHelper(_context);
        var result = await helper.DeleteCeilingAmtForTotalProjectCostAsync(proj_id);

        if (!result.Success)
            return NotFound(result.Message);

        return Ok(result.Message);
    }


    [HttpGet("GetFunding/{Proj_id}")]
    public async Task<IActionResult> GetFunding(string Proj_id)
    {
        try
        {
            List<FundingDetails> fundingDetails = new List<FundingDetails>();
            var funding = _context.PlProjects.FirstOrDefault(p => p.ProjId == Proj_id);
            var ProjModfunding = _context.ProjectModifications
                .Where(p => p.ProjId.StartsWith(Proj_id));
            FundingDetails fundingDetail = new FundingDetails();

            decimal costFunding = 0, feeFunding = 0;

            if (ProjModfunding != null && ProjModfunding.Count() > 0)
            {
                costFunding = ProjModfunding.ToList().Sum(p => p.ProjFCstAmt).GetValueOrDefault();
                feeFunding = ProjModfunding.ToList().Sum(p => p.ProjFFeeAmt).GetValueOrDefault();
            }
            fundingDetail.Funding = costFunding + feeFunding;

            var revenue = _context.PlForecasts.Where(p => p.PlId == 485).ToList().Sum(p => p.Revenue);
            var cost = _context.PlForecasts.Where(p => p.PlId == 485).ToList().Sum(p => p.ForecastedCost);
            var profit = revenue - cost;


            fundingDetail.Type = "Revenue";
            //fundingDetail.Funding = funding.proj_v_tot_amt.GetValueOrDefault();
            fundingDetail.Budget = revenue;
            fundingDetail.Balance = fundingDetail.Funding - fundingDetail.Budget;
            fundingDetail.Percent = (fundingDetail.Balance / fundingDetail.Funding) * 100;
            fundingDetails.Add(fundingDetail);

            fundingDetail = new FundingDetails();
            fundingDetail.Type = "Cost";
            fundingDetail.Funding = funding.proj_v_tot_amt.GetValueOrDefault();
            fundingDetail.Budget = cost.GetValueOrDefault();
            fundingDetail.Balance = fundingDetail.Funding - fundingDetail.Budget;
            fundingDetail.Percent = (fundingDetail.Balance / fundingDetail.Funding) * 100;
            fundingDetails.Add(fundingDetail);

            fundingDetail = new FundingDetails();
            fundingDetail.Type = "Profit";
            fundingDetail.Funding = fundingDetails[0].Funding - fundingDetails[1].Funding;
            fundingDetail.Budget = fundingDetails[0].Budget - fundingDetails[1].Budget;
            fundingDetail.Balance = 0;
            fundingDetail.Percent = 0;
            fundingDetails.Add(fundingDetail);

            return (Ok(fundingDetails));

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Updating Ceiling Amt For Total Project cost For Project: {projId}", Proj_id);
            return StatusCode(500, "Internal server error while Updating Ceiling For Total Project cost For Project.");
        }
    }


    [HttpGet("GetFundingV1/{Proj_id}")]
    public async Task<IActionResult> GetFunding(string Proj_id, int plid)
    {
        try
        {
            var projPlan = _context.PlProjectPlans.FirstOrDefault(p => p.PlId == plid);
            decimal runningTotal = 0;
            if (projPlan.PlType == "EAC")
            {
                var actualMonthlySummary = _context.PSRFinalData
                .Where(p => p.ProjId.StartsWith(projPlan.ProjId) && (p.RateType == projPlan.Type || p.RateType == "N") && Convert.ToInt16(p.FyCd) == projPlan.ClosedPeriod.Value.Year && p.PdNo <= projPlan.ClosedPeriod.Value.Month)
                .GroupBy(p => new { p.PdNo, p.FyCd, p.SubTotTypeNo })
                .Select(g => new MonthlySummary
                {
                    Month = g.Key.PdNo,
                    Year = Convert.ToInt16(g.Key.FyCd),
                    Cost = g.Sum(x => x.PtdIncurAmt),
                    subTotalType = g.Key.SubTotTypeNo
                    //Cost = (g.Key.SubTotTypeNo == 2 || g.Key.SubTotTypeNo == 3) ? g.Sum(x => x.PtdIncurAmt) : 0m

                })
                .ToList();

                runningTotal = actualMonthlySummary.Where(m => m.subTotalType == 1).Sum(m => m.Cost);
            }

            List<FundingDetails> fundingDetails = new List<FundingDetails>();
            //var funding = _context.PlProjects.FirstOrDefault(p => p.ProjId == Proj_id);
            var ProjModfunding = _context.ProjectModifications
                .Where(p => p.ProjId.StartsWith(Proj_id));
            FundingDetails fundingDetail = new FundingDetails();

            decimal costFunding = 0, feeFunding = 0;
            string system = _config["SystemSource"] ?? string.Empty;
            if (system.ToUpper() == "UNANET")
            {
                var project = _context.PlProjects.FirstOrDefault(p => p.ProjId == Proj_id);
                if (project != null)
                {
                    costFunding = project.proj_f_tot_amt.GetValueOrDefault();
                    feeFunding = project.proj_f_fee_amt.GetValueOrDefault();
                }
            }

            if (ProjModfunding != null && ProjModfunding.Count() > 0)
            {
                costFunding = ProjModfunding.ToList().Sum(p => p.ProjFCstAmt).GetValueOrDefault();
                feeFunding = ProjModfunding.ToList().Sum(p => p.ProjFFeeAmt).GetValueOrDefault();
            }

            var forecasts = _context.PlForecasts.Where(p => p.PlId == plid).ToList();

            var revenue = forecasts.Where(p => p.PlId == plid && p.Year == projPlan.ClosedPeriod.Value.Year && p.Month > projPlan.ClosedPeriod.Value.Month).ToList().Sum(p => p.Revenue) + runningTotal;
            var cost = forecasts.Where(p => p.PlId == plid).ToList().Sum(p => p.Cost);
            var profit = revenue - cost;
            var fee = forecasts.Where(p => p.PlId == plid).ToList().Sum(p => p.Fees);
            var AtRisk = _context.ProjBgtRevSetups.FirstOrDefault(p => p.PlId == plid)?.AtRiskAmt;
            fundingDetail.Type = "Revenue";
            //fundingDetail.Funding = funding.proj_v_tot_amt.GetValueOrDefault();
            fundingDetail.Funding = costFunding + feeFunding;
            fundingDetail.Budget = revenue;
            fundingDetail.Balance = AtRisk.GetValueOrDefault() + fundingDetail.Funding - fundingDetail.Budget;
            fundingDetail.AtRisk = AtRisk.GetValueOrDefault();
            //fundingDetail.Percent = (fundingDetail.Balance / fundingDetail.Funding) * 100;
            fundingDetail.Percent = fundingDetail.Funding != 0
                ? Math.Min(100, (fundingDetail.Balance / (fundingDetail.Funding + AtRisk.GetValueOrDefault())) * 100)
                : 0;

            fundingDetails.Add(fundingDetail);

            fundingDetail = new FundingDetails();
            fundingDetail.Type = "Cost";
            //fundingDetail.Funding = funding.proj_v_tot_amt.GetValueOrDefault();
            fundingDetail.Funding = costFunding;
            fundingDetail.Budget = cost;
            fundingDetail.Balance = AtRisk.GetValueOrDefault() + fundingDetail.Funding - fundingDetail.Budget;
            fundingDetail.AtRisk = AtRisk.GetValueOrDefault();

            //fundingDetail.Percent = (fundingDetail.Balance / fundingDetail.Funding) * 100;
            fundingDetail.Percent = fundingDetail.Funding != 0
                                    ? Math.Min(100, (fundingDetail.Balance / (fundingDetail.Funding + AtRisk.GetValueOrDefault())) * 100)
                                    : 0;

            fundingDetails.Add(fundingDetail);

            fundingDetail = new FundingDetails();
            fundingDetail.Type = "Fee";
            fundingDetail.Funding = feeFunding;
            fundingDetail.Budget = fee;
            fundingDetail.Balance = AtRisk.GetValueOrDefault() + fundingDetail.Funding - fundingDetail.Budget;
            fundingDetail.AtRisk = AtRisk.GetValueOrDefault();
            //fundingDetail.Percent = (fundingDetail.Balance / fundingDetail.Funding) * 100;
            fundingDetail.Percent = fundingDetail.Funding != 0
                                    ? Math.Min(100, (fundingDetail.Balance / (fundingDetail.Funding + AtRisk.GetValueOrDefault())) * 100)
                                    : 0;

            fundingDetails.Add(fundingDetail);

            return (Ok(fundingDetails));

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Updating Ceiling Amt For Total Project cost For Project: {projId}", Proj_id);
            return StatusCode(500, "Internal server error while Updating Ceiling For Total Project cost For Project.");
        }
    }

    [HttpGet("GetWarningsByPlId/{plId}")]
    public async Task<ActionResult<IEnumerable<PlWarning>>> GetWarningsByPlId(int plId)
    {
        CeilingHelper helper = new CeilingHelper(_context);
        var warnings = helper.GetWarningsByPlId(plId);

        if (warnings.Count() == 0)
        {
            return NotFound(new { Message = $"No warnings found for pl_id {plId}" });
        }

        return Ok(warnings);
    }
    [HttpGet("GetWarningsByEMployee/{plid}/{empl_id}")]
    public async Task<ActionResult<IEnumerable<PlWarning>>> GetWarningsByEMployee(int plid, string empl_id)
    {
        CeilingHelper helper = new CeilingHelper(_context);
        var warnings = helper.GetWarningsByEmployee(plid, empl_id);

        if (warnings.Count() == 0)
        {
            return NotFound(new { Message = $"No warnings found for empl_id {empl_id}" });
        }

        return Ok(warnings);
    }
    [HttpPut("UpdateDates")]
    public async Task<IActionResult> UpdateDates(ProjectUpdateDateDto project)
    {
        var existing = _context.PlProjects.FirstOrDefault(p => p.ProjId == project.ProjId);
        if (existing != null)
        {
            existing.ProjStartDt = project.ProjStartDt;
            existing.ProjEndDt = project.ProjEndDt;
            await _context.SaveChangesAsync();
        }
        var existingplan = _context.PlProjectPlans.FirstOrDefault(p => p.ProjId == project.ProjId);
        if (existingplan != null)
        {
            existingplan.ProjStartDt = project.ProjStartDt;
            existingplan.ProjEndDt = project.ProjEndDt;
            await _context.SaveChangesAsync();
        }
        return Ok();
    }

    [HttpPost("CreateProject")]
    public async Task<IActionResult> CreateProject([FromBody] PlProject project)
    {
        if (project == null)
            return BadRequest();

        await _context.PlProjects.AddAsync(project);
        await _context.SaveChangesAsync();

        return Ok(project);
    }

    [HttpPut("UpdateProject/{projId}")]
    public async Task<IActionResult> UpdateProject(string projId, [FromBody] PlProject updatedProject)
    {
        var project = await _context.PlProjects.FindAsync(projId);

        if (project == null)
            return NotFound();

        project.ProjName = updatedProject.ProjName;
        project.ProjLongName = updatedProject.ProjLongName;
        project.ProjMgrName = updatedProject.ProjMgrName;
        project.ProjStartDt = updatedProject.ProjStartDt;
        project.ProjEndDt = updatedProject.ProjEndDt;
        project.ActiveFl = updatedProject.ActiveFl;
        project.Notes = updatedProject.Notes;

        await _context.SaveChangesAsync();

        return Ok(project);
    }

    [HttpDelete("DeleteProject/{projId}")]
    public async Task<IActionResult> DeleteProject(string projId)
    {
        var project = await _context.PlProjects.FindAsync(projId);

        if (project == null)
            return NotFound();

        _context.PlProjects.Remove(project);
        await _context.SaveChangesAsync();

        return Ok("Project deleted successfully");
    }

    [HttpGet("GetProject/{projId}")]
    public async Task<IActionResult> GetProject(string projId)
    {
        var project = await _context.PlProjects
            .Include(p => p.Org)
            .FirstOrDefaultAsync(p => p.ProjId == projId);

        if (project == null)
            return NotFound();

        return Ok(project);
    }
}