namespace WebApi.Controllers;

using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using NPOI.SS.Formula.Functions;
using PlanningAPI.DTO;
using PlanningAPI.Helpers;
using PlanningAPI.Models;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Dynamic;
using System.Reflection;
using System.Text;
using System.Text.Json;
using WebApi.DTO;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models;
using WebApi.Models.Users;
using WebApi.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;

[ApiController]
[Route("[controller]")]
public class OrgnizationController : ControllerBase
{
    private IOrgService _orgService;
    private readonly MydatabaseContext _context;
    private readonly ILogger<OrgnizationController> _logger;
    Account_Org_Helpercs account_Org_Helpercs;
    public OrgnizationController(ILogger<OrgnizationController> logger, IOrgService orgService, MydatabaseContext context)
    {
        _logger = logger;
        _orgService = orgService;
        _context = context;
        account_Org_Helpercs = new Account_Org_Helpercs(context, logger);

    }


    [HttpGet("GetAllOrgs")]
    public async Task<IActionResult> GetAllOrgs()
    {
        try
        {
            _logger.LogInformation("GetAllOrgs called at {Time}", DateTime.UtcNow);
            var orgs = await _orgService.GetAllOrgs();

            return Ok(orgs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve organizations");

            return StatusCode(500, new
            {
                Message = "An error occurred while fetching organizations.",
                Error = ex.Message  // Remove in production if exposing internals is a concern
            });
        }
    }

    //[HttpGet("GetAllOrgAccounts/{orgID}")]
    //public Task<IActionResult> GetAllOrgAccounts(string orgID)
    //{
    //    var orgAccounts = _orgService.GetAllOrgAccounts(orgID);
    //    return Task.FromResult<IActionResult>(Ok(orgAccounts));
    //}

    [HttpGet("GetAllOrgAccounts/{orgID}")]
    public async Task<IActionResult> GetAllOrgAccounts(string orgID)
    {
        _logger.LogInformation("GetAllOrgAccounts called for OrgID: {OrgID} at {Time}", orgID, DateTime.UtcNow);

        try
        {
            var orgAccounts = await _orgService.GetAllOrgAccounts(orgID);

            _logger.LogInformation("Retrieved {Count} accounts for OrgID: {OrgID}", orgAccounts?.Count() ?? 0, orgID);
            return Ok(orgAccounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting OrgAccounts for OrgID: {OrgID} at {Time}", orgID, DateTime.UtcNow);

            return StatusCode(500, new
            {
                Message = "An error occurred while retrieving organization accounts.",
                Error = ex.Message // Consider hiding this in production
            });
        }
    }


    [HttpGet("GetAllOrgAccounts")]
    public async Task<IActionResult> GetAllOrgAccounts()
    {
        _logger.LogInformation("GetAllOrgAccounts called at {Time}", DateTime.UtcNow);

        try
        {
            var orgAccounts = await _orgService.GetAllOrgAccounts();

            _logger.LogInformation("Retrieved {Count} org accounts", orgAccounts?.Count() ?? 0);
            return Ok(orgAccounts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting OrgAccounts at {Time}", DateTime.UtcNow);

            return StatusCode(500, new
            {
                Message = "An error occurred while retrieving organization accounts.",
                Error = ex.Message // Consider hiding this in production
            });
        }
    }

    //[HttpGet("GetAllTemplates")]
    //public Task<IActionResult> GetAllTemplates()
    //{

    //    var templates = _context.BurdenTemplates.ToList();
    //    return Task.FromResult<IActionResult>(Ok(templates));
    //}

    [HttpGet("GetAllTemplates")]
    public async Task<IActionResult> GetAllTemplates()
    {
        _logger.LogInformation("GetAllTemplates called at {Time}", DateTime.UtcNow);

        try
        {
            var templates = await _context.BurdenTemplates.ToListAsync();

            _logger.LogInformation("Retrieved {Count} templates at {Time}", templates.Count, DateTime.UtcNow);
            return Ok(templates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving templates at {Time}", DateTime.UtcNow);

            return StatusCode(500, new
            {
                Message = "An error occurred while fetching burden templates.",
                Error = ex.Message  // Optional: hide in production
            });
        }
    }


    //[HttpGet("GetAllPools")]
    //public Task<IActionResult> GetAllActivePools()
    //{
    //    var templates = _context.AccountGroups.Where(p => p.IsActive).ToList();
    //    return Task.FromResult<IActionResult>(Ok(templates));
    //}

    [HttpGet("GetAllPools")]
    public async Task<IActionResult> GetAllActivePools()
    {
        _logger.LogInformation("GetAllActivePools called at {Time}", DateTime.UtcNow);

        try
        {
            var templates = await _context.AccountGroups
                .Where(p => p.IsActive)
                .OrderBy(p => p.Sequence)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} active pools at {Time}", templates.Count, DateTime.UtcNow);
            return Ok(templates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving active pools at {Time}", DateTime.UtcNow);

            return StatusCode(500, new
            {
                Message = "An error occurred while fetching active pools.",
                Error = ex.Message // Consider hiding in production
            });
        }
    }


    //[HttpGet("GetWorkingDays/{year}/{month}")]
    //public Task<IActionResult> GetWorkingDays(int year, int month)
    //{
    //    Helper helper = new Helper();


    //    var workingDays = helper.GetWorkingDaysInMonth(year, month, _orgService);

    //    return Task.FromResult<IActionResult>(Ok(workingDays));
    //}

    [HttpGet("GetWorkingDays/{year}/{month}")]
    public Task<IActionResult> GetWorkingDays(int year, int month)
    {
        _logger.LogInformation("GetWorkingDays called for Year: {Year}, Month: {Month} at {Time}", year, month, DateTime.UtcNow);

        try
        {
            // It's better to inject Helper via DI if it has dependencies
            ScheduleHelper scheduleHelper = new ScheduleHelper();

            var workingDays = scheduleHelper.GetWorkingDaysInMonth(year, month, _orgService);

            _logger.LogInformation("Calculated {Count} working days for {Month}/{Year}", workingDays, month, year);
            return Task.FromResult<IActionResult>(Ok(workingDays));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating working days for {Month}/{Year} at {Time}", month, year, DateTime.UtcNow);

            return Task.FromResult<IActionResult>(StatusCode(500, new
            {
                Message = "An error occurred while calculating working days.",
                Error = ex.Message // Consider hiding in production
            }));
        }
    }


    //[HttpGet("GetWorkingDaysForDuration/{startDate}/{endDate}")]
    //public Task<IActionResult> GetWorkingDaysForDuration(DateOnly startDate, DateOnly endDate)
    //{
    //    Helper helper = new Helper();


    //    var schedule = helper.GetWorkingDaysForDuration(startDate, endDate, _orgService);

    //    return Task.FromResult<IActionResult>(Ok(schedule));
    //}

    [HttpGet("GetWorkingDaysForDuration/{startDate}/{endDate}")]
    public Task<IActionResult> GetWorkingDaysForDuration(DateOnly startDate, DateOnly endDate)
    {
        _logger.LogInformation("GetWorkingDaysForDuration called from {StartDate} to {EndDate} at {Time}",
            startDate, endDate, DateTime.UtcNow);

        try
        {
            // Prefer DI for Helper if it's not purely static
            ScheduleHelper scheduleHelper = new ScheduleHelper();

            var schedule = scheduleHelper.GetWorkingDaysForDuration(startDate, endDate, _orgService);

            _logger.LogInformation("Retrieved {Count} working days between {StartDate} and {EndDate}",
                schedule.Count, startDate, endDate);

            return Task.FromResult<IActionResult>(Ok(schedule));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting working days from {StartDate} to {EndDate} at {Time}",
                startDate, endDate, DateTime.UtcNow);

            return Task.FromResult<IActionResult>(StatusCode(500, new
            {
                Message = "An error occurred while retrieving working days for the specified duration.",
                Error = ex.Message // Optional: remove in production
            }));
        }
    }

    [HttpGet("RefreshWorkingDaysForDuration/{startDate}/{endDate}")]
    public Task<IActionResult> RefreshWorkingDaysForDuration(DateOnly startDate, DateOnly endDate)
    {
        _logger.LogInformation("GetWorkingDaysForDuration called from {StartDate} to {EndDate} at {Time}",
            startDate, endDate, DateTime.UtcNow);

        try
        {
            // Prefer DI for Helper if it's not purely static
            ScheduleHelper scheduleHelper = new ScheduleHelper();

            var schedule = scheduleHelper.GetWorkingDaysForDuration(startDate, endDate, _orgService);

            _logger.LogInformation("Retrieved {Count} working days between {StartDate} and {EndDate}",
                schedule.Count, startDate, endDate);

            return Task.FromResult<IActionResult>(Ok(schedule));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while getting working days from {StartDate} to {EndDate} at {Time}",
                startDate, endDate, DateTime.UtcNow);

            return Task.FromResult<IActionResult>(StatusCode(500, new
            {
                Message = "An error occurred while retrieving working days for the specified duration.",
                Error = ex.Message // Optional: remove in production
            }));
        }
    }


    [HttpPost("AddTemplate")]
    public Task<IActionResult> AddTemplate(BurdenTemplate template, [FromQuery] string updatedBy)
    {
        PoolRateHelper helper = new PoolRateHelper(_context);


        var entry = helper.CreateTemplate(template, updatedBy);

        return Task.FromResult<IActionResult>(Ok(entry));
    }

    [HttpPost("DeleteTemplate")]
    public Task<IActionResult> DeleteTemplate(BurdenTemplate template, [FromQuery] string updatedBy)
    {
        PoolRateHelper helper = new PoolRateHelper(_context);

        var entry = helper.DeleteTemplate(template, updatedBy);

        return Task.FromResult<IActionResult>(Ok(entry));
    }


    [HttpPost("UpsertPoolRatesForTemplate")]
    public Task<IActionResult> UpsertPoolRatesForTemplate([FromBody] List<PlTemplatePoolRate> poolRates, [FromQuery] string updatedBy)
    {
        _logger.LogInformation("ConfigureRateForPool called by {UserStd} at {Time}", updatedBy, DateTime.UtcNow);

        try
        {
            PoolRateHelper helper = new PoolRateHelper(_context); // Recommend DI for better testability

            var schedule = helper.UpsertPoolRatesForTemplate(poolRates, updatedBy);

            _logger.LogInformation("Pool rate configuration successful for Template Id: {TemplateId}", poolRates.FirstOrDefault()?.TemplateId);

            return Task.FromResult<IActionResult>(Ok(schedule));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring pool rate for Template Id: {TemplateId} at {Time}",
                poolRates.FirstOrDefault()?.TemplateId, DateTime.UtcNow);

            return Task.FromResult<IActionResult>(StatusCode(500, new
            {
                Message = "An error occurred while configuring the pool rate.",
                Error = ex.Message // Hide in production if sensitive
            }));
        }
    }

    [HttpGet("GetAccountPools")]
    public async Task<IActionResult> GetAccountPools([FromQuery] int? Year = null)
    {

        try
        {
            _logger.LogInformation("GetAccountPools called at {Time}", DateTime.UtcNow);

            return Ok(account_Org_Helpercs.GetAccountPools(Year.GetValueOrDefault()));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving active pools at {Time}", DateTime.UtcNow);

            return StatusCode(500, new
            {
                Message = "An error occurred while fetching active pools.",
                Error = ex.Message // Consider hiding in production
            });
        }

    }

    [HttpGet("GetAccountPoolsV1")]
    public async Task<IActionResult> GetAccountPoolsV1(string? acctId, string? orgId, [FromQuery] int? Year = null)
    {

        try
        {
            _logger.LogInformation("GetAccountPools called at {Time}", DateTime.UtcNow);

            return Ok(account_Org_Helpercs.GetAccountPoolsV1(Year, acctId, orgId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving active pools at {Time}", DateTime.UtcNow);

            return StatusCode(500, new
            {
                Message = "An error occurred while fetching active pools.",
                Error = ex.Message // Consider hiding in production
            });
        }

    }
    [HttpGet("GetPoolsByTemplateId")]
    public async Task<IActionResult> GetPoolsByTemplateId(int templateId)
    {

        try
        {
            _logger.LogInformation("GetPoolsByTemplateId called at {Time}", DateTime.UtcNow);

            return Ok(account_Org_Helpercs.GetPoolsByTemplateId(templateId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving active pools at {Time}", DateTime.UtcNow);

            return StatusCode(500, new
            {
                Message = "An error occurred while fetching active pools.",
                Error = ex.Message // Consider hiding in production
            });
        }

    }

    [HttpGet("GetPoolsByOrgAccount")]
    public async Task<IActionResult> GetPoolsByOrgAccount(string accountId, string orgId)
    {

        try
        {
            _logger.LogInformation("GetPoolsByAccountId called at {Time}", DateTime.UtcNow);

            return Ok(account_Org_Helpercs.GetPoolsByOrgAccount(accountId, orgId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving active pools at {Time}", DateTime.UtcNow);

            return StatusCode(500, new
            {
                Message = "An error occurred while fetching active pools.",
                Error = ex.Message // Consider hiding in production
            });
        }

    }

    [HttpGet("GetRatesByPoolsTemplateId")]
    public async Task<IActionResult> GetRatesByPoolsTemplateId(int templateId, string poolId, int year)
    {

        try
        {
            _logger.LogInformation("GetRatesByPoolsTemplateId called at {Time}", DateTime.UtcNow);

            return Ok(account_Org_Helpercs.GetRatesByPoolsTemplateId(templateId, poolId, year));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving active pools at {Time}", DateTime.UtcNow);

            return StatusCode(500, new
            {
                Message = "An error occurred while fetching active pools.",
                Error = ex.Message // Consider hiding in production
            });
        }

    }

    [HttpPost("BulkUpSertOrgAccountPoolMapping")]
    public IActionResult BulkUpSertOrgAccountPoolMapping([FromBody] JsonElement recordss)
    {
        try
        {
            account_Org_Helpercs.BulkUpsertEfLoopAsync(recordss).GetAwaiter().GetResult();
            return StatusCode(200, "Success : Bulk UpSert Org Account Pool Mapping.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in AddOrgAccountPoolMapping at {Time}", DateTime.UtcNow);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpPost("BulkUpSertTemplatePoolMapping")]
    public IActionResult BulkUpSertTemplatePoolMapping([FromBody] JsonElement recordss)
    {
        try
        {
            account_Org_Helpercs.BulkUpSertTemplatePoolMapping(recordss);
            return StatusCode(200, "Success : Bulk UpSert Template Pool Mapping.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred in Template Pool Mapping at {Time}", DateTime.UtcNow);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    // ✅ READ all schedules
    [HttpGet("GetAll")]
    public async Task<IActionResult> GetAll()
    {
        var schedules = await _context.Schedules.ToListAsync();
        return Ok(schedules);
    }

    // ✅ READ single schedule
    [HttpGet("GetStandardHours/{year}/{monthNo}")]
    public async Task<IActionResult> GetStandardHours(int year, int monthNo)
    {
        var schedule = await _context.Schedules.FindAsync(year, monthNo);
        return schedule == null ? NotFound() : Ok(schedule);
    }


    // ✅ UPSERT (Insert or Update)
    [HttpPost("UpsertStandardHours")]
    public async Task<IActionResult> UpsertStandardHours([FromBody] DateOnly startDate, DateOnly endDate)
    {
        ScheduleHelper scheduleHelper = new ScheduleHelper();

        var schedules = scheduleHelper.GetWorkingDaysForDuration(startDate, endDate, _orgService);

        foreach (var sch in schedules)
        {
            var existing = await _context.Schedules.FindAsync(sch.Year, sch.MonthNo);

            if (existing == null)
            {
                _context.Schedules.Add(sch);
            }
            else
            {
                existing.WorkingHours = sch.WorkingHours;
                existing.WorkingDays = sch.WorkingDays;
                _context.Schedules.Update(existing);
            }
        }
        await _context.SaveChangesAsync();
        return Ok("Upsert successful");
    }

    // ✅ DELETE
    [HttpDelete("DeleteStandardHours/{year}/{monthNo}")]
    public async Task<IActionResult> DeleteStandardHours(int year, int monthNo)
    {
        var schedule = await _context.Schedules.FindAsync(year, monthNo);
        if (schedule == null) return NotFound();

        _context.Schedules.Remove(schedule);
        await _context.SaveChangesAsync();
        return Ok("Deleted successfully");
    }

    //[HttpPost("AddOrgAccountPoolMapping")]
    //public IActionResult AddOrgAccountPoolMapping([FromBody] List<AccountGroupPivot> input)
    //{
    //    try
    //    {
    //        _logger.LogInformation("AddOrgAccountPoolMapping endpoint hit at {Time}", DateTime.UtcNow);

    //        if (input == null || !input.Any())
    //        {
    //            _logger.LogWarning("Empty or null input received.");
    //            return BadRequest("Input is empty or null.");
    //        }

    //        var flatList = input.SelectMany(item =>
    //        {
    //            var entries = new List<PlOrgAcctPoolMapping>();

    //            if (item.FRINGE)
    //                entries.Add(new PlOrgAcctPoolMapping { PoolId = "FRINGE", AccountId = item.AcctId, OrgId = item.OrgId });

    //            if (item.OVERHEAD)
    //                entries.Add(new PlOrgAcctPoolMapping { PoolId = "OVERHEAD", AccountId = item.AcctId, OrgId = item.OrgId });

    //            if (item.GNA)
    //                entries.Add(new PlOrgAcctPoolMapping { PoolId = "GNA", AccountId = item.AcctId, OrgId = item.OrgId });

    //            return entries;
    //        }).ToList();

    //        _context.PlOrgAcctPoolMappings.AddRange(flatList);
    //        _context.SaveChanges();

    //        _logger.LogInformation("Successfully converted {Count} account group entries.", flatList.Count);

    //        return Ok(flatList);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error occurred in AddOrgAccountPoolMapping at {Time}", DateTime.UtcNow);
    //        return StatusCode(500, "An error occurred while processing your request.");
    //    }
    //}

    [HttpGet("GetFringe")]
    public async Task<IActionResult> GetFringe(string fycd)
    {
        RateCalculator rateCalculator = new RateCalculator(_context);
        PoolOrgFinancialDetail poolOrgFinancialDetail = new PoolOrgFinancialDetail();
        poolOrgFinancialDetail.poolOrgCostFinancialDetail = rateCalculator.GetCostFringeDetails(fycd);

        poolOrgFinancialDetail.poolOrgBaseFinancialDetail = rateCalculator.GetBaseLaborAccountDetails(fycd);

        poolOrgFinancialDetail.TotalYTDBaseActualAmt = poolOrgFinancialDetail.poolOrgBaseFinancialDetail.Sum(x => x.YTDActualAmt);
        poolOrgFinancialDetail.TotalYTDCostActualAmt = poolOrgFinancialDetail.poolOrgCostFinancialDetail.Sum(x => x.YTDActualAmt);

        poolOrgFinancialDetail.Rate =
                poolOrgFinancialDetail.TotalYTDBaseActualAmt != 0
                    ? Math.Round(
                        (poolOrgFinancialDetail.TotalYTDCostActualAmt.GetValueOrDefault() / poolOrgFinancialDetail.TotalYTDBaseActualAmt.GetValueOrDefault()) * 100,
                        4
                      )
                    : 0;

        foreach (var item in poolOrgFinancialDetail.poolOrgBaseFinancialDetail)
        {
            foreach (var itemDetails in item.PeriodDetails)
            {
                //itemDetails.AllocationAmt = Math.Round((itemDetails.baseAmt * poolOrgFinancialDetail.Rate), 2);
                itemDetails.AllocationAmt = Math.Round(
                        ((itemDetails.baseAmt ?? 0) * (poolOrgFinancialDetail.Rate ?? 0) / 100),
                        2
                    );
            }
            item.YTDAllocationAmt = item.PeriodDetails.Sum(x => x.AllocationAmt);
        }



        poolOrgFinancialDetail.TotalYTDBaseAllocationActualAmt = poolOrgFinancialDetail.poolOrgBaseFinancialDetail
            .SelectMany(x => x.PeriodDetails)
            .Sum(x => x.AllocationAmt);

        return Ok(poolOrgFinancialDetail);


    }

    [HttpGet("GetHRV1")]
    public async Task<IActionResult> GetPoolValues(string fycd, string type)
    {
        RateCalculator rateCalculator = new RateCalculator(_context);
        PoolOrgFinancialDetail poolOrgFinancialDetail = new PoolOrgFinancialDetail();
        PoolOrgFinancialDetail poolOrgFringeDetail = new PoolOrgFinancialDetail();

        int poolno = 0;
        switch (type.ToUpper())
        {
            case "OVERHEAD":
                poolno = 13;
                break;
            case "GNA":
                poolno = 17;
                break;
            case "FRINGE":
                poolno = 11;
                break;
            case "HR":
                poolno = 12;
                break;
            case "MNH":
                poolno = 16;
                break;
            default:
                poolno = 0;
                break;
        }
        var chartOfAccounts = _context.Charts_Of_Accounts.ToList();

        if (poolno != 11)
        {
            poolOrgFringeDetail.poolOrgCostFinancialDetail = rateCalculator.GetCostFringeDetails(fycd, 11);

            poolOrgFringeDetail.poolOrgBaseFinancialDetail = rateCalculator.GetBaseLaborAccountDetails(fycd, 11);

            poolOrgFringeDetail.TotalYTDBaseActualAmt = poolOrgFringeDetail.poolOrgBaseFinancialDetail.Sum(x => x.YTDActualAmt);
            poolOrgFringeDetail.TotalYTDCostActualAmt = poolOrgFringeDetail.poolOrgCostFinancialDetail.Sum(x => x.YTDActualAmt);

            poolOrgFringeDetail.Rate =
                    poolOrgFringeDetail.TotalYTDBaseActualAmt != 0
                        ? Math.Round(
                            (poolOrgFringeDetail.TotalYTDCostActualAmt.GetValueOrDefault() / poolOrgFringeDetail.TotalYTDBaseActualAmt.GetValueOrDefault()) * 100,
                            4
                          )
                        : 0;


            poolOrgFringeDetail.Rate =
                    poolOrgFringeDetail.TotalYTDBaseActualAmt != 0
                        ?
                            (poolOrgFringeDetail.TotalYTDCostActualAmt.GetValueOrDefault() / poolOrgFringeDetail.TotalYTDBaseActualAmt.GetValueOrDefault()) * 100
                        : 0;

            foreach (var item in poolOrgFringeDetail.poolOrgBaseFinancialDetail)
            {
                foreach (var itemDetails in item.PeriodDetails)
                {
                    //itemDetails.AllocationAmt = Math.Round((itemDetails.baseAmt * poolOrgFinancialDetail.Rate), 2);
                    itemDetails.AllocationAmt = Math.Round(
                            ((itemDetails.baseAmt ?? 0) * (poolOrgFringeDetail.Rate ?? 0) / 100),
                            2
                        );
                }
                item.YTDAllocationAmt = item.PeriodDetails.Sum(x => x.AllocationAmt);
            }



            poolOrgFringeDetail.TotalYTDBaseAllocationActualAmt = poolOrgFringeDetail.poolOrgBaseFinancialDetail
                .SelectMany(x => x.PeriodDetails)
                .Sum(x => x.AllocationAmt);


            foreach (var costDetail in poolOrgFringeDetail.poolOrgCostFinancialDetail)
            {
                var account = chartOfAccounts.FirstOrDefault(c => c.AccountId == costDetail.AcctId);
                if (account != null)
                {
                    costDetail.AccountName = account.AccountName;
                }
            }
            foreach (var costDetail in poolOrgFringeDetail.poolOrgBaseFinancialDetail)
            {
                var account = chartOfAccounts.FirstOrDefault(c => c.AccountId == costDetail.AcctId);
                if (account != null)
                {
                    costDetail.AccountName = account.AccountName;
                }
            }
        }


        poolOrgFinancialDetail.poolOrgCostFinancialDetail = rateCalculator.GetCostFringeDetails(fycd, poolno);

        //var temp = poolOrgFringeDetail.poolOrgBaseFinancialDetail.Where(p => poolOrgFinancialDetail.poolOrgCostFinancialDetail.Select(q => q.AcctId).Contains(p.AcctId)).ToList();

        var costAcctIds = poolOrgFinancialDetail.poolOrgCostFinancialDetail
            .Select(q => q.AcctId)
            .ToHashSet();

        if (poolno != 11)
        {
            var temp = poolOrgFringeDetail.poolOrgBaseFinancialDetail
            .Where(p => costAcctIds.Contains(p.AcctId))
            .ToList();

            var convertedList = temp.Select(x => new PoolOrgCostFinancialDetail
            {
                AcctId = x.AllocationAcctId,
                OrgId = x.OrgId,
                YTDActualAmt = x.YTDAllocationAmt,
                PeriodDetails = x.PeriodDetails.Select(pd => new PeriodCostFinancialDetail
                {
                    Period = pd.Period,
                    Actualamt = pd.AllocationAmt,
                    BudgetedAmt = pd.BudgetAmt,
                    Rate = pd.Rate
                }).ToList()

                // Add more property mappings here...
            }).ToList();

            poolOrgFinancialDetail.poolOrgCostFinancialDetail.AddRange(convertedList);
        }

        poolOrgFinancialDetail.poolOrgBaseFinancialDetail = rateCalculator.GetBaseLaborAccountDetails(fycd, poolno);



        poolOrgFinancialDetail.TotalYTDBaseActualAmt = poolOrgFinancialDetail.poolOrgBaseFinancialDetail.Sum(x => x.YTDActualAmt);
        poolOrgFinancialDetail.TotalYTDCostActualAmt = poolOrgFinancialDetail.poolOrgCostFinancialDetail.Sum(x => x.YTDActualAmt);

        poolOrgFinancialDetail.Rate =
                poolOrgFinancialDetail.TotalYTDBaseActualAmt != 0
                    ? Math.Round(
                        (poolOrgFinancialDetail.TotalYTDCostActualAmt.GetValueOrDefault() / poolOrgFinancialDetail.TotalYTDBaseActualAmt.GetValueOrDefault()) * 100,
                        4
                      )
                    : 0;


        poolOrgFinancialDetail.Rate =
                poolOrgFinancialDetail.TotalYTDBaseActualAmt != 0
                    ?
                        (poolOrgFinancialDetail.TotalYTDCostActualAmt.GetValueOrDefault() / poolOrgFinancialDetail.TotalYTDBaseActualAmt.GetValueOrDefault()) * 100
                    : 0;

        foreach (var item in poolOrgFinancialDetail.poolOrgBaseFinancialDetail)
        {
            foreach (var itemDetails in item.PeriodDetails)
            {
                //itemDetails.AllocationAmt = Math.Round((itemDetails.baseAmt * poolOrgFinancialDetail.Rate), 2);
                itemDetails.AllocationAmt = Math.Round(
                        ((itemDetails.baseAmt ?? 0) * (poolOrgFinancialDetail.Rate ?? 0) / 100),
                        2
                    );
            }
            item.YTDAllocationAmt = item.PeriodDetails.Sum(x => x.AllocationAmt);
        }



        poolOrgFinancialDetail.TotalYTDBaseAllocationActualAmt = poolOrgFinancialDetail.poolOrgBaseFinancialDetail
            .SelectMany(x => x.PeriodDetails)
            .Sum(x => x.AllocationAmt);


        foreach (var costDetail in poolOrgFinancialDetail.poolOrgCostFinancialDetail)
        {
            var account = chartOfAccounts.FirstOrDefault(c => c.AccountId == costDetail.AcctId);
            if (account != null)
            {
                costDetail.AccountName = account.AccountName;
            }
        }
        foreach (var costDetail in poolOrgFinancialDetail.poolOrgBaseFinancialDetail)
        {
            var account = chartOfAccounts.FirstOrDefault(c => c.AccountId == costDetail.AcctId);
            if (account != null)
            {
                costDetail.AccountName = account.AccountName;
            }
        }
        return Ok(poolOrgFinancialDetail);


    }


    [HttpGet("GetRates")]
    public async Task<IActionResult> GetRates(string fycd)
    {
        RateCalculator rateCalculator = new RateCalculator(_context);
        PoolOrgFinancialDetail poolOrgFinancialDetail = new PoolOrgFinancialDetail();

        List<string> poolnos = new List<string>() { "FRINGE", "HR", "OVERHEAD", "MNH", "GNA" };
        List<PoolRateDetailsByPeriod> poolRateDetailsByPeriods = new List<PoolRateDetailsByPeriod>();

        foreach (var poolno in poolnos)
        {
            PoolRateDetailsByPeriod poolRateDetailsByPeriod = new PoolRateDetailsByPeriod();
            poolRateDetailsByPeriod.PoolName = char.ToUpper(poolno[0]) + poolno.Substring(1).ToLower();

            switch (poolno.ToUpper())
            {
                case "OVERHEAD":
                    poolRateDetailsByPeriod.PoolNo = 13;
                    break;
                case "GNA":
                    poolRateDetailsByPeriod.PoolNo = 17;
                    break;
                case "FRINGE":
                    poolRateDetailsByPeriod.PoolNo = 11;
                    break;
                case "HR":
                    poolRateDetailsByPeriod.PoolNo = 12;
                    break;
                case "MNH":
                    poolRateDetailsByPeriod.PoolNo = 16;
                    break;
                default:
                    poolRateDetailsByPeriod.PoolNo = 0;
                    break;
            }

            poolRateDetailsByPeriod.PeriodDetails = new List<PeriodbaseFinancialDetail>();
            poolOrgFinancialDetail.poolOrgCostFinancialDetail = rateCalculator.GetCostFringeDetails(fycd, poolRateDetailsByPeriod.PoolNo.GetValueOrDefault());
            poolOrgFinancialDetail.poolOrgBaseFinancialDetail = rateCalculator.GetBaseLaborAccountDetails(fycd, poolRateDetailsByPeriod.PoolNo.GetValueOrDefault());
            var ClosedPeriod = DateOnly.Parse(_context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "closing_period").Value);

            //foreach (var poolOrgCostFinancialDetail in poolOrgFinancialDetail.poolOrgCostFinancialDetail)
            for (int i = 1; i <= 12; i++)
            {
                decimal? baseAmt = 0;
                decimal? costAmt = 0;

                int lastDay = DateTime.DaysInMonth(Convert.ToInt16(fycd), i);
                DateOnly date = new DateOnly(Convert.ToInt16(fycd), i, lastDay);
                //foreach (var periodDetail in poolOrgCostFinancialDetail.PeriodDetails)
                if (ClosedPeriod >= date)
                {
                    baseAmt = poolOrgFinancialDetail.poolOrgBaseFinancialDetail
                        .SelectMany(x => x.PeriodDetails)
                        .Where(x => x.Period <= i)
                        .Sum(x => x.baseAmt);

                    costAmt = poolOrgFinancialDetail.poolOrgCostFinancialDetail
                        .SelectMany(x => x.PeriodDetails)
                        .Where(x => x.Period <= i)
                        .Sum(x => x.Actualamt);

                    var Rate =
                        baseAmt != 0
                            ? Math.Round((costAmt.GetValueOrDefault() / baseAmt.GetValueOrDefault()) * 100, 4, MidpointRounding.AwayFromZero)
                            : 0;
                    poolRateDetailsByPeriod.PeriodDetails.Add(new PeriodbaseFinancialDetail
                    {
                        Period = i,
                        Rate = Rate
                    });
                }
                else
                {
                    baseAmt = poolOrgFinancialDetail.poolOrgBaseFinancialDetail
                        .SelectMany(x => x.PeriodDetails)
                        .Where(x => x.Period <= i)
                        .Sum(x => x.BudgetAmt);

                    costAmt = poolOrgFinancialDetail.poolOrgCostFinancialDetail
                        .SelectMany(x => x.PeriodDetails)
                        .Where(x => x.Period <= i)
                        .Sum(x => x.BudgetedAmt);

                    var Rate =
                        baseAmt != 0
                            ? Math.Round((costAmt.GetValueOrDefault() / baseAmt.GetValueOrDefault()) * 100, 4, MidpointRounding.AwayFromZero)
                            : 0;
                    poolRateDetailsByPeriod.PeriodDetails.Add(new PeriodbaseFinancialDetail
                    {
                        Period = i,
                        Rate = Rate
                    });
                }

            }
            //        poolOrgFinancialDetail.Rate =
            //poolOrgFinancialDetail.TotalYTDBaseActualAmt != 0
            //    ? Math.Round(
            //        (poolOrgFinancialDetail.TotalYTDCostActualAmt.GetValueOrDefault() / poolOrgFinancialDetail.TotalYTDBaseActualAmt.GetValueOrDefault()) * 100,
            //        4
            //      )
            //    : 0;
            poolRateDetailsByPeriods.Add(poolRateDetailsByPeriod);
        }
        List<PlTemplatePoolRate> plTemplatePoolRates = new List<PlTemplatePoolRate>();
        foreach (var poolRateDetailsByPeriod in poolRateDetailsByPeriods)
        {

            foreach (var periodDetail in poolRateDetailsByPeriod.PeriodDetails)
            {
                PlTemplatePoolRate plTemplatePoolRate = new PlTemplatePoolRate();
                plTemplatePoolRate.PoolId = poolRateDetailsByPeriod.PoolName.ToUpper();
                plTemplatePoolRate.TemplateId = 1;
                plTemplatePoolRate.ActualRate = periodDetail.Rate;
                plTemplatePoolRate.TargetRate = periodDetail.Rate;
                plTemplatePoolRate.Month = Convert.ToInt16(periodDetail.Period);
                plTemplatePoolRate.Year = Convert.ToInt16(fycd);
                plTemplatePoolRates.Add(plTemplatePoolRate);
            }
        }
        //account_Org_Helpercs.BulkUpsertTemplatePoolRatesAsync(plTemplatePoolRates).GetAwaiter().GetResult();
        return Ok(poolRateDetailsByPeriods);


    }


    [HttpGet("GetRatesV1")]
    public async Task<IActionResult> GetRatesV1(string fycd)
    {
        RateCalculator rateCalculator = new RateCalculator(_context);
        PoolOrgFinancialDetail poolOrgFinancialDetail = new PoolOrgFinancialDetail();
        List<PoolOrgFinancialDetail> poolOrgFinancialDetails = new List<PoolOrgFinancialDetail>();
        List<string> poolnos = new List<string>() { "FRINGE", "HR", "OVERHEAD", "MNH", "GNA" };
        List<PoolRateDetailsByPeriod> poolRateDetailsByPeriods = new List<PoolRateDetailsByPeriod>();
        var ClosedPeriod = DateOnly.Parse(_context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "closing_period").Value);

        foreach (var poolno in poolnos)
        {
            poolOrgFinancialDetail = new PoolOrgFinancialDetail();
            PoolRateDetailsByPeriod poolRateDetailsByPeriod = new PoolRateDetailsByPeriod();
            poolRateDetailsByPeriod.PoolName = char.ToUpper(poolno[0]) + poolno.Substring(1).ToLower();
            poolOrgFinancialDetail.PoolName = poolno;
            switch (poolno.ToUpper())
            {
                case "OVERHEAD":
                    poolRateDetailsByPeriod.PoolNo = 13;
                    poolOrgFinancialDetail.PoolNo = 13;
                    break;
                case "GNA":
                    poolRateDetailsByPeriod.PoolNo = 17;
                    poolOrgFinancialDetail.PoolNo = 17;
                    break;
                case "FRINGE":
                    poolRateDetailsByPeriod.PoolNo = 11;
                    poolOrgFinancialDetail.PoolNo = 11;
                    break;
                case "HR":
                    poolRateDetailsByPeriod.PoolNo = 12;
                    poolOrgFinancialDetail.PoolNo = 12;
                    break;
                case "MNH":
                    poolRateDetailsByPeriod.PoolNo = 16;
                    poolOrgFinancialDetail.PoolNo = 16;
                    break;
                default:
                    poolRateDetailsByPeriod.PoolNo = 0;
                    poolOrgFinancialDetail.PoolNo = 0;
                    break;
            }

            poolRateDetailsByPeriod.PeriodDetails = new List<PeriodbaseFinancialDetail>();
            poolOrgFinancialDetail.poolOrgCostFinancialDetail = rateCalculator.GetCostFringeDetails(fycd, poolRateDetailsByPeriod.PoolNo.GetValueOrDefault());
            poolOrgFinancialDetail.poolOrgBaseFinancialDetail = rateCalculator.GetBaseLaborAccountDetails(fycd, poolRateDetailsByPeriod.PoolNo.GetValueOrDefault());

            poolOrgFinancialDetail.TotalYTDBaseActualAmt = poolOrgFinancialDetail.poolOrgBaseFinancialDetail.Sum(x => x.YTDActualAmt);
            poolOrgFinancialDetail.TotalYTDCostActualAmt = poolOrgFinancialDetail.poolOrgCostFinancialDetail.Sum(x => x.YTDActualAmt);


            poolOrgFinancialDetail.Rate =
            poolOrgFinancialDetail.TotalYTDBaseActualAmt != 0
            ?
                (poolOrgFinancialDetail.TotalYTDCostActualAmt.GetValueOrDefault() / poolOrgFinancialDetail.TotalYTDBaseActualAmt.GetValueOrDefault()) * 100
            : 0;

            foreach (var item in poolOrgFinancialDetail.poolOrgBaseFinancialDetail)
            {
                foreach (var itemDetails in item.PeriodDetails)
                {
                    var Rate = (itemDetails.costAmt / itemDetails.baseAmt) * 100;
                    //itemDetails.AllocationAmt = Math.Round((itemDetails.baseAmt * poolOrgFinancialDetail.Rate), 2);
                    itemDetails.AllocationAmt = Math.Round(
                            ((itemDetails.baseAmt ?? 0) * (Rate ?? 0) / 100),
                            2
                        );
                }
                item.YTDAllocationAmt = item.PeriodDetails.Sum(x => x.AllocationAmt);
            }



            poolOrgFinancialDetail.TotalYTDBaseAllocationActualAmt = poolOrgFinancialDetail.poolOrgBaseFinancialDetail
                .SelectMany(x => x.PeriodDetails)
                .Sum(x => x.AllocationAmt);


            for (int i = 1; i <= 12; i++)
            {
                decimal? baseAmt = 0;
                decimal? costAmt = 0;

                int lastDay = DateTime.DaysInMonth(Convert.ToInt16(fycd), i);
                DateOnly date = new DateOnly(Convert.ToInt16(fycd), i, lastDay);
                if (ClosedPeriod >= date)
                {
                    baseAmt = poolOrgFinancialDetail.poolOrgBaseFinancialDetail
                        .SelectMany(x => x.PeriodDetails)
                        .Where(x => x.Period <= i)
                        .Sum(x => x.baseAmt);

                    costAmt = poolOrgFinancialDetail.poolOrgCostFinancialDetail
                        .SelectMany(x => x.PeriodDetails)
                        .Where(x => x.Period <= i)
                        .Sum(x => x.Actualamt);

                    //var Rate =
                    //    baseAmt != 0
                    //        ? Math.Round((costAmt.GetValueOrDefault() / baseAmt.GetValueOrDefault()) * 100, 4, MidpointRounding.AwayFromZero)
                    //        : 0;
                    decimal baseValue = baseAmt.GetValueOrDefault();
                    decimal costValue = costAmt.GetValueOrDefault();

                    decimal Rate = baseValue == 0
                        ? 0
                        : (costValue / baseValue) * 100;

                    //var Rate = (costAmt.GetValueOrDefault() / baseAmt.GetValueOrDefault()) * 100;
                    poolRateDetailsByPeriod.PeriodDetails.Add(new PeriodbaseFinancialDetail
                    {
                        baseAmt = baseAmt,
                        costAmt = costAmt,
                        Period = i,
                        Rate = Rate,
                        AllocationAmt = ((baseAmt ?? 0) * (Rate) / 100)
                    });
                }
                else
                {
                    baseAmt = poolOrgFinancialDetail.poolOrgBaseFinancialDetail
                        .SelectMany(x => x.PeriodDetails)
                        .Where(x => x.Period <= i)
                        .Sum(x => x.BudgetAmt);

                    costAmt = poolOrgFinancialDetail.poolOrgCostFinancialDetail
                        .SelectMany(x => x.PeriodDetails)
                        .Where(x => x.Period <= i)
                        .Sum(x => x.BudgetedAmt);

                    decimal baseValue = baseAmt.GetValueOrDefault();
                    decimal costValue = costAmt.GetValueOrDefault();

                    decimal Rate = baseValue == 0
                        ? 0
                        : (costValue / baseValue) * 100;


                    //var Rate = (costAmt.GetValueOrDefault() / baseAmt.GetValueOrDefault()) * 100;
                    poolRateDetailsByPeriod.PeriodDetails.Add(new PeriodbaseFinancialDetail
                    {
                        baseAmt = baseAmt,
                        costAmt = costAmt,
                        Period = i,
                        Rate = Rate,
                        AllocationAmt = ((baseAmt ?? 0) * (Rate) / 100)

                    });
                }

            }

            poolRateDetailsByPeriods.Add(poolRateDetailsByPeriod);
            poolOrgFinancialDetails.Add(poolOrgFinancialDetail);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////// Added Fringe to Hr


        List<string> FringeToHr = new List<string>() { "P0-01F-06S" };

        var baseaccountsToAddFromFringe = poolOrgFinancialDetails[0].poolOrgBaseFinancialDetail.Where(p => FringeToHr.Contains(p.AllocationAcctId)).ToList();
        //var baseaccountsToAddFromHr = poolOrgFinancialDetails[1].poolOrgBaseFinancialDetail.Where(p => FringeToOverHead.Contains(p.AllocationAcctId)).ToList();


        if (baseaccountsToAddFromFringe.Any())
        {
            var convertedList = baseaccountsToAddFromFringe.Select(x => new PoolOrgCostFinancialDetail
            {
                AcctId = x.AllocationAcctId,
                OrgId = x.OrgId,
                YTDActualAmt = x.YTDAllocationAmt,
                PeriodDetails = x.PeriodDetails.Select(pd => new PeriodCostFinancialDetail
                {
                    Period = pd.Period,
                    Actualamt = pd.AllocationAmt,
                    BudgetedAmt = pd.BudgetAmt,
                    Rate = pd.Rate
                }).ToList()

                // Add more property mappings here...
            }).ToList();
            poolOrgFinancialDetails[1].poolOrgCostFinancialDetail.AddRange(convertedList);
        }




        //////////////////////////////////////////////////////////////////////////////////////////////////////////// Added Fringe and Hr To OverHead

        List<string> FringeToOverHead = new List<string>() { "P0-12S-13D", "P0-12S-13H" };

        baseaccountsToAddFromFringe = poolOrgFinancialDetails[0].poolOrgBaseFinancialDetail.Where(p => p.AllocationAcctId == "P0-01F-03H").ToList();
        var baseaccountsToAddFromHr = poolOrgFinancialDetails[1].poolOrgBaseFinancialDetail.Where(p => FringeToOverHead.Contains(p.AllocationAcctId)).ToList();


        if (baseaccountsToAddFromFringe.Any())
        {
            var convertedList = baseaccountsToAddFromFringe.Select(x => new PoolOrgCostFinancialDetail
            {
                AcctId = x.AllocationAcctId,
                OrgId = x.OrgId,
                YTDActualAmt = x.YTDAllocationAmt,
                PeriodDetails = x.PeriodDetails.Select(pd => new PeriodCostFinancialDetail
                {
                    Period = pd.Period,
                    Actualamt = pd.AllocationAmt,
                    BudgetedAmt = pd.BudgetAmt,
                    Rate = pd.Rate
                }).ToList()

                // Add more property mappings here...
            }).ToList();
            poolOrgFinancialDetails[2].poolOrgCostFinancialDetail.AddRange(convertedList);
        }

        if (baseaccountsToAddFromHr.Any())
        {
            var convertedList = baseaccountsToAddFromHr.Select(x => new PoolOrgCostFinancialDetail
            {
                AcctId = x.AllocationAcctId,
                OrgId = x.OrgId,
                YTDActualAmt = x.YTDAllocationAmt,
                PeriodDetails = x.PeriodDetails.Select(pd => new PeriodCostFinancialDetail
                {
                    Period = pd.Period,
                    Actualamt = pd.AllocationAmt,
                    BudgetedAmt = pd.BudgetAmt,
                    Rate = pd.Rate
                }).ToList()

                // Add more property mappings here...
            }).ToList();
            poolOrgFinancialDetails[2].poolOrgCostFinancialDetail.AddRange(convertedList);
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        //////////////////////////////////////////////////////////////////////////////////////////////////////////// Added Fringe and Hr To MNH
        List<string> FringeToMnH = new List<string>() { "P0-12S-16M" };

        baseaccountsToAddFromFringe = poolOrgFinancialDetails[0].poolOrgBaseFinancialDetail.Where(p => p.AllocationAcctId == "P0-01F-06M").ToList();
        baseaccountsToAddFromHr = poolOrgFinancialDetails[1].poolOrgBaseFinancialDetail.Where(p => FringeToMnH.Contains(p.AllocationAcctId)).ToList();


        if (baseaccountsToAddFromFringe.Any())
        {
            var convertedList = baseaccountsToAddFromFringe.Select(x => new PoolOrgCostFinancialDetail
            {
                AcctId = x.AllocationAcctId,
                OrgId = x.OrgId,
                YTDActualAmt = x.YTDAllocationAmt,
                PeriodDetails = x.PeriodDetails.Select(pd => new PeriodCostFinancialDetail
                {
                    Period = pd.Period,
                    Actualamt = pd.AllocationAmt,
                    BudgetedAmt = pd.BudgetAmt,
                    Rate = pd.Rate
                }).ToList()

                // Add more property mappings here...
            }).ToList();
            poolOrgFinancialDetails[3].poolOrgCostFinancialDetail.AddRange(convertedList);
        }

        if (baseaccountsToAddFromHr.Any())
        {
            var convertedList = baseaccountsToAddFromHr.Select(x => new PoolOrgCostFinancialDetail
            {
                AcctId = x.AllocationAcctId,
                OrgId = x.OrgId,
                YTDActualAmt = x.YTDAllocationAmt,
                PeriodDetails = x.PeriodDetails.Select(pd => new PeriodCostFinancialDetail
                {
                    Period = pd.Period,
                    Actualamt = pd.AllocationAmt,
                    BudgetedAmt = pd.BudgetAmt,
                    Rate = pd.Rate
                }).ToList()

                // Add more property mappings here...
            }).ToList();
            poolOrgFinancialDetails[3].poolOrgCostFinancialDetail.AddRange(convertedList);
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        //////////////////////////////////////////////////////////////////////////////////////////////////////////// Added Fringe, Hr, OverHead and MNH To GNA To GNA
        //////////////////////////////////////////////////////////////////////////////////////////////////////////// Added Fringe and Hr To GNA To MNH
        List<string> FringeToGNA = new List<string>() { "P0-01F-07G", "P0-01F-07U", "P0-01F-07P" };
        List<string> HRToGNA = new List<string>() { "P0-12S-17G", "P0-12S-17U", "P0-12S-17P" };


        baseaccountsToAddFromFringe = poolOrgFinancialDetails[0].poolOrgBaseFinancialDetail.Where(p => FringeToGNA.Contains(p.AllocationAcctId)).ToList();
        baseaccountsToAddFromHr = poolOrgFinancialDetails[1].poolOrgBaseFinancialDetail.Where(p => HRToGNA.Contains(p.AllocationAcctId)).ToList();


        if (baseaccountsToAddFromFringe.Any())
        {
            var convertedList = baseaccountsToAddFromFringe.Select(x => new PoolOrgCostFinancialDetail
            {
                AcctId = x.AllocationAcctId,
                OrgId = x.OrgId,
                YTDActualAmt = x.YTDAllocationAmt,
                PeriodDetails = x.PeriodDetails.Select(pd => new PeriodCostFinancialDetail
                {
                    Period = pd.Period,
                    Actualamt = pd.AllocationAmt,
                    BudgetedAmt = pd.BudgetAmt,
                    Rate = pd.Rate
                }).ToList()

                // Add more property mappings here...
            }).ToList();
            poolOrgFinancialDetails[4].poolOrgCostFinancialDetail.AddRange(convertedList);
        }

        if (baseaccountsToAddFromHr.Any())
        {
            var convertedList = baseaccountsToAddFromHr.Select(x => new PoolOrgCostFinancialDetail
            {
                AcctId = x.AllocationAcctId,
                OrgId = x.OrgId,
                YTDActualAmt = x.YTDAllocationAmt,
                PeriodDetails = x.PeriodDetails.Select(pd => new PeriodCostFinancialDetail
                {
                    Period = pd.Period,
                    Actualamt = pd.AllocationAmt,
                    BudgetedAmt = pd.BudgetAmt,
                    Rate = pd.Rate
                }).ToList()

                // Add more property mappings here...
            }).ToList();
            poolOrgFinancialDetails[4].poolOrgCostFinancialDetail.AddRange(convertedList);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        List<PlTemplatePoolRate> plTemplatePoolRates = new List<PlTemplatePoolRate>();
        foreach (var poolRateDetailsByPeriod in poolRateDetailsByPeriods)
        {

            foreach (var periodDetail in poolRateDetailsByPeriod.PeriodDetails)
            {
                PlTemplatePoolRate plTemplatePoolRate = new PlTemplatePoolRate();
                plTemplatePoolRate.PoolId = poolRateDetailsByPeriod.PoolName.ToUpper();
                plTemplatePoolRate.TemplateId = 1;
                plTemplatePoolRate.ActualRate = periodDetail.Rate;
                plTemplatePoolRate.TargetRate = periodDetail.Rate;
                plTemplatePoolRate.Month = Convert.ToInt16(periodDetail.Period);
                plTemplatePoolRate.Year = Convert.ToInt16(fycd);
                plTemplatePoolRates.Add(plTemplatePoolRate);
            }
        }
        //account_Org_Helpercs.BulkUpsertTemplatePoolRatesAsync(plTemplatePoolRates).GetAwaiter().GetResult();
        return Ok(poolRateDetailsByPeriods);


    }

    [HttpGet("GetRatesV2")]
    public async Task<IActionResult> GetRatesV2FinalWorking(string fycd)
    {

        var poolDetails = _context.PoolRatesCostpoint.Where(p => p.FyCd == Convert.ToInt32(fycd)).ToList();


        PoolOrgFinancialDetail poolOrgFinancialDetail = new PoolOrgFinancialDetail();

        List<string> poolnos = new List<string>() { "FRINGE", "HR", "OVERHEAD", "MNH", "GNA" };
        List<PoolRateDetailsByPeriod> poolRateDetailsByPeriods = new List<PoolRateDetailsByPeriod>();

        foreach (var poolno in poolnos)
        {
            PoolRateDetailsByPeriod poolRateDetailsByPeriod = new PoolRateDetailsByPeriod();
            poolRateDetailsByPeriod.PoolName = char.ToUpper(poolno[0]) + poolno.Substring(1).ToLower();

            switch (poolno.ToUpper())
            {
                case "OVERHEAD":
                    poolRateDetailsByPeriod.PoolNo = 13;
                    break;
                case "GNA":
                    poolRateDetailsByPeriod.PoolNo = 17;
                    break;
                case "FRINGE":
                    poolRateDetailsByPeriod.PoolNo = 11;
                    break;
                case "HR":
                    poolRateDetailsByPeriod.PoolNo = 12;
                    break;
                case "MNH":
                    poolRateDetailsByPeriod.PoolNo = 16;
                    break;
                default:
                    poolRateDetailsByPeriod.PoolNo = 0;
                    break;
            }

            RateCalculator rateCalculator = new RateCalculator(_context);
            var BaseForecases = rateCalculator.GetBaseForecasts(fycd, poolRateDetailsByPeriod.PoolNo.GetValueOrDefault());
            var CostForecases = rateCalculator.GetCostForecasts(fycd, poolRateDetailsByPeriod.PoolNo.GetValueOrDefault());

            poolRateDetailsByPeriod.PeriodDetails = new List<PeriodbaseFinancialDetail>();
            var ClosedPeriod = DateOnly.Parse(_context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "closing_period").Value);

            var poolData = poolDetails.Where(p => p.PoolNo == poolRateDetailsByPeriod.PoolNo).ToList();

            //foreach (var poolOrgCostFinancialDetail in poolOrgFinancialDetail.poolOrgCostFinancialDetail)
            for (int i = 1; i <= 12; i++)
            {
                decimal? baseAmt = 0;
                decimal? costAmt = 0;

                int lastDay = DateTime.DaysInMonth(Convert.ToInt16(fycd), i);
                DateOnly date = new DateOnly(Convert.ToInt16(fycd), i, lastDay);
                //foreach (var periodDetail in poolOrgCostFinancialDetail.PeriodDetails)
                if (ClosedPeriod >= date)
                {
                    poolRateDetailsByPeriod.PeriodDetails.Add(new PeriodbaseFinancialDetail
                    {
                        Period = i,
                        Rate = poolData.FirstOrDefault(p => p.PdNo == i && p.CurRt > 0)?.CurRt * 100
                    });
                }
                else
                {
                    baseAmt = BaseForecases.Where(p => p.Month == i).Sum(x => x.TotalForecastedAmt);

                    costAmt = CostForecases.Where(p => p.Month == i).Sum(x => x.TotalForecastedAmt);

                    var Rate =
                        baseAmt != 0
                            ? Math.Round((costAmt.GetValueOrDefault() / baseAmt.GetValueOrDefault()) * 100, 4, MidpointRounding.AwayFromZero)
                            : 0;
                    poolRateDetailsByPeriod.PeriodDetails.Add(new PeriodbaseFinancialDetail
                    {
                        Period = i,
                        Rate = Rate
                    });
                }

            }

            poolRateDetailsByPeriods.Add(poolRateDetailsByPeriod);
        }


        List<PlTemplatePoolRate> plTemplatePoolRates = new List<PlTemplatePoolRate>();
        foreach (var poolRateDetailsByPeriod in poolRateDetailsByPeriods)
        {

            foreach (var periodDetail in poolRateDetailsByPeriod.PeriodDetails)
            {
                PlTemplatePoolRate plTemplatePoolRate = new PlTemplatePoolRate();
                plTemplatePoolRate.PoolId = poolRateDetailsByPeriod.PoolName.ToUpper();
                plTemplatePoolRate.TemplateId = 1;
                plTemplatePoolRate.ActualRate = periodDetail.Rate;
                plTemplatePoolRate.TargetRate = periodDetail.Rate;
                plTemplatePoolRate.Month = Convert.ToInt16(periodDetail.Period);
                plTemplatePoolRate.Year = Convert.ToInt16(fycd);
                plTemplatePoolRates.Add(plTemplatePoolRate);
            }
        }
        //account_Org_Helpercs.BulkUpsertTemplatePoolRatesAsync(plTemplatePoolRates).GetAwaiter().GetResult();
        return Ok(poolRateDetailsByPeriods);


        return Ok();
    }


    [HttpGet("GetRatesV3")]
    public async Task<IActionResult> GetRatesV3(string fycd)
    {

        var poolDetails = await _context.PoolRatesCostpoint
                        .Where(x => x.SAcctTypeCd == "B" && x.FyCd == Convert.ToInt32(fycd))
                        .Select(x => new
                        {
                            x.FyCd,
                            x.PdNo,
                            x.PoolNo,
                            x.YtdRt
                        })
                        .Distinct()
                        .ToListAsync();

        PoolOrgFinancialDetail poolOrgFinancialDetail = new PoolOrgFinancialDetail();

        //List<string> poolnos = new List<string>() { "FRINGE", "HR", "OVERHEAD", "MNH", "GNA" };
        List<PoolRateDetailsByPeriod> poolRateDetailsByPeriods = new List<PoolRateDetailsByPeriod>();

        var poolnos = _context.AccountGroups.Where(p => p.PoolNo != null).OrderBy(p => p.Sequence).ToList();
        foreach (var poolno in poolnos)
        {
            PoolRateDetailsByPeriod poolRateDetailsByPeriod = new PoolRateDetailsByPeriod();
            //poolRateDetailsByPeriod.PoolName = char.ToUpper(poolno[0]) + poolno.Substring(1).ToLower();
            poolRateDetailsByPeriod.PoolName = poolno.Name;
            //poolRateDetailsByPeriod.PoolName = char.ToUpper(poolno.Name[0]) + poolno.Name.Substring(1).ToLower();
            poolRateDetailsByPeriod.PoolNo = poolno.PoolNo.GetValueOrDefault();
            //switch (poolno.Code.ToUpper())
            //{
            //    case "OVERHEAD":
            //        poolRateDetailsByPeriod.PoolNo = 13;
            //        break;
            //    case "GNA":
            //        poolRateDetailsByPeriod.PoolNo = 17;
            //        break;
            //    case "FRINGE":
            //        poolRateDetailsByPeriod.PoolNo = 11;
            //        break;
            //    case "HR":
            //        poolRateDetailsByPeriod.PoolNo = 12;
            //        break;
            //    case "MNH":
            //        poolRateDetailsByPeriod.PoolNo = 16;
            //        break;
            //    default:
            //        poolRateDetailsByPeriod.PoolNo = 0;
            //        break;
            //}

            poolRateDetailsByPeriod.PeriodDetails = new List<PeriodbaseFinancialDetail>();
            var poolData = poolDetails.Where(p => p.PoolNo == poolRateDetailsByPeriod.PoolNo).ToList();

            for (int i = 1; i <= 12; i++)
            {
                int lastDay = DateTime.DaysInMonth(Convert.ToInt16(fycd), i);
                DateOnly date = new DateOnly(Convert.ToInt16(fycd), i, lastDay);
                poolRateDetailsByPeriod.PeriodDetails.Add(new PeriodbaseFinancialDetail
                {
                    Period = i,
                    Rate = poolData.FirstOrDefault(p => p.PdNo == i && p.YtdRt > 0)?.YtdRt * 100
                });

            }

            poolRateDetailsByPeriods.Add(poolRateDetailsByPeriod);
        }


        List<PlTemplatePoolRate> plTemplatePoolRates = new List<PlTemplatePoolRate>();
        foreach (var poolRateDetailsByPeriod in poolRateDetailsByPeriods)
        {

            foreach (var periodDetail in poolRateDetailsByPeriod.PeriodDetails)
            {
                PlTemplatePoolRate plTemplatePoolRate = new PlTemplatePoolRate();
                plTemplatePoolRate.PoolId = poolRateDetailsByPeriod.PoolName.ToUpper();
                plTemplatePoolRate.TemplateId = 1;
                plTemplatePoolRate.ActualRate = periodDetail.Rate;
                plTemplatePoolRate.TargetRate = periodDetail.Rate;
                plTemplatePoolRate.Month = Convert.ToInt16(periodDetail.Period);
                plTemplatePoolRate.Year = Convert.ToInt16(fycd);
                plTemplatePoolRates.Add(plTemplatePoolRate);
            }
        }
        return Ok(poolRateDetailsByPeriods);

    }


    [HttpGet("GetRatesV3_Old")]
    public async Task<IActionResult> GetRatesV3_Old(string fycd)
    {

        var poolDetails = _context.PoolRatesCostpoint.Where(p => p.FyCd == Convert.ToInt32(fycd)).ToList();


        PoolOrgFinancialDetail poolOrgFinancialDetail = new PoolOrgFinancialDetail();

        List<string> poolnos = new List<string>() { "FRINGE", "HR", "OVERHEAD", "MNH", "GNA" };
        List<PoolRateDetailsByPeriod> poolRateDetailsByPeriods = new List<PoolRateDetailsByPeriod>();
        foreach (var poolno in poolnos)
        {
            //await CalculateRates(fycd, poolno);
        }

        foreach (var poolno in poolnos)
        {
            PoolRateDetailsByPeriod poolRateDetailsByPeriod = new PoolRateDetailsByPeriod();
            poolRateDetailsByPeriod.PoolName = char.ToUpper(poolno[0]) + poolno.Substring(1).ToLower();

            switch (poolno.ToUpper())
            {
                case "OVERHEAD":
                    poolRateDetailsByPeriod.PoolNo = 13;
                    break;
                case "GNA":
                    poolRateDetailsByPeriod.PoolNo = 17;
                    break;
                case "FRINGE":
                    poolRateDetailsByPeriod.PoolNo = 11;
                    break;
                case "HR":
                    poolRateDetailsByPeriod.PoolNo = 12;
                    break;
                case "MNH":
                    poolRateDetailsByPeriod.PoolNo = 16;
                    break;
                default:
                    poolRateDetailsByPeriod.PoolNo = 0;
                    break;
            }

            RateCalculator rateCalculator = new RateCalculator(_context);
            var BaseForecases = rateCalculator.GetBaseForecasts(fycd, poolRateDetailsByPeriod.PoolNo.GetValueOrDefault());
            var CostForecases = rateCalculator.GetCostForecasts(fycd, poolRateDetailsByPeriod.PoolNo.GetValueOrDefault());

            poolRateDetailsByPeriod.PeriodDetails = new List<PeriodbaseFinancialDetail>();
            var ClosedPeriod = DateOnly.Parse(_context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "closing_period").Value);

            var poolData = poolDetails.Where(p => p.PoolNo == poolRateDetailsByPeriod.PoolNo).ToList();

            //foreach (var poolOrgCostFinancialDetail in poolOrgFinancialDetail.poolOrgCostFinancialDetail)
            for (int i = 1; i <= 12; i++)
            {
                decimal? baseAmt = 0;
                decimal? costAmt = 0;

                int lastDay = DateTime.DaysInMonth(Convert.ToInt16(fycd), i);
                DateOnly date = new DateOnly(Convert.ToInt16(fycd), i, lastDay);
                //foreach (var periodDetail in poolOrgCostFinancialDetail.PeriodDetails)
                if (ClosedPeriod >= date)
                {
                    poolRateDetailsByPeriod.PeriodDetails.Add(new PeriodbaseFinancialDetail
                    {
                        Period = i,
                        Rate = poolData.FirstOrDefault(p => p.PdNo == i && p.YtdRt > 0)?.YtdRt * 100
                    });
                }
                else
                {
                    baseAmt = BaseForecases.Where(p => p.Month == i).Sum(x => x.TotalForecastedAmt);

                    costAmt = CostForecases.Where(p => p.Month == i).Sum(x => x.TotalForecastedAmt);

                    var Rate =
                        baseAmt != 0
                            ? Math.Round((costAmt.GetValueOrDefault() / baseAmt.GetValueOrDefault()) * 100, 4, MidpointRounding.AwayFromZero)
                            : 0;
                    poolRateDetailsByPeriod.PeriodDetails.Add(new PeriodbaseFinancialDetail
                    {
                        Period = i,
                        Rate = Rate
                    });
                }

            }

            poolRateDetailsByPeriods.Add(poolRateDetailsByPeriod);
        }


        List<PlTemplatePoolRate> plTemplatePoolRates = new List<PlTemplatePoolRate>();
        foreach (var poolRateDetailsByPeriod in poolRateDetailsByPeriods)
        {

            foreach (var periodDetail in poolRateDetailsByPeriod.PeriodDetails)
            {
                PlTemplatePoolRate plTemplatePoolRate = new PlTemplatePoolRate();
                plTemplatePoolRate.PoolId = poolRateDetailsByPeriod.PoolName.ToUpper();
                plTemplatePoolRate.TemplateId = 1;
                plTemplatePoolRate.ActualRate = periodDetail.Rate;
                plTemplatePoolRate.TargetRate = periodDetail.Rate;
                plTemplatePoolRate.Month = Convert.ToInt16(periodDetail.Period);
                plTemplatePoolRate.Year = Convert.ToInt16(fycd);
                plTemplatePoolRates.Add(plTemplatePoolRate);
            }
        }
        //account_Org_Helpercs.BulkUpsertTemplatePoolRatesAsync(plTemplatePoolRates).GetAwaiter().GetResult();
        return Ok(poolRateDetailsByPeriods);


        return Ok();
    }


    [HttpGet("CalculateRates")]
    public async Task<IActionResult> CalculateRates(string fycd)
    {

        var poolDetails = _context.PoolRatesCostpoint.Where(p => p.FyCd == Convert.ToInt32(fycd)).ToList();


        PoolOrgFinancialDetail poolOrgFinancialDetail = new PoolOrgFinancialDetail();

        List<string> poolnos = new List<string>() { "FRINGE", "HR", "OVERHEAD", "MNH", "GNA" };
        List<PoolRateDetailsByPeriod> poolRateDetailsByPeriods = new List<PoolRateDetailsByPeriod>();
        foreach (var poolno in poolnos)
        {
            await CalculateRates(fycd, poolno);
        }

        foreach (var poolno in poolnos)
        {
            PoolRateDetailsByPeriod poolRateDetailsByPeriod = new PoolRateDetailsByPeriod();
            poolRateDetailsByPeriod.PoolName = char.ToUpper(poolno[0]) + poolno.Substring(1).ToLower();

            switch (poolno.ToUpper())
            {
                case "OVERHEAD":
                    poolRateDetailsByPeriod.PoolNo = 13;
                    break;
                case "GNA":
                    poolRateDetailsByPeriod.PoolNo = 17;
                    break;
                case "FRINGE":
                    poolRateDetailsByPeriod.PoolNo = 11;
                    break;
                case "HR":
                    poolRateDetailsByPeriod.PoolNo = 12;
                    break;
                case "MNH":
                    poolRateDetailsByPeriod.PoolNo = 16;
                    break;
                default:
                    poolRateDetailsByPeriod.PoolNo = 0;
                    break;
            }

            RateCalculator rateCalculator = new RateCalculator(_context);
            var BaseForecases = rateCalculator.GetBaseForecasts(fycd, poolRateDetailsByPeriod.PoolNo.GetValueOrDefault());
            var CostForecases = rateCalculator.GetCostForecasts(fycd, poolRateDetailsByPeriod.PoolNo.GetValueOrDefault());

            poolRateDetailsByPeriod.PeriodDetails = new List<PeriodbaseFinancialDetail>();
            var ClosedPeriod = DateOnly.Parse(_context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "closing_period").Value);

            var poolData = poolDetails.Where(p => p.PoolNo == poolRateDetailsByPeriod.PoolNo).ToList();

            //foreach (var poolOrgCostFinancialDetail in poolOrgFinancialDetail.poolOrgCostFinancialDetail)
            for (int i = 1; i <= 12; i++)
            {
                decimal? baseAmt = 0;
                decimal? costAmt = 0;

                int lastDay = DateTime.DaysInMonth(Convert.ToInt16(fycd), i);
                DateOnly date = new DateOnly(Convert.ToInt16(fycd), i, lastDay);
                //foreach (var periodDetail in poolOrgCostFinancialDetail.PeriodDetails)
                if (ClosedPeriod >= date)
                {
                    poolRateDetailsByPeriod.PeriodDetails.Add(new PeriodbaseFinancialDetail
                    {
                        Period = i,
                        Rate = poolData.FirstOrDefault(p => p.PdNo == i && p.YtdRt > 0)?.YtdRt * 100
                    });
                }
                else
                {
                    baseAmt = BaseForecases.Where(p => p.Month == i).Sum(x => x.TotalForecastedAmt);

                    costAmt = CostForecases.Where(p => p.Month == i).Sum(x => x.TotalForecastedAmt);

                    var Rate =
                        baseAmt != 0
                            ? Math.Round((costAmt.GetValueOrDefault() / baseAmt.GetValueOrDefault()) * 100, 4, MidpointRounding.AwayFromZero)
                            : 0;
                    poolRateDetailsByPeriod.PeriodDetails.Add(new PeriodbaseFinancialDetail
                    {
                        Period = i,
                        Rate = Rate
                    });
                }

            }

            poolRateDetailsByPeriods.Add(poolRateDetailsByPeriod);
        }


        List<PlTemplatePoolRate> plTemplatePoolRates = new List<PlTemplatePoolRate>();
        foreach (var poolRateDetailsByPeriod in poolRateDetailsByPeriods)
        {

            foreach (var periodDetail in poolRateDetailsByPeriod.PeriodDetails)
            {
                PlTemplatePoolRate plTemplatePoolRate = new PlTemplatePoolRate();
                plTemplatePoolRate.PoolId = poolRateDetailsByPeriod.PoolName.ToUpper();
                plTemplatePoolRate.TemplateId = 1;
                plTemplatePoolRate.ActualRate = periodDetail.Rate;
                plTemplatePoolRate.TargetRate = periodDetail.Rate;
                plTemplatePoolRate.Month = Convert.ToInt16(periodDetail.Period);
                plTemplatePoolRate.Year = Convert.ToInt16(fycd);
                plTemplatePoolRates.Add(plTemplatePoolRate);
            }
        }
        //account_Org_Helpercs.BulkUpsertTemplatePoolRatesAsync(plTemplatePoolRates).GetAwaiter().GetResult();
        return Ok(poolRateDetailsByPeriods);


        return Ok();
    }

    //[HttpGet("GetRatesV2")]
    //public async Task<IActionResult> GetRatesV2(string fycd)
    //{

    //    var poolDetails = _context.PoolRatesCostpoint.Where(p => p.FyCd == Convert.ToInt32(fycd)).ToList();


    //    PoolOrgFinancialDetail poolOrgFinancialDetail = new PoolOrgFinancialDetail();

    //    List<string> poolnos = new List<string>() { "FRINGE", "HR", "OVERHEAD", "MNH", "GNA" };
    //    List<PoolRateDetailsByPeriod> poolRateDetailsByPeriods = new List<PoolRateDetailsByPeriod>();

    //    foreach (var poolno in poolnos)
    //    {
    //        PoolRateDetailsByPeriod poolRateDetailsByPeriod = new PoolRateDetailsByPeriod();
    //        poolRateDetailsByPeriod.PoolName = char.ToUpper(poolno[0]) + poolno.Substring(1).ToLower();

    //        switch (poolno.ToUpper())
    //        {
    //            case "OVERHEAD":
    //                poolRateDetailsByPeriod.PoolNo = 13;
    //                break;
    //            case "GNA":
    //                poolRateDetailsByPeriod.PoolNo = 17;
    //                break;
    //            case "FRINGE":
    //                poolRateDetailsByPeriod.PoolNo = 11;
    //                break;
    //            case "HR":
    //                poolRateDetailsByPeriod.PoolNo = 12;
    //                break;
    //            case "MNH":
    //                poolRateDetailsByPeriod.PoolNo = 16;
    //                break;
    //            default:
    //                poolRateDetailsByPeriod.PoolNo = 0;
    //                break;
    //        }

    //        poolRateDetailsByPeriod.PeriodDetails = new List<PeriodbaseFinancialDetail>();
    //        var ClosedPeriod = DateOnly.Parse(_context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "closing_period").Value);

    //        var poolData = poolDetails.Where(p => p.PoolNo == poolRateDetailsByPeriod.PoolNo).ToList();

    //        //foreach (var poolOrgCostFinancialDetail in poolOrgFinancialDetail.poolOrgCostFinancialDetail)
    //        for (int i = 1; i <= 12; i++)
    //        {
    //            decimal? baseAmt = 0;
    //            decimal? costAmt = 0;

    //            int lastDay = DateTime.DaysInMonth(Convert.ToInt16(fycd), i);
    //            DateOnly date = new DateOnly(Convert.ToInt16(fycd), i, lastDay);
    //            //foreach (var periodDetail in poolOrgCostFinancialDetail.PeriodDetails)
    //            if (ClosedPeriod >= date)
    //            {
    //                poolRateDetailsByPeriod.PeriodDetails.Add(new PeriodbaseFinancialDetail
    //                {
    //                    Period = i,
    //                    Rate = poolData.FirstOrDefault(p => p.PdNo == i && p.CurRt > 0)?.CurRt * 100
    //                });
    //            }
    //            else
    //            {
    //                //baseAmt = poolOrgFinancialDetail.poolOrgBaseFinancialDetail
    //                //    .SelectMany(x => x.PeriodDetails)
    //                //    .Where(x => x.Period <= i)
    //                //    .Sum(x => x.BudgetAmt);

    //                //costAmt = poolOrgFinancialDetail.poolOrgCostFinancialDetail
    //                //    .SelectMany(x => x.PeriodDetails)
    //                //    .Where(x => x.Period <= i)
    //                //    .Sum(x => x.BudgetedAmt);

    //                //var Rate =
    //                //    baseAmt != 0
    //                //        ? Math.Round((costAmt.GetValueOrDefault() / baseAmt.GetValueOrDefault()) * 100, 4, MidpointRounding.AwayFromZero)
    //                //        : 0;
    //                //poolRateDetailsByPeriod.PeriodDetails.Add(new PeriodbaseFinancialDetail
    //                //{
    //                //    Period = i,
    //                //    Rate = Rate
    //                //});
    //            }

    //        }

    //        poolRateDetailsByPeriods.Add(poolRateDetailsByPeriod);
    //    }
    //    List<PlTemplatePoolRate> plTemplatePoolRates = new List<PlTemplatePoolRate>();
    //    foreach (var poolRateDetailsByPeriod in poolRateDetailsByPeriods)
    //    {

    //        foreach (var periodDetail in poolRateDetailsByPeriod.PeriodDetails)
    //        {
    //            PlTemplatePoolRate plTemplatePoolRate = new PlTemplatePoolRate();
    //            plTemplatePoolRate.PoolId = poolRateDetailsByPeriod.PoolName.ToUpper();
    //            plTemplatePoolRate.TemplateId = 1;
    //            plTemplatePoolRate.ActualRate = periodDetail.Rate;
    //            plTemplatePoolRate.TargetRate = periodDetail.Rate;
    //            plTemplatePoolRate.Month = Convert.ToInt16(periodDetail.Period);
    //            plTemplatePoolRate.Year = Convert.ToInt16(fycd);
    //            plTemplatePoolRates.Add(plTemplatePoolRate);
    //        }
    //    }
    //    account_Org_Helpercs.BulkUpsertTemplatePoolRatesAsync(plTemplatePoolRates).GetAwaiter().GetResult();
    //    return Ok(poolRateDetailsByPeriods);


    //    return Ok();
    //}

    [HttpGet("GetHR_Working")]
    public async Task<IActionResult> GetPoolValuesV1(string fycd, string type)
    {


        RateCalculator rateCalculator = new RateCalculator(_context);

        PoolOrgFinancialDetail poolOrgFinancialDetail = new PoolOrgFinancialDetail();
        PoolOrgFinancialDetail poolOrgFringeDetail = new PoolOrgFinancialDetail();

        int poolno = 0;
        switch (type.ToUpper())
        {
            case "OVERHEAD":
                poolno = 13;
                break;
            case "GNA":
                poolno = 17;
                break;
            case "FRINGE":
                poolno = 11;
                break;
            case "HR":
                poolno = 12;
                break;
            case "MNH":
                poolno = 16;
                break;
            default:
                poolno = 0;
                break;
        }
        var ClosedPeriod = DateOnly.Parse(_context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "closing_period").Value);
        var BaseForecases = rateCalculator.GetBaseForecasts(fycd, poolno);
        var CostForecases = rateCalculator.GetCostForecasts(fycd, poolno);

        //return Ok();
        var chartOfAccounts = _context.Charts_Of_Accounts.ToList();


        var poolDetails = await _context.PoolRatesCostpoint
            .Where(x => x.PoolNo == poolno && x.FyCd == Convert.ToInt32(fycd))
            .GroupBy(x => new
            {
                x.FyCd,
                x.PdNo,
                x.PoolNo,
                x.SAcctTypeCd,
                x.AcctId,
                x.OrgId
            })
            .Select(g => new
            {
                FyCd = g.Key.FyCd,
                PdNo = g.Key.PdNo,
                PoolNo = g.Key.PoolNo,
                SAcctTypeCd = g.Key.SAcctTypeCd,
                AcctId = g.Key.AcctId,
                OrgId = g.Key.OrgId,

                CurAmt = g.Sum(x => x.CurAmt ?? 0),
                YtdAmt = g.Sum(x => x.YtdAmt ?? 0),
                CurAllocAmt = g.Sum(x => x.CurAllocAmt ?? 0),
                YtdAllocAmt = g.Sum(x => x.YtdAllocAmt ?? 0),
                CurRt = g.Sum(x => x.CurRt ?? 0),
                YtdRt = g.Sum(x => x.YtdRt ?? 0),
                CurBudAmt = g.Sum(x => x.CurBudAmt ?? 0),
                YtdBudAmt = g.Sum(x => x.YtdBudAmt ?? 0),
                CurBudAllocAmt = g.Sum(x => x.CurBudAllocAmt ?? 0),
                YtdBudAllocAmt = g.Sum(x => x.YtdBudAllocAmt ?? 0),
                CurBudRt = g.Sum(x => x.CurBudRt ?? 0),
                YtdBudRt = g.Sum(x => x.YtdBudRt ?? 0),
                CurBaseAmt = g.Sum(x => x.CurBaseAmt ?? 0),
                YtdBaseAmt = g.Sum(x => x.YtdBaseAmt ?? 0)
            })
            .OrderBy(x => x.FyCd)
            .ThenBy(x => x.PdNo)
            .ThenBy(x => x.PoolNo)
            .ThenBy(x => x.AcctId)
            .ThenBy(x => x.OrgId)
            .ToListAsync();

        var baseAccountDetails = poolDetails.Where(p => p.SAcctTypeCd == "B").ToList();
        var costAccountDetails = poolDetails.Where(p => p.SAcctTypeCd == "C").ToList();
        poolOrgFinancialDetail.poolOrgBaseFinancialDetail = new List<PoolOrgBaseFinancialDetail>();
        poolOrgFinancialDetail.poolOrgCostFinancialDetail = new List<PoolOrgCostFinancialDetail>();

        var baseLaborAccountFinancialDetails = _context.PoolBaseAccounts.Where(p => p.PoolNo == poolno).ToList();

        foreach (var baseAccount in baseAccountDetails.Select(p => new { p.AcctId, p.OrgId }).Distinct())
        {
            var allocationDetails = baseLaborAccountFinancialDetails
                                .Where(p => p.PoolNo == poolno && p.AcctId == baseAccount.AcctId && p.OrgId == baseAccount.OrgId)
                                .FirstOrDefault();
            PoolOrgBaseFinancialDetail baseFinancialDetail = new PoolOrgBaseFinancialDetail
            {
                AcctId = baseAccount.AcctId,
                OrgId = baseAccount.OrgId,
                AllocationAcctId = allocationDetails?.AllocAcctId,
                AllocationOrgId = allocationDetails?.AllocOrgId,
                YTDActualAmt = baseAccountDetails.Where(p => p.AcctId == baseAccount.AcctId && p.OrgId == baseAccount.OrgId)?.Sum(q => q.YtdBaseAmt),
                PeriodDetails = new List<PeriodbaseFinancialDetail>()
            };
            for (int i = 1; i <= 12; i++)
            {
                //var periodData = poolDetails.FirstOrDefault(p => p.PdNo == i && p.AcctId == baseAccount.AcctId && p.OrgId == baseAccount.OrgId && p.SAcctTypeCd == "B");

                int lastDay = DateTime.DaysInMonth(Convert.ToInt16(fycd), i);
                DateOnly date = new DateOnly(Convert.ToInt16(fycd), i, lastDay);
                if (ClosedPeriod >= date)
                {
                    var periodData = baseAccountDetails.FirstOrDefault(p => p.PdNo == i && p.AcctId == baseAccount.AcctId && p.OrgId == baseAccount.OrgId);

                    if (periodData != null)
                    {
                        baseFinancialDetail.PeriodDetails.Add(new PeriodbaseFinancialDetail
                        {
                            Period = i,
                            baseAmt = periodData.CurAmt,
                            Rate = periodData.CurRt,
                            AllocationAmt = periodData.CurAllocAmt
                        });
                    }

                }
                else
                {
                    var periodData = CostForecases.FirstOrDefault(p => p.Month == i && p.AcctId == baseAccount.AcctId && p.OrgId == baseAccount.OrgId);
                    //var periodData = BaseForecases.FirstOrDefault(p => p.Month == i && p.AcctId == baseAccount.AcctId && p.OrgId == baseAccount.OrgId);
                    if (periodData != null)
                    {
                        baseFinancialDetail.PeriodDetails.Add(new PeriodbaseFinancialDetail
                        {
                            Period = i,
                            BudgetAmt = periodData.TotalForecastedAmt,
                            //Rate = (periodCostData.TotalForecastedAmt / periodData.TotalForecastedAmt) * 100,
                            //AllocationAmt = periodData.TotalForecastedAmt * (periodCostData.TotalForecastedAmt / periodData.TotalForecastedAmt)
                        });
                    }
                }
            }
            baseFinancialDetail.YTDAllocationAmt = baseFinancialDetail.PeriodDetails.Sum(x => x.AllocationAmt);
            poolOrgFinancialDetail.poolOrgBaseFinancialDetail.Add(baseFinancialDetail);
        }

        foreach (var costAccount in costAccountDetails.Select(p => new { p.AcctId, p.OrgId }).Distinct())
        {
            PoolOrgCostFinancialDetail costFinancialDetail = new PoolOrgCostFinancialDetail
            {
                AcctId = costAccount.AcctId,
                OrgId = costAccount.OrgId,
                //YTDActualAmt = costAccountDetails.FirstOrDefault(p=>p.AcctId == costAccount.AcctId && p.OrgId == costAccount.OrgId)?.YtdAmt,
                YTDActualAmt = costAccountDetails.Where(p => p.AcctId == costAccount.AcctId && p.OrgId == costAccount.OrgId)?.Sum(q => q.YtdAmt),

                PeriodDetails = new List<PeriodCostFinancialDetail>()
            };
            for (int i = 1; i <= 12; i++)
            {

                int lastDay = DateTime.DaysInMonth(Convert.ToInt16(fycd), i);
                DateOnly date = new DateOnly(Convert.ToInt16(fycd), i, lastDay);
                if (ClosedPeriod >= date)
                {
                    //var periodData = poolDetails.FirstOrDefault(p => p.PdNo == i && p.AcctId == costAccount.AcctId && p.OrgId == costAccount.OrgId && p.SAcctTypeCd == "C");
                    var periodData = costAccountDetails.FirstOrDefault(p => p.PdNo == i && p.AcctId == costAccount.AcctId && p.OrgId == costAccount.OrgId);
                    if (periodData != null)
                    {
                        costFinancialDetail.PeriodDetails.Add(new PeriodCostFinancialDetail
                        {
                            Period = i,
                            Actualamt = periodData.CurAmt,
                            Rate = periodData.CurRt
                        });
                    }
                }
                else
                {
                    var periodData = CostForecases.FirstOrDefault(p => p.Month == i && p.AcctId == costAccount.AcctId && p.OrgId == costAccount.OrgId);
                    if (periodData != null)
                    {
                        costFinancialDetail.PeriodDetails.Add(new PeriodCostFinancialDetail
                        {
                            Period = i,
                            BudgetedAmt = periodData.TotalForecastedAmt,
                            //Rate = periodData.TotalForecastedAmt / (periodBaseData != null && periodBaseData.TotalForecastedAmt != 0 ? periodBaseData.TotalForecastedAmt : 1) * 100,
                        });
                    }
                }
            }
            poolOrgFinancialDetail.poolOrgCostFinancialDetail.Add(costFinancialDetail);
        }

        poolOrgFinancialDetail.TotalYTDBaseActualAmt = poolOrgFinancialDetail.poolOrgBaseFinancialDetail.Sum(x => x.YTDActualAmt);
        poolOrgFinancialDetail.TotalYTDCostActualAmt = poolOrgFinancialDetail.poolOrgCostFinancialDetail.Sum(x => x.YTDActualAmt);

        poolOrgFinancialDetail.TotalYTDBaseAllocationActualAmt = poolOrgFinancialDetail.poolOrgBaseFinancialDetail
            .SelectMany(x => x.PeriodDetails)
            .Sum(x => x.AllocationAmt);

        poolOrgFinancialDetail.Rate =
            poolOrgFinancialDetail.TotalYTDBaseActualAmt != 0
                ? Math.Round(
                    (poolOrgFinancialDetail.TotalYTDCostActualAmt.GetValueOrDefault() / poolOrgFinancialDetail.TotalYTDBaseActualAmt.GetValueOrDefault()) * 100,
                    4
                  )
                : 0;


        foreach (var costDetail in poolOrgFinancialDetail.poolOrgCostFinancialDetail)
        {
            var account = chartOfAccounts.FirstOrDefault(c => c.AccountId == costDetail.AcctId);
            if (account != null)
            {
                costDetail.AccountName = account.AccountName;
            }
        }
        foreach (var costDetail in poolOrgFinancialDetail.poolOrgBaseFinancialDetail)
        {
            var account = chartOfAccounts.FirstOrDefault(c => c.AccountId == costDetail.AcctId);
            if (account != null)
            {
                costDetail.AccountName = account.AccountName;
            }
        }
        return Ok(poolOrgFinancialDetail);


    }


    [HttpGet("GetHR")]
    public async Task<IActionResult> GetPoolValuesV2(string fycd, string type)
    {


        RateCalculator rateCalculator = new RateCalculator(_context);

        PoolOrgFinancialDetail poolOrgFinancialDetail = new PoolOrgFinancialDetail();
        PoolOrgFinancialDetail poolOrgFringeDetail = new PoolOrgFinancialDetail();
        List<PoolRatesCostpoint> PoolRatesCostpoint = new List<PoolRatesCostpoint>();
        PoolRatesCostpoint PoolRateCostpoint = new PoolRatesCostpoint();
        int poolno = _context.AccountGroups.FirstOrDefault(p => p.Code.ToUpper().Equals(type.ToUpper())).PoolNo.GetValueOrDefault();
        //switch (type.ToUpper())
        //{
        //    case "OVERHEAD":
        //        poolno = 13;
        //        break;
        //    case "GNA":
        //        poolno = 17;
        //        break;
        //    case "FRINGE":
        //        poolno = 11;
        //        break;
        //    case "HR":
        //        poolno = 12;
        //        break;
        //    case "MNH":
        //        poolno = 16;
        //        break;
        //    default:
        //        poolno = 0;
        //        break;
        //}

        var chartOfAccounts = _context.Charts_Of_Accounts.ToList();
        var poolDetails = await _context.PoolRatesCostpoint
            .Where(x => x.PoolNo == poolno && x.FyCd == Convert.ToInt32(fycd))
            .GroupBy(x => new
            {
                x.FyCd,
                x.PdNo,
                x.PoolNo,
                x.SAcctTypeCd,
                x.AcctId,
                x.OrgId
            })
            .Select(g => new PoolRatesCostpoint
            {
                FyCd = g.Key.FyCd,
                PdNo = g.Key.PdNo,
                PoolNo = g.Key.PoolNo,
                SAcctTypeCd = g.Key.SAcctTypeCd,
                AcctId = g.Key.AcctId,
                OrgId = g.Key.OrgId,

                CurAmt = g.Sum(x => x.CurAmt ?? 0),
                YtdAmt = g.Sum(x => x.YtdAmt ?? 0),
                CurAllocAmt = g.Sum(x => x.CurAllocAmt ?? 0),
                YtdAllocAmt = g.Sum(x => x.YtdAllocAmt ?? 0),
                CurRt = g.Sum(x => x.CurRt ?? 0),
                YtdRt = g.Sum(x => x.YtdRt ?? 0),
                CurBudAmt = g.Sum(x => x.CurBudAmt ?? 0),
                YtdBudAmt = g.Sum(x => x.YtdBudAmt ?? 0),
                CurBudAllocAmt = g.Sum(x => x.CurBudAllocAmt ?? 0),
                YtdBudAllocAmt = g.Sum(x => x.YtdBudAllocAmt ?? 0),
                CurBudRt = g.Sum(x => x.CurBudRt ?? 0),
                YtdBudRt = g.Sum(x => x.YtdBudRt ?? 0),
                CurBaseAmt = g.Sum(x => x.CurBaseAmt ?? 0),
                YtdBaseAmt = g.Sum(x => x.YtdBaseAmt ?? 0)
            })
            .OrderBy(x => x.FyCd)
            .ThenBy(x => x.PdNo)
            .ThenBy(x => x.PoolNo)
            .ThenBy(x => x.AcctId)
            .ThenBy(x => x.OrgId)
            .ToListAsync();

        var baseAccountDetails = poolDetails.Where(p => p.SAcctTypeCd == "B").ToList();
        var costAccountDetails = poolDetails.Where(p => p.SAcctTypeCd == "C").ToList();

        var poolBaseAccounts = _context.PoolBaseAccounts.Where(p => p.PoolNo == poolno).ToList();

        poolOrgFinancialDetail.poolOrgBaseFinancialDetail = new List<PoolOrgBaseFinancialDetail>();
        poolOrgFinancialDetail.poolOrgCostFinancialDetail = new List<PoolOrgCostFinancialDetail>();

        var baseLaborAccountFinancialDetails = _context.PoolBaseAccounts.Where(p => p.PoolNo == poolno).ToList();

        foreach (var baseAccount in baseAccountDetails.Select(p => new { p.AcctId, p.OrgId }).Distinct())
        {
            var allocationDetails = baseLaborAccountFinancialDetails
                                .Where(p => p.PoolNo == poolno && p.AcctId == baseAccount.AcctId && p.OrgId == baseAccount.OrgId)
                                .FirstOrDefault();
            PoolOrgBaseFinancialDetail baseFinancialDetail = new PoolOrgBaseFinancialDetail
            {
                AcctId = baseAccount.AcctId,
                OrgId = baseAccount.OrgId,
                AllocationAcctId = allocationDetails?.AllocAcctId,
                AllocationOrgId = allocationDetails?.AllocOrgId,
                YTDActualAmt = baseAccountDetails.Where(p => p.AcctId == baseAccount.AcctId && p.OrgId == baseAccount.OrgId)?.Sum(q => q.YtdBaseAmt),
                PeriodDetails = new List<PeriodbaseFinancialDetail>()
            };

            for (int i = 1; i <= 12; i++)
            {
                int lastDay = DateTime.DaysInMonth(Convert.ToInt16(fycd), i);
                DateOnly date = new DateOnly(Convert.ToInt16(fycd), i, lastDay);
                var periodData = baseAccountDetails.FirstOrDefault(p => p.PdNo == i && p.AcctId == baseAccount.AcctId && p.OrgId == baseAccount.OrgId);

                if (periodData != null)
                {
                    baseFinancialDetail.PeriodDetails.Add(new PeriodbaseFinancialDetail
                    {
                        Period = i,
                        baseAmt = periodData.CurAmt,
                        YtdBudgetAmt = periodData.YtdBaseAmt,
                        Rate = periodData.CurRt,
                        AllocationAmt = periodData.CurAllocAmt,
                        YtdAllocationAmt = periodData.YtdAllocAmt,
                        YtdcostAmt = periodData.YtdAmt
                    });
                }

            }
            baseFinancialDetail.YTDAllocationAmt = baseFinancialDetail.PeriodDetails.Sum(x => x.AllocationAmt);
            poolOrgFinancialDetail.poolOrgBaseFinancialDetail.Add(baseFinancialDetail);
        }

        foreach (var costAccount in costAccountDetails.Select(p => new { p.AcctId, p.OrgId }).Distinct())
        {
            PoolOrgCostFinancialDetail costFinancialDetail = new PoolOrgCostFinancialDetail
            {
                AcctId = costAccount.AcctId,
                OrgId = costAccount.OrgId,
                //YTDActualAmt = costAccountDetails.FirstOrDefault(p=>p.AcctId == costAccount.AcctId && p.OrgId == costAccount.OrgId)?.YtdAmt,
                YTDActualAmt = costAccountDetails.Where(p => p.AcctId == costAccount.AcctId && p.OrgId == costAccount.OrgId)?.Sum(q => q.YtdAmt),

                PeriodDetails = new List<PeriodCostFinancialDetail>()
            };
            for (int k = 1; k <= 12; k++)
            {

                int lastDay = DateTime.DaysInMonth(Convert.ToInt16(fycd), k);
                DateOnly date = new DateOnly(Convert.ToInt16(fycd), k, lastDay);
                var periodData = costAccountDetails.FirstOrDefault(p => p.PdNo == k && p.AcctId == costAccount.AcctId && p.OrgId == costAccount.OrgId);
                if (periodData != null)
                {
                    costFinancialDetail.PeriodDetails.Add(new PeriodCostFinancialDetail
                    {
                        Period = k,
                        Actualamt = periodData.CurAmt,
                        Rate = periodData.CurRt,
                        Ytdamt = periodData.YtdAmt
                    });
                }
            }
            poolOrgFinancialDetail.poolOrgCostFinancialDetail.Add(costFinancialDetail);
        }

        poolOrgFinancialDetail.TotalYTDBaseActualAmt = poolOrgFinancialDetail.poolOrgBaseFinancialDetail.Sum(x => x.YTDActualAmt);
        poolOrgFinancialDetail.TotalYTDCostActualAmt = poolOrgFinancialDetail.poolOrgCostFinancialDetail.Sum(x => x.YTDActualAmt);

        poolOrgFinancialDetail.TotalYTDBaseAllocationActualAmt = poolOrgFinancialDetail.poolOrgBaseFinancialDetail
            .SelectMany(x => x.PeriodDetails)
            .Sum(x => x.AllocationAmt);

        poolOrgFinancialDetail.Rate =
            poolOrgFinancialDetail.TotalYTDBaseActualAmt != 0
                ? Math.Round(
                    (poolOrgFinancialDetail.TotalYTDCostActualAmt.GetValueOrDefault() / poolOrgFinancialDetail.TotalYTDBaseActualAmt.GetValueOrDefault()) * 100,
                    4
                  )
                : 0;

        foreach (var costDetail in poolOrgFinancialDetail.poolOrgCostFinancialDetail)
        {
            var account = chartOfAccounts.FirstOrDefault(c => c.AccountId == costDetail.AcctId);
            if (account != null)
            {
                costDetail.AccountName = account.AccountName;
            }
        }
        foreach (var baseDetail in poolOrgFinancialDetail.poolOrgBaseFinancialDetail)
        {

            var account = chartOfAccounts.FirstOrDefault(c => c.AccountId == baseDetail.AcctId);
            if (account != null)
            {
                baseDetail.AccountName = account.AccountName;
            }
        }

        return Ok(poolOrgFinancialDetail);


    }

    [HttpGet("GetHRLatestWorking")]
    public async Task<IActionResult> GetHRLatestWorking(string fycd, string type)
    {


        RateCalculator rateCalculator = new RateCalculator(_context);

        PoolOrgFinancialDetail poolOrgFinancialDetail = new PoolOrgFinancialDetail();
        PoolOrgFinancialDetail poolOrgFringeDetail = new PoolOrgFinancialDetail();
        List<PoolRatesCostpoint> PoolRatesCostpoint = new List<PoolRatesCostpoint>();
        PoolRatesCostpoint PoolRateCostpoint = new PoolRatesCostpoint();
        int poolno = 0;
        switch (type.ToUpper())
        {
            case "OVERHEAD":
                poolno = 13;
                break;
            case "GNA":
                poolno = 17;
                break;
            case "FRINGE":
                poolno = 11;
                break;
            case "HR":
                poolno = 12;
                break;
            case "MNH":
                poolno = 16;
                break;
            default:
                poolno = 0;
                break;
        }
        var ClosedPeriod = DateOnly.Parse(_context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "closing_period").Value);
        var BaseForecases = rateCalculator.GetBaseForecasts(fycd, poolno);
        var CostForecases = rateCalculator.GetCostForecasts(fycd, poolno);

        //return Ok();
        var chartOfAccounts = _context.Charts_Of_Accounts.ToList();


        var poolDetails = await _context.PoolRatesCostpoint
            .Where(x => x.PoolNo == poolno && x.FyCd == Convert.ToInt32(fycd))
            .GroupBy(x => new
            {
                x.FyCd,
                x.PdNo,
                x.PoolNo,
                x.SAcctTypeCd,
                x.AcctId,
                x.OrgId
            })
            .Select(g => new PoolRatesCostpoint
            {
                FyCd = g.Key.FyCd,
                PdNo = g.Key.PdNo,
                PoolNo = g.Key.PoolNo,
                SAcctTypeCd = g.Key.SAcctTypeCd,
                AcctId = g.Key.AcctId,
                OrgId = g.Key.OrgId,

                CurAmt = g.Sum(x => x.CurAmt ?? 0),
                YtdAmt = g.Sum(x => x.YtdAmt ?? 0),
                CurAllocAmt = g.Sum(x => x.CurAllocAmt ?? 0),
                YtdAllocAmt = g.Sum(x => x.YtdAllocAmt ?? 0),
                CurRt = g.Sum(x => x.CurRt ?? 0),
                YtdRt = g.Sum(x => x.YtdRt ?? 0),
                CurBudAmt = g.Sum(x => x.CurBudAmt ?? 0),
                YtdBudAmt = g.Sum(x => x.YtdBudAmt ?? 0),
                CurBudAllocAmt = g.Sum(x => x.CurBudAllocAmt ?? 0),
                YtdBudAllocAmt = g.Sum(x => x.YtdBudAllocAmt ?? 0),
                CurBudRt = g.Sum(x => x.CurBudRt ?? 0),
                YtdBudRt = g.Sum(x => x.YtdBudRt ?? 0),
                CurBaseAmt = g.Sum(x => x.CurBaseAmt ?? 0),
                YtdBaseAmt = g.Sum(x => x.YtdBaseAmt ?? 0)
            })
            .OrderBy(x => x.FyCd)
            .ThenBy(x => x.PdNo)
            .ThenBy(x => x.PoolNo)
            .ThenBy(x => x.AcctId)
            .ThenBy(x => x.OrgId)
            .ToListAsync();

        //    var result =
        //from pss in _context.PoolRatesCostpoint
        //join pba in _context.PoolBaseAccounts
        //    on new { pss.AcctId, pss.OrgId }
        //    equals new { AcctId = pba.AcctId, OrgId = pba.OrgId }
        //where pss.SAcctTypeCd == "B"
        //   && pss.FyCd == Convert.ToInt32(fycd)
        //   && _context.PoolCostAccounts
        //        .Any(pca =>
        //            pca.AcctId == pba.AllocAcctId &&
        //            pca.OrgId == pba.AllocOrgId &&
        //            pca.PoolNo == poolno)
        //group pss by new
        //{
        //    pss.PdNo,
        //    pss.PoolNo,
        //    pss.AcctId,
        //    pss.OrgId,
        //    pba.AllocAcctId,
        //    pba.AllocOrgId

        //}
        //into g
        //select new
        //{
        //    AllocationGroup = g.Key.AllocOrgId,
        //    AllocationAccount = g.Key.AllocAcctId,
        //    PdNo = g.Key.PdNo,
        //    PoolNo = g.Key.PoolNo,
        //    AcctId = g.Key.AcctId,
        //    OrgId = g.Key.OrgId,
        //    YtdAllocAmount = g.Sum(x => x.YtdAllocAmt),
        //    CurrAllocAmount = g.Sum(x => x.CurAllocAmt)
        //};


        var result =
            from pss in _context.PoolRatesCostpoint
            from pba in _context.PoolBaseAccounts
            where pss.AcctId == pba.AcctId
               && pss.OrgId == pba.OrgId
               && pss.PoolNo == pba.PoolNo
            join pca in _context.PoolCostAccounts
                on new
                {
                    AcctId = pba.AllocAcctId,
                    OrgId = pba.AllocOrgId
                }
                equals new
                {
                    AcctId = pca.AcctId,
                    OrgId = pca.OrgId
                }
            where pss.SAcctTypeCd == "B"
               && pss.FyCd == Convert.ToInt32(fycd)
               && pca.PoolNo == poolno
            group pss by new
            {
                pss.PdNo,
                pss.PoolNo,
                pss.AcctId,
                pss.OrgId,
                AllocAcctId = pba.AllocAcctId,
                AllocOrgId = pba.AllocOrgId
            }
            into g
            select new
            {
                AllocationGroup = g.Key.AllocOrgId,
                AllocationAccount = g.Key.AllocAcctId,
                PdNo = g.Key.PdNo,
                PoolNo = g.Key.PoolNo,
                AcctId = g.Key.AcctId,
                OrgId = g.Key.OrgId,
                YtdAllocAmount = g.Max(x => x.YtdAllocAmt ?? 0),
                CurrAllocAmount = g.Max(x => x.CurAllocAmt ?? 0)
            };

        //    var baseresult =
        //from pss in _context.PoolRatesCostpoint
        //from pba in _context.PoolBaseAccounts
        //where pss.AcctId == pba.AcctId
        //   && pss.OrgId == pba.OrgId
        //   && pss.PoolNo == pba.PoolNo
        //join pca in _context.PoolBaseAccounts
        //    on new
        //    {
        //        AcctId = pba.AllocAcctId,
        //        OrgId = pba.AllocOrgId
        //    }
        //    equals new
        //    {
        //        AcctId = pca.AcctId,
        //        OrgId = pca.OrgId
        //    }
        //where pss.SAcctTypeCd == "B"
        //   && pss.FyCd == Convert.ToInt32(fycd)
        //   && pca.PoolNo == poolno
        //group pss by new
        //{
        //    pss.PdNo,
        //    pss.PoolNo,
        //    pss.AcctId,
        //    pss.OrgId,
        //    AllocAcctId = pba.AllocAcctId,
        //    AllocOrgId = pba.AllocOrgId
        //}
        //into g
        //select new
        //{
        //    AllocationGroup = g.Key.AllocOrgId,
        //    AllocationAccount = g.Key.AllocAcctId,
        //    PdNo = g.Key.PdNo,
        //    PoolNo = g.Key.PoolNo,
        //    AcctId = g.Key.AcctId,
        //    OrgId = g.Key.OrgId,
        //    YtdAllocAmount = g.Sum(x => x.YtdAllocAmt ?? 0),
        //    CurrAllocAmount = g.Sum(x => x.CurAllocAmt ?? 0)
        //};

        var baseresult =
        (
            from a in _context.PoolBaseAccounts
            join b in _context.PoolBaseAccounts
                on new
                {
                    AcctId = a.AcctId,
                    OrgId = a.OrgId
                }
                equals new
                {
                    AcctId = b.AllocAcctId,
                    OrgId = b.AllocOrgId
                }

            join pr in _context.PoolRatesCostpoint
                on new
                {
                    AcctId = (string?)b.AcctId,
                    OrgId = (string?)b.OrgId,
                    PoolNo = (int?)b.PoolNo
                }
                equals new
                {
                    AcctId = pr.AcctId,
                    OrgId = pr.OrgId,
                    PoolNo = pr.PoolNo
                }

            where a.PoolNo != b.PoolNo
                  && pr.FyCd == 2025

            group pr by new
            {
                pr.PdNo,
                pr.PoolNo,
                pr.AcctId,
                pr.OrgId,
                AllocAcctId = b.AllocAcctId,
                AllocOrgId = b.AllocOrgId
            }
            into g
            select new
            {
                AllocationGroup = g.Key.AllocOrgId,
                AllocationAccount = g.Key.AllocAcctId,
                PdNo = g.Key.PdNo,
                PoolNo = g.Key.PoolNo,
                AcctId = g.Key.AcctId,
                OrgId = g.Key.OrgId,
                YtdAllocAmount = g.Max(x => x.YtdAllocAmt ?? 0),
                CurrAllocAmount = g.Max(x => x.CurAllocAmt ?? 0),
                BaseAmt = g.Max(x => x.CurBaseAmt) ?? 0
            }).ToList();

        //var baseResultfinal = await _context.PoolRatesCostpoint
        //        .Where(pr =>
        //            pr.PdNo == 11 &&
        //            pr.SAcctTypeCd == "B" &&
        //            _context.PoolBaseAccounts.Any(pba => pba.AcctId == pr.AcctId)
        //        )
        //        .Select(pr => new
        //        {
        //            pr.PdNo,
        //            pr.FyCd,
        //            pr.AcctId,
        //            pr.OrgId,
        //            pr.CurAllocAmt,
        //            pr.YtdAllocAmt
        //        })
        //        .ToListAsync();

        var baseResultfinal = await _context.PoolRatesCostpoint
            .Where(pr =>
                pr.PoolNo == poolno &&
                pr.SAcctTypeCd == "B" &&
                _context.PoolBaseAccounts.Any(pba => pba.AcctId == pr.AcctId)
            )
            .ToListAsync();


        var baseResultfinal1 = await _context.PoolRatesCostpoint
    .Where(pr =>
        pr.PoolNo != poolno &&
        pr.SAcctTypeCd == "B" &&
        _context.PoolBaseAccounts.Any(pba => pba.AcctId == pr.AcctId)
    )
    .ToListAsync();


        //var baseResultfinal = await _context.PoolRatesCostpoint
        //        .Where(pr =>
        //            pr.SAcctTypeCd == "B" && pr.FyCd == Convert.ToInt32(fycd) &&
        //            _context.PoolBaseAccounts.Any(pba => pba.AcctId == pr.AcctId)
        //        )
        //        .Select(pr => new
        //        {
        //            pr.AcctId,
        //            pr.OrgId,
        //            pr.CurAllocAmt,
        //            pr.YtdAllocAmt,
        //            pr.PdNo,
        //            pr.FyCd,
        //            pba.AllocAcctId,
        //            pba.AllocOrgId
        //        })
        //        .ToListAsync();

        //var baseResultfinal =
        //    await (
        //        from pr in _context.PoolRatesCostpoint
        //        join pba in _context.PoolBaseAccounts
        //            on pr.AcctId equals pba.AcctId
        //        where pr.SAcctTypeCd == "B"
        //              && pr.FyCd == Convert.ToInt32(fycd) && pr.PoolNo == poolno
        //        select new
        //        {
        //            pr.AcctId,
        //            pr.OrgId,
        //            pr.CurAllocAmt,
        //            pr.YtdAllocAmt,
        //            pr.PdNo,
        //            pr.FyCd,
        //            //pba.AllocAcctId,
        //            //pba.AllocOrgId
        //        }
        //    ).ToListAsync();





        var baseAccountDetails = poolDetails.Where(p => p.SAcctTypeCd == "B").ToList();
        var costAccountDetails = poolDetails.Where(p => p.SAcctTypeCd == "C").ToList();

        var poolBaseAccounts = _context.PoolBaseAccounts.Where(p => p.PoolNo == poolno).ToList();

        poolOrgFinancialDetail.poolOrgBaseFinancialDetail = new List<PoolOrgBaseFinancialDetail>();
        poolOrgFinancialDetail.poolOrgCostFinancialDetail = new List<PoolOrgCostFinancialDetail>();

        var baseLaborAccountFinancialDetails = _context.PoolBaseAccounts.Where(p => p.PoolNo == poolno).ToList();

        foreach (var baseAccount in baseAccountDetails.Select(p => new { p.AcctId, p.OrgId }).Distinct())
        {
            var allocationDetails = baseLaborAccountFinancialDetails
                                .Where(p => p.PoolNo == poolno && p.AcctId == baseAccount.AcctId && p.OrgId == baseAccount.OrgId)
                                .FirstOrDefault();
            PoolOrgBaseFinancialDetail baseFinancialDetail = new PoolOrgBaseFinancialDetail
            {
                AcctId = baseAccount.AcctId,
                OrgId = baseAccount.OrgId,
                AllocationAcctId = allocationDetails?.AllocAcctId,
                AllocationOrgId = allocationDetails?.AllocOrgId,
                YTDActualAmt = baseAccountDetails.Where(p => p.AcctId == baseAccount.AcctId && p.OrgId == baseAccount.OrgId)?.Sum(q => q.YtdBaseAmt),
                PeriodDetails = new List<PeriodbaseFinancialDetail>()
            };
            if (baseAccount.AcctId == "P0-01F-00D" && baseAccount.OrgId == "1.01.03.01")
            {

            }
            for (int i = 1; i <= 12; i++)
            {



                //baseFinancialDetail.PeriodDetails.Add(new PeriodbaseFinancialDetail
                //{
                //    Period = i,
                //    baseAmt = periodData.CurAmt,
                //    Rate = periodData.CurRt,
                //    AllocationAmt = periodData.CurAllocAmt
                //});
                //var periodData = poolDetails.FirstOrDefault(p => p.PdNo == i && p.AcctId == baseAccount.AcctId && p.OrgId == baseAccount.OrgId && p.SAcctTypeCd == "B");

                int lastDay = DateTime.DaysInMonth(Convert.ToInt16(fycd), i);
                DateOnly date = new DateOnly(Convert.ToInt16(fycd), i, lastDay);
                if (ClosedPeriod >= date)
                {
                    var periodData = baseAccountDetails.FirstOrDefault(p => p.PdNo == i && p.AcctId == baseAccount.AcctId && p.OrgId == baseAccount.OrgId);

                    if (periodData != null)
                    {
                        baseFinancialDetail.PeriodDetails.Add(new PeriodbaseFinancialDetail
                        {
                            Period = i,
                            baseAmt = periodData.CurAmt,
                            Rate = periodData.CurRt,
                            AllocationAmt = periodData.CurAllocAmt
                        });
                    }

                }
                else
                {

                    //var debug = await (
                    //        from pr in _context.PoolRatesCostpoint
                    //        join pba in _context.PoolBaseAccounts
                    //            on pr.AcctId equals pba.AcctId
                    //        where pr.PdNo == 11
                    //           && pr.SAcctTypeCd == "B"
                    //           && pba.AllocAcctId == "P0-01F-00D"
                    //        select new
                    //        {
                    //            pr.AcctId,
                    //            pr.OrgId,
                    //            pba.AllocOrgId
                    //        }
                    //    ).ToListAsync();

                    var debug = await (
                            from pr in _context.PoolRatesCostpoint
                            join pba in _context.PoolBaseAccounts
                                on pr.AcctId equals pba.AcctId
                            where pr.PdNo == 11
                               && pr.SAcctTypeCd == "B"
                               && pba.AllocAcctId == baseAccount.AcctId
                            select new
                            {
                                pr.AcctId,
                                pr.OrgId,
                                pr.PdNo,
                                pr.CurAllocAmt,
                                pr.CurAmt,
                                pr.CurBaseAmt,
                                pr.YtdAllocAmt,
                                pr.YtdAmt,
                                pr.PoolNo,
                                pba.AllocAcctId
                            }
                        ).ToListAsync();


                    if (debug == null || debug.Count() == 0)
                    {
                        var debug1 = await (
                               from pr in _context.PoolRatesCostpoint
                               join pba in _context.PoolBaseAccounts
                                   on pr.AcctId equals pba.AcctId
                               where pr.PdNo == 11
                                  && pr.SAcctTypeCd == "B"
                                  && pba.AcctId == baseAccount.AcctId
                               select new
                               {
                                   pr.AcctId,
                                   pr.OrgId,
                                   pr.PdNo,
                                   pr.CurAllocAmt,
                                   pr.CurAmt,
                                   pr.CurBaseAmt,
                                   pr.YtdAllocAmt,
                                   pr.YtdAmt,
                                   pr.PoolNo
                               }
                           ).ToListAsync();

                        //var data = baseresult.Where(p => p.PdNo == i && p.AcctId == baseAccount.AcctId && p.OrgId == baseAccount.OrgId && p.PoolNo == poolno).ToList();
                        //if (data != null && data.Count() > 0)
                        //{
                        //    PoolRateCostpoint.CurAmt = data.Sum(p => p.CurrAllocAmount);
                        //    PoolRateCostpoint.YtdAmt = data.Sum(p => p.YtdAllocAmount);
                        //    baseFinancialDetail.PeriodDetails.Add(new PeriodbaseFinancialDetail
                        //    {
                        //        Period = i,
                        //        baseAmt = data.Max(p => p.BaseAmt),
                        //        AllocationAmt = data.Max(p => p.CurrAllocAmount)
                        //    });
                        //}
                        decimal allocationAmount = 0;
                        var temp = baseResultfinal.FirstOrDefault(p => p.PdNo == i && p.AcctId == baseAccount.AcctId && p.OrgId == baseAccount.OrgId);
                        if (temp != null)
                        {
                            //allocationAmount = temp.CurAllocAmt.GetValueOrDefault();


                            baseFinancialDetail.PeriodDetails.Add(new PeriodbaseFinancialDetail
                            {
                                Period = i,
                                baseAmt = temp.CurBaseAmt.GetValueOrDefault(),
                                AllocationAmt = temp.CurAllocAmt.GetValueOrDefault(),
                                //BudgetAmt = periodData.Sum(p=>p.TotalForecastedAmt)
                                //Rate = (periodCostData.TotalForecastedAmt / periodData.TotalForecastedAmt) * 100,
                                //AllocationAmt = periodData.TotalForecastedAmt * (periodCostData.TotalForecastedAmt / periodData.TotalForecastedAmt)
                            });
                        }


                    }
                    else
                    {

                        decimal allocationAmount = 0;
                        var temp = baseResultfinal.FirstOrDefault(p => p.PdNo == i && p.AcctId == baseAccount.AcctId && p.OrgId == baseAccount.OrgId);
                        if (temp != null)
                        {
                            //allocationAmount = temp.CurAllocAmt.GetValueOrDefault();


                            baseFinancialDetail.PeriodDetails.Add(new PeriodbaseFinancialDetail
                            {
                                Period = i,
                                baseAmt = temp.CurBaseAmt.GetValueOrDefault(),
                                AllocationAmt = temp.CurAllocAmt.GetValueOrDefault(),
                                //BudgetAmt = periodData.Sum(p=>p.TotalForecastedAmt)
                                //Rate = (periodCostData.TotalForecastedAmt / periodData.TotalForecastedAmt) * 100,
                                //AllocationAmt = periodData.TotalForecastedAmt * (periodCostData.TotalForecastedAmt / periodData.TotalForecastedAmt)
                            });
                        }
                        else
                        {
                            PoolRateCostpoint.CurAmt = 0;
                            PoolRateCostpoint.YtdAmt = 0;
                            baseFinancialDetail.PeriodDetails.Add(new PeriodbaseFinancialDetail
                            {
                                Period = i,
                                baseAmt = 0
                            });
                        }

                    }

                    //Working Query
                    //var currResult = await _context.PoolRatesCostpoint
                    //        .Where(pr =>
                    //            pr.PdNo == 11 &&
                    //            pr.SAcctTypeCd == "B" &&
                    //            _context.PoolBaseAccounts.Any(pba =>
                    //                pba.AcctId == pr.AcctId &&
                    //                pba.AllocAcctId == baseAccount.AcctId &&
                    //                pba.OrgId == baseAccount.OrgId
                    //            )
                    //        )
                    //        .ToListAsync();

                    //PoolRateCostpoint = new PoolRatesCostpoint();
                    //PoolRateCostpoint.PoolNo = poolno;
                    //PoolRateCostpoint.AcctId = baseAccount.AcctId;
                    //PoolRateCostpoint.OrgId = baseAccount.OrgId;
                    //PoolRateCostpoint.PdNo = i;
                    //PoolRateCostpoint.FyCd = Convert.ToInt32(fycd);
                    //PoolRateCostpoint.SAcctTypeCd = "B";

                    //var periodData = BaseForecases.Where(p => p.Month == i);


                    //var periodData = BaseForecases.FirstOrDefault(p => p.Month == i && p.AcctId == baseAccount.AcctId && p.OrgId == baseAccount.OrgId);
                    //var periodData = BaseForecases.FirstOrDefault(p => p.Month == i && p.AcctId == baseAccount.AcctId && p.OrgId == baseAccount.OrgId);
                    //if (periodData != null)
                    {
                        //PoolRateCostpoint.CurAmt = periodData.TotalForecastedAmt;
                        //PoolRateCostpoint.YtdAmt = periodData.TotalForecastedAmt;

                        //*********************************************************************************************************************************************
                        //////Commented for debug pupose

                        var data = baseresult.Where(p => p.PdNo == i && p.AllocationAccount == baseAccount.AcctId && p.OrgId == baseAccount.OrgId).ToList();
                        if (data != null && data.Count() > 0)
                        {
                            PoolRateCostpoint.CurAmt = data.Sum(p => p.CurrAllocAmount);
                            PoolRateCostpoint.YtdAmt = data.Sum(p => p.YtdAllocAmount);
                            //baseFinancialDetail.PeriodDetails.Add(new PeriodbaseFinancialDetail
                            //{
                            //    Period = i,
                            //    baseAmt = data.Sum(p => p.BaseAmt),
                            //    AllocationAmt = data.Sum(p => p.CurrAllocAmount)
                            //});
                        }
                        else
                        {

                            decimal allocationAmount = 0;
                            var temp = baseResultfinal.FirstOrDefault(p => p.PdNo == i && p.AcctId == baseAccount.AcctId && p.OrgId == baseAccount.OrgId);
                            if (temp != null)
                            {
                                //allocationAmount = temp.CurAllocAmt.GetValueOrDefault();


                                //baseFinancialDetail.PeriodDetails.Add(new PeriodbaseFinancialDetail
                                //{
                                //    Period = i,
                                //    baseAmt = temp.CurBaseAmt.GetValueOrDefault(),
                                //    AllocationAmt = temp.CurAllocAmt.GetValueOrDefault(),
                                //});
                            }
                            else
                            {
                                PoolRateCostpoint.CurAmt = 0;
                                PoolRateCostpoint.YtdAmt = 0;
                                //baseFinancialDetail.PeriodDetails.Add(new PeriodbaseFinancialDetail
                                //{
                                //    Period = i,
                                //    baseAmt = 0
                                //});
                            }
                        }




                    }
                    //else
                    {
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        ///
                        if (baseAccount.AcctId == "P0-03H-00D" && baseAccount.OrgId == "1.01.01.04")
                        {

                        }
                        //var temp = baseResultfinal.FirstOrDefault(p => p.PdNo == i && p.AcctId == baseAccount.AcctId && p.OrgId == baseAccount.OrgId);
                        //if (temp != null)
                        //{
                        //    PoolRateCostpoint.CurAmt = temp.CurAmt;
                        //    PoolRateCostpoint.YtdAmt = temp.YtdAllocAmt;
                        //    baseFinancialDetail.PeriodDetails.Add(new PeriodbaseFinancialDetail
                        //    {
                        //        Period = i,
                        //        BudgetAmt = temp.CurAllocAmt
                        //    });
                        //}


                        //if (poolno == 13 || poolno == 17) COmmented FOr Testing
                        //{
                        //    var data = baseresult.Where(p => p.PdNo == i && p.AllocationAccount == baseAccount.AcctId && p.OrgId == baseAccount.OrgId).ToList();
                        //    if (data != null && data.Count() > 0)
                        //    {
                        //        PoolRateCostpoint.CurAmt = data.Sum(p => p.CurrAllocAmount);
                        //        PoolRateCostpoint.YtdAmt = data.Sum(p => p.YtdAllocAmount);
                        //        baseFinancialDetail.PeriodDetails.Add(new PeriodbaseFinancialDetail
                        //        {
                        //            Period = i,
                        //            baseAmt = data.Sum(p => p.BaseAmt),
                        //            AllocationAmt = data.Sum(p => p.CurrAllocAmount)
                        //        });
                        //    }
                        //    else
                        //    {
                        //        PoolRateCostpoint.CurAmt = 0;
                        //        PoolRateCostpoint.YtdAmt = 0;
                        //        baseFinancialDetail.PeriodDetails.Add(new PeriodbaseFinancialDetail
                        //        {
                        //            Period = i,
                        //            baseAmt = 0
                        //        });
                        //    }
                        //}

                    }

                    PoolRatesCostpoint.Add(PoolRateCostpoint);
                }
            }
            baseFinancialDetail.YTDAllocationAmt = baseFinancialDetail.PeriodDetails.Sum(x => x.AllocationAmt);
            poolOrgFinancialDetail.poolOrgBaseFinancialDetail.Add(baseFinancialDetail);
        }

        foreach (var costAccount in costAccountDetails.Select(p => new { p.AcctId, p.OrgId }).Distinct())
        {
            PoolOrgCostFinancialDetail costFinancialDetail = new PoolOrgCostFinancialDetail
            {
                AcctId = costAccount.AcctId,
                OrgId = costAccount.OrgId,
                //YTDActualAmt = costAccountDetails.FirstOrDefault(p=>p.AcctId == costAccount.AcctId && p.OrgId == costAccount.OrgId)?.YtdAmt,
                YTDActualAmt = costAccountDetails.Where(p => p.AcctId == costAccount.AcctId && p.OrgId == costAccount.OrgId)?.Sum(q => q.YtdAmt),

                PeriodDetails = new List<PeriodCostFinancialDetail>()
            };
            for (int k = 1; k <= 12; k++)
            {

                int lastDay = DateTime.DaysInMonth(Convert.ToInt16(fycd), k);
                DateOnly date = new DateOnly(Convert.ToInt16(fycd), k, lastDay);
                if (ClosedPeriod >= date)
                {
                    //var periodData = poolDetails.FirstOrDefault(p => p.PdNo == i && p.AcctId == costAccount.AcctId && p.OrgId == costAccount.OrgId && p.SAcctTypeCd == "C");
                    var periodData = costAccountDetails.FirstOrDefault(p => p.PdNo == k && p.AcctId == costAccount.AcctId && p.OrgId == costAccount.OrgId);
                    if (periodData != null)
                    {
                        costFinancialDetail.PeriodDetails.Add(new PeriodCostFinancialDetail
                        {
                            Period = k,
                            Actualamt = periodData.CurAmt,
                            Rate = periodData.CurRt
                        });
                    }
                }
                else
                {
                    PoolRateCostpoint = new PoolRatesCostpoint();
                    PoolRateCostpoint.PoolNo = poolno;
                    PoolRateCostpoint.SAcctTypeCd = "C";
                    PoolRateCostpoint.AcctId = costAccount.AcctId;
                    PoolRateCostpoint.OrgId = costAccount.OrgId;
                    PoolRateCostpoint.PdNo = k;
                    PoolRateCostpoint.FyCd = Convert.ToInt32(fycd);

                    var periodData = CostForecases.FirstOrDefault(p => p.Month == k && p.AcctId == costAccount.AcctId && p.OrgId == costAccount.OrgId);
                    if (periodData != null)
                    {
                        PoolRateCostpoint.CurAmt = periodData.TotalForecastedAmt;
                        PoolRateCostpoint.YtdAmt = periodData.TotalForecastedAmt;
                        costFinancialDetail.PeriodDetails.Add(new PeriodCostFinancialDetail
                        {
                            Period = k,
                            BudgetedAmt = periodData.TotalForecastedAmt,
                            //Rate = periodData.TotalForecastedAmt / (periodBaseData != null && periodBaseData.TotalForecastedAmt != 0 ? periodBaseData.TotalForecastedAmt : 1) * 100,
                        });
                    }
                    else
                    {
                        if (costAccount.AcctId == "80-901-000" && costAccount.OrgId == "1.01.01.04")
                        {

                        }
                        var data1 = result.Where(p => p.PdNo == k && p.AllocationAccount == costAccount.AcctId && p.OrgId == costAccount.OrgId).ToList();

                        var data = data1.FirstOrDefault();
                        if (data1 != null && data1.Count() > 0)
                        {
                            PoolRateCostpoint.CurAmt = data1.Sum(p => p.CurrAllocAmount);
                            PoolRateCostpoint.YtdAmt = data1.Sum(p => p.YtdAllocAmount);
                            costFinancialDetail.PeriodDetails.Add(new PeriodCostFinancialDetail
                            {
                                Period = k,
                                BudgetedAmt = data1.Sum(p => p.CurrAllocAmount),
                                //BudgetAmt = periodData.Sum(p=>p.TotalForecastedAmt)
                                //Rate = (periodCostData.TotalForecastedAmt / periodData.TotalForecastedAmt) * 100,
                                //AllocationAmt = periodData.TotalForecastedAmt * (periodCostData.TotalForecastedAmt / periodData.TotalForecastedAmt)
                            });
                        }
                        else
                        {
                            PoolRateCostpoint.CurAmt = 0;
                            PoolRateCostpoint.YtdAmt = 0;
                            costFinancialDetail.PeriodDetails.Add(new PeriodCostFinancialDetail
                            {
                                Period = k,
                                BudgetedAmt = 0
                                //BudgetAmt = periodData.Sum(p=>p.TotalForecastedAmt)
                                //Rate = (periodCostData.TotalForecastedAmt / periodData.TotalForecastedAmt) * 100,
                                //AllocationAmt = periodData.TotalForecastedAmt * (periodCostData.TotalForecastedAmt / periodData.TotalForecastedAmt)
                            });
                        }
                    }
                    PoolRatesCostpoint.Add(PoolRateCostpoint);
                }
            }
            poolOrgFinancialDetail.poolOrgCostFinancialDetail.Add(costFinancialDetail);
        }

        poolOrgFinancialDetail.TotalYTDBaseActualAmt = poolOrgFinancialDetail.poolOrgBaseFinancialDetail.Sum(x => x.YTDActualAmt);
        poolOrgFinancialDetail.TotalYTDCostActualAmt = poolOrgFinancialDetail.poolOrgCostFinancialDetail.Sum(x => x.YTDActualAmt);

        poolOrgFinancialDetail.TotalYTDBaseAllocationActualAmt = poolOrgFinancialDetail.poolOrgBaseFinancialDetail
            .SelectMany(x => x.PeriodDetails)
            .Sum(x => x.AllocationAmt);

        poolOrgFinancialDetail.Rate =
            poolOrgFinancialDetail.TotalYTDBaseActualAmt != 0
                ? Math.Round(
                    (poolOrgFinancialDetail.TotalYTDCostActualAmt.GetValueOrDefault() / poolOrgFinancialDetail.TotalYTDBaseActualAmt.GetValueOrDefault()) * 100,
                    4
                  )
                : 0;

        ////////////////////////////////////////////////////////////////////////////////

        //foreach (var PoolRate in PoolRatesCostpoint)
        //{

        //    var baseAmount = poolOrgFinancialDetail.poolOrgBaseFinancialDetail.FirstOrDefault(p => p.AcctId == PoolRate.AcctId && p.OrgId == PoolRate.OrgId)?
        //        .PeriodDetails.FirstOrDefault(q => q.Period == PoolRate.PdNo)?.BudgetAmt;

        //    var baseTotalAmount =
        //        poolOrgFinancialDetail.poolOrgBaseFinancialDetail
        //            //.Where(p => p.AcctId == PoolRate.AcctId
        //            //         && p.OrgId == PoolRate.OrgId)
        //            .SelectMany(p => p.PeriodDetails).Where(p => p.Period == PoolRate.PdNo).Sum(q => q.BudgetAmt);
        //    var costAmount =
        //        poolOrgFinancialDetail.poolOrgCostFinancialDetail
        //            //.Where(p => p.AcctId == PoolRate.AcctId
        //            //         && p.OrgId == PoolRate.OrgId)
        //            .SelectMany(p => p.PeriodDetails).Where(p => p.Period == PoolRate.PdNo).Sum(q => q.BudgetedAmt);

        //    //var ytdAmount = poolDetails.FirstOrDefault(p => p.SAcctTypeCd == PoolRate.SAcctTypeCd && p.AcctId == PoolRate.AcctId && p.OrgId == PoolRate.OrgId && p.PdNo == PoolRate.PdNo - 1).YtdAmt + baseAmount??0;
        //    var ytdAmount =
        //            (poolDetails.FirstOrDefault(p =>
        //                p.SAcctTypeCd == PoolRate.SAcctTypeCd &&
        //                p.AcctId == PoolRate.AcctId &&
        //                p.OrgId == PoolRate.OrgId &&
        //                p.PdNo == PoolRate.PdNo - 1
        //            )?.YtdAmt ?? 0)
        //            + (baseAmount ?? 0);

        //    PoolRate.CurAmt = baseAmount;
        //    PoolRate.YtdAmt = ytdAmount;
        //    if (PoolRate.SAcctTypeCd == "B")
        //    {
        //        //PoolRate.CurRt = (costAmount / baseTotalAmount) * 100;

        //        PoolRate.YtdRt = poolOrgFinancialDetail.Rate;

        //        PoolRate.CurAllocAmt = (baseAmount * PoolRate.CurRt) / 100;
        //        PoolRate.YtdAllocAmt = (ytdAmount * PoolRate.YtdRt) / 100;

        //        PoolRate.CurBaseAmt = baseAmount;
        //        PoolRate.YtdBaseAmt = ytdAmount;
        //    }
        //    poolDetails.Add(PoolRate);
        //    //var existingRate = await _context.PoolRatesCostpoint.FirstOrDefaultAsync(p => p.FyCd == PoolRate.FyCd && p.PdNo == PoolRate.PdNo && p.PoolNo == PoolRate.PoolNo && p.AcctId == PoolRate.AcctId && p.OrgId == PoolRate.OrgId && p.SAcctTypeCd == PoolRate.SAcctTypeCd);
        //    //if (existingRate != null)
        //    //{
        //    //    // Update existing record
        //    //    existingRate.CurAmt = PoolRate.CurAmt;
        //    //    existingRate.YtdAmt = PoolRate.YtdAmt;
        //    //    // Update other fields as necessary
        //    //}
        //    //else
        //    //{
        //    //    // Insert new record
        //    //    _context.PoolRatesCostpoint.Add(PoolRate);
        //    //}
        //}
        //await _context.BulkInsertOrUpdateAsync(
        //            PoolRatesCostpoint,
        //            new BulkConfig
        //            {
        //                PreserveInsertOrder = true,
        //                SetOutputIdentity = false,
        //                BatchSize = 5000
        //            });

        //_context.PoolRatesCostpoint.AddRange(PoolRatesCostpoint);
        //_context.SaveChanges();
        /////////////////////////////////////////////////////////////////////////////


        foreach (var costDetail in poolOrgFinancialDetail.poolOrgCostFinancialDetail)
        {
            var account = chartOfAccounts.FirstOrDefault(c => c.AccountId == costDetail.AcctId);
            if (account != null)
            {
                costDetail.AccountName = account.AccountName;
            }
        }
        foreach (var baseDetail in poolOrgFinancialDetail.poolOrgBaseFinancialDetail)
        {

            var account = chartOfAccounts.FirstOrDefault(c => c.AccountId == baseDetail.AcctId);
            if (account != null)
            {
                baseDetail.AccountName = account.AccountName;
            }
            //foreach (var periodDetail in baseDetail.PeriodDetails)
            //{
            //    var temp = PoolRatesCostpoint.FirstOrDefault(p => p.AcctId == baseDetail.AcctId && p.OrgId == baseDetail.OrgId && p.PdNo == periodDetail.Period);
            //    if (temp != null)
            //    {
            //        periodDetail.baseAmt = temp.CurAmt;
            //        periodDetail.AllocationAmt = temp.CurAllocAmt;
            //        periodDetail.Rate = temp.CurRt;
            //    }
            //    var data = result.FirstOrDefault(p => p.PdNo == periodDetail.Period && p.AllocationAccount == baseDetail.AcctId && p.AllocationGroup == baseDetail.OrgId);
            //    if (data != null)
            //    {
            //        periodDetail.baseAmt = temp.CurAmt;
            //        periodDetail.AllocationAmt = temp.CurAllocAmt;
            //        periodDetail.Rate = temp.CurRt;
            //    }

            //}
        }
        var tempfdf = poolOrgFinancialDetail.poolOrgCostFinancialDetail.Where(p => p.AcctId == "P0-01F-07G").ToList();

        return Ok(poolOrgFinancialDetail);


    }

    [HttpGet("CalculateRatesFromSP")]
    public async Task<IActionResult> CalculateRatesFromSP(string fycd, string type)
    {
        await _context.Database.ExecuteSqlRawAsync(
                    "CALL transfer_budget(@p_budget_id, @p_user_id, @p_remarks)",
                    new[]
                    {
                        new NpgsqlParameter("p_budget_id", fycd),
                        new NpgsqlParameter("p_user_id", type),
                        new NpgsqlParameter("p_remarks", fycd)
                    });

        var parameters = new[]
            {
                new NpgsqlParameter("p_status", "TRANSFERRED"),
                new NpgsqlParameter("p_start_date", DateTime.Now),
                new NpgsqlParameter("p_end_date", DateTime.Now),
                new NpgsqlParameter("p_org_id", fycd)
            };

        var result = await _context.NewBusinessBudgets
            .FromSqlRaw(
                "CALL get_budgets_by_filter(@p_status, @p_start_date, @p_end_date, @p_org_id)",
                parameters)
            .AsNoTracking()
            .ToListAsync();


        return Ok();
    }


    [HttpGet("GetHR_Last_Working")]
    public async Task<IActionResult> GetPoolValuesV2_Last_Working(string fycd, string type)
    {


        RateCalculator rateCalculator = new RateCalculator(_context);

        PoolOrgFinancialDetail poolOrgFinancialDetail = new PoolOrgFinancialDetail();
        PoolOrgFinancialDetail poolOrgFringeDetail = new PoolOrgFinancialDetail();
        List<PoolRatesCostpoint> PoolRatesCostpoint = new List<PoolRatesCostpoint>();
        PoolRatesCostpoint PoolRateCostpoint = new PoolRatesCostpoint();
        int poolno = 0;
        switch (type.ToUpper())
        {
            case "OVERHEAD":
                poolno = 13;
                break;
            case "GNA":
                poolno = 17;
                break;
            case "FRINGE":
                poolno = 11;
                break;
            case "HR":
                poolno = 12;
                break;
            case "MNH":
                poolno = 16;
                break;
            default:
                poolno = 0;
                break;
        }
        var ClosedPeriod = DateOnly.Parse(_context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "closing_period").Value);
        var BaseForecases = rateCalculator.GetBaseForecasts(fycd, poolno);
        var CostForecases = rateCalculator.GetCostForecasts(fycd, poolno);

        //return Ok();
        var chartOfAccounts = _context.Charts_Of_Accounts.ToList();


        var poolDetails = await _context.PoolRatesCostpoint
            .Where(x => x.PoolNo == poolno && x.FyCd == Convert.ToInt32(fycd))
            .GroupBy(x => new
            {
                x.FyCd,
                x.PdNo,
                x.PoolNo,
                x.SAcctTypeCd,
                x.AcctId,
                x.OrgId
            })
            .Select(g => new PoolRatesCostpoint
            {
                FyCd = g.Key.FyCd,
                PdNo = g.Key.PdNo,
                PoolNo = g.Key.PoolNo,
                SAcctTypeCd = g.Key.SAcctTypeCd,
                AcctId = g.Key.AcctId,
                OrgId = g.Key.OrgId,

                CurAmt = g.Sum(x => x.CurAmt ?? 0),
                YtdAmt = g.Sum(x => x.YtdAmt ?? 0),
                CurAllocAmt = g.Sum(x => x.CurAllocAmt ?? 0),
                YtdAllocAmt = g.Sum(x => x.YtdAllocAmt ?? 0),
                CurRt = g.Sum(x => x.CurRt ?? 0),
                YtdRt = g.Sum(x => x.YtdRt ?? 0),
                CurBudAmt = g.Sum(x => x.CurBudAmt ?? 0),
                YtdBudAmt = g.Sum(x => x.YtdBudAmt ?? 0),
                CurBudAllocAmt = g.Sum(x => x.CurBudAllocAmt ?? 0),
                YtdBudAllocAmt = g.Sum(x => x.YtdBudAllocAmt ?? 0),
                CurBudRt = g.Sum(x => x.CurBudRt ?? 0),
                YtdBudRt = g.Sum(x => x.YtdBudRt ?? 0),
                CurBaseAmt = g.Sum(x => x.CurBaseAmt ?? 0),
                YtdBaseAmt = g.Sum(x => x.YtdBaseAmt ?? 0)
            })
            .OrderBy(x => x.FyCd)
            .ThenBy(x => x.PdNo)
            .ThenBy(x => x.PoolNo)
            .ThenBy(x => x.AcctId)
            .ThenBy(x => x.OrgId)
            .ToListAsync();

        //    var result =
        //from pss in _context.PoolRatesCostpoint
        //join pba in _context.PoolBaseAccounts
        //    on new { pss.AcctId, pss.OrgId }
        //    equals new { AcctId = pba.AcctId, OrgId = pba.OrgId }
        //where pss.SAcctTypeCd == "B"
        //   && pss.FyCd == Convert.ToInt32(fycd)
        //   && _context.PoolCostAccounts
        //        .Any(pca =>
        //            pca.AcctId == pba.AllocAcctId &&
        //            pca.OrgId == pba.AllocOrgId &&
        //            pca.PoolNo == poolno)
        //group pss by new
        //{
        //    pss.PdNo,
        //    pss.PoolNo,
        //    pss.AcctId,
        //    pss.OrgId,
        //    pba.AllocAcctId,
        //    pba.AllocOrgId

        //}
        //into g
        //select new
        //{
        //    AllocationGroup = g.Key.AllocOrgId,
        //    AllocationAccount = g.Key.AllocAcctId,
        //    PdNo = g.Key.PdNo,
        //    PoolNo = g.Key.PoolNo,
        //    AcctId = g.Key.AcctId,
        //    OrgId = g.Key.OrgId,
        //    YtdAllocAmount = g.Sum(x => x.YtdAllocAmt),
        //    CurrAllocAmount = g.Sum(x => x.CurAllocAmt)
        //};


        var result =
            from pss in _context.PoolRatesCostpoint
            from pba in _context.PoolBaseAccounts
            where pss.AcctId == pba.AcctId
               && pss.OrgId == pba.OrgId
               && pss.PoolNo == pba.PoolNo
            join pca in _context.PoolCostAccounts
                on new
                {
                    AcctId = pba.AllocAcctId,
                    OrgId = pba.AllocOrgId
                }
                equals new
                {
                    AcctId = pca.AcctId,
                    OrgId = pca.OrgId
                }
            where pss.SAcctTypeCd == "B"
               && pss.FyCd == Convert.ToInt32(fycd)
               && pca.PoolNo == poolno
            group pss by new
            {
                pss.PdNo,
                pss.PoolNo,
                pss.AcctId,
                pss.OrgId,
                AllocAcctId = pba.AllocAcctId,
                AllocOrgId = pba.AllocOrgId
            }
            into g
            select new
            {
                AllocationGroup = g.Key.AllocOrgId,
                AllocationAccount = g.Key.AllocAcctId,
                PdNo = g.Key.PdNo,
                PoolNo = g.Key.PoolNo,
                AcctId = g.Key.AcctId,
                OrgId = g.Key.OrgId,
                YtdAllocAmount = g.Sum(x => x.YtdAllocAmt ?? 0),
                CurrAllocAmount = g.Sum(x => x.CurAllocAmt ?? 0)
            };

        //    var baseresult =
        //from pss in _context.PoolRatesCostpoint
        //from pba in _context.PoolBaseAccounts
        //where pss.AcctId == pba.AcctId
        //   && pss.OrgId == pba.OrgId
        //   && pss.PoolNo == pba.PoolNo
        //join pca in _context.PoolBaseAccounts
        //    on new
        //    {
        //        AcctId = pba.AllocAcctId,
        //        OrgId = pba.AllocOrgId
        //    }
        //    equals new
        //    {
        //        AcctId = pca.AcctId,
        //        OrgId = pca.OrgId
        //    }
        //where pss.SAcctTypeCd == "B"
        //   && pss.FyCd == Convert.ToInt32(fycd)
        //   && pca.PoolNo == poolno
        //group pss by new
        //{
        //    pss.PdNo,
        //    pss.PoolNo,
        //    pss.AcctId,
        //    pss.OrgId,
        //    AllocAcctId = pba.AllocAcctId,
        //    AllocOrgId = pba.AllocOrgId
        //}
        //into g
        //select new
        //{
        //    AllocationGroup = g.Key.AllocOrgId,
        //    AllocationAccount = g.Key.AllocAcctId,
        //    PdNo = g.Key.PdNo,
        //    PoolNo = g.Key.PoolNo,
        //    AcctId = g.Key.AcctId,
        //    OrgId = g.Key.OrgId,
        //    YtdAllocAmount = g.Sum(x => x.YtdAllocAmt ?? 0),
        //    CurrAllocAmount = g.Sum(x => x.CurAllocAmt ?? 0)
        //};

        var baseresult =
        (
            from a in _context.PoolBaseAccounts
            join b in _context.PoolBaseAccounts
                on new
                {
                    AcctId = a.AcctId,
                    OrgId = a.OrgId
                }
                equals new
                {
                    AcctId = b.AllocAcctId,
                    OrgId = b.AllocOrgId
                }

            join pr in _context.PoolRatesCostpoint
                on new
                {
                    AcctId = (string?)b.AcctId,
                    OrgId = (string?)b.OrgId,
                    PoolNo = (int?)b.PoolNo
                }
                equals new
                {
                    AcctId = pr.AcctId,
                    OrgId = pr.OrgId,
                    PoolNo = pr.PoolNo
                }

            where a.PoolNo != b.PoolNo
                  && pr.FyCd == 2025

            group pr by new
            {
                pr.PdNo,
                pr.PoolNo,
                pr.AcctId,
                pr.OrgId,
                AllocAcctId = b.AllocAcctId,
                AllocOrgId = b.AllocOrgId
            }
            into g
            select new
            {
                AllocationGroup = g.Key.AllocOrgId,
                AllocationAccount = g.Key.AllocAcctId,
                PdNo = g.Key.PdNo,
                PoolNo = g.Key.PoolNo,
                AcctId = g.Key.AcctId,
                OrgId = g.Key.OrgId,
                YtdAllocAmount = g.Sum(x => x.YtdAllocAmt ?? 0),
                CurrAllocAmount = g.Sum(x => x.CurAllocAmt ?? 0)
            }).ToList();

        //var baseResultfinal = await _context.PoolRatesCostpoint
        //        .Where(pr =>
        //            pr.PdNo == 11 &&
        //            pr.SAcctTypeCd == "B" &&
        //            _context.PoolBaseAccounts.Any(pba => pba.AcctId == pr.AcctId)
        //        )
        //        .Select(pr => new
        //        {
        //            pr.PdNo,
        //            pr.FyCd,
        //            pr.AcctId,
        //            pr.OrgId,
        //            pr.CurAllocAmt,
        //            pr.YtdAllocAmt
        //        })
        //        .ToListAsync();

        var baseResultfinal = await _context.PoolRatesCostpoint
            .Where(pr =>
                pr.PoolNo == poolno &&
                pr.SAcctTypeCd == "B" &&
                _context.PoolBaseAccounts.Any(pba => pba.AcctId == pr.AcctId)
            )
            .ToListAsync();


        //var baseResultfinal = await _context.PoolRatesCostpoint
        //        .Where(pr =>
        //            pr.SAcctTypeCd == "B" && pr.FyCd == Convert.ToInt32(fycd) &&
        //            _context.PoolBaseAccounts.Any(pba => pba.AcctId == pr.AcctId)
        //        )
        //        .Select(pr => new
        //        {
        //            pr.AcctId,
        //            pr.OrgId,
        //            pr.CurAllocAmt,
        //            pr.YtdAllocAmt,
        //            pr.PdNo,
        //            pr.FyCd,
        //            pba.AllocAcctId,
        //            pba.AllocOrgId
        //        })
        //        .ToListAsync();

        //var baseResultfinal =
        //    await (
        //        from pr in _context.PoolRatesCostpoint
        //        join pba in _context.PoolBaseAccounts
        //            on pr.AcctId equals pba.AcctId
        //        where pr.SAcctTypeCd == "B"
        //              && pr.FyCd == Convert.ToInt32(fycd) && pr.PoolNo == poolno
        //        select new
        //        {
        //            pr.AcctId,
        //            pr.OrgId,
        //            pr.CurAllocAmt,
        //            pr.YtdAllocAmt,
        //            pr.PdNo,
        //            pr.FyCd,
        //            //pba.AllocAcctId,
        //            //pba.AllocOrgId
        //        }
        //    ).ToListAsync();





        var baseAccountDetails = poolDetails.Where(p => p.SAcctTypeCd == "B").ToList();
        var costAccountDetails = poolDetails.Where(p => p.SAcctTypeCd == "C").ToList();

        var poolBaseAccounts = _context.PoolBaseAccounts.Where(p => p.PoolNo == poolno).ToList();

        poolOrgFinancialDetail.poolOrgBaseFinancialDetail = new List<PoolOrgBaseFinancialDetail>();
        poolOrgFinancialDetail.poolOrgCostFinancialDetail = new List<PoolOrgCostFinancialDetail>();

        var baseLaborAccountFinancialDetails = _context.PoolBaseAccounts.Where(p => p.PoolNo == poolno).ToList();

        foreach (var baseAccount in baseAccountDetails.Select(p => new { p.AcctId, p.OrgId }).Distinct())
        {
            var allocationDetails = baseLaborAccountFinancialDetails
                                .Where(p => p.PoolNo == poolno && p.AcctId == baseAccount.AcctId && p.OrgId == baseAccount.OrgId)
                                .FirstOrDefault();
            PoolOrgBaseFinancialDetail baseFinancialDetail = new PoolOrgBaseFinancialDetail
            {
                AcctId = baseAccount.AcctId,
                OrgId = baseAccount.OrgId,
                AllocationAcctId = allocationDetails?.AllocAcctId,
                AllocationOrgId = allocationDetails?.AllocOrgId,
                YTDActualAmt = baseAccountDetails.Where(p => p.AcctId == baseAccount.AcctId && p.OrgId == baseAccount.OrgId)?.Sum(q => q.YtdBaseAmt),
                PeriodDetails = new List<PeriodbaseFinancialDetail>()
            };
            if (baseAccount.AcctId == "80-901-000" && baseAccount.OrgId == "1.01.01.04")
            {

            }
            for (int i = 1; i <= 12; i++)
            {



                //baseFinancialDetail.PeriodDetails.Add(new PeriodbaseFinancialDetail
                //{
                //    Period = i,
                //    baseAmt = periodData.CurAmt,
                //    Rate = periodData.CurRt,
                //    AllocationAmt = periodData.CurAllocAmt
                //});
                //var periodData = poolDetails.FirstOrDefault(p => p.PdNo == i && p.AcctId == baseAccount.AcctId && p.OrgId == baseAccount.OrgId && p.SAcctTypeCd == "B");

                int lastDay = DateTime.DaysInMonth(Convert.ToInt16(fycd), i);
                DateOnly date = new DateOnly(Convert.ToInt16(fycd), i, lastDay);
                if (ClosedPeriod >= date)
                {
                    var periodData = baseAccountDetails.FirstOrDefault(p => p.PdNo == i && p.AcctId == baseAccount.AcctId && p.OrgId == baseAccount.OrgId);

                    if (periodData != null)
                    {
                        baseFinancialDetail.PeriodDetails.Add(new PeriodbaseFinancialDetail
                        {
                            Period = i,
                            baseAmt = periodData.CurAmt,
                            Rate = periodData.CurRt,
                            AllocationAmt = periodData.CurAllocAmt
                        });
                    }

                }
                else
                {
                    //PoolRateCostpoint = new PoolRatesCostpoint();
                    //PoolRateCostpoint.PoolNo = poolno;
                    //PoolRateCostpoint.AcctId = baseAccount.AcctId;
                    //PoolRateCostpoint.OrgId = baseAccount.OrgId;
                    //PoolRateCostpoint.PdNo = i;
                    //PoolRateCostpoint.FyCd = Convert.ToInt32(fycd);
                    //PoolRateCostpoint.SAcctTypeCd = "B";

                    //var periodData = BaseForecases.Where(p => p.Month == i);


                    //var periodData = BaseForecases.FirstOrDefault(p => p.Month == i && p.AcctId == baseAccount.AcctId && p.OrgId == baseAccount.OrgId);
                    //var periodData = BaseForecases.FirstOrDefault(p => p.Month == i && p.AcctId == baseAccount.AcctId && p.OrgId == baseAccount.OrgId);
                    //if (periodData != null)
                    {
                        //PoolRateCostpoint.CurAmt = periodData.TotalForecastedAmt;
                        //PoolRateCostpoint.YtdAmt = periodData.TotalForecastedAmt;
                        decimal allocationAmount = 0;
                        var temp = baseResultfinal.FirstOrDefault(p => p.PdNo == i && p.AcctId == baseAccount.AcctId && p.OrgId == baseAccount.OrgId);
                        if (temp != null)
                        {
                            //allocationAmount = temp.CurAllocAmt.GetValueOrDefault();


                            baseFinancialDetail.PeriodDetails.Add(new PeriodbaseFinancialDetail
                            {
                                Period = i,
                                baseAmt = temp.CurBaseAmt.GetValueOrDefault(),
                                AllocationAmt = temp.CurAllocAmt.GetValueOrDefault(),
                                //BudgetAmt = periodData.Sum(p=>p.TotalForecastedAmt)
                                //Rate = (periodCostData.TotalForecastedAmt / periodData.TotalForecastedAmt) * 100,
                                //AllocationAmt = periodData.TotalForecastedAmt * (periodCostData.TotalForecastedAmt / periodData.TotalForecastedAmt)
                            });
                        }
                    }
                    //else
                    {
                        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
                        ///
                        if (baseAccount.AcctId == "80-901-000" && baseAccount.OrgId == "1.01.01.04")
                        {

                        }
                        //var temp = baseResultfinal.FirstOrDefault(p => p.PdNo == i && p.AcctId == baseAccount.AcctId && p.OrgId == baseAccount.OrgId);
                        //if (temp != null)
                        //{
                        //    PoolRateCostpoint.CurAmt = temp.CurAmt;
                        //    PoolRateCostpoint.YtdAmt = temp.YtdAllocAmt;
                        //    baseFinancialDetail.PeriodDetails.Add(new PeriodbaseFinancialDetail
                        //    {
                        //        Period = i,
                        //        BudgetAmt = temp.CurAllocAmt
                        //    });
                        //}


                        if (poolno == 13 || poolno == 17)
                        {
                            var data = baseresult.Where(p => p.PdNo == i && p.AllocationAccount == baseAccount.AcctId && p.OrgId == baseAccount.OrgId).ToList();
                            if (data != null && data.Count() > 0)
                            {
                                PoolRateCostpoint.CurAmt = data.Sum(p => p.CurrAllocAmount);
                                PoolRateCostpoint.YtdAmt = data.Sum(p => p.YtdAllocAmount);
                                baseFinancialDetail.PeriodDetails.Add(new PeriodbaseFinancialDetail
                                {
                                    Period = i,
                                    baseAmt = data.Sum(p => p.CurrAllocAmount),
                                });
                            }
                            else
                            {
                                PoolRateCostpoint.CurAmt = 0;
                                PoolRateCostpoint.YtdAmt = 0;
                                baseFinancialDetail.PeriodDetails.Add(new PeriodbaseFinancialDetail
                                {
                                    Period = i,
                                    baseAmt = 0
                                });
                            }
                        }

                    }

                    PoolRatesCostpoint.Add(PoolRateCostpoint);
                }
            }
            baseFinancialDetail.YTDAllocationAmt = baseFinancialDetail.PeriodDetails.Sum(x => x.AllocationAmt);
            poolOrgFinancialDetail.poolOrgBaseFinancialDetail.Add(baseFinancialDetail);
        }

        foreach (var costAccount in costAccountDetails.Select(p => new { p.AcctId, p.OrgId }).Distinct())
        {
            PoolOrgCostFinancialDetail costFinancialDetail = new PoolOrgCostFinancialDetail
            {
                AcctId = costAccount.AcctId,
                OrgId = costAccount.OrgId,
                //YTDActualAmt = costAccountDetails.FirstOrDefault(p=>p.AcctId == costAccount.AcctId && p.OrgId == costAccount.OrgId)?.YtdAmt,
                YTDActualAmt = costAccountDetails.Where(p => p.AcctId == costAccount.AcctId && p.OrgId == costAccount.OrgId)?.Sum(q => q.YtdAmt),

                PeriodDetails = new List<PeriodCostFinancialDetail>()
            };
            for (int k = 1; k <= 12; k++)
            {

                int lastDay = DateTime.DaysInMonth(Convert.ToInt16(fycd), k);
                DateOnly date = new DateOnly(Convert.ToInt16(fycd), k, lastDay);
                if (ClosedPeriod >= date)
                {
                    //var periodData = poolDetails.FirstOrDefault(p => p.PdNo == i && p.AcctId == costAccount.AcctId && p.OrgId == costAccount.OrgId && p.SAcctTypeCd == "C");
                    var periodData = costAccountDetails.FirstOrDefault(p => p.PdNo == k && p.AcctId == costAccount.AcctId && p.OrgId == costAccount.OrgId);
                    if (periodData != null)
                    {
                        costFinancialDetail.PeriodDetails.Add(new PeriodCostFinancialDetail
                        {
                            Period = k,
                            Actualamt = periodData.CurAmt,
                            Rate = periodData.CurRt
                        });
                    }
                }
                else
                {
                    PoolRateCostpoint = new PoolRatesCostpoint();
                    PoolRateCostpoint.PoolNo = poolno;
                    PoolRateCostpoint.SAcctTypeCd = "C";
                    PoolRateCostpoint.AcctId = costAccount.AcctId;
                    PoolRateCostpoint.OrgId = costAccount.OrgId;
                    PoolRateCostpoint.PdNo = k;
                    PoolRateCostpoint.FyCd = Convert.ToInt32(fycd);

                    var periodData = CostForecases.FirstOrDefault(p => p.Month == k && p.AcctId == costAccount.AcctId && p.OrgId == costAccount.OrgId);
                    if (periodData != null)
                    {
                        PoolRateCostpoint.CurAmt = periodData.TotalForecastedAmt;
                        PoolRateCostpoint.YtdAmt = periodData.TotalForecastedAmt;
                        costFinancialDetail.PeriodDetails.Add(new PeriodCostFinancialDetail
                        {
                            Period = k,
                            BudgetedAmt = periodData.TotalForecastedAmt,
                            //Rate = periodData.TotalForecastedAmt / (periodBaseData != null && periodBaseData.TotalForecastedAmt != 0 ? periodBaseData.TotalForecastedAmt : 1) * 100,
                        });
                    }
                    else
                    {
                        if (costAccount.AcctId == "80-901-000" && costAccount.OrgId == "1.01.01.04")
                        {

                        }
                        var data1 = result.Where(p => p.PdNo == k && p.AllocationAccount == costAccount.AcctId && p.OrgId == costAccount.OrgId).ToList();

                        var data = data1.FirstOrDefault();
                        if (data1 != null && data1.Count() > 0)
                        {
                            PoolRateCostpoint.CurAmt = data1.Sum(p => p.CurrAllocAmount);
                            PoolRateCostpoint.YtdAmt = data1.Sum(p => p.YtdAllocAmount);
                            costFinancialDetail.PeriodDetails.Add(new PeriodCostFinancialDetail
                            {
                                Period = k,
                                BudgetedAmt = data1.Sum(p => p.CurrAllocAmount),
                                //BudgetAmt = periodData.Sum(p=>p.TotalForecastedAmt)
                                //Rate = (periodCostData.TotalForecastedAmt / periodData.TotalForecastedAmt) * 100,
                                //AllocationAmt = periodData.TotalForecastedAmt * (periodCostData.TotalForecastedAmt / periodData.TotalForecastedAmt)
                            });
                        }
                        else
                        {
                            PoolRateCostpoint.CurAmt = 0;
                            PoolRateCostpoint.YtdAmt = 0;
                            costFinancialDetail.PeriodDetails.Add(new PeriodCostFinancialDetail
                            {
                                Period = k,
                                BudgetedAmt = 0
                                //BudgetAmt = periodData.Sum(p=>p.TotalForecastedAmt)
                                //Rate = (periodCostData.TotalForecastedAmt / periodData.TotalForecastedAmt) * 100,
                                //AllocationAmt = periodData.TotalForecastedAmt * (periodCostData.TotalForecastedAmt / periodData.TotalForecastedAmt)
                            });
                        }
                    }
                    PoolRatesCostpoint.Add(PoolRateCostpoint);
                }
            }
            poolOrgFinancialDetail.poolOrgCostFinancialDetail.Add(costFinancialDetail);
        }

        poolOrgFinancialDetail.TotalYTDBaseActualAmt = poolOrgFinancialDetail.poolOrgBaseFinancialDetail.Sum(x => x.YTDActualAmt);
        poolOrgFinancialDetail.TotalYTDCostActualAmt = poolOrgFinancialDetail.poolOrgCostFinancialDetail.Sum(x => x.YTDActualAmt);

        poolOrgFinancialDetail.TotalYTDBaseAllocationActualAmt = poolOrgFinancialDetail.poolOrgBaseFinancialDetail
            .SelectMany(x => x.PeriodDetails)
            .Sum(x => x.AllocationAmt);

        poolOrgFinancialDetail.Rate =
            poolOrgFinancialDetail.TotalYTDBaseActualAmt != 0
                ? Math.Round(
                    (poolOrgFinancialDetail.TotalYTDCostActualAmt.GetValueOrDefault() / poolOrgFinancialDetail.TotalYTDBaseActualAmt.GetValueOrDefault()) * 100,
                    4
                  )
                : 0;

        ////////////////////////////////////////////////////////////////////////////////

        //foreach (var PoolRate in PoolRatesCostpoint)
        //{

        //    var baseAmount = poolOrgFinancialDetail.poolOrgBaseFinancialDetail.FirstOrDefault(p => p.AcctId == PoolRate.AcctId && p.OrgId == PoolRate.OrgId)?
        //        .PeriodDetails.FirstOrDefault(q => q.Period == PoolRate.PdNo)?.BudgetAmt;

        //    var baseTotalAmount =
        //        poolOrgFinancialDetail.poolOrgBaseFinancialDetail
        //            //.Where(p => p.AcctId == PoolRate.AcctId
        //            //         && p.OrgId == PoolRate.OrgId)
        //            .SelectMany(p => p.PeriodDetails).Where(p => p.Period == PoolRate.PdNo).Sum(q => q.BudgetAmt);
        //    var costAmount =
        //        poolOrgFinancialDetail.poolOrgCostFinancialDetail
        //            //.Where(p => p.AcctId == PoolRate.AcctId
        //            //         && p.OrgId == PoolRate.OrgId)
        //            .SelectMany(p => p.PeriodDetails).Where(p => p.Period == PoolRate.PdNo).Sum(q => q.BudgetedAmt);

        //    //var ytdAmount = poolDetails.FirstOrDefault(p => p.SAcctTypeCd == PoolRate.SAcctTypeCd && p.AcctId == PoolRate.AcctId && p.OrgId == PoolRate.OrgId && p.PdNo == PoolRate.PdNo - 1).YtdAmt + baseAmount??0;
        //    var ytdAmount =
        //            (poolDetails.FirstOrDefault(p =>
        //                p.SAcctTypeCd == PoolRate.SAcctTypeCd &&
        //                p.AcctId == PoolRate.AcctId &&
        //                p.OrgId == PoolRate.OrgId &&
        //                p.PdNo == PoolRate.PdNo - 1
        //            )?.YtdAmt ?? 0)
        //            + (baseAmount ?? 0);

        //    PoolRate.CurAmt = baseAmount;
        //    PoolRate.YtdAmt = ytdAmount;
        //    if (PoolRate.SAcctTypeCd == "B")
        //    {
        //        //PoolRate.CurRt = (costAmount / baseTotalAmount) * 100;

        //        PoolRate.YtdRt = poolOrgFinancialDetail.Rate;

        //        PoolRate.CurAllocAmt = (baseAmount * PoolRate.CurRt) / 100;
        //        PoolRate.YtdAllocAmt = (ytdAmount * PoolRate.YtdRt) / 100;

        //        PoolRate.CurBaseAmt = baseAmount;
        //        PoolRate.YtdBaseAmt = ytdAmount;
        //    }
        //    poolDetails.Add(PoolRate);
        //    //var existingRate = await _context.PoolRatesCostpoint.FirstOrDefaultAsync(p => p.FyCd == PoolRate.FyCd && p.PdNo == PoolRate.PdNo && p.PoolNo == PoolRate.PoolNo && p.AcctId == PoolRate.AcctId && p.OrgId == PoolRate.OrgId && p.SAcctTypeCd == PoolRate.SAcctTypeCd);
        //    //if (existingRate != null)
        //    //{
        //    //    // Update existing record
        //    //    existingRate.CurAmt = PoolRate.CurAmt;
        //    //    existingRate.YtdAmt = PoolRate.YtdAmt;
        //    //    // Update other fields as necessary
        //    //}
        //    //else
        //    //{
        //    //    // Insert new record
        //    //    _context.PoolRatesCostpoint.Add(PoolRate);
        //    //}
        //}
        //await _context.BulkInsertOrUpdateAsync(
        //            PoolRatesCostpoint,
        //            new BulkConfig
        //            {
        //                PreserveInsertOrder = true,
        //                SetOutputIdentity = false,
        //                BatchSize = 5000
        //            });

        //_context.PoolRatesCostpoint.AddRange(PoolRatesCostpoint);
        //_context.SaveChanges();
        /////////////////////////////////////////////////////////////////////////////


        foreach (var costDetail in poolOrgFinancialDetail.poolOrgCostFinancialDetail)
        {
            var account = chartOfAccounts.FirstOrDefault(c => c.AccountId == costDetail.AcctId);
            if (account != null)
            {
                costDetail.AccountName = account.AccountName;
            }
        }
        foreach (var baseDetail in poolOrgFinancialDetail.poolOrgBaseFinancialDetail)
        {

            var account = chartOfAccounts.FirstOrDefault(c => c.AccountId == baseDetail.AcctId);
            if (account != null)
            {
                baseDetail.AccountName = account.AccountName;
            }
            //foreach (var periodDetail in baseDetail.PeriodDetails)
            //{
            //    var temp = PoolRatesCostpoint.FirstOrDefault(p => p.AcctId == baseDetail.AcctId && p.OrgId == baseDetail.OrgId && p.PdNo == periodDetail.Period);
            //    if (temp != null)
            //    {
            //        periodDetail.baseAmt = temp.CurAmt;
            //        periodDetail.AllocationAmt = temp.CurAllocAmt;
            //        periodDetail.Rate = temp.CurRt;
            //    }
            //    var data = result.FirstOrDefault(p => p.PdNo == periodDetail.Period && p.AllocationAccount == baseDetail.AcctId && p.AllocationGroup == baseDetail.OrgId);
            //    if (data != null)
            //    {
            //        periodDetail.baseAmt = temp.CurAmt;
            //        periodDetail.AllocationAmt = temp.CurAllocAmt;
            //        periodDetail.Rate = temp.CurRt;
            //    }

            //}
        }
        var tempfdf = poolOrgFinancialDetail.poolOrgCostFinancialDetail.Where(p => p.AcctId == "P0-01F-07G").ToList();

        return Ok(poolOrgFinancialDetail);


    }


    [NonAction]
    public async Task<IActionResult> CalculateRates(string fycd, string type)
    {

        RateCalculator rateCalculator = new RateCalculator(_context);

        PoolOrgFinancialDetail poolOrgFinancialDetail = new PoolOrgFinancialDetail();
        PoolOrgFinancialDetail poolOrgFringeDetail = new PoolOrgFinancialDetail();
        List<PoolRatesCostpoint> PoolRatesCostpoint = new List<PoolRatesCostpoint>();
        PoolRatesCostpoint PoolRateCostpoint = new PoolRatesCostpoint();
        int poolno = 0;
        switch (type.ToUpper())
        {
            case "OVERHEAD":
                poolno = 13;
                break;
            case "GNA":
                poolno = 17;
                break;
            case "FRINGE":
                poolno = 11;
                break;
            case "HR":
                poolno = 12;
                break;
            case "MNH":
                poolno = 16;
                break;
            default:
                poolno = 0;
                break;
        }
        var ClosedPeriod = DateOnly.Parse(_context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "closing_period").Value);
        var BaseForecases = rateCalculator.GetBaseForecasts(fycd, poolno);
        var CostForecases = rateCalculator.GetCostForecasts(fycd, poolno);

        //return Ok();
        var chartOfAccounts = _context.Charts_Of_Accounts.ToList();


        var poolDetails = await _context.PoolRatesCostpoint
            .Where(x => x.PoolNo == poolno && x.FyCd == Convert.ToInt32(fycd))
            .GroupBy(x => new
            {
                x.FyCd,
                x.PdNo,
                x.PoolNo,
                x.SAcctTypeCd,
                x.AcctId,
                x.OrgId
            })
            .Select(g => new PoolRatesCostpoint
            {
                FyCd = g.Key.FyCd,
                PdNo = g.Key.PdNo,
                PoolNo = g.Key.PoolNo,
                SAcctTypeCd = g.Key.SAcctTypeCd,
                AcctId = g.Key.AcctId,
                OrgId = g.Key.OrgId,

                CurAmt = g.Sum(x => x.CurAmt ?? 0),
                YtdAmt = g.Sum(x => x.YtdAmt ?? 0),
                CurAllocAmt = g.Sum(x => x.CurAllocAmt ?? 0),
                YtdAllocAmt = g.Sum(x => x.YtdAllocAmt ?? 0),
                CurRt = g.Sum(x => x.CurRt ?? 0),
                YtdRt = g.Sum(x => x.YtdRt ?? 0),
                CurBudAmt = g.Sum(x => x.CurBudAmt ?? 0),
                YtdBudAmt = g.Sum(x => x.YtdBudAmt ?? 0),
                CurBudAllocAmt = g.Sum(x => x.CurBudAllocAmt ?? 0),
                YtdBudAllocAmt = g.Sum(x => x.YtdBudAllocAmt ?? 0),
                CurBudRt = g.Sum(x => x.CurBudRt ?? 0),
                YtdBudRt = g.Sum(x => x.YtdBudRt ?? 0),
                CurBaseAmt = g.Max(x => x.CurBaseAmt ?? 0),
                YtdBaseAmt = g.Sum(x => x.YtdBaseAmt ?? 0)
            })
            .OrderBy(x => x.FyCd)
            .ThenBy(x => x.PdNo)
            .ThenBy(x => x.PoolNo)
            .ThenBy(x => x.AcctId)
            .ThenBy(x => x.OrgId)
            .ToListAsync();


        var result =
    from pss in _context.PoolRatesCostpoint
    join pba in _context.PoolBaseAccounts
        on new { pss.AcctId, pss.OrgId }
        equals new { AcctId = pba.AcctId, OrgId = pba.OrgId }
    where pss.SAcctTypeCd == "B"
       && pss.FyCd == Convert.ToInt32(fycd)
       && _context.PoolCostAccounts
            .Any(pca =>
                pca.AcctId == pba.AllocAcctId &&
                pca.OrgId == pba.AllocOrgId &&
                pca.PoolNo == poolno)
    group pss by new
    {
        pss.PdNo,
        pss.PoolNo,
        pss.AcctId,
        pss.OrgId,
        pba.AllocAcctId,
        pba.AllocOrgId

    }
    into g
    select new
    {
        AllocationGroup = g.Key.AllocOrgId,
        AllocationAccount = g.Key.AllocAcctId,
        PdNo = g.Key.PdNo,
        PoolNo = g.Key.PoolNo,
        AcctId = g.Key.AcctId,
        OrgId = g.Key.OrgId,
        YtdAllocAmount = g.Sum(x => x.YtdAllocAmt),
        CurrAllocAmount = g.Sum(x => x.CurAllocAmt)
    };

        var baseresult =
from pss in _context.PoolRatesCostpoint
from pba in _context.PoolBaseAccounts
where pss.AcctId == pba.AcctId
   && pss.OrgId == pba.OrgId
   && pss.PoolNo == pba.PoolNo
join pca in _context.PoolBaseAccounts
    on new
    {
        AcctId = pba.AllocAcctId,
        OrgId = pba.AllocOrgId
    }
    equals new
    {
        AcctId = pca.AcctId,
        OrgId = pca.OrgId
    }
where pss.SAcctTypeCd == "B"
   && pss.FyCd == Convert.ToInt32(fycd)
   && pca.PoolNo == poolno
group pss by new
{
    pss.PdNo,
    pss.PoolNo,
    pss.AcctId,
    pss.OrgId,
    AllocAcctId = pba.AllocAcctId,
    AllocOrgId = pba.AllocOrgId
}
into g
select new
{
    AllocationGroup = g.Key.AllocOrgId,
    AllocationAccount = g.Key.AllocAcctId,
    PdNo = g.Key.PdNo,
    PoolNo = g.Key.PoolNo,
    AcctId = g.Key.AcctId,
    OrgId = g.Key.OrgId,
    YtdAllocAmount = g.Sum(x => x.YtdAllocAmt ?? 0),
    CurrAllocAmount = g.Sum(x => x.CurAllocAmt ?? 0)
};
        var baseAccountDetails = poolDetails.Where(p => p.SAcctTypeCd == "B").ToList();
        var costAccountDetails = poolDetails.Where(p => p.SAcctTypeCd == "C").ToList();

        var poolBaseAccounts = _context.PoolBaseAccounts.Where(p => p.PoolNo == poolno).ToList();

        poolOrgFinancialDetail.poolOrgBaseFinancialDetail = new List<PoolOrgBaseFinancialDetail>();
        poolOrgFinancialDetail.poolOrgCostFinancialDetail = new List<PoolOrgCostFinancialDetail>();

        var baseLaborAccountFinancialDetails = _context.PoolBaseAccounts.Where(p => p.PoolNo == poolno).ToList();

        foreach (var baseAccount in baseAccountDetails.Select(p => new { p.AcctId, p.OrgId }).Distinct())
        {
            var allocationDetails = baseLaborAccountFinancialDetails
                                .Where(p => p.PoolNo == poolno && p.AcctId == baseAccount.AcctId && p.OrgId == baseAccount.OrgId)
                                .FirstOrDefault();
            PoolOrgBaseFinancialDetail baseFinancialDetail = new PoolOrgBaseFinancialDetail
            {
                AcctId = baseAccount.AcctId,
                OrgId = baseAccount.OrgId,
                AllocationAcctId = allocationDetails?.AllocAcctId,
                AllocationOrgId = allocationDetails?.AllocOrgId,
                YTDActualAmt = baseAccountDetails.Where(p => p.AcctId == baseAccount.AcctId && p.OrgId == baseAccount.OrgId)?.Sum(q => q.YtdBaseAmt),
                PeriodDetails = new List<PeriodbaseFinancialDetail>()
            };
            for (int i = 1; i <= 12; i++)
            {
                int lastDay = DateTime.DaysInMonth(Convert.ToInt16(fycd), i);
                DateOnly date = new DateOnly(Convert.ToInt16(fycd), i, lastDay);
                if (ClosedPeriod >= date)
                {
                    var periodData = baseAccountDetails.FirstOrDefault(p => p.PdNo == i && p.AcctId == baseAccount.AcctId && p.OrgId == baseAccount.OrgId);

                    if (periodData != null)
                    {
                        baseFinancialDetail.PeriodDetails.Add(new PeriodbaseFinancialDetail
                        {
                            Period = i,
                            baseAmt = periodData.CurAmt,
                            Rate = periodData.CurRt,
                            AllocationAmt = periodData.CurAllocAmt
                        });
                    }

                }
                else
                {
                    PoolRateCostpoint = new PoolRatesCostpoint();
                    PoolRateCostpoint.PoolNo = poolno;
                    PoolRateCostpoint.AcctId = baseAccount.AcctId;
                    PoolRateCostpoint.OrgId = baseAccount.OrgId;
                    PoolRateCostpoint.PdNo = i;
                    PoolRateCostpoint.FyCd = Convert.ToInt32(fycd);
                    PoolRateCostpoint.SAcctTypeCd = "B";

                    var periodData = BaseForecases.FirstOrDefault(p => p.Month == i && p.AcctId == baseAccount.AcctId && p.OrgId == baseAccount.OrgId);
                    if (periodData != null)
                    {
                        PoolRateCostpoint.CurAmt = periodData.TotalForecastedAmt;
                        PoolRateCostpoint.YtdAmt = periodData.TotalForecastedAmt;


                        baseFinancialDetail.PeriodDetails.Add(new PeriodbaseFinancialDetail
                        {
                            Period = i,
                            BudgetAmt = periodData.TotalForecastedAmt,
                        });
                    }
                    else
                    {
                        ///GNA Pool Base need to check allocation amount from other base accounts
                        if (poolno == 13 && i == 11 && baseAccount.AcctId == "P0-01F-00D" && baseAccount.OrgId == "1.01.03.01")
                        {

                        }
                        var resultGetCurr_Alloc1 = await _context.PoolAllocationRaw
                                .FromSqlRaw(@"
                                                        SELECT
                                                            A.pool_no  AS ""BasePoolNo"",
                                                            A.acct_id  AS ""BaseAcctId"",
                                                            A.org_id   AS ""BaseOrgId"",
                                                            B.pool_no  AS ""AllocPoolNo"",
                                                            PR.cur_alloc_amt AS ""CurAllocAmt""
                                                        FROM pool_base_account A
                                                        JOIN pool_base_account B
                                                            ON A.pool_no <> B.pool_no
                                                           AND A.acct_id = B.alloc_acct_id
                                                           AND A.org_id  = B.alloc_org_id
                                                        JOIN poolrates_costpoint PR
                                                            ON PR.acct_id = B.acct_id
                                                           AND PR.org_id  = B.org_id
                                                           AND PR.pool_no = B.pool_no
                                                        WHERE PR.pd_no = @pdNo
                                                          AND A.acct_id = @baseAcctId
                                                          AND A.org_id  = @baseOrgId
                                                    ",
                                new Npgsql.NpgsqlParameter("@pdNo", i),
                                new Npgsql.NpgsqlParameter("@baseAcctId", baseAccount.AcctId),
                                new Npgsql.NpgsqlParameter("@baseOrgId", baseAccount.OrgId)
                            )
                            .ToListAsync();

                        var AllocationAmt = resultGetCurr_Alloc1.Where(p => p.BasePoolNo == poolno).Sum(x => x.CurAllocAmt.GetValueOrDefault());
                        PoolRateCostpoint.CurAmt = AllocationAmt;
                        //PoolRateCostpoint.YtdAmt = data.YtdAllocAmount;
                        baseFinancialDetail.PeriodDetails.Add(new PeriodbaseFinancialDetail
                        {
                            Period = i,
                            BudgetAmt = AllocationAmt,
                        });


                        //if (poolno == 13 || poolno == 17)
                        //{
                        //    var data = baseresult.FirstOrDefault(p => p.PdNo == i && p.AllocationAccount == baseAccount.AcctId && p.OrgId == baseAccount.OrgId);
                        //    if (data != null)
                        //    {
                        //        PoolRateCostpoint.CurAmt = data.CurrAllocAmount;
                        //        PoolRateCostpoint.YtdAmt = data.YtdAllocAmount;
                        //        baseFinancialDetail.PeriodDetails.Add(new PeriodbaseFinancialDetail
                        //        {
                        //            Period = i,
                        //            BudgetAmt = data.CurrAllocAmount,
                        //        });
                        //    }
                        //    else
                        //    {
                        //        PoolRateCostpoint.CurAmt = 0;
                        //        PoolRateCostpoint.YtdAmt = 0;
                        //        baseFinancialDetail.PeriodDetails.Add(new PeriodbaseFinancialDetail
                        //        {
                        //            Period = i,
                        //            BudgetAmt = 0
                        //        });
                        //    }
                        //}
                        //else
                        //{
                        //    PoolRateCostpoint.CurAmt = 0;
                        //    PoolRateCostpoint.YtdAmt = 0;

                        //    baseFinancialDetail.PeriodDetails.Add(new PeriodbaseFinancialDetail
                        //    {
                        //        Period = i,
                        //        BudgetAmt = 0
                        //    });
                        //}
                    }

                    PoolRatesCostpoint.Add(PoolRateCostpoint);
                }
            }
            baseFinancialDetail.YTDAllocationAmt = baseFinancialDetail.PeriodDetails.Sum(x => x.AllocationAmt);
            poolOrgFinancialDetail.poolOrgBaseFinancialDetail.Add(baseFinancialDetail);
        }

        foreach (var costAccount in costAccountDetails.Select(p => new { p.AcctId, p.OrgId }).Distinct())
        {
            PoolOrgCostFinancialDetail costFinancialDetail = new PoolOrgCostFinancialDetail
            {
                AcctId = costAccount.AcctId,
                OrgId = costAccount.OrgId,
                YTDActualAmt = costAccountDetails.Where(p => p.AcctId == costAccount.AcctId && p.OrgId == costAccount.OrgId)?.Sum(q => q.YtdAmt),

                PeriodDetails = new List<PeriodCostFinancialDetail>()
            };
            for (int k = 1; k <= 12; k++)
            {

                int lastDay = DateTime.DaysInMonth(Convert.ToInt16(fycd), k);
                DateOnly date = new DateOnly(Convert.ToInt16(fycd), k, lastDay);
                if (ClosedPeriod >= date)
                {
                    var periodData = costAccountDetails.FirstOrDefault(p => p.PdNo == k && p.AcctId == costAccount.AcctId && p.OrgId == costAccount.OrgId);
                    if (periodData != null)
                    {
                        costFinancialDetail.PeriodDetails.Add(new PeriodCostFinancialDetail
                        {
                            Period = k,
                            Actualamt = periodData.CurAmt,
                            Rate = periodData.CurRt
                        });
                    }
                }
                else
                {
                    PoolRateCostpoint = new PoolRatesCostpoint();
                    PoolRateCostpoint.PoolNo = poolno;
                    PoolRateCostpoint.SAcctTypeCd = "C";
                    PoolRateCostpoint.AcctId = costAccount.AcctId;
                    PoolRateCostpoint.OrgId = costAccount.OrgId;
                    PoolRateCostpoint.PdNo = k;
                    PoolRateCostpoint.FyCd = Convert.ToInt32(fycd);

                    var periodData = CostForecases.FirstOrDefault(p => p.Month == k && p.AcctId == costAccount.AcctId && p.OrgId == costAccount.OrgId);
                    if (periodData != null)
                    {
                        PoolRateCostpoint.CurAmt = periodData.TotalForecastedAmt;
                        PoolRateCostpoint.YtdAmt = periodData.TotalForecastedAmt;
                        costFinancialDetail.PeriodDetails.Add(new PeriodCostFinancialDetail
                        {
                            Period = k,
                            BudgetedAmt = periodData.TotalForecastedAmt,
                        });
                    }
                    else
                    {
                        var data = result.FirstOrDefault(p => p.PdNo == k && p.AllocationAccount == costAccount.AcctId && p.OrgId == costAccount.OrgId);
                        if (data != null)
                        {
                            PoolRateCostpoint.CurAmt = data.CurrAllocAmount;
                            PoolRateCostpoint.YtdAmt = data.YtdAllocAmount;
                            costFinancialDetail.PeriodDetails.Add(new PeriodCostFinancialDetail
                            {
                                Period = k,
                                BudgetedAmt = data.CurrAllocAmount,
                            });
                        }
                        else
                        {
                            PoolRateCostpoint.CurAmt = 0;
                            PoolRateCostpoint.YtdAmt = 0;
                            costFinancialDetail.PeriodDetails.Add(new PeriodCostFinancialDetail
                            {
                                Period = k,
                                BudgetedAmt = 0
                            });
                        }
                    }
                    PoolRatesCostpoint.Add(PoolRateCostpoint);
                }
            }
            poolOrgFinancialDetail.poolOrgCostFinancialDetail.Add(costFinancialDetail);
        }


        //////////////////////////////////////////////////////////////////////////////////////////////////
        ///


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        poolOrgFinancialDetail.TotalYTDBaseActualAmt = poolOrgFinancialDetail.poolOrgBaseFinancialDetail.Sum(x => x.YTDActualAmt);
        poolOrgFinancialDetail.TotalYTDCostActualAmt = poolOrgFinancialDetail.poolOrgCostFinancialDetail.Sum(x => x.YTDActualAmt);

        poolOrgFinancialDetail.TotalYTDBaseAllocationActualAmt = poolOrgFinancialDetail.poolOrgBaseFinancialDetail
            .SelectMany(x => x.PeriodDetails)
            .Sum(x => x.AllocationAmt);

        poolOrgFinancialDetail.Rate =
            poolOrgFinancialDetail.TotalYTDBaseActualAmt != 0
                ? Math.Round(
                    (poolOrgFinancialDetail.TotalYTDCostActualAmt.GetValueOrDefault() / poolOrgFinancialDetail.TotalYTDBaseActualAmt.GetValueOrDefault()) * 100,
                    4
                  )
                : 0;

        ////////////////////////////////////////////////////////////////////////////////

        foreach (var PoolRate in PoolRatesCostpoint)
        {
            if (poolno == 13)
            {

            }

            //if (poolno == 13 && PoolRate.PoolNo == 11 && PoolRate.AcctId == "P0-01F-00D" && PoolRate.OrgId == "1.01.03.01")
            //{

            //}
            var baseAmount = poolOrgFinancialDetail.poolOrgBaseFinancialDetail.FirstOrDefault(p => p.AcctId == PoolRate.AcctId && p.OrgId == PoolRate.OrgId)?
                .PeriodDetails.FirstOrDefault(q => q.Period == PoolRate.PdNo)?.BudgetAmt;

            //var baseTotalAmount =
            //    poolOrgFinancialDetail.poolOrgBaseFinancialDetail
            //        .SelectMany(p => p.PeriodDetails).Where(p => p.Period == PoolRate.PdNo).Sum(q => q.BudgetAmt);
            var baseTotalAmount =
                    poolOrgFinancialDetail.poolOrgBaseFinancialDetail
                        .SelectMany(p => p.PeriodDetails)
                        .Where(p => p.Period == PoolRate.PdNo)
                        .Sum(q => q.BudgetAmt) ?? 0m;

            var costAmount =
                    poolOrgFinancialDetail.poolOrgCostFinancialDetail
                        .SelectMany(p => p.PeriodDetails)
                        .Where(p => p.Period == PoolRate.PdNo)
                        .Sum(q => q.BudgetedAmt) ?? 0m;

            var YtdbaseTotalAmount =
                    poolOrgFinancialDetail.poolOrgBaseFinancialDetail
                        .SelectMany(p => p.PeriodDetails)
                        .Where(p => p.Period <= PoolRate.PdNo)
                        .Sum(q => q.BudgetAmt) ?? 0m;
            YtdbaseTotalAmount = YtdbaseTotalAmount + poolOrgFinancialDetail.TotalYTDBaseActualAmt.GetValueOrDefault();



            var YtdcostAmount =
                    poolOrgFinancialDetail.poolOrgCostFinancialDetail
                        .SelectMany(p => p.PeriodDetails)
                        .Where(p => p.Period <= PoolRate.PdNo)
                        .Sum(q => q.BudgetedAmt) ?? 0m;
            YtdcostAmount = YtdcostAmount + poolOrgFinancialDetail.TotalYTDCostActualAmt.GetValueOrDefault();

            //var costAmount =
            //    poolOrgFinancialDetail.poolOrgCostFinancialDetail
            //        .SelectMany(p => p.PeriodDetails).Where(p => p.Period == PoolRate.PdNo).Sum(q => q.BudgetedAmt);

            var ytdAmount =
                    (poolDetails.FirstOrDefault(p =>
                        p.SAcctTypeCd == PoolRate.SAcctTypeCd &&
                        p.AcctId == PoolRate.AcctId &&
                        p.OrgId == PoolRate.OrgId &&
                        p.PdNo == PoolRate.PdNo - 1
                    )?.YtdAmt ?? 0)
                    + (baseAmount ?? 0);

            PoolRate.CurAmt = baseAmount;
            PoolRate.YtdAmt = ytdAmount;
            if (PoolRate.SAcctTypeCd == "B")
            {
                //PoolRate.CurRt = (costAmount / baseTotalAmount) * 100;
                //PoolRate.CurRt =
                //        baseTotalAmount != 0
                //            ? (costAmount / baseTotalAmount) * 100
                //            : 0;
                PoolRate.CurRt =
                    baseTotalAmount > 0
                        ? Math.Round((costAmount / baseTotalAmount) * 100, 4)
                        : 0m;

                PoolRate.YtdRt =
                    YtdbaseTotalAmount > 0
                        ? Math.Round((YtdcostAmount / YtdbaseTotalAmount) * 100, 4)
                        : 0m;

                PoolRate.CurAllocAmt = (baseAmount * PoolRate.CurRt) / 100;
                PoolRate.YtdAllocAmt = (ytdAmount * PoolRate.YtdRt) / 100;

                PoolRate.CurBaseAmt = baseAmount;
                PoolRate.YtdBaseAmt = ytdAmount;
            }
            poolDetails.Add(PoolRate);
        }
        await _context.BulkInsertOrUpdateAsync(
                    PoolRatesCostpoint,
                    new BulkConfig
                    {
                        PreserveInsertOrder = true,
                        SetOutputIdentity = false,
                        BatchSize = 5000
                    });


        foreach (var costDetail in poolOrgFinancialDetail.poolOrgCostFinancialDetail)
        {
            var account = chartOfAccounts.FirstOrDefault(c => c.AccountId == costDetail.AcctId);
            if (account != null)
            {
                costDetail.AccountName = account.AccountName;
            }
        }
        foreach (var costDetail in poolOrgFinancialDetail.poolOrgBaseFinancialDetail)
        {

            var account = chartOfAccounts.FirstOrDefault(c => c.AccountId == costDetail.AcctId);
            if (account != null)
            {
                costDetail.AccountName = account.AccountName;
            }
            foreach (var periodDetail in costDetail.PeriodDetails)
            {
                var temp = PoolRatesCostpoint.FirstOrDefault(p => p.AcctId == costDetail.AcctId && p.OrgId == costDetail.OrgId && p.PdNo == periodDetail.Period);
                if (temp != null)
                {
                    periodDetail.baseAmt = temp.CurAmt;
                    periodDetail.AllocationAmt = temp.CurAllocAmt;
                    periodDetail.Rate = temp.CurRt;
                }
                var data = result.FirstOrDefault(p => p.PdNo == periodDetail.Period && p.AllocationAccount == costDetail.AcctId && p.AllocationGroup == costDetail.OrgId);
                if (data != null)
                {
                    periodDetail.baseAmt = temp.CurAmt;
                    periodDetail.AllocationAmt = temp.CurAllocAmt;
                    periodDetail.Rate = temp.CurRt;
                }

            }
        }
        //return Ok(poolOrgFinancialDetail);
        return Ok();
    }


    [NonAction]
    public async Task<IActionResult> CalculateRatesOld(string fycd, string type)
    {


        RateCalculator rateCalculator = new RateCalculator(_context);

        PoolOrgFinancialDetail poolOrgFinancialDetail = new PoolOrgFinancialDetail();
        PoolOrgFinancialDetail poolOrgFringeDetail = new PoolOrgFinancialDetail();
        List<PoolRatesCostpoint> PoolRatesCostpoint = new List<PoolRatesCostpoint>();
        PoolRatesCostpoint PoolRateCostpoint = new PoolRatesCostpoint();
        int poolno = 0;
        switch (type.ToUpper())
        {
            case "OVERHEAD":
                poolno = 13;
                break;
            case "GNA":
                poolno = 17;
                break;
            case "FRINGE":
                poolno = 11;
                break;
            case "HR":
                poolno = 12;
                break;
            case "MNH":
                poolno = 16;
                break;
            default:
                poolno = 0;
                break;
        }
        var ClosedPeriod = DateOnly.Parse(_context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "closing_period").Value);
        var BaseForecases = rateCalculator.GetBaseForecasts(fycd, poolno);
        var CostForecases = rateCalculator.GetCostForecasts(fycd, poolno);

        //return Ok();
        var chartOfAccounts = _context.Charts_Of_Accounts.ToList();


        var poolDetails = await _context.PoolRatesCostpoint
            .Where(x => x.PoolNo == poolno && x.FyCd == Convert.ToInt32(fycd))
            .GroupBy(x => new
            {
                x.FyCd,
                x.PdNo,
                x.PoolNo,
                x.SAcctTypeCd,
                x.AcctId,
                x.OrgId
            })
            .Select(g => new PoolRatesCostpoint
            {
                FyCd = g.Key.FyCd,
                PdNo = g.Key.PdNo,
                PoolNo = g.Key.PoolNo,
                SAcctTypeCd = g.Key.SAcctTypeCd,
                AcctId = g.Key.AcctId,
                OrgId = g.Key.OrgId,

                CurAmt = g.Sum(x => x.CurAmt ?? 0),
                YtdAmt = g.Sum(x => x.YtdAmt ?? 0),
                CurAllocAmt = g.Sum(x => x.CurAllocAmt ?? 0),
                YtdAllocAmt = g.Sum(x => x.YtdAllocAmt ?? 0),
                CurRt = g.Sum(x => x.CurRt ?? 0),
                YtdRt = g.Sum(x => x.YtdRt ?? 0),
                CurBudAmt = g.Sum(x => x.CurBudAmt ?? 0),
                YtdBudAmt = g.Sum(x => x.YtdBudAmt ?? 0),
                CurBudAllocAmt = g.Sum(x => x.CurBudAllocAmt ?? 0),
                YtdBudAllocAmt = g.Sum(x => x.YtdBudAllocAmt ?? 0),
                CurBudRt = g.Sum(x => x.CurBudRt ?? 0),
                YtdBudRt = g.Sum(x => x.YtdBudRt ?? 0),
                CurBaseAmt = g.Sum(x => x.CurBaseAmt ?? 0),
                YtdBaseAmt = g.Sum(x => x.YtdBaseAmt ?? 0)
            })
            .OrderBy(x => x.FyCd)
            .ThenBy(x => x.PdNo)
            .ThenBy(x => x.PoolNo)
            .ThenBy(x => x.AcctId)
            .ThenBy(x => x.OrgId)
            .ToListAsync();


        var result =
    from pss in _context.PoolRatesCostpoint
    join pba in _context.PoolBaseAccounts
        on new { pss.AcctId, pss.OrgId }
        equals new { AcctId = pba.AcctId, OrgId = pba.OrgId }
    where pss.SAcctTypeCd == "B"
       && pss.FyCd == Convert.ToInt32(fycd)
       && _context.PoolCostAccounts
            .Any(pca =>
                pca.AcctId == pba.AllocAcctId &&
                pca.OrgId == pba.AllocOrgId &&
                pca.PoolNo == poolno)
    group pss by new
    {
        pss.PdNo,
        pss.PoolNo,
        pss.AcctId,
        pss.OrgId,
        pba.AllocAcctId,
        pba.AllocOrgId

    }
    into g
    select new
    {
        AllocationGroup = g.Key.AllocOrgId,
        AllocationAccount = g.Key.AllocAcctId,
        PdNo = g.Key.PdNo,
        PoolNo = g.Key.PoolNo,
        AcctId = g.Key.AcctId,
        OrgId = g.Key.OrgId,
        YtdAllocAmount = g.Sum(x => x.YtdAllocAmt),
        CurrAllocAmount = g.Sum(x => x.CurAllocAmt)
    };

        var baseresult =
from pss in _context.PoolRatesCostpoint
from pba in _context.PoolBaseAccounts
where pss.AcctId == pba.AcctId
   && pss.OrgId == pba.OrgId
   && pss.PoolNo == pba.PoolNo
join pca in _context.PoolBaseAccounts
    on new
    {
        AcctId = pba.AllocAcctId,
        OrgId = pba.AllocOrgId
    }
    equals new
    {
        AcctId = pca.AcctId,
        OrgId = pca.OrgId
    }
where pss.SAcctTypeCd == "B"
   && pss.FyCd == Convert.ToInt32(fycd)
   && pca.PoolNo == poolno
group pss by new
{
    pss.PdNo,
    pss.PoolNo,
    pss.AcctId,
    pss.OrgId,
    AllocAcctId = pba.AllocAcctId,
    AllocOrgId = pba.AllocOrgId
}
into g
select new
{
    AllocationGroup = g.Key.AllocOrgId,
    AllocationAccount = g.Key.AllocAcctId,
    PdNo = g.Key.PdNo,
    PoolNo = g.Key.PoolNo,
    AcctId = g.Key.AcctId,
    OrgId = g.Key.OrgId,
    YtdAllocAmount = g.Sum(x => x.YtdAllocAmt ?? 0),
    CurrAllocAmount = g.Sum(x => x.CurAllocAmt ?? 0)
};
        var baseAccountDetails = poolDetails.Where(p => p.SAcctTypeCd == "B").ToList();
        var costAccountDetails = poolDetails.Where(p => p.SAcctTypeCd == "C").ToList();

        var poolBaseAccounts = _context.PoolBaseAccounts.Where(p => p.PoolNo == poolno).ToList();

        poolOrgFinancialDetail.poolOrgBaseFinancialDetail = new List<PoolOrgBaseFinancialDetail>();
        poolOrgFinancialDetail.poolOrgCostFinancialDetail = new List<PoolOrgCostFinancialDetail>();

        var baseLaborAccountFinancialDetails = _context.PoolBaseAccounts.Where(p => p.PoolNo == poolno).ToList();

        foreach (var baseAccount in baseAccountDetails.Select(p => new { p.AcctId, p.OrgId }).Distinct())
        {
            var allocationDetails = baseLaborAccountFinancialDetails
                                .Where(p => p.PoolNo == poolno && p.AcctId == baseAccount.AcctId && p.OrgId == baseAccount.OrgId)
                                .FirstOrDefault();
            PoolOrgBaseFinancialDetail baseFinancialDetail = new PoolOrgBaseFinancialDetail
            {
                AcctId = baseAccount.AcctId,
                OrgId = baseAccount.OrgId,
                AllocationAcctId = allocationDetails?.AllocAcctId,
                AllocationOrgId = allocationDetails?.AllocOrgId,
                YTDActualAmt = baseAccountDetails.Where(p => p.AcctId == baseAccount.AcctId && p.OrgId == baseAccount.OrgId)?.Sum(q => q.YtdBaseAmt),
                PeriodDetails = new List<PeriodbaseFinancialDetail>()
            };
            for (int i = 1; i <= 12; i++)
            {
                int lastDay = DateTime.DaysInMonth(Convert.ToInt16(fycd), i);
                DateOnly date = new DateOnly(Convert.ToInt16(fycd), i, lastDay);
                if (ClosedPeriod >= date)
                {
                    var periodData = baseAccountDetails.FirstOrDefault(p => p.PdNo == i && p.AcctId == baseAccount.AcctId && p.OrgId == baseAccount.OrgId);

                    if (periodData != null)
                    {
                        baseFinancialDetail.PeriodDetails.Add(new PeriodbaseFinancialDetail
                        {
                            Period = i,
                            baseAmt = periodData.CurAmt,
                            Rate = periodData.CurRt,
                            AllocationAmt = periodData.CurAllocAmt
                        });
                    }

                }
                else
                {
                    PoolRateCostpoint = new PoolRatesCostpoint();
                    PoolRateCostpoint.PoolNo = poolno;
                    PoolRateCostpoint.AcctId = baseAccount.AcctId;
                    PoolRateCostpoint.OrgId = baseAccount.OrgId;
                    PoolRateCostpoint.PdNo = i;
                    PoolRateCostpoint.FyCd = Convert.ToInt32(fycd);
                    PoolRateCostpoint.SAcctTypeCd = "B";

                    var periodData = BaseForecases.FirstOrDefault(p => p.Month == i && p.AcctId == baseAccount.AcctId && p.OrgId == baseAccount.OrgId);
                    if (periodData != null)
                    {
                        PoolRateCostpoint.CurAmt = periodData.TotalForecastedAmt;
                        PoolRateCostpoint.YtdAmt = periodData.TotalForecastedAmt;


                        baseFinancialDetail.PeriodDetails.Add(new PeriodbaseFinancialDetail
                        {
                            Period = i,
                            BudgetAmt = periodData.TotalForecastedAmt,
                        });
                    }
                    else
                    {
                        if (poolno == 13 || poolno == 17)
                        {
                            var data = baseresult.FirstOrDefault(p => p.PdNo == i && p.AllocationAccount == baseAccount.AcctId && p.OrgId == baseAccount.OrgId);
                            if (data != null)
                            {
                                PoolRateCostpoint.CurAmt = data.CurrAllocAmount;
                                PoolRateCostpoint.YtdAmt = data.YtdAllocAmount;
                                baseFinancialDetail.PeriodDetails.Add(new PeriodbaseFinancialDetail
                                {
                                    Period = i,
                                    BudgetAmt = data.CurrAllocAmount,
                                });
                            }
                            else
                            {
                                PoolRateCostpoint.CurAmt = 0;
                                PoolRateCostpoint.YtdAmt = 0;
                                baseFinancialDetail.PeriodDetails.Add(new PeriodbaseFinancialDetail
                                {
                                    Period = i,
                                    BudgetAmt = 0
                                });
                            }
                        }
                        else
                        {
                            PoolRateCostpoint.CurAmt = 0;
                            PoolRateCostpoint.YtdAmt = 0;

                            baseFinancialDetail.PeriodDetails.Add(new PeriodbaseFinancialDetail
                            {
                                Period = i,
                                BudgetAmt = 0
                            });
                        }
                    }

                    PoolRatesCostpoint.Add(PoolRateCostpoint);
                }
            }
            baseFinancialDetail.YTDAllocationAmt = baseFinancialDetail.PeriodDetails.Sum(x => x.AllocationAmt);
            poolOrgFinancialDetail.poolOrgBaseFinancialDetail.Add(baseFinancialDetail);
        }

        foreach (var costAccount in costAccountDetails.Select(p => new { p.AcctId, p.OrgId }).Distinct())
        {
            PoolOrgCostFinancialDetail costFinancialDetail = new PoolOrgCostFinancialDetail
            {
                AcctId = costAccount.AcctId,
                OrgId = costAccount.OrgId,
                YTDActualAmt = costAccountDetails.Where(p => p.AcctId == costAccount.AcctId && p.OrgId == costAccount.OrgId)?.Sum(q => q.YtdAmt),

                PeriodDetails = new List<PeriodCostFinancialDetail>()
            };
            for (int k = 1; k <= 12; k++)
            {

                int lastDay = DateTime.DaysInMonth(Convert.ToInt16(fycd), k);
                DateOnly date = new DateOnly(Convert.ToInt16(fycd), k, lastDay);
                if (ClosedPeriod >= date)
                {
                    var periodData = costAccountDetails.FirstOrDefault(p => p.PdNo == k && p.AcctId == costAccount.AcctId && p.OrgId == costAccount.OrgId);
                    if (periodData != null)
                    {
                        costFinancialDetail.PeriodDetails.Add(new PeriodCostFinancialDetail
                        {
                            Period = k,
                            Actualamt = periodData.CurAmt,
                            Rate = periodData.CurRt
                        });
                    }
                }
                else
                {
                    PoolRateCostpoint = new PoolRatesCostpoint();
                    PoolRateCostpoint.PoolNo = poolno;
                    PoolRateCostpoint.SAcctTypeCd = "C";
                    PoolRateCostpoint.AcctId = costAccount.AcctId;
                    PoolRateCostpoint.OrgId = costAccount.OrgId;
                    PoolRateCostpoint.PdNo = k;
                    PoolRateCostpoint.FyCd = Convert.ToInt32(fycd);

                    var periodData = CostForecases.FirstOrDefault(p => p.Month == k && p.AcctId == costAccount.AcctId && p.OrgId == costAccount.OrgId);
                    if (periodData != null)
                    {
                        PoolRateCostpoint.CurAmt = periodData.TotalForecastedAmt;
                        PoolRateCostpoint.YtdAmt = periodData.TotalForecastedAmt;
                        costFinancialDetail.PeriodDetails.Add(new PeriodCostFinancialDetail
                        {
                            Period = k,
                            BudgetedAmt = periodData.TotalForecastedAmt,
                        });
                    }
                    else
                    {
                        var data = result.FirstOrDefault(p => p.PdNo == k && p.AllocationAccount == costAccount.AcctId && p.OrgId == costAccount.OrgId);
                        if (data != null)
                        {
                            PoolRateCostpoint.CurAmt = data.CurrAllocAmount;
                            PoolRateCostpoint.YtdAmt = data.YtdAllocAmount;
                            costFinancialDetail.PeriodDetails.Add(new PeriodCostFinancialDetail
                            {
                                Period = k,
                                BudgetedAmt = data.CurrAllocAmount,
                            });
                        }
                        else
                        {
                            PoolRateCostpoint.CurAmt = 0;
                            PoolRateCostpoint.YtdAmt = 0;
                            costFinancialDetail.PeriodDetails.Add(new PeriodCostFinancialDetail
                            {
                                Period = k,
                                BudgetedAmt = 0
                            });
                        }
                    }
                    PoolRatesCostpoint.Add(PoolRateCostpoint);
                }
            }
            poolOrgFinancialDetail.poolOrgCostFinancialDetail.Add(costFinancialDetail);
        }


        //////////////////////////////////////////////////////////////////////////////////////////////////
        ///


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        poolOrgFinancialDetail.TotalYTDBaseActualAmt = poolOrgFinancialDetail.poolOrgBaseFinancialDetail.Sum(x => x.YTDActualAmt);
        poolOrgFinancialDetail.TotalYTDCostActualAmt = poolOrgFinancialDetail.poolOrgCostFinancialDetail.Sum(x => x.YTDActualAmt);

        poolOrgFinancialDetail.TotalYTDBaseAllocationActualAmt = poolOrgFinancialDetail.poolOrgBaseFinancialDetail
            .SelectMany(x => x.PeriodDetails)
            .Sum(x => x.AllocationAmt);

        poolOrgFinancialDetail.Rate =
            poolOrgFinancialDetail.TotalYTDBaseActualAmt != 0
                ? Math.Round(
                    (poolOrgFinancialDetail.TotalYTDCostActualAmt.GetValueOrDefault() / poolOrgFinancialDetail.TotalYTDBaseActualAmt.GetValueOrDefault()) * 100,
                    4
                  )
                : 0;

        ////////////////////////////////////////////////////////////////////////////////

        foreach (var PoolRate in PoolRatesCostpoint)
        {

            var baseAmount = poolOrgFinancialDetail.poolOrgBaseFinancialDetail.FirstOrDefault(p => p.AcctId == PoolRate.AcctId && p.OrgId == PoolRate.OrgId)?
                .PeriodDetails.FirstOrDefault(q => q.Period == PoolRate.PdNo)?.BudgetAmt;

            var baseTotalAmount =
                    poolOrgFinancialDetail.poolOrgBaseFinancialDetail
                        .SelectMany(p => p.PeriodDetails)
                        .Where(p => p.Period == PoolRate.PdNo)
                        .Sum(q => q.BudgetAmt) ?? 0m;

            var costAmount =
                    poolOrgFinancialDetail.poolOrgCostFinancialDetail
                        .SelectMany(p => p.PeriodDetails)
                        .Where(p => p.Period == PoolRate.PdNo)
                        .Sum(q => q.BudgetedAmt) ?? 0m;

            var YtdbaseTotalAmount =
                    poolOrgFinancialDetail.poolOrgBaseFinancialDetail
                        .SelectMany(p => p.PeriodDetails)
                        .Where(p => p.Period <= PoolRate.PdNo)
                        .Sum(q => q.BudgetAmt) ?? 0m;
            YtdbaseTotalAmount = YtdbaseTotalAmount + poolOrgFinancialDetail.TotalYTDBaseActualAmt.GetValueOrDefault();



            var YtdcostAmount =
                    poolOrgFinancialDetail.poolOrgCostFinancialDetail
                        .SelectMany(p => p.PeriodDetails)
                        .Where(p => p.Period <= PoolRate.PdNo)
                        .Sum(q => q.BudgetedAmt) ?? 0m;
            YtdcostAmount = YtdcostAmount + poolOrgFinancialDetail.TotalYTDCostActualAmt.GetValueOrDefault();

            //var costAmount =
            //    poolOrgFinancialDetail.poolOrgCostFinancialDetail
            //        .SelectMany(p => p.PeriodDetails).Where(p => p.Period == PoolRate.PdNo).Sum(q => q.BudgetedAmt);

            var ytdAmount =
                    (poolDetails.FirstOrDefault(p =>
                        p.SAcctTypeCd == PoolRate.SAcctTypeCd &&
                        p.AcctId == PoolRate.AcctId &&
                        p.OrgId == PoolRate.OrgId &&
                        p.PdNo == PoolRate.PdNo - 1
                    )?.YtdAmt ?? 0)
                    + (baseAmount ?? 0);

            PoolRate.CurAmt = baseAmount;
            PoolRate.YtdAmt = ytdAmount;
            if (PoolRate.SAcctTypeCd == "B")
            {
                //PoolRate.CurRt = (costAmount / baseTotalAmount) * 100;
                //PoolRate.CurRt =
                //        baseTotalAmount != 0
                //            ? (costAmount / baseTotalAmount) * 100
                //            : 0;
                PoolRate.CurRt =
                    baseTotalAmount > 0
                        ? Math.Round((costAmount / baseTotalAmount) * 100, 4)
                        : 0m;

                PoolRate.YtdRt =
                    YtdbaseTotalAmount > 0
                        ? Math.Round((YtdcostAmount / YtdbaseTotalAmount) * 100, 4)
                        : 0m;

                PoolRate.CurAllocAmt = (baseAmount * PoolRate.CurRt) / 100;
                PoolRate.YtdAllocAmt = (ytdAmount * PoolRate.YtdRt) / 100;

                PoolRate.CurBaseAmt = baseAmount;
                PoolRate.YtdBaseAmt = ytdAmount;
            }
            poolDetails.Add(PoolRate);
        }
        await _context.BulkInsertOrUpdateAsync(
                    PoolRatesCostpoint,
                    new BulkConfig
                    {
                        PreserveInsertOrder = true,
                        SetOutputIdentity = false,
                        BatchSize = 5000
                    });


        foreach (var costDetail in poolOrgFinancialDetail.poolOrgCostFinancialDetail)
        {
            var account = chartOfAccounts.FirstOrDefault(c => c.AccountId == costDetail.AcctId);
            if (account != null)
            {
                costDetail.AccountName = account.AccountName;
            }
        }
        foreach (var costDetail in poolOrgFinancialDetail.poolOrgBaseFinancialDetail)
        {

            var account = chartOfAccounts.FirstOrDefault(c => c.AccountId == costDetail.AcctId);
            if (account != null)
            {
                costDetail.AccountName = account.AccountName;
            }
            foreach (var periodDetail in costDetail.PeriodDetails)
            {
                var temp = PoolRatesCostpoint.FirstOrDefault(p => p.AcctId == costDetail.AcctId && p.OrgId == costDetail.OrgId && p.PdNo == periodDetail.Period);
                if (temp != null)
                {
                    periodDetail.baseAmt = temp.CurAmt;
                    periodDetail.AllocationAmt = temp.CurAllocAmt;
                    periodDetail.Rate = temp.CurRt;
                }
                var data = result.FirstOrDefault(p => p.PdNo == periodDetail.Period && p.AllocationAccount == costDetail.AcctId && p.AllocationGroup == costDetail.OrgId);
                if (data != null)
                {
                    periodDetail.baseAmt = temp.CurAmt;
                    periodDetail.AllocationAmt = temp.CurAllocAmt;
                    periodDetail.Rate = temp.CurRt;
                }

            }
        }
        //return Ok(poolOrgFinancialDetail);
        return Ok();
    }



    [HttpPost("CalculateAnalogusRates")]
    public async Task<IActionResult> ExecuteSpEf()
    {
        await _context.Database.ExecuteSqlRawAsync(
            "CALL sp_insert_analgs_rt()"
        );

        return Ok("Stored procedure executed successfully");
    }

    [HttpPost("CalculateForcastedRates")]
    public async Task<IActionResult> CalculateForcastedRates()
    {
        try
        {
            _context.Database.SetCommandTimeout(300);
            await _context.Database.ExecuteSqlRawAsync(
                "CALL SP_FORECAST_RT()"
            );
        }
        catch (Exception ex)
        {

        }
        return Ok("Stored procedure executed successfully");
    }
    [HttpGet("role-permissions/{roleId}")]
    public async Task<ActionResult<RolePermissionsResponse>> GetRolePermissions(int roleId)
    {
        var role = await _context.Roles
            .Include(r => r.ScreenPermissions)
            .Include(r => r.FieldPermissions)
            .FirstOrDefaultAsync(r => r.RoleId == roleId);

        if (role == null)
            return NotFound("Role not found");

        var response = new RolePermissionsResponse
        {
            RoleId = role.RoleId
        };

        foreach (var screen in role.ScreenPermissions)
        {
            response.Screens[screen.ScreenCode] = new PermissionAction
            {
                View = screen.CanView,
                Edit = screen.CanEdit
            };
        }

        foreach (var field in role.FieldPermissions)
        {
            response.Fields[field.FieldCode] = new PermissionAction
            {
                View = field.CanView,
                Edit = field.CanEdit
            };
        }

        return Ok(response);
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<PermissionResponse>> GetUserPermissions(int userId)
    {
        var user = await _context.Users
            .Include(u => u.UserRole)
                .ThenInclude(r => r.ScreenPermissions)
            .Include(u => u.UserRole)
                .ThenInclude(r => r.FieldPermissions)
            .Include(u => u.ScreenOverrides)
            .Include(u => u.FieldOverrides)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
            return NotFound();

        var response = new PermissionResponse();

        // Screens
        foreach (var roleScreen in user.UserRole.ScreenPermissions)
        {
            var userOverride = user.ScreenOverrides
                .FirstOrDefault(x => x.ScreenCode == roleScreen.ScreenCode);

            response.Screens[roleScreen.ScreenCode] = new PermissionAction
            {
                View = PermissionResolver.Resolve(userOverride?.CanView, roleScreen.CanView),
                Edit = PermissionResolver.Resolve(userOverride?.CanEdit, roleScreen.CanEdit)
            };
        }

        // Fields
        foreach (var roleField in user.UserRole.FieldPermissions)
        {
            var userOverride = user.FieldOverrides
                .FirstOrDefault(x => x.FieldCode == roleField.FieldCode);

            response.Fields[roleField.FieldCode] = new PermissionAction
            {
                View = PermissionResolver.Resolve(userOverride?.CanView, roleField.CanView),
                Edit = PermissionResolver.Resolve(userOverride?.CanEdit, roleField.CanEdit)
            };
        }

        return Ok(response);
    }

    [HttpGet("GetUserPermissionsV1/{userId}")]
    public async Task<ActionResult<PermissionResponse>> GetUserPermissionsV1(int userId)
    {
        var user = await _context.Users
            .Include(u => u.UserRole)
                .ThenInclude(r => r.ScreenPermissions)
            .Include(u => u.UserRole)
                .ThenInclude(r => r.FieldPermissions)
            .Include(u => u.ScreenOverrides)
            .Include(u => u.FieldOverrides)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
            return NotFound();

        var response = new PermissionResponse();

        // Screens
        foreach (var roleScreen in user.UserRole.ScreenPermissions)
        {
            var userOverride = user.ScreenOverrides
                .FirstOrDefault(x => x.ScreenCode == roleScreen.ScreenCode);

            response.Screens[roleScreen.ScreenCode] = new PermissionAction
            {
                View = PermissionResolver.Resolve(userOverride?.CanView, roleScreen.CanView),
                Edit = PermissionResolver.Resolve(userOverride?.CanEdit, roleScreen.CanEdit)
            };
        }

        // Fields
        foreach (var roleField in user.UserRole.FieldPermissions)
        {
            var userOverride = user.FieldOverrides
                .FirstOrDefault(x => x.FieldCode == roleField.FieldCode);

            response.Fields[roleField.FieldCode] = new PermissionAction
            {
                View = PermissionResolver.Resolve(userOverride?.CanView, roleField.CanView),
                Edit = PermissionResolver.Resolve(userOverride?.CanEdit, roleField.CanEdit)
            };
        }

        return Ok(response);
    }

    [NonController]
    public static class PermissionResolver
    {
        public static bool Resolve(bool? userValue, bool roleValue)
        {
            return userValue ?? roleValue;
        }
    }
    [HttpPost("user-settings")]
    public async Task<IActionResult> SetUserSettings([FromBody] UserSettingsRequest request)
    {
        var user = await _context.Users
            .Include(u => u.ScreenOverrides)
            .Include(u => u.FieldOverrides)
            .FirstOrDefaultAsync(u => u.UserId == request.UserId);

        if (user == null)
            return NotFound("User not found");

        // -----------------------------
        // SCREEN OVERRIDES
        // -----------------------------
        if (request.Screens != null)
        {
            foreach (var screen in request.Screens)
            {
                var existing = user.ScreenOverrides
                    .FirstOrDefault(x => x.ScreenCode == screen.Key);

                if (existing == null)
                {
                    user.ScreenOverrides.Add(new UserScreenPermission
                    {
                        UserId = request.UserId,
                        ScreenCode = screen.Key,
                        CanView = screen.Value.View,
                        CanEdit = screen.Value.Edit
                    });
                }
                else
                {
                    existing.CanView = screen.Value.View;
                    existing.CanEdit = screen.Value.Edit;
                }
            }
        }

        // -----------------------------
        // FIELD OVERRIDES
        // -----------------------------
        if (request.Fields != null)
        {
            foreach (var field in request.Fields)
            {
                var existing = user.FieldOverrides
                    .FirstOrDefault(x => x.FieldCode == field.Key);

                if (existing == null)
                {
                    user.FieldOverrides.Add(new UserFieldPermission
                    {
                        UserId = request.UserId,
                        FieldCode = field.Key,
                        CanView = field.Value.View,
                        CanEdit = field.Value.Edit
                    });
                }
                else
                {
                    existing.CanView = field.Value.View;
                    existing.CanEdit = field.Value.Edit;
                }
            }
        }

        await _context.SaveChangesAsync();
        return Ok("User settings updated successfully");
    }

    [HttpPost("role-settings")]
    public async Task<IActionResult> SetRoleSettings([FromBody] RoleSettingsRequest request)
    {
        var role = await _context.Roles
            .Include(r => r.ScreenPermissions)
            .Include(r => r.FieldPermissions)
            .FirstOrDefaultAsync(r => r.RoleId == request.RoleId);

        if (role == null)
            return NotFound("Role not found");

        // -----------------------------
        // SCREEN PERMISSIONS
        // -----------------------------
        if (request.Screens != null)
        {
            foreach (var screen in request.Screens)
            {

                if (screen.Key == "financialReport")
                {

                }

                var existing = role.ScreenPermissions
                    .FirstOrDefault(x => x.ScreenCode == screen.Key);

                if (existing == null)
                {
                    role.ScreenPermissions.Add(new RoleScreenPermission
                    {
                        RoleId = request.RoleId,
                        ScreenCode = screen.Key,
                        CanView = screen.Value.View,
                        CanEdit = screen.Value.Edit
                    });
                }
                else
                {
                    //if (screen.Value.View)
                    existing.CanView = screen.Value.View;

                    //if (screen.Value.Edit)
                    existing.CanEdit = screen.Value.Edit;
                }
            }
        }

        // -----------------------------
        // FIELD PERMISSIONS
        // -----------------------------
        if (request.Fields != null)
        {
            foreach (var field in request.Fields)
            {
                var existing = role.FieldPermissions
                    .FirstOrDefault(x => x.FieldCode == field.Key);

                if (existing == null)
                {
                    role.FieldPermissions.Add(new RoleFieldPermission
                    {
                        RoleId = request.RoleId,
                        FieldCode = field.Key,
                        CanView = field.Value.View,
                        CanEdit = field.Value.Edit
                    });
                }
                else
                {
                    if (field.Value.View)
                        existing.CanView = field.Value.View;

                    if (field.Value.Edit)
                        existing.CanEdit = field.Value.Edit;
                }
            }
        }

        await _context.SaveChangesAsync();
        return Ok("Role settings updated successfully");
    }

    [HttpPost("role-permissions/bulk")]
    public async Task<IActionResult> BulkImportRolePermissions(
    [FromBody] BulkRolePermissionRequest request)
    {
        var role = await _context.Roles
            .Include(r => r.ScreenPermissions)
            .Include(r => r.FieldPermissions)
            .FirstOrDefaultAsync(r => r.RoleId == request.RoleId);

        if (role == null)
            return NotFound("Role not found");

        // -----------------------------
        // BULK SCREENS
        // -----------------------------
        if (request.Screens != null)
        {
            foreach (var screen in request.Screens)
            {
                var existing = role.ScreenPermissions
                    .FirstOrDefault(x => x.ScreenCode == screen.ScreenCode);

                if (existing == null)
                {
                    role.ScreenPermissions.Add(new RoleScreenPermission
                    {
                        RoleId = request.RoleId,
                        ScreenCode = screen.ScreenCode,
                        CanView = screen.CanView,
                        CanEdit = screen.CanEdit
                    });
                }
                else
                {
                    existing.CanView = screen.CanView;
                    existing.CanEdit = screen.CanEdit;
                }
            }
        }

        // -----------------------------
        // BULK FIELDS
        // -----------------------------
        if (request.Fields != null)
        {
            foreach (var field in request.Fields)
            {
                var existing = role.FieldPermissions
                    .FirstOrDefault(x => x.FieldCode == field.FieldCode);

                if (existing == null)
                {
                    role.FieldPermissions.Add(new RoleFieldPermission
                    {
                        RoleId = request.RoleId,
                        FieldCode = field.FieldCode,
                        CanView = field.CanView,
                        CanEdit = field.CanEdit
                    });
                }
                else
                {
                    existing.CanView = field.CanView;
                    existing.CanEdit = field.CanEdit;
                }
            }
        }

        await _context.SaveChangesAsync();
        return Ok("Bulk role permissions imported successfully");
    }

    [HttpPost("user-permissions/bulk")]
    public async Task<IActionResult> BulkImportUserPermissions(
    [FromBody] BulkUserPermissionRequest request)
    {
        var user = await _context.Users
            .Include(u => u.ScreenOverrides)
            .Include(u => u.FieldOverrides)
            .FirstOrDefaultAsync(u => u.UserId == request.UserId);

        if (user == null)
            return NotFound("User not found");

        // -----------------------------
        // BULK SCREEN OVERRIDES
        // -----------------------------
        if (request.Screens != null)
        {
            foreach (var screen in request.Screens)
            {
                var existing = user.ScreenOverrides
                    .FirstOrDefault(x => x.ScreenCode == screen.ScreenCode);

                if (existing == null)
                {
                    user.ScreenOverrides.Add(new UserScreenPermission
                    {
                        UserId = request.UserId,
                        ScreenCode = screen.ScreenCode,
                        CanView = screen.CanView,
                        CanEdit = screen.CanEdit
                    });
                }
                else
                {
                    existing.CanView = screen.CanView;
                    existing.CanEdit = screen.CanEdit;
                }
            }
        }

        // -----------------------------
        // BULK FIELD OVERRIDES
        // -----------------------------
        if (request.Fields != null)
        {
            foreach (var field in request.Fields)
            {
                var existing = user.FieldOverrides
                    .FirstOrDefault(x => x.FieldCode == field.FieldCode);

                if (existing == null)
                {
                    user.FieldOverrides.Add(new UserFieldPermission
                    {
                        UserId = request.UserId,
                        FieldCode = field.FieldCode,
                        CanView = field.CanView,
                        CanEdit = field.CanEdit
                    });
                }
                else
                {
                    existing.CanView = field.CanView;
                    existing.CanEdit = field.CanEdit;
                }
            }
        }

        await _context.SaveChangesAsync();
        return Ok("Bulk user permissions imported successfully");
    }
    [HttpGet("roles")]
    public async Task<ActionResult<List<RoleResponse>>> GetAllRoles()
    {
        var roles = await _context.Roles
            .Select(r => new RoleResponse
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName
            })
            .ToListAsync();

        return Ok(roles);
    }


    // GET: api/VersionCode
    [HttpGet("GetAllVersionCodes")]
    public async Task<ActionResult<IEnumerable<VersionCode>>> GetAllVersionCodes()
    {
        return await _context.VersionCodes
                             .OrderBy(v => v.Id)
                             .ToListAsync();
    }

    // GET: api/VersionCode/5
    [HttpGet("GetVersionCodeById/{id}")]
    public async Task<ActionResult<VersionCode>> GetVersionCodeById(int id)
    {
        var versionCode = await _context.VersionCodes.FindAsync(id);

        if (versionCode == null)
            return NotFound();

        return versionCode;
    }

    // POST: api/VersionCode
    [HttpPost("CreateVersionCode")]
    public async Task<ActionResult<VersionCode>> CreateVersionCode(VersionCode model)
    {
        if (await _context.VersionCodes
                          .AnyAsync(v => v.VersionCodeValue == model.VersionCodeValue))
        {
            return BadRequest("Version code already exists.");
        }

        _context.VersionCodes.Add(model);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetVersionCodeById), new { id = model.Id }, model);
    }

    // PUT: api/VersionCode/5
    [HttpPut("UpdateVersionCode/{id}")]
    public async Task<IActionResult> UpdateVersionCode(int id, VersionCode model)
    {
        if (id != model.Id)
            return BadRequest();

        var existing = await _context.VersionCodes.FindAsync(id);
        if (existing == null)
            return NotFound();

        // Check duplicate version code (excluding current record)
        if (await _context.VersionCodes
                          .AnyAsync(v => v.VersionCodeValue == model.VersionCodeValue && v.Id != id))
        {
            return BadRequest("Version code already exists.");
        }

        existing.VersionCodeValue = model.VersionCodeValue;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/VersionCode/5
    [HttpDelete("DeleteVersionCode/{id}")]
    public async Task<IActionResult> DeleteVersionCode(int id)
    {
        var versionCode = await _context.VersionCodes.FindAsync(id);

        if (versionCode == null)
            return NotFound();

        _context.VersionCodes.Remove(versionCode);
        await _context.SaveChangesAsync();

        return NoContent();
    }
    [HttpPost("CreateOrganization")]
    public async Task<IActionResult> CreateOrganization([FromBody] Organization org)
    {
        if (org == null)
            return BadRequest();

        await _context.Organizations.AddAsync(org);
        await _context.SaveChangesAsync();

        return Ok(org);
    }

    [HttpGet("GetOrganization/{orgId}")]
    public async Task<IActionResult> GetOrganization(string orgId)
    {
        var org = await _context.Organizations
            .Include(o => o.PlProjects)
            .Include(o => o.PlEmployees)
            .FirstOrDefaultAsync(o => o.OrgId == orgId);

        if (org == null)
            return NotFound("Organization not found");

        return Ok(org);
    }

    [HttpPut("UpdateOrganization/{orgId}")]
    public async Task<IActionResult> UpdateOrganization(string orgId, [FromBody] Organization updatedOrg)
    {
        if (orgId != updatedOrg.OrgId)
            return BadRequest("Organization ID mismatch");

        var org = await _context.Organizations.FindAsync(orgId);

        if (org == null)
            return NotFound("Organization not found");

        org.OrgName = updatedOrg.OrgName;
        org.LvlNo = updatedOrg.LvlNo;
        org.L1OrgName = updatedOrg.L1OrgName;
        org.L2OrgName = updatedOrg.L2OrgName;
        org.L3OrgName = updatedOrg.L3OrgName;
        org.L4OrgName = updatedOrg.L4OrgName;
        org.L5OrgName = updatedOrg.L5OrgName;
        org.L6OrgName = updatedOrg.L6OrgName;
        org.L7OrgName = updatedOrg.L7OrgName;
        org.L8OrgName = updatedOrg.L8OrgName;
        org.L9OrgName = updatedOrg.L9OrgName;

        await _context.SaveChangesAsync();

        return Ok(org);
    }

    [HttpDelete("DeleteOrganization/{orgId}")]
    public async Task<IActionResult> DeleteOrganization(string orgId)
    {
        var org = await _context.Organizations.FindAsync(orgId);

        if (org == null)
            return NotFound("Organization not found");

        _context.Organizations.Remove(org);
        await _context.SaveChangesAsync();

        return Ok("Organization deleted successfully");
    }
}