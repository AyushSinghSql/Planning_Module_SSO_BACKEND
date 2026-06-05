namespace WebApi.Repositories;

using Dapper;
using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NPOI.POIFS.Properties;
using NPOI.SS.Formula.Functions;
using PlanningAPI.Helpers;
using PlanningAPI.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using WebApi.Controllers;
using WebApi.DTO;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models;
using WebApi.Services;
using YourNamespace.Models;

public interface IPlForecastRepository
{
    Task AddAsync(PlForecast forecast);
    Task AddRangeAsync(List<PlForecast> forecasts);
    Task UpdateAsync(PlForecast forecast);
    Task DeleteAsync(int forecastId);
    Task<PlForecast?> GetByIdAsync(int forecastId);
    Task<List<PlForecast>> GetAllAsync();
    Task UpdateAmountAsync(PlForecast forecast);
    Task UpdateAmountAsync(PlForecast forecast, string type);
    Task UpdateAmountAsync(List<PlForecast> forecast, string type);
    Task UpsertAmountAsync(List<PlForecast> forecast, string type);
    Task UpsertAmountAsync(List<PlForecast> forecasts, int plid, int templateId, string type);
    Task UpdateHoursAsync(PlForecast forecast);
    Task UpdateHoursAsync(PlForecast forecast, string type);
    Task UpdateHoursAsync(List<PlForecast> forecasts, string type);
    Task UpsertHoursAsync(List<PlForecast> forecasts, string type);
    Task UpsertHoursAsync(List<PlForecast> forecasts, int plid, int templateId, string type);
    Task<PlanForecastSummary> CalculateCost(int planID, int templateId, string type);
    Task<PlanForecastSummary> CalculateCostAI(int planID, int templateId, string type, int year);
    Task<List<PlForecast>> GetByPlanIdAsync(int planId);
    Task<List<PlForecast>> GetForecastByProjectIDAndVersion(string projId, int version, string type);
    Task<List<PlForecast>> GetByPlanIdandProjectIdAsync(int planId, string ProjectId);
    Task<PlanForecastSummary> CalculateRevenueCost(int planID, int templateId, string type);
    //Task CalculateBurdenCost(int planID, int templateId, string type);
    Task<PlanForecastSummary> CalculateRevenueCostForSelectedHours(int planID, int templateId, string type, List<PlForecast> hoursForecast);
    Task<PlanForecastSummary> CalculateBurdenCost(int planID, int templateId, string type);
}

public class PlForecastRepository : IPlForecastRepository
{
    private readonly MydatabaseContext _context;
    private readonly IConfiguration _config;
    //IOrgService _orgService;
    public decimal ForecastedHours { get; private set; }
    public string SystemName = "";

    public PlForecastRepository(MydatabaseContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
        SystemName = _config.GetValue<string>("SystemSource") ?? "COSTPOINT";
    }
    //public PlForecastRepository(MydatabaseContext context, IConfiguration config, IOrgService orgService)
    //{
    //    _context = context;
    //    _config = config;
    //    _orgService = orgService;
    //    SystemName = _config.GetValue<string>("SystemSource") ?? "COSTPOINT";
    //}
    // Add a new PlForecast
    public async Task AddAsync(PlForecast forecast)
    {
        //forecast.Createdat = DateTime.UtcNow;
        _context.PlForecasts.Add(forecast);
        await _context.SaveChangesAsync();
    }

    public async Task AddRangeAsync(List<PlForecast> forecasts)
    {

        var utcNow = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

        foreach (var forecast in forecasts)
        {
            forecast.Createdat = utcNow;
        }
        await _context.PlForecasts.AddRangeAsync(forecasts);
        await _context.SaveChangesAsync();
    }


    // Update an existing PlForecast
    public async Task UpdateAsync(PlForecast forecast)
    {
        var existingForecast = await _context.PlForecasts
            .FirstOrDefaultAsync(f => f.Forecastid == forecast.Forecastid);

        if (existingForecast != null)
        {
            existingForecast.Forecastedamt = forecast.Forecastedamt;
            existingForecast.ProjId = forecast.ProjId;
            existingForecast.PlId = forecast.PlId;
            existingForecast.EmplId = forecast.EmplId;
            existingForecast.Month = forecast.Month;
            existingForecast.Year = forecast.Year;
            existingForecast.Forecastedhours = forecast.Forecastedhours;
            existingForecast.Updatedat = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateHoursAsync(PlForecast forecast)
    {
        try
        {
            var existingForecast = await _context.PlForecasts
                .FirstOrDefaultAsync(f => f.Forecastid == forecast.Forecastid);

            if (existingForecast != null)
            {
                existingForecast.Forecastedhours = forecast.Forecastedhours;
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {

        }
    }
    public async Task UpdateHoursAsync(PlForecast forecast, string type)
    {
        try
        {
            var existingForecast = await _context.PlForecasts
            .FirstOrDefaultAsync(f => f.Forecastid == forecast.Forecastid);

            if (existingForecast != null)
            {
                if (type.ToUpper() == "BUD" || type.Equals("NBBUD", StringComparison.OrdinalIgnoreCase))
                {
                    existingForecast.Forecastedhours = forecast.Forecastedhours;
                }
                else
                {
                    existingForecast.Forecastedhours = forecast.Actualhours;
                    existingForecast.Actualhours = forecast.Actualhours;

                }
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {

        }
    }

    public async Task UpdateHoursAsync(List<PlForecast> forecasts, string type)
    {
        try
        {
            var existing = _context.PlForecasts.Where(p => forecasts.Select(i => i.Forecastid).Contains(p.Forecastid)).ToList();

            foreach (var forecast in forecasts)
            {
                var existoingForecast = existing.FirstOrDefault(f => f.Forecastid == forecast.Forecastid);

                if (type.ToUpper() == "BUD" || type.Equals("NBBUD", StringComparison.OrdinalIgnoreCase))
                {
                    existoingForecast.Forecastedhours = forecast.Forecastedhours;
                }
                else
                {
                    existoingForecast.Forecastedhours = forecast.Actualhours;
                    existoingForecast.Actualhours = forecast.Actualhours;

                }
                _context.Update(existoingForecast);
            }
            await _context.SaveChangesAsync();

        }
        catch (Exception ex)
        {

        }
    }

    public async Task UpsertHoursAsync(
    List<PlForecast> forecasts,
    string type)
    {
        if (forecasts == null || !forecasts.Any())
        {
            return;
        }

        Helper helper = new Helper(_context, _config);
        

        //---------------------------------------------------------
        // CONSTANTS
        //---------------------------------------------------------

        bool isBudget =
            type.Trim().Equals("BUD", StringComparison.OrdinalIgnoreCase)
            || type.Trim().Equals("NBBUD", StringComparison.OrdinalIgnoreCase);

        var employees = forecasts
            .Select(x => x.EmplId)
            .Distinct()
            .ToList();


        var eacPlids = await _context.PlProjectPlans
           .Where(p => p.FinalVersion == true && p.PlType == "EAC")
           .Select(p => p.PlId.HasValue ? p.PlId.Value : 0)
           .Distinct()
           .ToListAsync();

        var budPlids = await _context.PlProjectPlans
           .Where(p => p.FinalVersion == true && p.PlType != "EAC")
           .Select(p => p.PlId.HasValue ? p.PlId.Value : 0)
           .Distinct()
           .ToListAsync();


        var finalPlids = eacPlids
            .Union(budPlids)
            .ToList();

        var existingplids = forecasts
            .Select(x => x.PlId)
            .Distinct()
            .ToList();
        finalPlids.AddRange(existingplids);
        //---------------------------------------------------------
        // SCHEDULE HELPER
        //---------------------------------------------------------

        Helper scheduleHelper = new Helper(_context, _config);

        //---------------------------------------------------------
        // FORECAST IDS
        //---------------------------------------------------------

        var forecastIds =
            forecasts
                .Select(x => x.Forecastid)
                .Distinct()
                .ToList();

        //---------------------------------------------------------
        // LOAD EXISTING FORECASTS
        //---------------------------------------------------------

        var existingForecasts =
            await _context.PlForecasts
                .Where(x => finalPlids.Contains(x.PlId) && employees.Contains(x.EmplId))
                .ToListAsync();

        var existingMap =
            existingForecasts.ToDictionary(x => x.Forecastid);

        //---------------------------------------------------------
        // EMPLOYEE/MONTH KEYS
        //---------------------------------------------------------

        var employeeMonthKeys =
            forecasts
                .Select(x => new
                {
                    x.EmplId,
                    x.Year,
                    x.Month
                })
                .Distinct()
                .ToList();

        //---------------------------------------------------------
        // LOAD ALL EMPLOYEE HOURS IN ONE QUERY
        //---------------------------------------------------------

        var emplIds =
            employeeMonthKeys
                .Select(x => x.EmplId)
                .Distinct()
                .ToList();

        var years =
            employeeMonthKeys
                .Select(x => x.Year)
                .Distinct()
                .ToList();

        var months =
            employeeMonthKeys
                .Select(x => x.Month)
                .Distinct()
                .ToList();

        var employeeForecasts =
            await (from f in _context.PlForecasts
                   join p in _context.PlProjectPlans
                       on f.PlId equals p.PlId
                   where emplIds.Contains(f.EmplId)
                         && years.Contains(f.Year)
                         && months.Contains(f.Month)
                         && finalPlids.Contains(f.PlId)
                   select new
                   {
                       f.EmplId,
                       f.Year,
                       f.Month,
                       f.Forecastedhours,
                       f.Actualhours,
                       p.PlType,
                       p.FinalVersion
                   })
                   .ToListAsync();
        //var employeeForecasts =
        //    await _context.PlForecasts
        //        .Where(x =>
        //            emplIds.Contains(x.EmplId) &&
        //            years.Contains(x.Year) &&
        //            months.Contains(x.Month) && finalPlids.Contains(x.PlId))
        //        .ToListAsync();

        //---------------------------------------------------------
        // GROUP EXISTING HOURS
        //---------------------------------------------------------

        //var existingHoursLookup =
        //    employeeForecasts
        //        .GroupBy(x => new
        //        {
        //            x.EmplId,
        //            x.Year,
        //            x.Month
        //        })
        //        .ToDictionary(
        //            g => (
        //                g.Key.EmplId,
        //                g.Key.Year,
        //                g.Key.Month
        //            ),
        //            g => isBudget
        //                ? g.Sum(x => x.Forecastedhours)
        //                : g.Sum(x => x.Actualhours)
        //        );


        var existingHoursLookup =
            employeeForecasts
                .GroupBy(x => new
                {
                    x.EmplId,
                    x.Year,
                    x.Month
                })
                .ToDictionary(
                    g => (
                        g.Key.EmplId,
                        g.Key.Year,
                        g.Key.Month
                    ),
                    g =>
                    {
                        var hasFinalEac =
                            g.Any(x => x.PlType == "EAC" && x.FinalVersion.HasValue && x.FinalVersion.Value);

                        return hasFinalEac
                            ? g.Where(x => x.PlType == "EAC" && x.FinalVersion.HasValue && x.FinalVersion.Value)
                               .Sum(x => x.Actualhours)
                            : g.Where(x => x.PlType != "EAC" && x.FinalVersion.HasValue && x.FinalVersion.Value)
                               .Sum(x => x.Forecastedhours);
                    });

        //---------------------------------------------------------
        // VALIDATION
        //---------------------------------------------------------

        foreach (var forecast in forecasts)
        {
            if (forecast.EmplId == "000679")
            {

            }

            if (forecast.EmplId.StartsWith("PLC_", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            decimal hours = 0;
            if(isBudget)
            {
                hours = forecast.Forecastedhours;
            }
            else
            {
                hours = forecast.Actualhours;
            }
            if (string.IsNullOrWhiteSpace(forecast.EmplId) || hours == 0)
                continue;

            //-----------------------------------------------------
            // HOURS
            //-----------------------------------------------------

            decimal incomingHours =
                isBudget
                    ? forecast.Forecastedhours
                    : forecast.Actualhours;

            //-----------------------------------------------------
            // STANDARD HOURS
            //-----------------------------------------------------

            var startDate =
                new DateOnly(
                    forecast.Year,
                    forecast.Month,
                    1);

            var endDate =
                new DateOnly(
                    forecast.Year,
                    forecast.Month,
                    DateTime.DaysInMonth(
                        forecast.Year,
                        forecast.Month));

            var schedule =
                scheduleHelper.GetWorkingDaysForDuration(
                    startDate,
                    endDate);

            decimal standardHours =
                schedule.Sum(x => x.WorkingHours);

            decimal allowedHours =
                standardHours * 1.30m;

            //decimal allowedHours =
            //        standardHours * (1 + (_forecastSettings.AllowedHoursPercentage / 100));

            //-----------------------------------------------------
            // EXISTING HOURS
            //-----------------------------------------------------

            existingHoursLookup.TryGetValue(
                (
                    forecast.EmplId,
                    forecast.Year,
                    forecast.Month
                ),
                out decimal existingHours);

            //-----------------------------------------------------
            // REMOVE CURRENT EXISTING
            //-----------------------------------------------------

            decimal currentExistingHours = 0;

            if (existingMap.TryGetValue(
                forecast.Forecastid,
                out var existing))
            {
                currentExistingHours =
                    isBudget
                        ? existing.Forecastedhours
                        : existing.Actualhours;
            }

            //-----------------------------------------------------
            // FINAL HOURS
            //-----------------------------------------------------

            decimal finalHours =
                existingHours
                - currentExistingHours
                + incomingHours;

            //        decimal finalHours =
            //existingHours + currentExistingHours;

            //-----------------------------------------------------
            // VALIDATION
            //-----------------------------------------------------

            if (finalHours > allowedHours)
            {
                var alternateEmployees =
                    await scheduleHelper.GetAlternateEmployees(
                        forecast.Year,
                        forecast.Month,
                        allowedHours - existingHours + currentExistingHours,
                        allowedHours,
                        standardHours,
                        null,
                        null,
                        null);

                var alternateEmployeeMessage =
                    alternateEmployees.Any()
                        ? "\nAlternative Employees:\n" +
                          string.Join(
                              "\n",
                              alternateEmployees
                                  .Take(5)
                                  .Select((x, i) =>
                                      $"{i + 1}. {x.EmployeeId} - {x.EmployeeName} | " +
                                      $"Available: {x.AvailableHours:N2} hrs | " +
                                      $"Assigned: {x.AssignedHours:N2} hrs"))
                        : "\nNo alternative employees available.";

                throw new Exception(
                    $"Employee {forecast.EmplId} exceeds allowed hours " +
                    $"for {forecast.Month}/{forecast.Year}. " +
                    $"Assigned: {finalHours:N2}, " +
                    $"Allowed: {allowedHours:N2}" +
                    $"{alternateEmployeeMessage}");
            }
        }



        //////////////////////////////////////////////////////////////////////////////////////

        //var forecastIds = forecasts
        //    .Select(x => x.Forecastid)
        //    .ToList();

        //var existingForecasts = await _context.PlForecasts
        //    .Where(x => forecastIds.Contains(x.Forecastid))
        //    .ToListAsync();

        //var existingMap = existingForecasts
        //    .ToDictionary(x => x.Forecastid);

        var plid = forecasts.FirstOrDefault()?.PlId;
        foreach (var forecast in forecasts)
        {
            if (existingMap.TryGetValue(forecast.Forecastid, out var existing))
            {
                // 🔁 UPDATE
                if (type.Equals("BUD", StringComparison.OrdinalIgnoreCase) || type.Equals("NBBUD", StringComparison.OrdinalIgnoreCase))
                {
                    existing.Forecastedhours = forecast.Forecastedhours;
                }
                else
                {
                    existing.Forecastedhours = forecast.Actualhours;
                    existing.Actualhours = forecast.Actualhours;
                }
            }
            else
            {
                // ➕ INSERT
                var newForecast = new PlForecast
                {
                    Forecastid = forecast.Forecastid,
                    Forecastedhours = type.Equals("BUD", StringComparison.OrdinalIgnoreCase) || type.Equals("NBBUD", StringComparison.OrdinalIgnoreCase)
                        ? forecast.Forecastedhours
                        : forecast.Actualhours,
                    Actualhours = type.Equals("BUD", StringComparison.OrdinalIgnoreCase) || type.Equals("NBBUD", StringComparison.OrdinalIgnoreCase)
                        ? 0
                        : forecast.Actualhours,

                    // copy other required fields
                    EmplId = forecast.EmplId,
                    ProjId = forecast.ProjId,
                    OrgId = forecast.OrgId,
                    Plc = forecast.Plc,
                    AcctId = forecast.AcctId,
                    PlId = forecast.PlId,
                    Month = forecast.Month,
                    Year = forecast.Year,
                    empleId = forecast.empleId
                };

                _context.PlForecasts.Add(newForecast);
            }
        }
        try
        {
            await _context.SaveChangesAsync();
            var existingPlan = _context.PlProjectPlans.FirstOrDefault(p => p.PlId == plid);
            if (existingPlan != null)
                await CalculateRevenueCost(existingPlan.PlId.GetValueOrDefault(), existingPlan.TemplateId.GetValueOrDefault(), type);
        }
        catch (Exception ex)
        {

        }
    }

    public async Task UpsertHoursAsync_Working(
List<PlForecast> forecasts,
string type)
    {
        if (forecasts == null || !forecasts.Any())
        {
            return;
        }

        Helper helper = new Helper(_context, _config);


        //---------------------------------------------------------
        // CONSTANTS
        //---------------------------------------------------------

        bool isBudget =
            type.Equals("BUD", StringComparison.OrdinalIgnoreCase)
            || type.Equals("NBBUD", StringComparison.OrdinalIgnoreCase);

        var employees = forecasts
            .Select(x => x.EmplId)
            .Distinct()
            .ToList();


        var eacPlids = await _context.PlProjectPlans
           .Where(p => p.FinalVersion == true && p.PlType == "EAC")
           .Select(p => p.PlId.HasValue ? p.PlId.Value : 0)
           .Distinct()
           .ToListAsync();

        var budPlids = await _context.PlProjectPlans
           .Where(p => p.FinalVersion == true && p.PlType != "EAC")
           .Select(p => p.PlId.HasValue ? p.PlId.Value : 0)
           .Distinct()
           .ToListAsync();


        var finalPlids = eacPlids
            .Union(budPlids)
            .ToList();

        var existingplids = forecasts
            .Select(x => x.PlId)
            .Distinct()
            .ToList();
        finalPlids.AddRange(existingplids);
        //---------------------------------------------------------
        // SCHEDULE HELPER
        //---------------------------------------------------------

        Helper scheduleHelper = new Helper(_context, _config);

        //---------------------------------------------------------
        // FORECAST IDS
        //---------------------------------------------------------

        var forecastIds =
            forecasts
                .Select(x => x.Forecastid)
                .Distinct()
                .ToList();

        //---------------------------------------------------------
        // LOAD EXISTING FORECASTS
        //---------------------------------------------------------

        var existingForecasts =
            await _context.PlForecasts
                .Where(x => finalPlids.Contains(x.PlId) && employees.Contains(x.EmplId))
                .ToListAsync();

        var existingMap =
            existingForecasts.ToDictionary(x => x.Forecastid);

        //---------------------------------------------------------
        // EMPLOYEE/MONTH KEYS
        //---------------------------------------------------------

        var employeeMonthKeys =
            forecasts
                .Select(x => new
                {
                    x.EmplId,
                    x.Year,
                    x.Month
                })
                .Distinct()
                .ToList();

        //---------------------------------------------------------
        // LOAD ALL EMPLOYEE HOURS IN ONE QUERY
        //---------------------------------------------------------

        var emplIds =
            employeeMonthKeys
                .Select(x => x.EmplId)
                .Distinct()
                .ToList();

        var years =
            employeeMonthKeys
                .Select(x => x.Year)
                .Distinct()
                .ToList();

        var months =
            employeeMonthKeys
                .Select(x => x.Month)
                .Distinct()
                .ToList();

        var employeeForecasts =
            await _context.PlForecasts
                .Where(x =>
                    emplIds.Contains(x.EmplId) &&
                    years.Contains(x.Year) &&
                    months.Contains(x.Month) && finalPlids.Contains(x.PlId))
                .ToListAsync();

        //---------------------------------------------------------
        // GROUP EXISTING HOURS
        //---------------------------------------------------------

        var existingHoursLookup =
            employeeForecasts
                .GroupBy(x => new
                {
                    x.EmplId,
                    x.Year,
                    x.Month
                })
                .ToDictionary(
                    g => (
                        g.Key.EmplId,
                        g.Key.Year,
                        g.Key.Month
                    ),
                    g => isBudget
                        ? g.Sum(x => x.Forecastedhours)
                        : g.Sum(x => x.Actualhours)
                );

        //---------------------------------------------------------
        // VALIDATION
        //---------------------------------------------------------

        foreach (var forecast in forecasts)
        {
            if (forecast.EmplId.StartsWith("PLC_", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            decimal hours = 0;
            if (isBudget)
            {
                hours = forecast.Forecastedhours;
            }
            else
            {
                hours = forecast.Actualhours;
            }
            if (string.IsNullOrWhiteSpace(forecast.EmplId) || hours == 0)
                continue;

            //-----------------------------------------------------
            // HOURS
            //-----------------------------------------------------

            decimal incomingHours =
                isBudget
                    ? forecast.Forecastedhours
                    : forecast.Actualhours;

            //-----------------------------------------------------
            // STANDARD HOURS
            //-----------------------------------------------------

            var startDate =
                new DateOnly(
                    forecast.Year,
                    forecast.Month,
                    1);

            var endDate =
                new DateOnly(
                    forecast.Year,
                    forecast.Month,
                    DateTime.DaysInMonth(
                        forecast.Year,
                        forecast.Month));

            var schedule =
                scheduleHelper.GetWorkingDaysForDuration(
                    startDate,
                    endDate);

            decimal standardHours =
                schedule.Sum(x => x.WorkingHours);

            decimal allowedHours =
                standardHours * 1.30m;

            //decimal allowedHours =
            //        standardHours * (1 + (_forecastSettings.AllowedHoursPercentage / 100));

            //-----------------------------------------------------
            // EXISTING HOURS
            //-----------------------------------------------------

            existingHoursLookup.TryGetValue(
                (
                    forecast.EmplId,
                    forecast.Year,
                    forecast.Month
                ),
                out decimal existingHours);

            //-----------------------------------------------------
            // REMOVE CURRENT EXISTING
            //-----------------------------------------------------

            decimal currentExistingHours = 0;

            if (existingMap.TryGetValue(
                forecast.Forecastid,
                out var existing))
            {
                currentExistingHours =
                    isBudget
                        ? existing.Forecastedhours
                        : existing.Actualhours;
            }

            //-----------------------------------------------------
            // FINAL HOURS
            //-----------------------------------------------------

            decimal finalHours =
                existingHours
                - currentExistingHours
                + incomingHours;

            //-----------------------------------------------------
            // VALIDATION
            //-----------------------------------------------------

            if (finalHours > allowedHours)
            {
                var alternateEmployees =
                    await scheduleHelper.GetAlternateEmployees(
                        forecast.Year,
                        forecast.Month,
                        allowedHours - existingHours + currentExistingHours,
                        allowedHours,
                        standardHours,
                        null,
                        null,
                        null);

                var alternateEmployeeMessage =
                    alternateEmployees.Any()
                        ? "\nAlternative Employees:\n" +
                          string.Join(
                              "\n",
                              alternateEmployees
                                  .Take(5)
                                  .Select((x, i) =>
                                      $"{i + 1}. {x.EmployeeId} - {x.EmployeeName} | " +
                                      $"Available: {x.AvailableHours:N2} hrs | " +
                                      $"Assigned: {x.AssignedHours:N2} hrs"))
                        : "\nNo alternative employees available.";

                throw new Exception(
                    $"Employee {forecast.EmplId} exceeds allowed hours " +
                    $"for {forecast.Month}/{forecast.Year}. " +
                    $"Assigned: {finalHours:N2}, " +
                    $"Allowed: {allowedHours:N2}" +
                    $"{alternateEmployeeMessage}");
            }
        }



        //////////////////////////////////////////////////////////////////////////////////////

        //var forecastIds = forecasts
        //    .Select(x => x.Forecastid)
        //    .ToList();

        //var existingForecasts = await _context.PlForecasts
        //    .Where(x => forecastIds.Contains(x.Forecastid))
        //    .ToListAsync();

        //var existingMap = existingForecasts
        //    .ToDictionary(x => x.Forecastid);

        var plid = forecasts.FirstOrDefault()?.PlId;
        foreach (var forecast in forecasts)
        {
            if (existingMap.TryGetValue(forecast.Forecastid, out var existing))
            {
                // 🔁 UPDATE
                if (type.Equals("BUD", StringComparison.OrdinalIgnoreCase) || type.Equals("NBBUD", StringComparison.OrdinalIgnoreCase))
                {
                    existing.Forecastedhours = forecast.Forecastedhours;
                }
                else
                {
                    existing.Forecastedhours = forecast.Actualhours;
                    existing.Actualhours = forecast.Actualhours;
                }
            }
            else
            {
                // ➕ INSERT
                var newForecast = new PlForecast
                {
                    Forecastid = forecast.Forecastid,
                    Forecastedhours = type.Equals("BUD", StringComparison.OrdinalIgnoreCase) || type.Equals("NBBUD", StringComparison.OrdinalIgnoreCase)
                        ? forecast.Forecastedhours
                        : forecast.Actualhours,
                    Actualhours = type.Equals("BUD", StringComparison.OrdinalIgnoreCase) || type.Equals("NBBUD", StringComparison.OrdinalIgnoreCase)
                        ? 0
                        : forecast.Actualhours,

                    // copy other required fields
                    EmplId = forecast.EmplId,
                    ProjId = forecast.ProjId,
                    OrgId = forecast.OrgId,
                    Plc = forecast.Plc,
                    AcctId = forecast.AcctId,
                    PlId = forecast.PlId,
                    Month = forecast.Month,
                    Year = forecast.Year,
                    empleId = forecast.empleId
                };

                _context.PlForecasts.Add(newForecast);
            }
        }
        try
        {
            await _context.SaveChangesAsync();
            var existingPlan = _context.PlProjectPlans.FirstOrDefault(p => p.PlId == plid);
            if (existingPlan != null)
                await CalculateRevenueCost(existingPlan.PlId.GetValueOrDefault(), existingPlan.TemplateId.GetValueOrDefault(), type);
        }
        catch (Exception ex)
        {

        }
    }


    public async Task UpsertHoursAsyncWorking(
List<PlForecast> forecasts,
string type)
    {
        if (forecasts == null || !forecasts.Any())
            return;

        var forecastIds = forecasts
            .Select(x => x.Forecastid)
            .ToList();

        var existingForecasts = await _context.PlForecasts
            .Where(x => forecastIds.Contains(x.Forecastid))
            .ToListAsync();

        var existingMap = existingForecasts
            .ToDictionary(x => x.Forecastid);

        var plid = forecasts.FirstOrDefault()?.PlId;
        foreach (var forecast in forecasts)
        {
            if (existingMap.TryGetValue(forecast.Forecastid, out var existing))
            {
                // 🔁 UPDATE
                if (type.Equals("BUD", StringComparison.OrdinalIgnoreCase) || type.Equals("NBBUD", StringComparison.OrdinalIgnoreCase))
                {
                    existing.Forecastedhours = forecast.Forecastedhours;
                }
                else
                {
                    existing.Forecastedhours = forecast.Actualhours;
                    existing.Actualhours = forecast.Actualhours;
                }
            }
            else
            {
                // ➕ INSERT
                var newForecast = new PlForecast
                {
                    Forecastid = forecast.Forecastid,
                    Forecastedhours = type.Equals("BUD", StringComparison.OrdinalIgnoreCase) || type.Equals("NBBUD", StringComparison.OrdinalIgnoreCase)
                        ? forecast.Forecastedhours
                        : forecast.Actualhours,
                    Actualhours = type.Equals("BUD", StringComparison.OrdinalIgnoreCase) || type.Equals("NBBUD", StringComparison.OrdinalIgnoreCase)
                        ? 0
                        : forecast.Actualhours,

                    // copy other required fields
                    EmplId = forecast.EmplId,
                    ProjId = forecast.ProjId,
                    OrgId = forecast.OrgId,
                    Plc = forecast.Plc,
                    AcctId = forecast.AcctId,
                    PlId = forecast.PlId,
                    Month = forecast.Month,
                    Year = forecast.Year,
                    empleId = forecast.empleId
                };

                _context.PlForecasts.Add(newForecast);
            }
        }
        try
        {
            await _context.SaveChangesAsync();
            var existingPlan = _context.PlProjectPlans.FirstOrDefault(p => p.PlId == plid);
            if (existingPlan != null)
                await CalculateRevenueCost(existingPlan.PlId.GetValueOrDefault(), existingPlan.TemplateId.GetValueOrDefault(), type);
        }
        catch (Exception ex)
        {

        }
    }


    public async Task UpsertHoursAsync(
    List<PlForecast> forecasts,
    int plid,
    int templateId,
    string type)
    {
        if (forecasts == null || !forecasts.Any())
            return;

        //---------------------------------------------------------
        // CONSTANTS
        //---------------------------------------------------------

        bool isBudget =
            type.Equals("BUD", StringComparison.OrdinalIgnoreCase)
            || type.Equals("NBBUD", StringComparison.OrdinalIgnoreCase);

        //---------------------------------------------------------
        // SCHEDULE HELPER
        //---------------------------------------------------------

        Helper scheduleHelper = new Helper(_context, _config);

        //---------------------------------------------------------
        // FORECAST IDS
        //---------------------------------------------------------

        var forecastIds =
            forecasts
                .Select(x => x.Forecastid)
                .Distinct()
                .ToList();

        //---------------------------------------------------------
        // LOAD EXISTING FORECASTS
        //---------------------------------------------------------

        var existingForecasts =
            await _context.PlForecasts
                .Where(x => forecastIds.Contains(x.Forecastid))
                .ToListAsync();

        var existingMap =
            existingForecasts.ToDictionary(x => x.Forecastid);

        //---------------------------------------------------------
        // EMPLOYEE/MONTH KEYS
        //---------------------------------------------------------

        var employeeMonthKeys =
            forecasts
                .Select(x => new
                {
                    x.EmplId,
                    x.Year,
                    x.Month
                })
                .Distinct()
                .ToList();

        //---------------------------------------------------------
        // LOAD ALL EMPLOYEE HOURS IN ONE QUERY
        //---------------------------------------------------------

        var emplIds =
            employeeMonthKeys
                .Select(x => x.EmplId)
                .Distinct()
                .ToList();

        var years =
            employeeMonthKeys
                .Select(x => x.Year)
                .Distinct()
                .ToList();

        var months =
            employeeMonthKeys
                .Select(x => x.Month)
                .Distinct()
                .ToList();

        var employeeForecasts =
            await _context.PlForecasts
                .Where(x =>
                    emplIds.Contains(x.EmplId) &&
                    years.Contains(x.Year) &&
                    months.Contains(x.Month))
                .ToListAsync();

        //---------------------------------------------------------
        // GROUP EXISTING HOURS
        //---------------------------------------------------------

        var existingHoursLookup =
            employeeForecasts
                .GroupBy(x => new
                {
                    x.EmplId,
                    x.Year,
                    x.Month
                })
                .ToDictionary(
                    g => (
                        g.Key.EmplId,
                        g.Key.Year,
                        g.Key.Month
                    ),
                    g => isBudget
                        ? g.Sum(x => x.Forecastedhours)
                        : g.Sum(x => x.Actualhours)
                );

        //---------------------------------------------------------
        // VALIDATION
        //---------------------------------------------------------

        foreach (var forecast in forecasts)
        {
            if (string.IsNullOrWhiteSpace(forecast.EmplId))
                continue;

            //-----------------------------------------------------
            // HOURS
            //-----------------------------------------------------

            decimal incomingHours =
                isBudget
                    ? forecast.Forecastedhours
                    : forecast.Actualhours;

            //-----------------------------------------------------
            // STANDARD HOURS
            //-----------------------------------------------------

            var startDate =
                new DateOnly(
                    forecast.Year,
                    forecast.Month,
                    1);

            var endDate =
                new DateOnly(
                    forecast.Year,
                    forecast.Month,
                    DateTime.DaysInMonth(
                        forecast.Year,
                        forecast.Month));

            var schedule =
                scheduleHelper.GetWorkingDaysForDuration(
                    startDate,
                    endDate);

            decimal standardHours =
                schedule.Sum(x => x.WorkingHours);

            decimal allowedHours =
                standardHours * 1.30m;

            //-----------------------------------------------------
            // EXISTING HOURS
            //-----------------------------------------------------

            existingHoursLookup.TryGetValue(
                (
                    forecast.EmplId,
                    forecast.Year,
                    forecast.Month
                ),
                out decimal existingHours);

            //-----------------------------------------------------
            // REMOVE CURRENT EXISTING
            //-----------------------------------------------------

            decimal currentExistingHours = 0;

            if (existingMap.TryGetValue(
                forecast.Forecastid,
                out var existing))
            {
                currentExistingHours =
                    isBudget
                        ? existing.Forecastedhours
                        : existing.Actualhours;
            }

            //-----------------------------------------------------
            // FINAL HOURS
            //-----------------------------------------------------

            decimal finalHours =
                existingHours
                - currentExistingHours
                + incomingHours;

            //-----------------------------------------------------
            // VALIDATION
            //-----------------------------------------------------

            if (finalHours > allowedHours)
            {
                throw new Exception(
                    $"Employee {forecast.EmplId} exceeds allowed hours " +
                    $"for {forecast.Month}/{forecast.Year}. " +
                    $"Assigned: {finalHours:N2}, " +
                    $"Allowed: {allowedHours:N2}");
            }
        }

        //---------------------------------------------------------
        // UPSERT
        //---------------------------------------------------------

        foreach (var forecast in forecasts)
        {
            if (existingMap.TryGetValue(
                forecast.Forecastid,
                out var existing))
            {
                //-------------------------------------------------
                // UPDATE
                //-------------------------------------------------

                if (isBudget)
                {
                    existing.Forecastedhours =
                        forecast.Forecastedhours;
                }
                else
                {
                    existing.Forecastedhours =
                        forecast.Actualhours;

                    existing.Actualhours =
                        forecast.Actualhours;
                }
            }
            else
            {
                //-------------------------------------------------
                // INSERT
                //-------------------------------------------------

                var newForecast = new PlForecast
                {
                    Forecastid = forecast.Forecastid,

                    Forecastedhours =
                        isBudget
                            ? forecast.Forecastedhours
                            : forecast.Actualhours,

                    Actualhours =
                        isBudget
                            ? 0
                            : forecast.Actualhours,

                    EmplId = forecast.EmplId,
                    ProjId = forecast.ProjId,
                    OrgId = forecast.OrgId,
                    Plc = forecast.Plc,
                    AcctId = forecast.AcctId,
                    PlId = forecast.PlId,
                    Month = forecast.Month,
                    Year = forecast.Year,
                    empleId = forecast.empleId
                };

                _context.PlForecasts.Add(newForecast);
            }
        }

        //---------------------------------------------------------
        // SAVE ONCE
        //---------------------------------------------------------

        await _context.SaveChangesAsync();

        //---------------------------------------------------------
        // CALCULATE
        //---------------------------------------------------------

        await CalculateRevenueCost(
            plid,
            templateId,
            type);
    }

    //    public async Task UpsertHoursAsyncWorking(
    //List<PlForecast> forecasts, int plid, int templateId,
    //string type)
    //    {
    //        if (forecasts == null || !forecasts.Any())
    //            return;

    //        var forecastIds = forecasts
    //            .Select(x => x.Forecastid)
    //            .ToList();

    //        var existingForecasts = await _context.PlForecasts
    //            .Where(x => forecastIds.Contains(x.Forecastid))
    //            .ToListAsync();

    //        var existingMap = existingForecasts
    //            .ToDictionary(x => x.Forecastid);

    //        foreach (var forecast in forecasts)
    //        {
    //            if (existingMap.TryGetValue(forecast.Forecastid, out var existing))
    //            {
    //                // 🔁 UPDATE
    //                if (type.Equals("BUD", StringComparison.OrdinalIgnoreCase) || type.Equals("NBBUD", StringComparison.OrdinalIgnoreCase))
    //                {
    //                    existing.Forecastedhours = forecast.Forecastedhours;
    //                }
    //                else
    //                {
    //                    existing.Forecastedhours = forecast.Actualhours;
    //                    existing.Actualhours = forecast.Actualhours;
    //                }
    //            }
    //            else
    //            {
    //                // ➕ INSERT
    //                var newForecast = new PlForecast
    //                {
    //                    Forecastid = forecast.Forecastid,
    //                    Forecastedhours = type.Equals("BUD", StringComparison.OrdinalIgnoreCase) || type.Equals("NBBUD", StringComparison.OrdinalIgnoreCase)
    //                        ? forecast.Forecastedhours
    //                        : forecast.Actualhours,
    //                    Actualhours = type.Equals("BUD", StringComparison.OrdinalIgnoreCase) || type.Equals("NBBUD", StringComparison.OrdinalIgnoreCase)
    //                        ? 0
    //                        : forecast.Actualhours,

    //                    // copy other required fields
    //                    EmplId = forecast.EmplId,
    //                    ProjId = forecast.ProjId,
    //                    OrgId = forecast.OrgId,
    //                    Plc = forecast.Plc,
    //                    AcctId = forecast.AcctId,
    //                    PlId = forecast.PlId,
    //                    Month = forecast.Month,
    //                    Year = forecast.Year,
    //                    empleId = forecast.empleId
    //                };

    //                _context.PlForecasts.Add(newForecast);
    //            }
    //        }
    //        try
    //        {
    //            await _context.SaveChangesAsync();
    //            await CalculateRevenueCost(plid, templateId, type);
    //        }
    //        catch (Exception ex)
    //        {

    //        }
    //    }

    public async Task UpdateAmountAsync(PlForecast forecast)
    {
        var existingForecast = await _context.PlForecasts
            .FirstOrDefaultAsync(f => f.Forecastid == forecast.Forecastid);

        if (existingForecast != null)
        {
            existingForecast.Forecastedamt = forecast.Forecastedamt;

            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateAmountAsync(PlForecast forecast, string type)
    {
        var existingForecast = await _context.PlForecasts
            .FirstOrDefaultAsync(f => f.Forecastid == forecast.Forecastid);

        if (existingForecast != null)
        {
            if (type.ToUpper() == "BUD" || type.Equals("NBBUD", StringComparison.OrdinalIgnoreCase))
            {
                existingForecast.Forecastedamt = forecast.Forecastedamt;
            }
            else
            {
                existingForecast.Forecastedamt = forecast.Actualamt;
                existingForecast.Actualamt = forecast.Actualamt;

            }
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateAmountAsync(List<PlForecast> forecasts, string type)
    {
        var existing = _context.PlForecasts.Where(p => forecasts.Select(i => i.Forecastid).Contains(p.Forecastid)).ToList();
        foreach (var forecast in forecasts)
        {
            var existoingForecast = existing.FirstOrDefault(f => f.Forecastid == forecast.Forecastid);
            //_context.Attach(forecast);
            if (type.ToUpper() == "BUD" || type.Equals("NBBUD", StringComparison.OrdinalIgnoreCase))
            {
                existoingForecast.Forecastedamt = forecast.Forecastedamt;
            }
            else
            {
                existoingForecast.Forecastedamt = forecast.Actualamt;
                existoingForecast.Actualamt = forecast.Actualamt;

            }
            _context.Update(existoingForecast);
            //_context.Entry(forecast).State = EntityState.Modified;
        }
        await _context.SaveChangesAsync();

    }

    public async Task UpsertAmountAsync(
    List<PlForecast> forecasts,
    string type)
    {
        if (forecasts == null || !forecasts.Any())
            return;

        var forecastIds = forecasts
            .Select(x => x.Forecastid)
            .ToList();

        var existingForecasts = await _context.PlForecasts
            .Where(x => forecastIds.Contains(x.Forecastid))
            .ToListAsync();

        var existingMap = existingForecasts
            .ToDictionary(x => x.Forecastid);

        var plid = forecasts.FirstOrDefault()?.PlId;

        foreach (var forecast in forecasts)
        {
            if (existingMap.TryGetValue(forecast.Forecastid, out var existing))
            {
                // 🔁 UPDATE
                if (type.Equals("BUD", StringComparison.OrdinalIgnoreCase) || type.Equals("NBBUD", StringComparison.OrdinalIgnoreCase))
                {
                    existing.Forecastedamt = forecast.Forecastedamt;
                }
                else
                {
                    existing.Forecastedamt = forecast.Actualamt;
                    existing.Actualamt = forecast.Actualamt;
                }
            }
            else
            {
                // ➕ INSERT
                var newForecast = new PlForecast
                {
                    Forecastid = forecast.Forecastid,

                    Forecastedamt = type.Equals("BUD", StringComparison.OrdinalIgnoreCase) || type.Equals("NBBUD", StringComparison.OrdinalIgnoreCase)
                        ? forecast.Forecastedamt
                        : forecast.Actualamt,

                    Actualamt = type.Equals("BUD", StringComparison.OrdinalIgnoreCase) || type.Equals("NBBUD", StringComparison.OrdinalIgnoreCase)
                        ? null
                        : forecast.Actualamt,

                    // ⚠️ copy mandatory fields
                    EmplId = forecast.EmplId,
                    ProjId = forecast.ProjId,
                    OrgId = forecast.OrgId,
                    Plc = forecast.Plc,
                    AcctId = forecast.AcctId,
                    DctId = forecast.DctId,
                    PlId = forecast.PlId,
                    Month = forecast.Month,
                    Year = forecast.Year,

                };

                _context.PlForecasts.Add(newForecast);
            }
        }

        await _context.SaveChangesAsync();
        var existingPlan = _context.PlProjectPlans.FirstOrDefault(p => p.PlId == plid);
        if (existingPlan != null)
            await CalculateRevenueCost(existingPlan.PlId.GetValueOrDefault(), existingPlan.TemplateId.GetValueOrDefault(), type);
    }


    public async Task UpsertAmountAsync(
List<PlForecast> forecasts, int plid, int templateId,
string type)
    {
        if (forecasts == null || !forecasts.Any())
            return;

        var forecastIds = forecasts
            .Select(x => x.Forecastid)
            .ToList();

        var existingForecasts = await _context.PlForecasts
            .Where(x => forecastIds.Contains(x.Forecastid))
            .ToListAsync();

        var existingMap = existingForecasts
            .ToDictionary(x => x.Forecastid);

        foreach (var forecast in forecasts)
        {
            if (existingMap.TryGetValue(forecast.Forecastid, out var existing))
            {
                // 🔁 UPDATE
                if (type.Equals("BUD", StringComparison.OrdinalIgnoreCase) || type.Equals("NBBUD", StringComparison.OrdinalIgnoreCase))
                {
                    existing.Forecastedamt = forecast.Forecastedamt;
                }
                else
                {
                    existing.Forecastedamt = forecast.Actualamt;
                    existing.Actualamt = forecast.Actualamt;
                }
            }
            else
            {
                // ➕ INSERT
                var newForecast = new PlForecast
                {
                    Forecastid = forecast.Forecastid,

                    Forecastedamt = type.Equals("BUD", StringComparison.OrdinalIgnoreCase) || type.Equals("NBBUD", StringComparison.OrdinalIgnoreCase)
                        ? forecast.Forecastedamt
                        : forecast.Actualamt,

                    Actualamt = type.Equals("BUD", StringComparison.OrdinalIgnoreCase) || type.Equals("NBBUD", StringComparison.OrdinalIgnoreCase)
                        ? null
                        : forecast.Actualamt,

                    // ⚠️ copy mandatory fields
                    EmplId = forecast.EmplId,
                    ProjId = forecast.ProjId,
                    OrgId = forecast.OrgId,
                    Plc = forecast.Plc,
                    AcctId = forecast.AcctId,
                    DctId = forecast.DctId,
                    PlId = forecast.PlId,
                    Month = forecast.Month,
                    Year = forecast.Year,

                };

                _context.PlForecasts.Add(newForecast);
            }
        }
        try
        {
            await _context.SaveChangesAsync();
            await CalculateRevenueCost(plid, templateId, type);
        }
        catch (Exception ex)
        {

        }
    }


    // Delete a PlForecast by Forecastid
    public async Task DeleteAsync(int forecastId)
    {
        var forecast = await _context.PlForecasts
            .FirstOrDefaultAsync(f => f.Forecastid == forecastId);

        if (forecast != null)
        {
            _context.PlForecasts.Remove(forecast);
            await _context.SaveChangesAsync();
        }
    }

    // Optionally: Get a forecast by ID
    public async Task<PlForecast?> GetByIdAsync(int forecastId)
    {
        return await _context.PlForecasts
               .Include(p => p.Empl)
               .Include(p => p.Proj)
            .FirstOrDefaultAsync(f => f.Forecastid == forecastId);
    }

    // Optionally: Get all forecasts
    public async Task<List<PlForecast>> GetAllAsync()
    {
        return await _context.PlForecasts
    .Include(p => p.Empl)
    .Include(p => p.Proj)
    .ToListAsync();

    }

    // Optionally: Get a forecast by ID
    public async Task<List<PlForecast>> GetByPlanIdAsync(int planId)
    {
        return await _context.PlForecasts
               .Include(p => p.Empl)
               .Include(p => p.Proj)
            .Where(f => f.PlId == planId).ToListAsync();
    }
    public Task<List<PlForecast>> GetByPlanIdandProjectIdAsync(int planId, string ProjectId)
    {
        return _context.PlForecasts.AsNoTracking()
                       .Include(p => p.Emple)
                       .Include(p => p.DirectCost)
                       .Include(p => p.Proj)
                    .Where(f => f.PlId == planId && f.ProjId == ProjectId).ToListAsync();
    }
    public decimal GetForecastedHours()
    {
        return ForecastedHours;
    }

    public async Task<PlanForecastSummary> CalculateCost(int planID, int templateId, string type)
    {
        string escallation_month = "0", escallation_percent = "0";
        string projId = string.Empty, organization = string.Empty, projType = string.Empty, revenueFormula = string.Empty;
        decimal RevenueAdj = 0, fundingValue = 0, ActualRevenue = 0, fixed_fee = 0;
        List<EmployeeForecastSummary> employeeForecastSummary = new List<EmployeeForecastSummary>();
        List<EmployeeForecastSummary> directCostForecastSummary = new List<EmployeeForecastSummary>();
        List<MonthlyCostRevenue> MonthlyCostRevenue = new List<MonthlyCostRevenue>();
        bool OverrideFRevenueSettings = false;
        List<ProjRevWrkPd> adj = new List<ProjRevWrkPd>();

        PlanForecastSummary planForecastSummary = new PlanForecastSummary();
        planForecastSummary.EmployeeForecastSummary = new List<EmployeeForecastSummary>();

        FinanceHelper financeHelper = new FinanceHelper(_context, projId);

        var projPlan = _context.PlProjectPlans.Where(p => p.PlId == planID).Include(p => p.Proj).FirstOrDefault();
        if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
        {
            projId = projPlan.Proj.ProjId;
            projType = projPlan.Proj.ProjTypeDc;
            //projPlan.ProjEndDt = projPlan.Proj.ProjEndDt;
            //projPlan.ProjStartDt = projPlan.Proj.ProjStartDt;
            fundingValue = projPlan.Proj.proj_f_tot_amt.GetValueOrDefault();
        }
        else
        {
            projId = projPlan.ProjId;
            var projPlanNBBUD = _context.NewBusinessBudgets.Where(p => p.BusinessBudgetId == projPlan.ProjId).FirstOrDefault();
            projPlan.ProjEndDt = projPlanNBBUD?.EndDate != null
                                    ? DateOnly.FromDateTime(projPlanNBBUD.EndDate)
                                    : (DateOnly?)null;
            projPlan.ProjStartDt = projPlanNBBUD?.StartDate != null
                                    ? DateOnly.FromDateTime(projPlanNBBUD.StartDate)
                                    : (DateOnly?)null;
            organization = "New Business";
            escallation_percent = projPlanNBBUD?.EscalationRate != null ? projPlanNBBUD.EscalationRate.ToString() : "3";
            fundingValue = 20000000000;
        }

        if (projId != null && projPlan.PlType.ToUpper() != "NBBUD")
        {
            organization = _context.PlProjects
                .Where(p => p.ProjId == projId)
                .Include(p => p.Org)
                .Select(p => p.Org.OrgName)
                .FirstOrDefault();
        }


        var forecastsDirectCosts = await _context.PlForecasts
                    .Include(p => p.DirectCost)
                .Include(p => p.Proj)
                    .Where(f => f.Emple == null && f.ProjId == projId && f.PlId == planID).ToListAsync();


        var forecasts = await _context.PlForecasts
                        .Include(p => p.Emple)
                        .Include(p => p.Proj)
                        .Where(f => f.Emple != null && f.ProjId == projId && f.PlId == planID).ToListAsync();

        if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
        {
            escallation_month =
                        _context.PlConfigValues
                            .Where(r => r.Name.ToLower() == "escallation_month"
                                     && (r.ProjId == projId || r.ProjId == "xxxxx"))
                            .OrderByDescending(r => r.ProjId == projId) // prefer project-specific
                            .Select(r => r.Value)
                            .FirstOrDefault() ?? "3";

            escallation_percent =
                        _context.PlConfigValues
                            .Where(r => r.Name.ToLower() == "escallation_percent"
                                     && (r.ProjId == projId || r.ProjId == "xxxxx"))
                            .OrderByDescending(r => r.ProjId == projId) // prefer project-specific
                            .Select(r => r.Value)
                            .FirstOrDefault() ?? "3";
        }
        else
        {
            escallation_month = _context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "escallation_month" && r.ProjId == "xxxxx")?.Value ?? "7";
        }
        if (string.IsNullOrEmpty(escallation_month))
        {
            escallation_month = "7";
        }
        if (string.IsNullOrEmpty(escallation_percent))
        {
            escallation_percent = "0";
        }
        List<EmplSchedule> res = new List<EmplSchedule>();
        List<DirectCostSchedule> res1 = new List<DirectCostSchedule>();

        //if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
        {
            res = financeHelper.CalculateSalaryPlan(forecasts, 0, 2025, Convert.ToInt32(escallation_month), Convert.ToDecimal(escallation_percent), projPlan);
            res1 = financeHelper.CalculateDirectCostPlan(forecastsDirectCosts);
        }


        var Adjusted = forecasts
                        .Concat(forecastsDirectCosts)
                        .OrderBy(x => x.Year)
                        .ThenBy(x => x.Month)
                        .ToList();

        var revenueSetup = _context.ProjBgtRevSetups.FirstOrDefault(p => p.ProjId == projId && p.VersionNo == projPlan.Version && p.BgtType == projPlan.PlType);
        adj = _context.ProjRevWrkPds.Where(p => p.Pl_Id == planID).ToList();

        if (revenueSetup != null)
        {
            revenueFormula = revenueSetup.RevType;
            if (revenueFormula.ToUpper() == "CPFF")
            {
                fixed_fee = revenueSetup.LabFeeRt.GetValueOrDefault();
            }
            if (revenueSetup.OverrideRevAdjFl)
            {
                //adj = _context.ProjRevWrkPds.Where(p => p.ProjId == projId && p.VersionNo == projPlan.Version && p.BgtType == projPlan.PlType).ToList();
                RevenueAdj = adj.Sum(p => p.RevAdj).GetValueOrDefault() + adj.Sum(p => p.RevAdj1).GetValueOrDefault();
                ActualRevenue = adj.Sum(p => p.RevAmt).GetValueOrDefault();
            }
            if (revenueSetup.OverrideFundingCeilingFl)
            {
                if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
                    fundingValue = revenueSetup.AtRiskAmt + projPlan.Proj.proj_f_tot_amt.GetValueOrDefault();
                else
                    fundingValue = 2000000000;
            }
            else
            {
                if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
                    fundingValue = projPlan.Proj.proj_f_tot_amt.GetValueOrDefault();
                else
                    fundingValue = 2000000000;
            }
            if (revenueSetup.OverrideRevSettingFl)
            {
                OverrideFRevenueSettings = true;
            }

        }
        else
        {

            var parts = projId.Split('.', StringSplitOptions.RemoveEmptyEntries);
            var prefixes = Enumerable
                .Range(2, parts.Length - 1)
                .Select(i => string.Join('.', parts.Take(i)))
                .ToList();
            var ProjRevDef = _context.ProjRevDefinitions
                .AsNoTracking()
                .Where(p => prefixes.Contains(p.ProjectId))
                .OrderByDescending(p => p.ProjectId.Length)
                .FirstOrDefault();

            if (ProjRevDef != null)
            {
                revenueFormula = ProjRevDef.RevenueFormulaCd;
                //laborFees = ProjRevDef.GetValueOrDefault();
                //nonLaborFees = ProjRevDef.RevenueCalculation1Amount.GetValueOrDefault();
            }
            else
            {
                revenueFormula = projType;
            }
            if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
                fundingValue = projPlan.Proj.proj_f_tot_amt.GetValueOrDefault();
            else
                fundingValue = 2000000000;
        }

        foreach (var emplSchedule in res)
        {
            bool revenue = true;
            decimal? ceilingemplHour = null;
            decimal? ceilingLabCatHour = null;
            decimal? ceilingHours = null;


            var employeeForecast = Adjusted.Where(f => f.Emple != null)
                                    .Where(p => p.EmplId == emplSchedule.EmpId && p.Plc == emplSchedule.PlcCode && p.AcctId == emplSchedule.AccID && p.OrgId == emplSchedule.OrgID)
                                    .OrderBy(p => p.Year)
                                    .ThenBy(p => p.Month)
                                    .ToList();

            if (emplSchedule.EmpId == "000972" && emplSchedule.PlcCode == "SS4" && emplSchedule.AccID == "500-001-110")
            {

            }
            decimal totalHours = 0;

            foreach (var monthlySalary in emplSchedule.payrollSalary)
            {
                try
                {
                    var monthDetails = employeeForecast.FirstOrDefault(p => p.Month == monthlySalary.Month && p.Year == monthlySalary.Year);
                    if (monthDetails != null)
                    {
                        if (monthlySalary.Year == 2026 && monthlySalary.Month == 1 && emplSchedule.EmpId == "9030916")
                        {
                        }
                        if (projPlan.PlType == "EAC")
                            monthlySalary.Hours = monthDetails.Actualhours;

                        else
                            monthlySalary.Hours = monthDetails.Forecastedhours;

                        if (emplSchedule.isRev)
                        {
                            if (ceilingHours.HasValue)
                            {
                                totalHours = totalHours + monthDetails.Forecastedhours;
                                decimal minHours = Math.Min(Convert.ToDecimal(totalHours), ceilingHours.GetValueOrDefault());

                                if (totalHours == minHours)
                                {
                                    monthlySalary.Revenue = monthDetails.Revenue;
                                    monthlySalary.Fees = monthDetails.Fees;
                                }
                                else
                                {
                                    if (revenue)
                                    {
                                        var hoursConsideredForRevenue = minHours - (totalHours - monthDetails.Forecastedhours);//monthDetails.Forecastedhours = 
                                        monthlySalary.Revenue = monthDetails.Revenue;
                                        monthlySalary.Fees = monthDetails.Fees;
                                        revenue = false;
                                    }
                                    else
                                    {
                                        monthlySalary.Revenue = monthDetails.Revenue;
                                        monthlySalary.Fees = monthDetails.Fees;
                                    }
                                }
                            }
                            else
                            {
                                monthlySalary.Revenue = monthDetails.Revenue;
                                monthlySalary.Fees = monthDetails.Fees;

                            }
                        }
                        monthlySalary.TotalBurdenCost = monthDetails.Cost + monthDetails.Burden;
                        monthlySalary.Cost = monthDetails.Cost;
                        monthlySalary.Fringe = monthDetails.Fringe;
                        monthlySalary.Overhead = monthDetails.Overhead;
                        monthlySalary.Gna = monthDetails.Gna;
                        monthlySalary.Burden = monthDetails.Burden;
                        monthlySalary.Fees = monthDetails.Fees;
                        monthlySalary.Materials = monthDetails.Materials;
                        monthlySalary.Hr = monthDetails.Hr;

                    }
                }
                catch (Exception ex)
                {

                }
            }
        }

        var tempss = Adjusted
                    .GroupBy(f => new { f.Month, f.Year }).ToList();

        List<MonthlyRevenueSummary> monthlyRevenueSummary = new List<MonthlyRevenueSummary>();

        monthlyRevenueSummary = Adjusted
            .GroupBy(f => new { f.Month, f.Year })
            .Select(g => new MonthlyRevenueSummary
            {
                Month = g.Key.Month,
                Year = g.Key.Year,
                Revenue = g.Sum(p => p.Revenue),
                Cost = (new DateOnly(g.Key.Year, g.Key.Month, 1) < projPlan.ClosedPeriod.GetValueOrDefault() && projPlan.PlType.ToUpper() == "EAC")
                    ? g.Where(p => p.DirectCost == null).Sum(p => (p.Cost + p.Burden))
                    : g.Where(p => p.DirectCost == null).Sum(p => (p.Cost + p.Burden)),
                OtherDifrectCost = g.Where(p => p.DirectCost != null).Sum(p => (p.Cost + p.Burden))

            })
            .ToList();

        foreach (var monthlyRevenue in monthlyRevenueSummary)
        {
            var adustment = adj.FirstOrDefault(p => p.EndDate.GetValueOrDefault().Year == monthlyRevenue.Year && p.Period == monthlyRevenue.Month);
            if (adustment != null)
            {
                monthlyRevenue.Revenue += adustment.RevAdj.GetValueOrDefault();
                monthlyRevenue.Revenue += adustment.RevAdj1.GetValueOrDefault();

                if (revenueFormula.ToUpper() == "CPFC")
                {
                    if (projPlan.Type.Trim().ToUpper() == "A")
                    {
                        monthlyRevenue.Revenue += adustment.ActualFeeAmountOnCost;
                    }
                    else
                    {
                        monthlyRevenue.Revenue += adustment.TargetFeeAmountOnCost;

                    }
                }
            }
        }



        if (projPlan.PlType.ToUpper() == "EAC")
        {
            //var actualMonthlySummary = await _context.PSRFinalData
            //    .Where(p => p.ProjId.StartsWith(projId) && (p.RateType == "A" || p.RateType == "N"))
            //    .GroupBy(p => new { p.PdNo, p.FyCd, p.SubTotTypeNo, p.ProjId })
            //    .Select(g => new MonthlySummary
            //    {
            //        Month = g.Key.PdNo,
            //        Year = Convert.ToInt16(g.Key.FyCd),
            //        Cost = g.Sum(x => x.PtdIncurAmt),
            //        subTotalType = g.Key.SubTotTypeNo
            //        //Cost = (g.Key.SubTotTypeNo == 2 || g.Key.SubTotTypeNo == 3) ? g.Sum(x => x.PtdIncurAmt) : 0m

            //    })
            //    .ToListAsync();

            var actualMonthlySummary = await _context.PSRFinalData
                    .Where(p => p.ProjId.StartsWith(projId) && (p.RateType == projPlan.Type || p.RateType == "N"))
                    .GroupBy(p => new { p.PdNo, p.FyCd, p.SubTotTypeNo })
                    .Select(g => new MonthlySummary
                    {
                        Month = g.Key.PdNo,
                        Year = Convert.ToInt16(g.Key.FyCd),
                        Cost = g.Sum(x => x.PtdIncurAmt),
                        subTotalType = g.Key.SubTotTypeNo
                        //Cost = (g.Key.SubTotTypeNo == 2 || g.Key.SubTotTypeNo == 3) ? g.Sum(x => x.PtdIncurAmt) : 0m

                    })
                    .ToListAsync();

            var summaryLookup = actualMonthlySummary
                .ToDictionary(
                    x => (x.Month, x.Year, x.subTotalType),
                    x => x.Cost);

            foreach (var temp in monthlyRevenueSummary)
            {
                summaryLookup.TryGetValue((temp.Month, temp.Year, 1), out var revenue);
                summaryLookup.TryGetValue((temp.Month, temp.Year, 2), out var laborCost);
                summaryLookup.TryGetValue((temp.Month, temp.Year, 3), out var otherDirectCost);
                summaryLookup.TryGetValue((temp.Month, temp.Year, 4), out var indirectCost);


                if (new DateOnly(temp.Year, temp.Month, 1) < projPlan.ClosedPeriod.GetValueOrDefault())
                {
                    temp.Revenue = revenue;

                    var adustment = adj.FirstOrDefault(p => p.EndDate.GetValueOrDefault().Year == temp.Year && p.Period == temp.Month);
                    if (adustment != null)
                    {
                        temp.Revenue += adustment.RevAdj.GetValueOrDefault();
                        temp.Revenue += adustment.RevAdj1.GetValueOrDefault();
                        if (revenueFormula.ToUpper() == "CPFC")
                        {
                            if (projPlan.Type.Trim().ToUpper() == "A")
                            {
                                temp.Revenue += adustment.ActualFeeAmountOnCost;
                            }
                            else
                            {
                                temp.Revenue += adustment.TargetFeeAmountOnCost;

                            }
                        }
                    }

                    if (revenueFormula.ToUpper() == "UNIT")
                    {
                        //temp.Revenue = adustment.RevAmt.GetValueOrDefault();
                        temp.Revenue = adustment?.RevAmt ?? 0;
                    }
                    //temp.Cost = laborCost  ;
                    //temp.OtherDifrectCost = otherDirectCost;
                }
            }
        }

        if ((revenueSetup != null && revenueSetup.OverrideRevAdjFl) || revenueFormula == "UNIT")
        {
            foreach (var monthlyRevenue in monthlyRevenueSummary)
            {
                var adustment = adj.FirstOrDefault(p => p.EndDate.GetValueOrDefault().Year == monthlyRevenue.Year && p.Period == monthlyRevenue.Month);
                if (adustment != null)
                {
                    monthlyRevenue.Revenue = adustment.RevAmt.GetValueOrDefault();
                }
            }
        }

        employeeForecastSummary = Adjusted
                 .Where(f => f.Emple != null && f.PlId == planID)
                 .GroupBy(f => new { f.EmplId, f.Emple.PerHourRate, f.ProjId, f.Emple.OrgId, f.Emple.AccId, f.Emple.PlcGlcCode, f.Emple.FirstName, f.Emple.LastName })
                 .Select(g => new EmployeeForecastSummary
                 {
                     OrgID = organization,
                     //AccID = g.Key.AccId ?? string.Empty,
                     AccID = _context.Accounts.Where(p => p.AcctId == g.Key.AccId).Select(p => p.AcctId + " - " + p.AcctName).FirstOrDefault(),
                     EmplId = g.Key.EmplId?.ToString() ?? string.Empty,
                     Name = g.Key.FirstName + " " + g.Key.LastName,
                     PerHourRate = g.Key.PerHourRate ?? 0m,
                     TotalForecastedHours = g.Sum(f => f.Forecastedhours),
                     TotalForecastedCost = (decimal)(g.Sum(f => f.Cost)),
                     Fringe = (decimal)(g.Sum(f => f.Fringe)),
                     Overhead = (decimal)(g.Sum(f => f.Overhead)),
                     Gna = (decimal)(g.Sum(f => f.Gna)),
                     Hr = (decimal)(g.Sum(f => f.Hr)),
                     Materials = (decimal)(g.Sum(f => f.Materials)),
                     Burden = g.Sum(f => f.Burden),
                     TotalBurdonCost = g.Sum(f => f.TotalBurdenCost),
                     //TNMRevenue = (decimal)(g.Sum(f => f.TNMRevenue)),
                     //CPFFRevenue = (decimal)(g.Sum(f => f.CCFFRevenue)),
                     Revenue = (decimal)(g.Sum(f => f.Revenue)),
                     Fees = (decimal)(g.Sum(f => f.Fees)),
                     PlcCode = !string.IsNullOrEmpty(g.Key.PlcGlcCode)
                                    ? g.Key.PlcGlcCode + "-(" + (_context.PlcCodes.FirstOrDefault(p => p.LaborCategoryCode == g.Key.PlcGlcCode)?.Description ?? "") + ")"
                                    : string.Empty,
                     emplSchedule = res.FirstOrDefault(p => p.EmpId == g.Key.EmplId && p.PlcCode == g.Key.PlcGlcCode && p.AccID == g.Key.AccId && p.OrgID == g.Key.OrgId) ?? new EmplSchedule() //res.FirstOrDefault(p => p.EmpId == g.Key.EmplId)

                 }).ToList();

        directCostForecastSummary = Adjusted
                 .Where(f => f.Emple == null && f.PlId == planID)
                 .GroupBy(f => new { f.DirectCost?.DctId, f.DirectCost?.Id, f.DirectCost?.OrgId, f.ProjId, f.DirectCost?.AcctId })
                 .Select(g => new EmployeeForecastSummary
                 {
                     OrgID = organization,
                     //AccID = g.Key.AcctId ?? string.Empty,
                     AccID = _context.Accounts.Where(p => p.AcctId == g.Key.AcctId).Select(p => p.AcctId + " - " + p.AcctName).FirstOrDefault(),
                     EmplId = g.Key.Id?.ToString() ?? string.Empty,
                     PerHourRate = 0m,
                     TotalForecastedHours = g.Sum(f => f.Forecastedhours),
                     TotalForecastedCost = (decimal)(g.Sum(f => f.Cost)),
                     Fringe = (decimal)(g.Sum(f => f.Fringe)),
                     Overhead = (decimal)(g.Sum(f => f.Overhead)),
                     Gna = (decimal)(g.Sum(f => f.Gna)),
                     Hr = (decimal)(g.Sum(f => f.Hr)),
                     Materials = (decimal)(g.Sum(f => f.Materials)),
                     Burden = g.Sum(f => f.Burden),
                     TotalBurdonCost = g.Sum(f => f.TotalBurdenCost),
                     Revenue = g.Sum(f => f.Revenue),
                     //TNMRevenue = (decimal)(g.Sum(f => f.TNMRevenue)),
                     //CPFFRevenue = (decimal)(g.Sum(f => f.CCFFRevenue)),
                     Fees = (decimal)(g.Sum(f => f.Fees)), //PlcCode = g.Key.PlcGlc ??string.Empty,
                     //PlcCode = !string.IsNullOrEmpty(g.Key.PlcGlc)
                     //           ? g.Key.PlcGlc + "-(" + (_context.PlcCodes.FirstOrDefault(p => p.LaborCategoryCode == g.Key.PlcGlc)?.Description ?? "") + ")"
                     //           : string.Empty,
                     directCostSchedule = res1.FirstOrDefault(p => p.DctId == g.Key.DctId) ?? new DirectCostSchedule()


                 }).ToList();


        if (employeeForecastSummary.Any())
        {
            var keys = employeeForecastSummary
                    .Select(c => $"{c.emplSchedule.OrgID}|{c.emplSchedule.AccID}")
                    .ToList();

            var result = _context.PlOrgAcctPoolMappings
                            .Where(plm => keys.Contains(plm.OrgId + "|" + plm.AccountId))
                            .Join(_context.AccountGroups,
                                plm => plm.PoolId,
                                pool => pool.Code,
                                (plm, pool) => new
                                {
                                    plm.OrgId,
                                    plm.AccountId,
                                    pool.Code,
                                    pool.Name,
                                    pool.Type
                                })
                            .Distinct()
                            .ToList();

            // ✅ Create lookup dictionary
            var poolLookup = result
                .GroupBy(x => (x.OrgId, x.AccountId))
                .ToDictionary(g => g.Key, g => g.ToList());

            var pools = _context.AccountGroups.Where(p => p.PoolNo != null).ToList();

            var specialMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["MNH"] = "MaterialsName"
            };

            foreach (var employee in employeeForecastSummary)
            {
                poolLookup.TryGetValue((employee.emplSchedule?.OrgID, employee.emplSchedule?.AccID), out var actualPools);



                foreach (var pool in pools)
                {
                    if (string.IsNullOrWhiteSpace(pool.Type))
                        continue;

                    var propertyName = specialMap.ContainsKey(pool.Type)
                        ? specialMap[pool.Type]
                        : char.ToUpper(pool.Type[0])
                          + pool.Type.Substring(1).ToLower()
                          + "Name";

                    var property = employee.GetType().GetProperty(propertyName);

                    if (property != null && property.CanWrite)
                        property.SetValue(employee, pool.Name);
                }

                if (actualPools != null)
                {
                    foreach (var pool in actualPools)
                    {
                        if (string.IsNullOrWhiteSpace(pool.Type))
                            continue;

                        var propertyName = specialMap.ContainsKey(pool.Type)
                            ? specialMap[pool.Type]
                            : char.ToUpper(pool.Type[0])
                              + pool.Type.Substring(1).ToLower()
                              + "Name";

                        var property = employee.GetType().GetProperty(propertyName);

                        if (property != null && property.CanWrite)
                            property.SetValue(employee, pool.Name);
                    }
                }
                //foreach(var pool in pools)
                //{
                //    switch(pool.Type.ToUpper())
                //    {
                //        case "FRINGE":
                //            employee.FringeName = pool.Name;
                //            break;
                //        case "OVERHEAD":
                //            employee.OverheadName = pool.Name;
                //            break;
                //        case "HR":
                //            employee.HrName = pool.Name;
                //            break;
                //        case "MNH":
                //            employee.MaterialsName = pool.Name;
                //            break;
                //        case "GNA":
                //            employee.GnaName = pool.Name;
                //            break;
                //    }
                //}
            }
        }

        if (directCostForecastSummary.Any())
        {
            var keys = directCostForecastSummary
                    .Select(c => $"{c.directCostSchedule.OrgID}|{c.directCostSchedule.AcctId}")
                    .ToList();

            var result = _context.PlOrgAcctPoolMappings
                            .Where(plm => keys.Contains(plm.OrgId + "|" + plm.AccountId))
                            .Join(_context.AccountGroups,
                                plm => plm.PoolId,
                                pool => pool.Code,
                                (plm, pool) => new
                                {
                                    plm.OrgId,
                                    plm.AccountId,
                                    pool.Code,
                                    pool.Name,
                                    pool.Type
                                })
                            .Distinct()
                            .ToList();

            // ✅ Create lookup dictionary
            var poolLookup = result
                .GroupBy(x => (x.OrgId, x.AccountId))
                .ToDictionary(g => g.Key, g => g.ToList());

            var pools = _context.AccountGroups.Where(p => p.PoolNo != null).ToList();

            var specialMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["MNH"] = "MaterialsName"
            };

            foreach (var employee in directCostForecastSummary)
            {
                poolLookup.TryGetValue((employee.directCostSchedule?.OrgID, employee.directCostSchedule?.AcctId), out var actualPools);



                foreach (var pool in pools)
                {
                    if (string.IsNullOrWhiteSpace(pool.Type))
                        continue;

                    var propertyName = specialMap.ContainsKey(pool.Type)
                        ? specialMap[pool.Type]
                        : char.ToUpper(pool.Type[0])
                          + pool.Type.Substring(1).ToLower()
                          + "Name";

                    var property = employee.GetType().GetProperty(propertyName);

                    if (property != null && property.CanWrite)
                        property.SetValue(employee, pool.Name);
                }

                if (actualPools != null)
                {
                    foreach (var pool in actualPools)
                    {
                        if (string.IsNullOrWhiteSpace(pool.Type))
                            continue;

                        var propertyName = specialMap.ContainsKey(pool.Type)
                            ? specialMap[pool.Type]
                            : char.ToUpper(pool.Type[0])
                              + pool.Type.Substring(1).ToLower()
                              + "Name";

                        var property = employee.GetType().GetProperty(propertyName);

                        if (property != null && property.CanWrite)
                            property.SetValue(employee, pool.Name);
                    }
                }
            }
        }

        planForecastSummary.EmployeeForecastSummary = employeeForecastSummary;
        planForecastSummary.DirectCOstForecastSummary = directCostForecastSummary;
        planForecastSummary.MonthlyRevenueSummary = monthlyRevenueSummary;
        planForecastSummary.TotalCost = employeeForecastSummary.Sum(p => p.TotalForecastedCost) + directCostForecastSummary.Sum(p => p.TotalForecastedCost);
        planForecastSummary.TotalBurdenCost = employeeForecastSummary.Sum(p => p.TotalForecastedCost + p.Burden) + directCostForecastSummary.Sum(p => p.TotalForecastedCost + p.Burden);
        planForecastSummary.Version = projPlan.Version.GetValueOrDefault();
        //planForecastSummary.CPFFRevenue = employeeForecastSummary.Sum(p => p.CPFFRevenue) + directCostForecastSummary.Sum(p => p.CPFFRevenue);
        planForecastSummary.TotalFringe = employeeForecastSummary.Sum(p => p.Fringe) + directCostForecastSummary.Sum(p => p.Fringe);
        planForecastSummary.TotalHr = employeeForecastSummary.Sum(p => p.Hr) + directCostForecastSummary.Sum(p => p.Hr);
        planForecastSummary.TotalMaterials = employeeForecastSummary.Sum(p => p.Materials) + directCostForecastSummary.Sum(p => p.Materials);
        planForecastSummary.TotalOverhead = employeeForecastSummary.Sum(p => p.Overhead) + directCostForecastSummary.Sum(p => p.Overhead);
        planForecastSummary.TotalGna = employeeForecastSummary.Sum(p => p.Gna) + directCostForecastSummary.Sum(p => p.Gna);
        planForecastSummary.TotalBurden = employeeForecastSummary.Sum(p => p.Burden) + directCostForecastSummary.Sum(p => p.Burden);
        planForecastSummary.AdjustedRevenue = RevenueAdj;
        //planForecastSummary.Revenue = monthlyRevenueSummary.Sum(p => p.Revenue);
        planForecastSummary.Revenue = fixed_fee;
        planForecastSummary.Fees = employeeForecastSummary.Sum(p => p.Fees) + directCostForecastSummary.Sum(p => p.Fees);
        planForecastSummary.RevenueFormula = revenueFormula;
        planForecastSummary.AtRiskAmt = revenueSetup == null ? 0 : revenueSetup.AtRiskAmt;
        planForecastSummary.FundingValue = fundingValue;
        planForecastSummary.Proj_Id = projId;
        planForecastSummary.Type = projPlan.PlType;

        if (revenueSetup != null && revenueSetup.OverrideRevAdjFl)
        {
            planForecastSummary.Revenue = ActualRevenue;
        }
        planForecastSummary.Revenue = Math.Min(planForecastSummary.Revenue, fundingValue);
        return planForecastSummary;

    }

    public async Task<PlanForecastSummary> CalculateCostAI(int planID, int templateId, string type, int year)
    {
        string escallation_month = "0", escallation_percent = "0";
        string projId = string.Empty, organization = string.Empty, projType = string.Empty, revenueFormula = string.Empty;
        decimal RevenueAdj = 0, fundingValue = 0, ActualRevenue = 0, fixed_fee = 0;
        List<EmployeeForecastSummary> employeeForecastSummary = new List<EmployeeForecastSummary>();
        List<EmployeeForecastSummary> directCostForecastSummary = new List<EmployeeForecastSummary>();
        List<MonthlyCostRevenue> MonthlyCostRevenue = new List<MonthlyCostRevenue>();
        bool OverrideFRevenueSettings = false;
        List<ProjRevWrkPd> adj = new List<ProjRevWrkPd>();

        PlanForecastSummary planForecastSummary = new PlanForecastSummary();
        planForecastSummary.EmployeeForecastSummary = new List<EmployeeForecastSummary>();

        FinanceHelper financeHelper = new FinanceHelper(_context, projId);

        var projPlan = _context.PlProjectPlans.Where(p => p.PlId == planID).Include(p => p.Proj).FirstOrDefault();
        if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
        {
            projId = projPlan.Proj.ProjId;
            projType = projPlan.Proj.ProjTypeDc;
            //projPlan.ProjEndDt = projPlan.Proj.ProjEndDt;
            //projPlan.ProjStartDt = projPlan.Proj.ProjStartDt;
            fundingValue = projPlan.Proj.proj_f_tot_amt.GetValueOrDefault();
        }
        else
        {
            projId = projPlan.ProjId;
            var projPlanNBBUD = _context.NewBusinessBudgets.Where(p => p.BusinessBudgetId == projPlan.ProjId).FirstOrDefault();
            projPlan.ProjEndDt = projPlanNBBUD?.EndDate != null
                                    ? DateOnly.FromDateTime(projPlanNBBUD.EndDate)
                                    : (DateOnly?)null;
            projPlan.ProjStartDt = projPlanNBBUD?.StartDate != null
                                    ? DateOnly.FromDateTime(projPlanNBBUD.StartDate)
                                    : (DateOnly?)null;
            organization = "New Business";
            escallation_percent = projPlanNBBUD?.EscalationRate != null ? projPlanNBBUD.EscalationRate.ToString() : "3";
            fundingValue = 20000000000;
        }

        if (projId != null && projPlan.PlType.ToUpper() != "NBBUD")
        {
            organization = _context.PlProjects
                .Where(p => p.ProjId == projId)
                .Include(p => p.Org)
                .Select(p => p.Org.OrgName)
                .FirstOrDefault();
        }


        var forecastsDirectCosts = await _context.PlForecasts
                    .Include(p => p.DirectCost)
                .Include(p => p.Proj)
                    .Where(f => f.Emple == null && f.ProjId == projId && f.PlId == planID && f.Year == year).ToListAsync();


        var forecasts = await _context.PlForecasts
                        .Include(p => p.Emple)
                        .Include(p => p.Proj)
                        .Where(f => f.Emple != null && f.ProjId == projId && f.PlId == planID && f.Year == year).ToListAsync();

        if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
        {
            escallation_month =
                        _context.PlConfigValues
                            .Where(r => r.Name.ToLower() == "escallation_month"
                                     && (r.ProjId == projId || r.ProjId == "xxxxx"))
                            .OrderByDescending(r => r.ProjId == projId) // prefer project-specific
                            .Select(r => r.Value)
                            .FirstOrDefault() ?? "3";

            escallation_percent =
                        _context.PlConfigValues
                            .Where(r => r.Name.ToLower() == "escallation_percent"
                                     && (r.ProjId == projId || r.ProjId == "xxxxx"))
                            .OrderByDescending(r => r.ProjId == projId) // prefer project-specific
                            .Select(r => r.Value)
                            .FirstOrDefault() ?? "3";
        }
        else
        {
            escallation_month = _context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "escallation_month" && r.ProjId == "xxxxx")?.Value ?? "7";
        }
        if (string.IsNullOrEmpty(escallation_month))
        {
            escallation_month = "7";
        }
        if (string.IsNullOrEmpty(escallation_percent))
        {
            escallation_percent = "0";
        }
        List<EmplSchedule> res = new List<EmplSchedule>();
        List<DirectCostSchedule> res1 = new List<DirectCostSchedule>();

        //if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
        {
            res = financeHelper.CalculateSalaryPlan(forecasts, 0, 2025, Convert.ToInt32(escallation_month), Convert.ToDecimal(escallation_percent), projPlan);
            res1 = financeHelper.CalculateDirectCostPlan(forecastsDirectCosts);
        }


        var Adjusted = forecasts
                        .Concat(forecastsDirectCosts)
                        .OrderBy(x => x.Year)
                        .ThenBy(x => x.Month)
                        .ToList();

        var revenueSetup = _context.ProjBgtRevSetups.FirstOrDefault(p => p.ProjId == projId && p.VersionNo == projPlan.Version && p.BgtType == projPlan.PlType);
        adj = _context.ProjRevWrkPds.Where(p => p.Pl_Id == planID).ToList();

        if (revenueSetup != null)
        {
            revenueFormula = revenueSetup.RevType;
            if (revenueFormula.ToUpper() == "CPFF")
            {
                fixed_fee = revenueSetup.LabFeeRt.GetValueOrDefault();
            }
            if (revenueSetup.OverrideRevAdjFl)
            {
                //adj = _context.ProjRevWrkPds.Where(p => p.ProjId == projId && p.VersionNo == projPlan.Version && p.BgtType == projPlan.PlType).ToList();
                RevenueAdj = adj.Sum(p => p.RevAdj).GetValueOrDefault() + adj.Sum(p => p.RevAdj1).GetValueOrDefault();
                ActualRevenue = adj.Sum(p => p.RevAmt).GetValueOrDefault();
            }
            if (revenueSetup.OverrideFundingCeilingFl)
            {
                if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
                    fundingValue = revenueSetup.AtRiskAmt + projPlan.Proj.proj_f_tot_amt.GetValueOrDefault();
                else
                    fundingValue = 2000000000;
            }
            else
            {
                if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
                    fundingValue = projPlan.Proj.proj_f_tot_amt.GetValueOrDefault();
                else
                    fundingValue = 2000000000;
            }
            if (revenueSetup.OverrideRevSettingFl)
            {
                OverrideFRevenueSettings = true;
            }

        }
        else
        {

            var parts = projId.Split('.', StringSplitOptions.RemoveEmptyEntries);
            var prefixes = Enumerable
                .Range(2, parts.Length - 1)
                .Select(i => string.Join('.', parts.Take(i)))
                .ToList();
            var ProjRevDef = _context.ProjRevDefinitions
                .AsNoTracking()
                .Where(p => prefixes.Contains(p.ProjectId))
                .OrderByDescending(p => p.ProjectId.Length)
                .FirstOrDefault();

            if (ProjRevDef != null)
            {
                revenueFormula = ProjRevDef.RevenueFormulaCd;
                //laborFees = ProjRevDef.GetValueOrDefault();
                //nonLaborFees = ProjRevDef.RevenueCalculation1Amount.GetValueOrDefault();
            }
            else
            {
                revenueFormula = projType;
            }
            if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
                fundingValue = projPlan.Proj.proj_f_tot_amt.GetValueOrDefault();
            else
                fundingValue = 2000000000;
        }

        foreach (var emplSchedule in res)
        {
            bool revenue = true;
            decimal? ceilingemplHour = null;
            decimal? ceilingLabCatHour = null;
            decimal? ceilingHours = null;


            var employeeForecast = Adjusted.Where(f => f.Emple != null)
                                    .Where(p => p.EmplId == emplSchedule.EmpId && p.Plc == emplSchedule.PlcCode && p.AcctId == emplSchedule.AccID && p.OrgId == emplSchedule.OrgID)
                                    .OrderBy(p => p.Year)
                                    .ThenBy(p => p.Month)
                                    .ToList();

            if (emplSchedule.EmpId == "000972" && emplSchedule.PlcCode == "SS4" && emplSchedule.AccID == "500-001-110")
            {

            }
            decimal totalHours = 0;

            foreach (var monthlySalary in emplSchedule.payrollSalary)
            {
                try
                {
                    var monthDetails = employeeForecast.FirstOrDefault(p => p.Month == monthlySalary.Month && p.Year == monthlySalary.Year);
                    if (monthDetails != null)
                    {
                        if (monthlySalary.Year == 2026 && monthlySalary.Month == 1 && emplSchedule.EmpId == "9030916")
                        {
                        }
                        if (projPlan.PlType == "EAC")
                            monthlySalary.Hours = monthDetails.Actualhours;

                        else
                            monthlySalary.Hours = monthDetails.Forecastedhours;

                        if (emplSchedule.isRev)
                        {
                            if (ceilingHours.HasValue)
                            {
                                totalHours = totalHours + monthDetails.Forecastedhours;
                                decimal minHours = Math.Min(Convert.ToDecimal(totalHours), ceilingHours.GetValueOrDefault());

                                if (totalHours == minHours)
                                {
                                    monthlySalary.Revenue = monthDetails.Revenue;
                                    monthlySalary.Fees = monthDetails.Fees;
                                }
                                else
                                {
                                    if (revenue)
                                    {
                                        var hoursConsideredForRevenue = minHours - (totalHours - monthDetails.Forecastedhours);//monthDetails.Forecastedhours = 
                                        monthlySalary.Revenue = monthDetails.Revenue;
                                        monthlySalary.Fees = monthDetails.Fees;
                                        revenue = false;
                                    }
                                    else
                                    {
                                        monthlySalary.Revenue = monthDetails.Revenue;
                                        monthlySalary.Fees = monthDetails.Fees;
                                    }
                                }
                            }
                            else
                            {
                                monthlySalary.Revenue = monthDetails.Revenue;
                                monthlySalary.Fees = monthDetails.Fees;

                            }
                        }
                        monthlySalary.TotalBurdenCost = monthDetails.Cost + monthDetails.Burden;
                        monthlySalary.Cost = monthDetails.Cost;
                        monthlySalary.Fringe = monthDetails.Fringe;
                        monthlySalary.Overhead = monthDetails.Overhead;
                        monthlySalary.Gna = monthDetails.Gna;
                        monthlySalary.Burden = monthDetails.Burden;
                        monthlySalary.Fees = monthDetails.Fees;
                        monthlySalary.Materials = monthDetails.Materials;
                        monthlySalary.Hr = monthDetails.Hr;

                    }
                }
                catch (Exception ex)
                {

                }
            }
        }

        var tempss = Adjusted
                    .GroupBy(f => new { f.Month, f.Year }).ToList();

        List<MonthlyRevenueSummary> monthlyRevenueSummary = new List<MonthlyRevenueSummary>();

        monthlyRevenueSummary = Adjusted
            .GroupBy(f => new { f.Month, f.Year })
            .Select(g => new MonthlyRevenueSummary
            {
                Month = g.Key.Month,
                Year = g.Key.Year,
                Revenue = g.Sum(p => p.Revenue),
                Cost = (new DateOnly(g.Key.Year, g.Key.Month, 1) < projPlan.ClosedPeriod.GetValueOrDefault() && projPlan.PlType.ToUpper() == "EAC")
                    ? g.Where(p => p.DirectCost == null).Sum(p => (p.Cost + p.Burden))
                    : g.Where(p => p.DirectCost == null).Sum(p => (p.Cost + p.Burden)),
                OtherDifrectCost = g.Where(p => p.DirectCost != null).Sum(p => (p.Cost + p.Burden))

            })
            .ToList();

        foreach (var monthlyRevenue in monthlyRevenueSummary)
        {
            var adustment = adj.FirstOrDefault(p => p.EndDate.GetValueOrDefault().Year == monthlyRevenue.Year && p.Period == monthlyRevenue.Month);
            if (adustment != null)
            {
                monthlyRevenue.Revenue += adustment.RevAdj.GetValueOrDefault();
                monthlyRevenue.Revenue += adustment.RevAdj1.GetValueOrDefault();

                if (revenueFormula.ToUpper() == "CPFC")
                {
                    if (projPlan.Type.Trim().ToUpper() == "A")
                    {
                        monthlyRevenue.Revenue += adustment.ActualFeeAmountOnCost;
                    }
                    else
                    {
                        monthlyRevenue.Revenue += adustment.TargetFeeAmountOnCost;

                    }
                }
            }
        }



        if (projPlan.PlType.ToUpper() == "EAC")
        {
            //var actualMonthlySummary = await _context.PSRFinalData
            //    .Where(p => p.ProjId.StartsWith(projId) && (p.RateType == "A" || p.RateType == "N"))
            //    .GroupBy(p => new { p.PdNo, p.FyCd, p.SubTotTypeNo, p.ProjId })
            //    .Select(g => new MonthlySummary
            //    {
            //        Month = g.Key.PdNo,
            //        Year = Convert.ToInt16(g.Key.FyCd),
            //        Cost = g.Sum(x => x.PtdIncurAmt),
            //        subTotalType = g.Key.SubTotTypeNo
            //        //Cost = (g.Key.SubTotTypeNo == 2 || g.Key.SubTotTypeNo == 3) ? g.Sum(x => x.PtdIncurAmt) : 0m

            //    })
            //    .ToListAsync();

            var actualMonthlySummary = await _context.PSRFinalData
                    .Where(p => p.ProjId.StartsWith(projId) && (p.RateType == projPlan.Type || p.RateType == "N"))
                    .GroupBy(p => new { p.PdNo, p.FyCd, p.SubTotTypeNo })
                    .Select(g => new MonthlySummary
                    {
                        Month = g.Key.PdNo,
                        Year = Convert.ToInt16(g.Key.FyCd),
                        Cost = g.Sum(x => x.PtdIncurAmt),
                        subTotalType = g.Key.SubTotTypeNo
                        //Cost = (g.Key.SubTotTypeNo == 2 || g.Key.SubTotTypeNo == 3) ? g.Sum(x => x.PtdIncurAmt) : 0m

                    })
                    .ToListAsync();

            var summaryLookup = actualMonthlySummary
                .ToDictionary(
                    x => (x.Month, x.Year, x.subTotalType),
                    x => x.Cost);

            foreach (var temp in monthlyRevenueSummary)
            {
                summaryLookup.TryGetValue((temp.Month, temp.Year, 1), out var revenue);
                summaryLookup.TryGetValue((temp.Month, temp.Year, 2), out var laborCost);
                summaryLookup.TryGetValue((temp.Month, temp.Year, 3), out var otherDirectCost);
                summaryLookup.TryGetValue((temp.Month, temp.Year, 4), out var indirectCost);


                if (new DateOnly(temp.Year, temp.Month, 1) < projPlan.ClosedPeriod.GetValueOrDefault())
                {
                    temp.Revenue = revenue;

                    var adustment = adj.FirstOrDefault(p => p.EndDate.GetValueOrDefault().Year == temp.Year && p.Period == temp.Month);
                    if (adustment != null)
                    {
                        temp.Revenue += adustment.RevAdj.GetValueOrDefault();
                        temp.Revenue += adustment.RevAdj1.GetValueOrDefault();
                        if (revenueFormula.ToUpper() == "CPFC")
                        {
                            if (projPlan.Type.Trim().ToUpper() == "A")
                            {
                                temp.Revenue += adustment.ActualFeeAmountOnCost;
                            }
                            else
                            {
                                temp.Revenue += adustment.TargetFeeAmountOnCost;

                            }
                        }
                    }

                    if (revenueFormula.ToUpper() == "UNIT")
                    {
                        temp.Revenue = adustment.RevAmt.GetValueOrDefault();
                    }
                    //temp.Cost = laborCost  ;
                    //temp.OtherDifrectCost = otherDirectCost;
                }
            }
        }

        if ((revenueSetup != null && revenueSetup.OverrideRevAdjFl) || revenueFormula == "UNIT")
        {
            foreach (var monthlyRevenue in monthlyRevenueSummary)
            {
                var adustment = adj.FirstOrDefault(p => p.EndDate.GetValueOrDefault().Year == monthlyRevenue.Year && p.Period == monthlyRevenue.Month);
                if (adustment != null)
                {
                    monthlyRevenue.Revenue = adustment.RevAmt.GetValueOrDefault();
                }
            }
        }

        employeeForecastSummary = Adjusted
                 .Where(f => f.Emple != null && f.PlId == planID)
                 .GroupBy(f => new { f.EmplId, f.Emple.PerHourRate, f.ProjId, f.Emple.OrgId, f.Emple.AccId, f.Emple.PlcGlcCode, f.Emple.FirstName, f.Emple.LastName })
                 .Select(g => new EmployeeForecastSummary
                 {
                     OrgID = organization,
                     //AccID = g.Key.AccId ?? string.Empty,
                     AccID = _context.Accounts.Where(p => p.AcctId == g.Key.AccId).Select(p => p.AcctId + " - " + p.AcctName).FirstOrDefault(),
                     EmplId = g.Key.EmplId?.ToString() ?? string.Empty,
                     Name = g.Key.FirstName + " " + g.Key.LastName,
                     PerHourRate = g.Key.PerHourRate ?? 0m,
                     TotalForecastedHours = g.Sum(f => f.Forecastedhours),
                     TotalForecastedCost = (decimal)(g.Sum(f => f.Cost)),
                     Fringe = (decimal)(g.Sum(f => f.Fringe)),
                     Overhead = (decimal)(g.Sum(f => f.Overhead)),
                     Gna = (decimal)(g.Sum(f => f.Gna)),
                     Hr = (decimal)(g.Sum(f => f.Hr)),
                     Materials = (decimal)(g.Sum(f => f.Materials)),
                     Burden = g.Sum(f => f.Burden),
                     TotalBurdonCost = g.Sum(f => f.TotalBurdenCost),
                     //TNMRevenue = (decimal)(g.Sum(f => f.TNMRevenue)),
                     //CPFFRevenue = (decimal)(g.Sum(f => f.CCFFRevenue)),
                     Revenue = (decimal)(g.Sum(f => f.Revenue)),
                     Fees = (decimal)(g.Sum(f => f.Fees)),
                     PlcCode = !string.IsNullOrEmpty(g.Key.PlcGlcCode)
                                    ? g.Key.PlcGlcCode + "-(" + (_context.PlcCodes.FirstOrDefault(p => p.LaborCategoryCode == g.Key.PlcGlcCode)?.Description ?? "") + ")"
                                    : string.Empty,
                     emplSchedule = res.FirstOrDefault(p => p.EmpId == g.Key.EmplId && p.PlcCode == g.Key.PlcGlcCode && p.AccID == g.Key.AccId && p.OrgID == g.Key.OrgId) ?? new EmplSchedule() //res.FirstOrDefault(p => p.EmpId == g.Key.EmplId)

                 }).ToList();

        directCostForecastSummary = Adjusted
                 .Where(f => f.Emple == null && f.PlId == planID)
                 .GroupBy(f => new { f.DirectCost?.DctId, f.DirectCost?.Id, f.DirectCost?.OrgId, f.ProjId, f.DirectCost?.AcctId })
                 .Select(g => new EmployeeForecastSummary
                 {
                     OrgID = organization,
                     //AccID = g.Key.AcctId ?? string.Empty,
                     AccID = _context.Accounts.Where(p => p.AcctId == g.Key.AcctId).Select(p => p.AcctId + " - " + p.AcctName).FirstOrDefault(),
                     EmplId = g.Key.Id?.ToString() ?? string.Empty,
                     PerHourRate = 0m,
                     TotalForecastedHours = g.Sum(f => f.Forecastedhours),
                     TotalForecastedCost = (decimal)(g.Sum(f => f.Cost)),
                     Fringe = (decimal)(g.Sum(f => f.Fringe)),
                     Overhead = (decimal)(g.Sum(f => f.Overhead)),
                     Gna = (decimal)(g.Sum(f => f.Gna)),
                     Hr = (decimal)(g.Sum(f => f.Hr)),
                     Materials = (decimal)(g.Sum(f => f.Materials)),
                     Burden = g.Sum(f => f.Burden),
                     TotalBurdonCost = g.Sum(f => f.TotalBurdenCost),
                     Revenue = g.Sum(f => f.Revenue),
                     //TNMRevenue = (decimal)(g.Sum(f => f.TNMRevenue)),
                     //CPFFRevenue = (decimal)(g.Sum(f => f.CCFFRevenue)),
                     Fees = (decimal)(g.Sum(f => f.Fees)), //PlcCode = g.Key.PlcGlc ??string.Empty,
                     //PlcCode = !string.IsNullOrEmpty(g.Key.PlcGlc)
                     //           ? g.Key.PlcGlc + "-(" + (_context.PlcCodes.FirstOrDefault(p => p.LaborCategoryCode == g.Key.PlcGlc)?.Description ?? "") + ")"
                     //           : string.Empty,
                     directCostSchedule = res1.FirstOrDefault(p => p.DctId == g.Key.DctId) ?? new DirectCostSchedule()


                 }).ToList();


        if (employeeForecastSummary.Any())
        {
            var keys = employeeForecastSummary
                    .Select(c => $"{c.emplSchedule.OrgID}|{c.emplSchedule.AccID}")
                    .ToList();

            var result = _context.PlOrgAcctPoolMappings
                            .Where(plm => keys.Contains(plm.OrgId + "|" + plm.AccountId))
                            .Join(_context.AccountGroups,
                                plm => plm.PoolId,
                                pool => pool.Code,
                                (plm, pool) => new
                                {
                                    plm.OrgId,
                                    plm.AccountId,
                                    pool.Code,
                                    pool.Name,
                                    pool.Type
                                })
                            .Distinct()
                            .ToList();

            // ✅ Create lookup dictionary
            var poolLookup = result
                .GroupBy(x => (x.OrgId, x.AccountId))
                .ToDictionary(g => g.Key, g => g.ToList());

            var pools = _context.AccountGroups.Where(p => p.PoolNo != null).ToList();

            var specialMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["MNH"] = "MaterialsName"
            };

            foreach (var employee in employeeForecastSummary)
            {
                poolLookup.TryGetValue((employee.emplSchedule?.OrgID, employee.emplSchedule?.AccID), out var actualPools);



                foreach (var pool in pools)
                {
                    if (string.IsNullOrWhiteSpace(pool.Type))
                        continue;

                    var propertyName = specialMap.ContainsKey(pool.Type)
                        ? specialMap[pool.Type]
                        : char.ToUpper(pool.Type[0])
                          + pool.Type.Substring(1).ToLower()
                          + "Name";

                    var property = employee.GetType().GetProperty(propertyName);

                    if (property != null && property.CanWrite)
                        property.SetValue(employee, pool.Name);
                }

                if (actualPools != null)
                {
                    foreach (var pool in actualPools)
                    {
                        if (string.IsNullOrWhiteSpace(pool.Type))
                            continue;

                        var propertyName = specialMap.ContainsKey(pool.Type)
                            ? specialMap[pool.Type]
                            : char.ToUpper(pool.Type[0])
                              + pool.Type.Substring(1).ToLower()
                              + "Name";

                        var property = employee.GetType().GetProperty(propertyName);

                        if (property != null && property.CanWrite)
                            property.SetValue(employee, pool.Name);
                    }
                }
                //foreach(var pool in pools)
                //{
                //    switch(pool.Type.ToUpper())
                //    {
                //        case "FRINGE":
                //            employee.FringeName = pool.Name;
                //            break;
                //        case "OVERHEAD":
                //            employee.OverheadName = pool.Name;
                //            break;
                //        case "HR":
                //            employee.HrName = pool.Name;
                //            break;
                //        case "MNH":
                //            employee.MaterialsName = pool.Name;
                //            break;
                //        case "GNA":
                //            employee.GnaName = pool.Name;
                //            break;
                //    }
                //}
            }
        }

        if (directCostForecastSummary.Any())
        {
            var keys = directCostForecastSummary
                    .Select(c => $"{c.directCostSchedule.OrgID}|{c.directCostSchedule.AcctId}")
                    .ToList();

            var result = _context.PlOrgAcctPoolMappings
                            .Where(plm => keys.Contains(plm.OrgId + "|" + plm.AccountId))
                            .Join(_context.AccountGroups,
                                plm => plm.PoolId,
                                pool => pool.Code,
                                (plm, pool) => new
                                {
                                    plm.OrgId,
                                    plm.AccountId,
                                    pool.Code,
                                    pool.Name,
                                    pool.Type
                                })
                            .Distinct()
                            .ToList();

            // ✅ Create lookup dictionary
            var poolLookup = result
                .GroupBy(x => (x.OrgId, x.AccountId))
                .ToDictionary(g => g.Key, g => g.ToList());

            var pools = _context.AccountGroups.Where(p => p.PoolNo != null).ToList();

            var specialMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["MNH"] = "MaterialsName"
            };

            foreach (var employee in directCostForecastSummary)
            {
                poolLookup.TryGetValue((employee.directCostSchedule?.OrgID, employee.directCostSchedule?.AcctId), out var actualPools);



                foreach (var pool in pools)
                {
                    if (string.IsNullOrWhiteSpace(pool.Type))
                        continue;

                    var propertyName = specialMap.ContainsKey(pool.Type)
                        ? specialMap[pool.Type]
                        : char.ToUpper(pool.Type[0])
                          + pool.Type.Substring(1).ToLower()
                          + "Name";

                    var property = employee.GetType().GetProperty(propertyName);

                    if (property != null && property.CanWrite)
                        property.SetValue(employee, pool.Name);
                }

                if (actualPools != null)
                {
                    foreach (var pool in actualPools)
                    {
                        if (string.IsNullOrWhiteSpace(pool.Type))
                            continue;

                        var propertyName = specialMap.ContainsKey(pool.Type)
                            ? specialMap[pool.Type]
                            : char.ToUpper(pool.Type[0])
                              + pool.Type.Substring(1).ToLower()
                              + "Name";

                        var property = employee.GetType().GetProperty(propertyName);

                        if (property != null && property.CanWrite)
                            property.SetValue(employee, pool.Name);
                    }
                }
            }
        }

        foreach (var emplForecastSummary in employeeForecastSummary)
        {
            emplForecastSummary.emplSchedule.payrollSalary = emplForecastSummary.emplSchedule.payrollSalary.Where(x => x.Year == year).ToList();
            // Perform any additional processing for each employee forecast summary if needed
        }

        planForecastSummary.EmployeeForecastSummary = employeeForecastSummary;
        planForecastSummary.DirectCOstForecastSummary = directCostForecastSummary;
        planForecastSummary.MonthlyRevenueSummary = monthlyRevenueSummary;
        planForecastSummary.TotalCost = employeeForecastSummary.Sum(p => p.TotalForecastedCost) + directCostForecastSummary.Sum(p => p.TotalForecastedCost);
        planForecastSummary.TotalBurdenCost = employeeForecastSummary.Sum(p => p.TotalForecastedCost + p.Burden) + directCostForecastSummary.Sum(p => p.TotalForecastedCost + p.Burden);
        planForecastSummary.Version = projPlan.Version.GetValueOrDefault();
        //planForecastSummary.CPFFRevenue = employeeForecastSummary.Sum(p => p.CPFFRevenue) + directCostForecastSummary.Sum(p => p.CPFFRevenue);
        planForecastSummary.TotalFringe = employeeForecastSummary.Sum(p => p.Fringe) + directCostForecastSummary.Sum(p => p.Fringe);
        planForecastSummary.TotalHr = employeeForecastSummary.Sum(p => p.Hr) + directCostForecastSummary.Sum(p => p.Hr);
        planForecastSummary.TotalMaterials = employeeForecastSummary.Sum(p => p.Materials) + directCostForecastSummary.Sum(p => p.Materials);
        planForecastSummary.TotalOverhead = employeeForecastSummary.Sum(p => p.Overhead) + directCostForecastSummary.Sum(p => p.Overhead);
        planForecastSummary.TotalGna = employeeForecastSummary.Sum(p => p.Gna) + directCostForecastSummary.Sum(p => p.Gna);
        planForecastSummary.TotalBurden = employeeForecastSummary.Sum(p => p.Burden) + directCostForecastSummary.Sum(p => p.Burden);
        planForecastSummary.AdjustedRevenue = RevenueAdj;
        //planForecastSummary.Revenue = monthlyRevenueSummary.Sum(p => p.Revenue);
        planForecastSummary.Revenue = fixed_fee;
        planForecastSummary.Fees = employeeForecastSummary.Sum(p => p.Fees) + directCostForecastSummary.Sum(p => p.Fees);
        planForecastSummary.RevenueFormula = revenueFormula;
        planForecastSummary.AtRiskAmt = revenueSetup == null ? 0 : revenueSetup.AtRiskAmt;
        planForecastSummary.FundingValue = fundingValue;
        planForecastSummary.Proj_Id = projId;
        planForecastSummary.Type = projPlan.PlType;

        if (revenueSetup != null && revenueSetup.OverrideRevAdjFl)
        {
            planForecastSummary.Revenue = ActualRevenue;
        }
        planForecastSummary.Revenue = Math.Min(planForecastSummary.Revenue, fundingValue);
        return planForecastSummary;

    }

    public async Task<PlanForecastSummary> CalculateRevenueCost(int planID, int templateId, string type)
    {
        int cost = 0;
        List<ProjRevWrkPd> adj = new List<ProjRevWrkPd>();
        string revenueFormula = "";
        string escallation_month = "0", escallation_percent = "0";


        string projId = string.Empty, organization = string.Empty, projType = string.Empty;
        List<EmployeeForecastSummary> employeeForecastSummary = new List<EmployeeForecastSummary>();
        List<EmployeeForecastSummary> directCostForecastSummary = new List<EmployeeForecastSummary>();
        List<MonthlyCostRevenue> MonthlyCostRevenue = new List<MonthlyCostRevenue>();
        bool OverrideFRevenueSettings = false;

        PlanForecastSummary planForecastSummary = new PlanForecastSummary();
        planForecastSummary.EmployeeForecastSummary = new List<EmployeeForecastSummary>();
        DateTime currentMonth;

        var closingPeriodConfig = _context.PlConfigValues.AsNoTracking().FirstOrDefault(r => r.Name.ToLower() == "closing_period");

        try
        {

            if (closingPeriodConfig != null && DateTime.TryParse(closingPeriodConfig.Value, out currentMonth))
            {
                // currentMonth is now safely parsed
            }
            else
            {
                // Handle the missing or invalid value here
                throw new Exception("Invalid or missing 'closing_period' configuration.");
            }

            var AllAvaialbepools = _context.AccountGroups.AsNoTracking().ToList();

            decimal fringe = 0, gna = 0, overHead = 0, laborFees = 0, hr = 0, mnh = 0, nonLaborFees = 0, RevenueAdj = 0, fundingValue = 0, ActualRevenue = 0;
            bool OverHeadOnGna = false, FringeOnOverhead = false, FringeOnGna = false;

            List<PlTemplatePoolRate> burdensByTemplate = new List<PlTemplatePoolRate>();
            List<pl_EmployeeBurdenCalculated> plEmployeeBurdenCalculated = new List<pl_EmployeeBurdenCalculated>();
            var projPlan = _context.PlProjectPlans.Where(p => p.PlId == planID).Include(p => p.Proj).AsNoTracking().FirstOrDefault();

            if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
            {
                projId = projPlan.Proj.ProjId;
                projType = projPlan.Proj.ProjTypeDc;
                //if (projPlan.ProjEndDt == null)
                //    projPlan.ProjEndDt = projPlan.Proj.ProjEndDt;

                //if (projPlan.ProjEndDt == null)
                //    projPlan.ProjStartDt = projPlan.Proj.ProjStartDt;
                fundingValue = projPlan.Proj.proj_f_tot_amt.GetValueOrDefault();
            }
            else
            {
                projId = projPlan.ProjId;
                var projPlanNBBUD = _context.NewBusinessBudgets.Where(p => p.BusinessBudgetId == projPlan.ProjId).AsNoTracking().FirstOrDefault();
                projPlan.ProjEndDt = projPlanNBBUD?.EndDate != null
                                        ? DateOnly.FromDateTime(projPlanNBBUD.EndDate)
                                        : (DateOnly?)null;
                projPlan.ProjStartDt = projPlanNBBUD?.StartDate != null
                                        ? DateOnly.FromDateTime(projPlanNBBUD.StartDate)
                                        : (DateOnly?)null;
                escallation_percent = projPlanNBBUD?.EscalationRate != null ? projPlanNBBUD.EscalationRate.ToString() : "3";
            }

            var revenueSetup = _context.ProjBgtRevSetups.AsNoTracking().FirstOrDefault(p => p.ProjId == projId && p.VersionNo == projPlan.Version && p.BgtType == projPlan.PlType);
            //var revenueSetup = _context.ProjBgtRevSetups.FirstOrDefault(p => p.PlId == planID);
            var validOrgs = _context.PlEmployeees.AsNoTracking()
                                .Where(e => e.PlId == projPlan.PlId && e.OrgId != null)
                                .Select(e => e.OrgId)
                                .Union(
                                    _context.PlDcts.AsNoTracking()  
                                        .Where(d => d.PlId == projPlan.PlId && d.OrgId != null)
                                        .Select(d => d.OrgId)
                                )
                                .Distinct()
                                .ToList();

            var acctPool = (from pool in _context.PlOrgAcctPoolMappings.AsNoTracking()  
                            join orgId in validOrgs on pool.OrgId equals orgId
                            select pool).ToList();

            FinanceHelper financeHelper = new FinanceHelper(_context, projId);
            List<string> prefixes = new List<string>();

            if (SystemName.ToUpper() != "UNANET")
            {
                var parts = projId.Split('.', StringSplitOptions.RemoveEmptyEntries);
                prefixes = Enumerable
                    .Range(2, parts.Length - 1)
                    .Select(i => string.Join('.', parts.Take(i)))
                    .ToList();
            }
            else
            {
                prefixes.Add(projId);
            }
            var ProjRevDef = _context.ProjRevDefinitions
                .AsNoTracking()
                .Where(p => prefixes.Contains(p.ProjectId))
                .OrderByDescending(p => p.ProjectId.Length)
                .FirstOrDefault();

            if (ProjRevDef != null)
            {
                revenueFormula = ProjRevDef.RevenueFormulaCd;
                laborFees = ProjRevDef.RevenueCalculationAmount.GetValueOrDefault();
                nonLaborFees = ProjRevDef.RevenueCalculation1Amount.GetValueOrDefault();
            }


            if (revenueSetup != null)
            {
                revenueFormula = revenueSetup.RevType;
                if (revenueSetup.LabFeeCostFl.GetValueOrDefault())
                    laborFees = revenueSetup.LabFeeRt.GetValueOrDefault();
                if (revenueSetup.NonLabFeeCostFl.GetValueOrDefault())
                    nonLaborFees = revenueSetup.NonLabFeeRt.GetValueOrDefault();
                if (revenueSetup.OverrideRevAdjFl)
                {
                    adj = _context.ProjRevWrkPds.AsNoTracking().Where(p => p.ProjId == projId && p.VersionNo == projPlan.Version && p.BgtType == projPlan.PlType).ToList();
                    RevenueAdj = adj.Sum(p => p.RevAdj).GetValueOrDefault();
                    ActualRevenue = adj.Sum(p => p.RevAmt).GetValueOrDefault();
                }
                if (revenueSetup.OverrideFundingCeilingFl)
                {
                    if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
                        fundingValue = revenueSetup.AtRiskAmt + projPlan.Proj.proj_f_tot_amt.GetValueOrDefault();
                }
                else
                {
                    if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
                        fundingValue = projPlan.Proj.proj_f_tot_amt.GetValueOrDefault();
                }
                if (revenueSetup.OverrideRevSettingFl)
                {
                    OverrideFRevenueSettings = true;
                }

            }
            else
            {
                if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
                {
                    revenueFormula = projType;
                    laborFees = financeHelper.GetActiveFeesForProject(projId);
                    nonLaborFees = laborFees;
                    fundingValue = projPlan.Proj.proj_f_tot_amt.GetValueOrDefault();
                }
            }


            burdensByTemplate = await _context.PlTemplatePoolRates.AsNoTracking().Where(r => r.TemplateId == templateId).ToListAsync();
            var All_Config = _context.PlConfigValues.AsNoTracking().ToList();

            Account_Org_Helpercs account_Org_Helpercs = new Account_Org_Helpercs(_context);
            var templatePools = account_Org_Helpercs.GetPoolsByTemplateId(templateId).Select(p => p.PoolId).ToList();

            if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
            {
                if (projId != null)
                {
                    organization = _context.PlProjects.AsNoTracking()
                        .Where(p => p.ProjId == projId)
                        .Include(p => p.Org)
                        .Select(p => p.Org.OrgName)
                        .FirstOrDefault();
                }
            }

            var forecastsDirectCosts = await _context.PlForecasts.AsNoTracking()
                .Include(p => p.DirectCost)
            .Include(p => p.Proj)
                .Where(f => f.Emple == null && f.DirectCost != null && f.ProjId == projId && f.PlId == planID).OrderBy(p => p.Month).ThenBy(q => q.Year).ToListAsync();

            /////////////////Get Ceiling Configuration For Burdens

            var ceilingPools = _context.PlCeilBurdens.AsNoTracking().Where(p => p.ProjectId == projId).ToList();


            //////////////////////////////////////////////////////////////

            foreach (var forecast in forecastsDirectCosts)
            {
                try
                {
                    forecast.Revenue = 0;
                    if (acctPool.Count() > 0)
                    {
                        if (forecast.Year == 2026 && forecast.Month == 3 && forecast.AcctId == "50-400-002" && forecast.OrgId == "1.01.03.01" && forecast.EmplId == "TBD20024")
                        {

                        }
                        if (forecast.Year == 2026 && forecast.Month == 1 && forecast.EmplId.StartsWith("004411"))
                        {

                        }
                        var burdens = burdensByTemplate.Where(r => r.Month == forecast.Month && r.Year == forecast.Year).ToList();
                        var validBurdens = financeHelper.GetBurdenRates(templatePools, burdens, type);

                        if (forecast.Year == 2025 && forecast.Month == 9 && forecast.EmplId == "S0019")
                        {

                        }
                        var pools = acctPool.Where(p => p.AccountId == forecast.AcctId && p.OrgId == forecast.OrgId);
                        fringe = 0; gna = 0; overHead = 0; mnh = 0; hr = 0;
                        OverHeadOnGna = false; FringeOnOverhead = false; FringeOnGna = false;
                        foreach (var pool in pools)
                        {
                            switch (pool.PoolId.ToUpper())
                            {
                                case "FRINGE":
                                    validBurdens.TryGetValue("FRINGE", out fringe);
                                    break;
                                case "GNA":
                                    validBurdens.TryGetValue("GNA", out gna);
                                    break;
                                case "OVERHEAD":
                                    validBurdens.TryGetValue("OVERHEAD", out overHead);
                                    break;
                                case "MNH":
                                    validBurdens.TryGetValue("MNH", out mnh);
                                    break;
                                case "HR":
                                    validBurdens.TryGetValue("HR", out hr);
                                    break;
                                case "FRINGEONOVERHEAD":
                                    FringeOnOverhead = true;
                                    break;
                                case "FRINGEONGNA":
                                    FringeOnGna = true;
                                    break;
                                case "OVERHEADONGNA":
                                    OverHeadOnGna = true;
                                    break;
                            }
                        }
                    }

                    //////////////////Calculate Burden 
                    switch (projPlan.PlType)
                    {
                        case "EAC":
                            forecast.Cost = (decimal)(forecast.Actualamt.GetValueOrDefault());
                            break;
                        case "BUD":
                            forecast.Cost = (decimal)(forecast.Forecastedamt.GetValueOrDefault());
                            break;
                        case "NBBUD":
                            forecast.Cost = (decimal)(forecast.Forecastedamt.GetValueOrDefault());
                            break;

                    }
                    if (forecast.DirectCost.IsBrd)
                    {
                        //var ytdCost = forecastsDirectCosts.Where(p => p.EmplId == forecast.EmplId && p.Year == forecast.Year && p.Month <= forecast.Month && p.OrgId == forecast.OrgId && p.AcctId == forecast.AcctId && p.Plc == forecast.Plc).Sum(p => p.Cost);
                        //forecast.YtdCost = ytdCost;


                        //decimal ytdCost = 0;
                        //switch (forecast.DirectCost.Type.ToUpper())
                        //{
                        //    case "EMPLOYEE":
                        //        ytdCost = forecastsDirectCosts.Where(p => p.EmplId == forecast.EmplId && p.Year == forecast.Year && p.Month <= forecast.Month && p.OrgId == forecast.OrgId && p.AcctId == forecast.AcctId && p.Plc == forecast.Plc && forecast.DirectCost.Type.ToUpper() == "EMPLOYEE").Sum(p => p.Cost);
                        //        break;
                        //    case "OTHER":
                        //        ytdCost = forecastsDirectCosts.Where(p => p.EmplId == forecast.EmplId && p.Year == forecast.Year && p.Month <= forecast.Month && p.OrgId == forecast.OrgId && p.AcctId == forecast.AcctId && p.Plc == forecast.Plc && forecast.DirectCost.Type.ToUpper() == "OTHER").Sum(p => p.Cost);
                        //        break;
                        //    case "VENDOR EMPLOYEE":
                        //    case "VENDOR":
                        //    case "VENDOREMPLOYEE":
                        //        ytdCost = forecastsDirectCosts.Where(p => p.EmplId == forecast.EmplId && p.Year == forecast.Year && p.Month <= forecast.Month && p.OrgId == forecast.OrgId && p.AcctId == forecast.AcctId && p.Plc == forecast.Plc && forecast.DirectCost.Type.ToUpper() == "VENDOR EMPLOYEE").Sum(p => p.Cost);
                        //        break;
                        //}

                        decimal ytdCost = 0;
                        switch (forecast.DirectCost.Type.ToUpper())
                        {
                            case "EMPLOYEE":
                                ytdCost = forecastsDirectCosts.Where(p => p.EmplId == forecast.EmplId && p.Year == forecast.Year && p.Month <= forecast.Month && p.OrgId == forecast.OrgId && p.AcctId == forecast.AcctId && forecast.DirectCost.Type.ToUpper() == "EMPLOYEE").Sum(p => p.Cost);
                                break;
                            case "OTHER":
                                ytdCost = forecastsDirectCosts.Where(p => p.EmplId == forecast.EmplId && p.Year == forecast.Year && p.Month <= forecast.Month && p.OrgId == forecast.OrgId && p.AcctId == forecast.AcctId && forecast.DirectCost.Type.ToUpper() == "OTHER").Sum(p => p.Cost);
                                break;
                            case "VENDOR EMPLOYEE":
                            case "VENDOR":
                            case "VENDOREMPLOYEE":
                                ytdCost = forecastsDirectCosts.Where(p => p.EmplId == forecast.EmplId && p.Year == forecast.Year && p.Month <= forecast.Month && p.OrgId == forecast.OrgId && p.AcctId == forecast.AcctId && forecast.DirectCost.Type.ToUpper() == "VENDOR EMPLOYEE").Sum(p => p.Cost);
                                break;
                        }


                        forecast.YtdCost = ytdCost;


                        PlForecast prevFringe = new PlForecast();
                        decimal previousFringe = 0m, previousOverHead = 0m, previousGNA = 0m, previousMNH = 0m;

                        if (forecast.Year == 2026 && forecast.Month == 1 && forecast.AcctId == "647-02-140" && forecast.EmplId == "004411")
                        {

                        }

                        if (forecast.Month > 1)
                        {
                            prevFringe = forecastsDirectCosts
                                .FirstOrDefault(p =>
                                    p.EmplId == forecast.EmplId &&
                                    p.Year == forecast.Year &&
                                    p.Month == forecast.Month - 1
                                     && p.OrgId == forecast.OrgId && p.AcctId == forecast.AcctId && p.Plc == forecast.Plc);
                        }
                        if (prevFringe != null)
                        {
                            previousFringe = prevFringe.YtdFringe;
                            previousOverHead = prevFringe.YtdOverhead;
                            previousGNA = prevFringe.YtdGna;
                            previousMNH = prevFringe.YtdMaterials;
                        }

                        var ytdFringe = ytdCost * (fringe / 100);
                        forecast.YtdFringe = ytdFringe;

                        if (fringe == 0)
                        {
                            forecast.Fringe = 0;
                        }
                        else
                        {
                            forecast.Fringe = ytdFringe - previousFringe;
                        }

                        var baseForOverhead = ytdCost + (FringeOnOverhead ? ytdFringe : 0);
                        var ytdOverHead = (decimal)(baseForOverhead * overHead / 100);
                        forecast.YtdOverhead = ytdOverHead;

                        if (overHead == 0)
                        {
                            forecast.Overhead  = 0;
                        }
                        else
                        {
                            forecast.Overhead = ytdOverHead - previousOverHead;
                        }                 

                        var ytdMaterials = (decimal)(ytdCost * mnh / 100);
                        forecast.YtdMaterials = ytdMaterials;
                        forecast.Materials = ytdMaterials - previousMNH;


                        // Calculate Gna
                        decimal baseForGna = ytdCost;

                        var ytdGna = (decimal)(baseForGna * gna / 100);
                        forecast.YtdGna = ytdGna;

                        if (gna == 0)
                        {
                            forecast.Gna = 0;
                        }
                        else
                        {
                            forecast.Gna = ytdGna - previousGNA;
                        }

                        forecast.Burden = forecast.Fringe + forecast.Overhead + forecast.Gna + forecast.Materials;
                       // forecast.TotalBurdenCost = financeHelper.CalculateBurdenedCostBYRAPRate(new BurdenInput() { FringeRate = fringe, GnaRate = gna, HourlyRate = 0, Hours = forecast.Forecastedhours, OverheadRate = overHead, DirectCost = forecast.Forecastedamt.GetValueOrDefault(), plEmployee = forecast.Empl });
                        //forecast.TotalBurdenCost = financeHelper.CalculateBurdenedCost(new BurdenInput() { FringeRate = fringe, GnaRate = gna, HourlyRate = 0, Hours = forecast.Forecastedhours, OverheadRate = overHead, DirectCost = forecast.Forecastedamt.GetValueOrDefault(), plEmployee = forecast.Empl });
                    }
                    else
                    {
                        forecast.Materials = 0;
                        forecast.Gna = 0;
                        forecast.Overhead = 0;
                        forecast.Fringe = 0;
                        forecast.Burden = 0;
                        forecast.Hr = 0;
                        //forecast.Cost = (decimal)(forecast.Forecastedamt.GetValueOrDefault());
                        forecast.TotalBurdenCost = forecast.Cost;

                    }

                    ///////////////////Calculate Revenue

                    //if (OverrideFRevenueSettings)
                    {
                        if (forecast.DirectCost.IsRev)
                        {
                            switch (projPlan.PlType)
                            {
                                case "EAC":
                                    forecast.Cost = (decimal)(forecast.Actualamt.GetValueOrDefault());
                                    break;
                                case "BUD":
                                    forecast.Cost = (decimal)(forecast.Forecastedamt.GetValueOrDefault());
                                    break;
                                case "NBBUD":
                                    forecast.Cost = (decimal)(forecast.Forecastedamt.GetValueOrDefault());
                                    break;
                            }

                            switch (revenueFormula)
                            {
                                case "CPFH":

                                    if (revenueSetup != null && OverrideFRevenueSettings)
                                    {
                                        if (revenueSetup.NonLabCostFl.GetValueOrDefault())
                                            forecast.Revenue += forecast.Cost;

                                        if (revenueSetup.NonLabBurdFl.GetValueOrDefault())
                                            forecast.Revenue += forecast.Burden;

                                        forecast.Revenue = forecast.Revenue + forecast.Fees;

                                    }
                                    else
                                    {
                                        if (ProjRevDef != null)
                                        {
                                            forecast.Revenue += forecast.Cost;
                                            forecast.Revenue += forecast.Burden;
                                        }
                                    }
                                    break;

                                case "CPFC":
                                    if (revenueSetup != null && OverrideFRevenueSettings)
                                    {
                                        if (revenueSetup.NonLabCostFl.GetValueOrDefault())
                                            forecast.Revenue += forecast.Cost;

                                        if (revenueSetup.NonLabBurdFl.GetValueOrDefault())
                                            forecast.Revenue += forecast.Burden;

                                        if (revenueSetup.NonLabFeeCostFl.GetValueOrDefault())
                                            forecast.Fees = forecast.Revenue * (revenueSetup.NonLabFeeRt.GetValueOrDefault() / 100);
                                        forecast.Revenue = forecast.Revenue + forecast.Fees;
                                    }
                                    else
                                    {
                                        if (ProjRevDef != null)
                                        {
                                            forecast.Revenue += forecast.Cost;
                                            forecast.Revenue += forecast.Burden;
                                            forecast.Fees = forecast.Revenue * (ProjRevDef.RevenueCalculation1Amount.GetValueOrDefault() / 100);
                                            forecast.Revenue = forecast.Revenue + forecast.Fees;
                                        }
                                    }
                                    break;

                                case "LLRCINL":
                                    if (revenueSetup != null && OverrideFRevenueSettings)
                                    {
                                        if (revenueSetup.NonLabCostFl.GetValueOrDefault())
                                            forecast.Revenue += forecast.Cost;
                                    }
                                    else
                                    {
                                        if (ProjRevDef != null)
                                        {
                                            forecast.Revenue += forecast.Cost;
                                        }
                                    }
                                    break;

                                case "LLR":
                                    forecast.Revenue = 0;
                                    break;

                                case "LLRCINBF":
                                    if (revenueSetup != null && OverrideFRevenueSettings)
                                    {
                                        if (revenueSetup.NonLabCostFl.GetValueOrDefault())
                                            forecast.Revenue += forecast.Cost;
                                        if (revenueSetup.NonLabBurdFl.GetValueOrDefault())
                                            forecast.Revenue += forecast.Burden;
                                        if (revenueSetup.NonLabFeeCostFl.GetValueOrDefault())
                                            forecast.Fees = forecast.Revenue * (revenueSetup.NonLabFeeRt.GetValueOrDefault() / 100);

                                        forecast.Revenue += forecast.Fees;
                                    }
                                    else
                                    {
                                        if (ProjRevDef != null)
                                        {
                                            forecast.Revenue += forecast.Cost;
                                            forecast.Revenue += forecast.Burden;
                                            forecast.Fees = forecast.Revenue * (ProjRevDef.RevenueCalculation1Amount.GetValueOrDefault() / 100);
                                            forecast.Revenue = forecast.Revenue + forecast.Fees;
                                        }
                                    }
                                    break;
                                case "RSBFNLBF":
                                    if (revenueSetup != null && OverrideFRevenueSettings)
                                    {
                                        if (revenueSetup.NonLabCostFl.GetValueOrDefault())
                                            forecast.Revenue += forecast.Cost;
                                        if (revenueSetup.NonLabBurdFl.GetValueOrDefault())
                                            forecast.Revenue += forecast.Burden;
                                        if (revenueSetup.NonLabFeeCostFl.GetValueOrDefault())
                                            forecast.Fees = forecast.Revenue * (revenueSetup.NonLabFeeRt.GetValueOrDefault() / 100);

                                        forecast.Revenue += forecast.Fees;
                                    }
                                    else
                                    {
                                        if (ProjRevDef != null)
                                        {
                                            forecast.Revenue += forecast.Cost;
                                            forecast.Revenue += forecast.Burden;
                                            forecast.Fees = forecast.Revenue * (ProjRevDef.RevenueCalculation1Amount.GetValueOrDefault() / 100);
                                            forecast.Revenue = forecast.Revenue + forecast.Fees;
                                        }
                                    }
                                    break;

                                case "CPFF":
                                    if (revenueSetup != null && OverrideFRevenueSettings)
                                    {
                                        if (revenueSetup.NonLabCostFl.GetValueOrDefault())
                                            forecast.Revenue += forecast.Cost;

                                        if (revenueSetup.NonLabBurdFl.GetValueOrDefault())
                                            forecast.Revenue += forecast.Burden;

                                        //if (revenueSetup.NonLabFeeCostFl.GetValueOrDefault())
                                        //    forecast.Fees = forecast.Revenue * (revenueSetup.NonLabFeeRt.GetValueOrDefault() / 100);

                                        //forecast.Revenue = forecast.Revenue + forecast.Fees;
                                    }
                                    else
                                    {
                                        if (ProjRevDef != null)
                                        {
                                            forecast.Revenue += forecast.Cost;
                                            forecast.Revenue += forecast.Burden;
                                            forecast.Fees = forecast.Revenue * (ProjRevDef.RevenueCalculation1Amount.GetValueOrDefault() / 100);
                                            forecast.Revenue = forecast.Revenue + forecast.Fees;
                                        }
                                    }
                                    break;

                                case "LLRCINLB":
                                    if (revenueSetup != null && OverrideFRevenueSettings)
                                    {
                                        if (revenueSetup.NonLabCostFl.GetValueOrDefault())
                                            forecast.Revenue += forecast.Cost;

                                        if (revenueSetup.NonLabBurdFl.GetValueOrDefault())
                                            forecast.Revenue += forecast.Burden;
                                    }
                                    else
                                    {
                                        if (ProjRevDef != null)
                                        {
                                            forecast.Revenue += forecast.Cost;
                                            forecast.Revenue += forecast.Burden;
                                        }
                                    }
                                    break;

                                case "LLRFNLBF":
                                    if (revenueSetup != null && OverrideFRevenueSettings)
                                    {
                                        if (revenueSetup.NonLabCostFl.GetValueOrDefault())
                                            forecast.Revenue += forecast.Cost;

                                        if (revenueSetup.NonLabBurdFl.GetValueOrDefault())
                                            forecast.Revenue += forecast.Burden;

                                        if (revenueSetup.NonLabFeeCostFl.GetValueOrDefault())
                                        {
                                            forecast.Fees = forecast.Revenue * (revenueSetup.NonLabFeeRt.GetValueOrDefault() / 100);
                                            forecast.Revenue = forecast.Revenue + forecast.Fees;
                                        }
                                    }
                                    else
                                    {
                                        if (ProjRevDef != null)
                                        {
                                            forecast.Revenue += forecast.Cost;
                                            forecast.Revenue += forecast.Burden;
                                            forecast.Fees = forecast.Revenue * (ProjRevDef.RevenueCalculation1Amount.GetValueOrDefault() / 100);
                                            forecast.Revenue = forecast.Revenue + forecast.Fees;
                                        }
                                    }
                                    break;

                            }
                        }
                        else
                        {
                            forecast.Revenue = 0;
                        }
                    }
                    //else
                    //{
                    //    forecast.Revenue = 0;
                    //}
                }
                catch (Exception ex)
                {
                }
            }

            if (organization == string.Empty || organization == null)
            {
                //projId = forecastsDirectCosts.FirstOrDefault()?.Proj?.ProjId;

                if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
                {
                    if (projId != null)
                    {
                        organization = _context.PlProjects.AsNoTracking()
                            .Where(p => p.ProjId == projId)
                            .Include(p => p.Org)
                            .Select(p => p.Org.OrgName)
                            .FirstOrDefault();
                    }
                }
            }

            var forecasts = await _context.PlForecasts.AsNoTracking()
                            .Include(p => p.Emple)
                            .Include(p => p.Proj)
                            .Where(f => f.Emple != null && f.ProjId == projId && f.PlId == planID).OrderBy(p => p.Year).ThenBy(q => q.Month).ToListAsync();

            var organizationName = _context.PlProjects.AsNoTracking()
                                    .Include(p => p.Org)
                                    .Where(p => p.ProjId == projId)
                                    .Select(p => p.Org.OrgName)
                                    .FirstOrDefault();

            var accountName = _context.Accounts.AsNoTracking()
                              .Where(p => p.AcctId == projId)
                              .Select(p => p.AcctName)
                              .FirstOrDefault();

            if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
            {
                escallation_month = _context.PlConfigValues.AsNoTracking().FirstOrDefault(r => r.Name.ToLower() == "escallation_month" && r.ProjId == projId)?.Value ?? _context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "escallation_month" && r.ProjId == "xxxxx")?.Value ?? "3";
                escallation_percent = _context.PlConfigValues.AsNoTracking().FirstOrDefault(r => r.Name.ToLower() == "escallation_percent" && r.ProjId == projId)?.Value ?? _context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "escallation_percent" && r.ProjId == "xxxxx")?.Value ?? "3";
            }
            else
            {
                escallation_month = _context.PlConfigValues.AsNoTracking().FirstOrDefault(r => r.Name.ToLower() == "escallation_month" && r.ProjId == "xxxxx")?.Value ?? "7";
            }

            if (string.IsNullOrEmpty(escallation_month))
                escallation_month = "7";

            if (string.IsNullOrEmpty(escallation_percent))
                escallation_percent = "0";

            List<EmplSchedule> res = new List<EmplSchedule>();
            List<DirectCostSchedule> res1 = new List<DirectCostSchedule>();

            if (forecasts.Count() > 0)
            {
                res = financeHelper.CalculateSalaryPlanV1(forecasts, 0, DateTime.UtcNow.Year, Convert.ToInt32(escallation_month), Convert.ToDecimal(escallation_percent), projPlan);
                res1 = financeHelper.CalculateDirectCostPlan(forecastsDirectCosts);
            }

            foreach (var forecastempl in forecasts)
            {
                forecastempl.Revenue = 0;

                decimal hours = 0;

                fringe = 0; gna = 0; overHead = 0; ; mnh = 0; hr = 0;
                OverHeadOnGna = false; FringeOnOverhead = false; FringeOnGna = false;
                decimal perHrRate = 0;

                if (forecastempl.Year == 2026 && forecastempl.EmplId.StartsWith("PLC") && forecastempl.Month == 4)
                {

                }

                if (forecastempl.EmplId == "AMY.EWING" && forecastempl.Month == 1 && forecastempl.Year == 2026)
                {

                }

                if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
                {
                    //commented for Unanet
                    //perHrRate = res.FirstOrDefault(p => p.EmpId == forecastempl.EmplId && p.AccID == forecastempl.AcctId && Convert.ToString(p.PlcCode) == forecastempl.Plc && p.OrgID == forecastempl.OrgId)?.payrollSalary.FirstOrDefault(p => p.Month == forecastempl.Month && p.Year == forecastempl.Year)?.Salary ?? 0;
                    perHrRate = res.FirstOrDefault(p => p.EmpId == forecastempl.EmplId)?.payrollSalary.FirstOrDefault(p => p.Month == forecastempl.Month && p.Year == forecastempl.Year)?.Salary ?? 0;

                    if (forecastempl.ProjId.Contains("MANDH") || forecastempl.ProjId.Contains("FRNGE") || forecastempl.ProjId.Contains("GANDA") || forecastempl.ProjId.Contains("OVRHD") || forecastempl.ProjId.Contains("HRPAY"))
                        perHrRate = res.FirstOrDefault(p => p.EmpId == forecastempl.EmplId && p.AccID == forecastempl.AcctId && p.OrgID == forecastempl.OrgId)?.payrollSalary.FirstOrDefault(p => p.Month == forecastempl.Month && p.Year == forecastempl.Year)?.Salary ?? 0;
                }
                else
                    perHrRate = forecastempl.Emple.PerHourRate.GetValueOrDefault();

                if (forecastempl.Emple.Type.ToUpper() == "VENDOR EMPLOYEE" || forecastempl.Emple.Type.ToUpper() == "VENDOR" || forecastempl.Emple.Type.ToUpper() == "VENDOREMPLOYEE")
                {
                    if (forecastempl.HrlyRate.HasValue)
                        forecastempl.ForecastedCost = hours * forecastempl.HrlyRate.GetValueOrDefault();
                    else
                        forecastempl.ForecastedCost = hours * forecastempl.Emple.PerHourRate.GetValueOrDefault();
                }
                else
                {

                    DateTime forecastDay = new DateTime(forecastempl.Year, forecastempl.Month, DateTime.DaysInMonth(forecastempl.Year, forecastempl.Month));
                    if (forecastDay <= currentMonth && projPlan.PlType.ToUpper() == "EAC")
                    {
                        if (forecastempl.HrlyRate.HasValue)
                            forecastempl.ForecastedCost = hours * forecastempl.HrlyRate.GetValueOrDefault();
                        else
                            forecastempl.ForecastedCost = hours * perHrRate;
                    }
                    else
                    {
                        forecastempl.ForecastedCost = hours * perHrRate;
                    }
                }
                forecastempl.Cost = forecastempl.ForecastedCost.GetValueOrDefault();

                if (forecastempl.Year == 2026 && forecastempl.Month == 3 && forecastempl.EmplId == "CAT.CAVANAUGH")
                {

                }

                if (acctPool.Count() > 0)
                {
                    var burdens = burdensByTemplate.Where(r => r.Month == forecastempl.Month && r.Year == forecastempl.Year).ToList();
                    var validBurdens = financeHelper.GetBurdenRates(templatePools, burdens, type);
                    var pools = acctPool.Where(p => p.AccountId == forecastempl.AcctId && p.OrgId == forecastempl.OrgId && p.Year == forecastempl.Year);
                    foreach (var pool in pools)
                    {
                        //AllAvaialbepools.FirstOrDefault(p => p.Code == pool.PoolId).Type;
                        //switch (pool.PoolId.ToUpper())
                        switch (AllAvaialbepools.FirstOrDefault(p => p.Code == pool.PoolId).Type.ToUpper())
                        {
                            case "FRINGE":
                                validBurdens.TryGetValue(pool.PoolId.ToUpper(), out fringe);
                                break;
                            case "GNA":
                                validBurdens.TryGetValue(pool.PoolId.ToUpper(), out gna);
                                break;
                            case "MNH":
                                validBurdens.TryGetValue("MNH", out mnh);
                                break;
                            case "HR":
                                validBurdens.TryGetValue("HR", out hr);
                                break;
                            case "OVERHEAD":
                                validBurdens.TryGetValue(pool.PoolId.ToUpper(), out overHead);
                                break;
                            case "FRINGEONOVERHEAD":
                                FringeOnOverhead = true;
                                break;
                            case "FRINGEONGNA":
                                FringeOnGna = true;
                                break;
                            case "OVERHEADONGNA":
                                OverHeadOnGna = true;
                                break;
                        }
                    }
                }


                if (ceilingPools.Count > 0)
                {

                    var ceilingFringe = ceilingPools.FirstOrDefault(p => p.PoolCode == "FRINGE" && p.AccountId == forecastempl.AcctId && Convert.ToInt16(p.FiscalYear) == forecastempl.Year)?.RateCeiling.GetValueOrDefault();
                    var ceilingGNA = ceilingPools.FirstOrDefault(p => p.PoolCode == "GNA" && p.AccountId == forecastempl.AcctId && Convert.ToInt16(p.FiscalYear) == forecastempl.Year)?.RateCeiling.GetValueOrDefault();
                    var ceilingOVERHEAD = ceilingPools.FirstOrDefault(p => p.PoolCode == "OVERHEAD" && p.AccountId == forecastempl.AcctId && Convert.ToInt16(p.FiscalYear) == forecastempl.Year)?.RateCeiling.GetValueOrDefault();


                    if (ceilingFringe.HasValue)
                        fringe = Math.Min(fringe, ceilingFringe.GetValueOrDefault());

                    if (ceilingGNA.HasValue)
                        gna = Math.Min(gna, ceilingGNA.GetValueOrDefault());


                    if (ceilingOVERHEAD.HasValue)
                        overHead = Math.Min(overHead, ceilingOVERHEAD.GetValueOrDefault());

                }
                //////////////////////////////////Calculate Cost and Burden


                switch (projPlan.PlType)
                {
                    case "EAC":
                        hours = (decimal)(forecastempl.Actualhours);
                        break;
                    case "BUD":
                        hours = (decimal)(forecastempl.Forecastedhours);
                        break;
                    case "NBBUD":
                        hours = (decimal)(forecastempl.Forecastedhours);
                        break;
                }

                if (forecastempl.Emple.Type.ToUpper() == "VENDOR EMPLOYEE" || forecastempl.Emple.Type.ToUpper() == "VENDOR" || forecastempl.Emple.Type.ToUpper() == "VENDOREMPLOYEE")
                {
                    if (forecastempl.HrlyRate.HasValue && forecastempl.HrlyRate != 0)
                        forecastempl.ForecastedCost = hours * forecastempl.HrlyRate.GetValueOrDefault();
                    else
                        forecastempl.ForecastedCost = hours * forecastempl.Emple.PerHourRate.GetValueOrDefault();
                }
                else
                {

                    DateTime forecastDay = new DateTime(forecastempl.Year, forecastempl.Month, DateTime.DaysInMonth(forecastempl.Year, forecastempl.Month));
                    if (forecastDay <= currentMonth && projPlan.PlType.ToUpper() == "EAC")
                    {
                        if (forecastempl.HrlyRate.HasValue)
                            forecastempl.ForecastedCost = hours * forecastempl.HrlyRate.GetValueOrDefault();
                        else
                            forecastempl.ForecastedCost = hours * perHrRate;
                    }
                    else
                    {
                        forecastempl.ForecastedCost = hours * perHrRate;
                    }
                }
                forecastempl.Cost = forecastempl.ForecastedCost.GetValueOrDefault();
                if (forecastempl.Emple.IsBrd.GetValueOrDefault())
                {
                    if (forecastempl.Year == 2026 && forecastempl.Month == 1)
                    {

                    }
                    decimal ytdCost = 0;
                    switch (forecastempl.Emple.Type.ToUpper())
                    {
                        case "EMPLOYEE":
                            ytdCost = forecasts.Where(p => p.EmplId == forecastempl.EmplId && p.Year == forecastempl.Year && p.Month <= forecastempl.Month && p.OrgId == forecastempl.OrgId && p.AcctId == forecastempl.AcctId && p.Plc == forecastempl.Plc && forecastempl.Emple.Type.ToUpper() == "EMPLOYEE").Sum(p => p.Cost);
                            break;
                        case "OTHER":
                            ytdCost = forecasts.Where(p => p.EmplId == forecastempl.EmplId && p.Year == forecastempl.Year && p.Month <= forecastempl.Month && p.OrgId == forecastempl.OrgId && p.AcctId == forecastempl.AcctId && p.Plc == forecastempl.Plc && forecastempl.Emple.Type.ToUpper() == "OTHER").Sum(p => p.Cost);
                            break;
                        case "PLC":
                            ytdCost = forecasts.Where(p => p.EmplId == forecastempl.EmplId && p.Year == forecastempl.Year && p.Month <= forecastempl.Month && p.OrgId == forecastempl.OrgId && p.AcctId == forecastempl.AcctId && p.Plc == forecastempl.Plc && forecastempl.Emple.Type.ToUpper() == "PLC").Sum(p => p.Cost);
                            break;
                        case "VENDOR EMPLOYEE":
                        case "VENDOR":
                        case "VENDOREMPLOYEE":
                            ytdCost = forecasts.Where(p => p.EmplId == forecastempl.EmplId && p.Year == forecastempl.Year && p.Month <= forecastempl.Month && p.OrgId == forecastempl.OrgId && p.AcctId == forecastempl.AcctId && p.Plc == forecastempl.Plc && (forecastempl.Emple.Type.ToUpper() == "VENDOR EMPLOYEE" || forecastempl.Emple.Type.ToUpper() == "VENDOREMPLOYEE")).Sum(p => p.Cost);
                            break;
                    }


                    //if (forecastempl.Emple.Type.ToUpper() == "EMPLOYEE" || forecastempl.Emple.Type.ToUpper() == "OTHER")
                    //    ytdCost = forecasts.Where(p => p.EmplId == forecastempl.EmplId && p.Year == forecastempl.Year && p.Month <= forecastempl.Month && p.OrgId == forecastempl.OrgId && p.AcctId == forecastempl.AcctId && p.Plc == forecastempl.Plc && forecastempl.Emple.Type.ToUpper() == "EMPLOYEE").Sum(p => p.Cost);
                    //else
                    //    ytdCost = forecasts.Where(p => p.EmplId == forecastempl.EmplId && p.Year == forecastempl.Year && p.Month <= forecastempl.Month && p.OrgId == forecastempl.OrgId && p.AcctId == forecastempl.AcctId && p.Plc == forecastempl.Plc && forecastempl.Emple.Type.ToUpper() == "VENDOR EMPLOYEE").Sum(p => p.Cost);
                    forecastempl.YtdCost = ytdCost;
                    var ytdSubContractorCost = forecasts.Where(p => p.EmplId == forecastempl.EmplId && p.Year == forecastempl.Year && p.Month <= forecastempl.Month && p.OrgId == forecastempl.OrgId && p.AcctId == forecastempl.AcctId && p.Plc == forecastempl.Plc && forecastempl.Emple.Type.ToUpper() == "VENDOR EMPLOYEE").Sum(p => p.Cost);
                    //var prevFringe = forecasts.FirstOrDefault(p => p.EmplId == forecastempl.EmplId && p.Year == forecastempl.Year && p.Month == forecastempl.Month - 1);
                    PlForecast prevFringe = new PlForecast();
                    List<PlForecast> ytdPrev = new List<PlForecast>();
                    decimal previousFringe = 0m, previousOverHead = 0m, previousGNA = 0m, previousMNH = 0m;

                    if (forecastempl.Month > 1)
                    {
                        prevFringe = forecasts
                            .FirstOrDefault(p =>
                                p.EmplId == forecastempl.EmplId &&
                                p.Year == forecastempl.Year &&
                                p.Month == forecastempl.Month - 1
                                && p.OrgId == forecastempl.OrgId && p.AcctId == forecastempl.AcctId && p.Plc == forecastempl.Plc);
                    }
                    if (prevFringe != null)
                    {
                        previousFringe = prevFringe.YtdFringe;
                        previousOverHead = prevFringe.YtdOverhead;
                        previousGNA = prevFringe.YtdGna;
                        previousMNH = prevFringe.YtdMaterials;
                    }

                    var ytdFringe = ytdCost * (fringe / 100);
                    forecastempl.YtdFringe = ytdFringe;
                    forecastempl.Fringe = ytdFringe - previousFringe;


                    var baseForOverhead = ytdCost + (FringeOnOverhead ? ytdFringe : 0);
                    var ytdOverHead = (decimal)(baseForOverhead * overHead / 100);
                    forecastempl.YtdOverhead = ytdOverHead;

                    forecastempl.Overhead = ytdOverHead - previousOverHead;



                    var ytdMaterials = (decimal)((ytdCost) * mnh / 100);
                    forecastempl.YtdMaterials = ytdMaterials;

                    forecastempl.Materials = ytdMaterials - previousMNH;


                    // Calculate Gna
                    decimal baseForGna = ytdCost;
                    if (FringeOnGna) baseForGna += ytdFringe;
                    if (OverHeadOnGna) baseForGna += ytdOverHead;

                    var ytdGna = (decimal)(baseForGna * gna / 100);
                    forecastempl.YtdGna = ytdGna;

                    forecastempl.Gna = ytdGna - previousGNA;

                    forecastempl.Burden = forecastempl.Fringe + forecastempl.Overhead + forecastempl.Gna + forecastempl.Materials;
                    //forecastempl.TotalBurdenCost = financeHelper.CalculateBurdenedCostBYRAPRate(new BurdenInput() { FringeRate = fringe, GnaRate = gna, HourlyRate = perHrRate, Hours = hours, OverheadRate = overHead, plEmployee = forecastempl.Empl });
                    //forecastempl.TotalBurdenCost = financeHelper.CalculateBurdenedCost(new BurdenInput() { FringeRate = fringe, GnaRate = gna, HourlyRate = perHrRate, Hours = hours, OverheadRate = overHead, plEmployee = forecastempl.Empl });
                    forecastempl.TotalBurdenCost = forecastempl.ForecastedCost.GetValueOrDefault() + forecastempl.Burden;

                }
                else
                {
                    //Need to discuss whether to calculate burden for non burden employees  for actuals?
                    if (projPlan.PlType.ToUpper() == "EAC" && new DateOnly(forecastempl.Year, forecastempl.Month, 1) < projPlan.ClosedPeriod.GetValueOrDefault())
                    {
                        forecastempl.Cost = forecastempl.Cost;

                        decimal ytdCost = 0;
                        //var ytdCost = forecasts.Where(p => p.EmplId == forecastempl.EmplId && (new DateOnly(p.Year, p.Month, 1) <= new DateOnly(forecastempl.Year, forecastempl.Month, 1))).Sum(p => p.Cost);
                        if (forecastempl.Emple.Type.ToUpper() == "EMPLOYEE")
                            ytdCost = forecasts.Where(p => p.EmplId == forecastempl.EmplId && p.Year == forecastempl.Year && p.Month <= forecastempl.Month && p.OrgId == forecastempl.OrgId && p.AcctId == forecastempl.AcctId && p.Plc == forecastempl.Plc && forecastempl.Emple.Type.ToUpper() == "EMPLOYEE").Sum(p => p.Cost);
                        else
                            ytdCost = forecasts.Where(p => p.EmplId == forecastempl.EmplId && p.Year == forecastempl.Year && p.Month <= forecastempl.Month && p.OrgId == forecastempl.OrgId && p.AcctId == forecastempl.AcctId && p.Plc == forecastempl.Plc && forecastempl.Emple.Type.ToUpper() == "VENDOR EMPLOYEE").Sum(p => p.Cost);
                        forecastempl.YtdCost = ytdCost;
                        var ytdSubContractorCost = forecasts.Where(p => p.EmplId == forecastempl.EmplId && p.Year == forecastempl.Year && p.Month <= forecastempl.Month && p.OrgId == forecastempl.OrgId && p.AcctId == forecastempl.AcctId && p.Plc == forecastempl.Plc && forecastempl.Emple.Type.ToUpper() == "VENDOR EMPLOYEE").Sum(p => p.Cost);
                        //var prevFringe = forecasts.FirstOrDefault(p => p.EmplId == forecastempl.EmplId && p.Year == forecastempl.Year && p.Month == forecastempl.Month - 1);
                        PlForecast prevFringe = new PlForecast();
                        List<PlForecast> ytdPrev = new List<PlForecast>();
                        decimal previousFringe = 0m, previousOverHead = 0m, previousGNA = 0m, previousMNH = 0m;

                        if (forecastempl.Month > 1)
                        {
                            prevFringe = forecasts
                                .FirstOrDefault(p =>
                                    p.EmplId == forecastempl.EmplId &&
                                    p.Year == forecastempl.Year &&
                                    p.Month == forecastempl.Month - 1
                                    && p.OrgId == forecastempl.OrgId && p.AcctId == forecastempl.AcctId && p.Plc == forecastempl.Plc);


                            //ytdPrev = forecasts.Where(p => p.EmplId == forecastempl.EmplId && p.Year == forecastempl.Year && p.Month <= forecastempl.Month && p.OrgId == forecastempl.OrgId && p.AcctId == forecastempl.AcctId && p.Plc == forecastempl.Plc).ToList();
                        }
                        if (prevFringe != null)
                        {
                            previousFringe = prevFringe.YtdFringe;
                            previousOverHead = prevFringe.YtdOverhead;
                            previousGNA = prevFringe.YtdGna;
                            previousMNH = prevFringe.YtdMaterials;
                            //previousFringe = ytdPrev.Sum(p=>p.Fringe);
                            //previousOverHead = ytdPrev.Sum(p => p.Overhead);
                            //previousGNA = ytdPrev.Sum(p => p.Gna);
                            //previousMNH = ytdPrev.Sum(p => p.Materials);
                        }

                        var ytdFringe = ytdCost * (fringe / 100);
                        forecastempl.YtdFringe = ytdFringe;
                        forecastempl.Fringe = ytdFringe - previousFringe;


                        var baseForOverhead = ytdCost + (FringeOnOverhead ? ytdFringe : 0);
                        var ytdOverHead = (decimal)(baseForOverhead * overHead / 100);
                        forecastempl.YtdOverhead = ytdOverHead;

                        forecastempl.Overhead = ytdOverHead - previousOverHead;



                        var ytdMaterials = (decimal)((ytdCost) * mnh / 100);
                        forecastempl.YtdMaterials = ytdMaterials;

                        forecastempl.Materials = ytdMaterials - previousMNH;


                        // Calculate Gna
                        decimal baseForGna = ytdCost;
                        if (FringeOnGna) baseForGna += ytdFringe;
                        if (OverHeadOnGna) baseForGna += ytdOverHead;

                        var ytdGna = (decimal)(baseForGna * gna / 100);
                        forecastempl.YtdGna = ytdGna;

                        forecastempl.Gna = ytdGna - previousGNA;


                        forecastempl.Burden = forecastempl.Fringe + forecastempl.Overhead + forecastempl.Gna + forecastempl.Materials;
                        forecastempl.TotalBurdenCost = forecastempl.Cost + forecastempl.Burden;
                    }
                    else
                    {
                        forecastempl.Cost = forecastempl.ForecastedCost.GetValueOrDefault();
                        forecastempl.Materials = 0;
                        forecastempl.Gna = 0;
                        forecastempl.Overhead = 0;
                        forecastempl.Fringe = 0;
                        forecastempl.Burden = 0;
                        forecastempl.Hr = 0;
                        // forecastempl.TotalBurdenCost = forecastempl.Cost;
                    }
                }
                /////////////////////////Calculate Revenue
                //if (OverrideFRevenueSettings)
                {
                    if (projPlan.PlType.ToUpper() == "EAC")
                    {
                        hours = forecastempl.Actualhours;
                    }
                    else
                    {
                        hours = forecastempl.Forecastedhours;

                    }

                    if (forecastempl.Emple.IsRev.GetValueOrDefault())
                    {
                        if (forecastempl.Year == 2025 && forecastempl.Month == 7 && forecastempl.EmplId == "1003546")
                        {

                        }
                        switch (revenueFormula)
                        {
                            case "CPFH":

                                if (revenueSetup != null && OverrideFRevenueSettings)
                                {
                                    if (revenueSetup.LabCostFl.GetValueOrDefault())
                                        forecastempl.Revenue += forecastempl.Cost;

                                    if (revenueSetup.LabBurdFl.GetValueOrDefault())
                                        forecastempl.Revenue += forecastempl.Burden;


                                    if (revenueSetup.LabFeeHrsFl.GetValueOrDefault())
                                    {
                                        laborFees = revenueSetup.LabFeeRt.GetValueOrDefault();
                                        forecastempl.Fees = hours * laborFees;
                                        forecastempl.Revenue += forecastempl.Fees;
                                    }
                                }
                                else
                                {
                                    if (ProjRevDef != null)
                                    {
                                        forecastempl.Revenue = forecastempl.Cost + forecastempl.Burden + (hours * ProjRevDef.RevenueCalculationAmount.GetValueOrDefault());
                                    }
                                }
                                break;

                            case "CPFC":
                                if (revenueSetup != null && OverrideFRevenueSettings)
                                {
                                    if (revenueSetup.LabCostFl.GetValueOrDefault())
                                        forecastempl.Revenue += forecastempl.Cost;

                                    if (revenueSetup.LabBurdFl.GetValueOrDefault())
                                        forecastempl.Revenue += forecastempl.Burden;


                                    if (revenueSetup.LabFeeCostFl.GetValueOrDefault())
                                    {
                                        laborFees = revenueSetup.LabFeeRt.GetValueOrDefault();
                                        forecastempl.Fees = forecastempl.Revenue * (laborFees / 100);
                                    }
                                    forecastempl.Revenue += forecastempl.Fees;
                                }
                                else
                                {
                                    if (ProjRevDef != null)
                                    {
                                        forecastempl.Revenue = (forecastempl.Cost + forecastempl.Burden) + (forecastempl.Cost + forecastempl.Burden) * (ProjRevDef.RevenueCalculationAmount.GetValueOrDefault() / 100);
                                    }
                                }
                                break;

                            case "LLRCINL":
                                if (revenueSetup != null && OverrideFRevenueSettings)
                                {
                                    if (revenueSetup.LabTmFl.GetValueOrDefault())
                                        forecastempl.Revenue = await financeHelper.CalculateRevenueTNMAsync(forecastempl.ProjId ?? string.Empty, forecastempl.Plc ?? string.Empty, hours, forecastempl.Month, forecastempl.Year, forecastempl.EmplId ?? string.Empty);
                                }
                                else
                                {
                                    if (ProjRevDef != null)
                                    {
                                        forecastempl.Revenue = await financeHelper.CalculateRevenueTNMAsync(forecastempl.ProjId ?? string.Empty, forecastempl.Plc ?? string.Empty, hours, forecastempl.Month, forecastempl.Year, forecastempl.EmplId ?? string.Empty);
                                    }
                                }
                                //forecastempl.Revenue = await financeHelper.CalculateRevenueTNMAsync(forecastempl.ProjId ?? string.Empty, forecastempl.Plc ?? string.Empty, hours, forecastempl.Month, forecastempl.Year, forecastempl.EmplId ?? string.Empty);
                                //forecastempl.Fees = forecastempl.Revenue * (laborFees / 100);
                                //forecastempl.Revenue = forecastempl.Revenue + forecastempl.Fees;
                                break;

                            case "LLR":
                                if (revenueSetup != null && OverrideFRevenueSettings)
                                {
                                    if (revenueSetup.LabTmFl.GetValueOrDefault())
                                        forecastempl.Revenue = await financeHelper.CalculateRevenueTNMAsync(forecastempl.ProjId ?? string.Empty, forecastempl.Plc ?? string.Empty, hours, forecastempl.Month, forecastempl.Year, forecastempl.EmplId ?? string.Empty);
                                }
                                else
                                {
                                    if (ProjRevDef != null)
                                    {
                                        forecastempl.Revenue = await financeHelper.CalculateRevenueTNMAsync(forecastempl.ProjId ?? string.Empty, forecastempl.Plc ?? string.Empty, hours, forecastempl.Month, forecastempl.Year, forecastempl.EmplId ?? string.Empty);
                                    }
                                }
                                //forecastempl.Revenue = await financeHelper.CalculateRevenueTNMAsync(forecastempl.ProjId ?? string.Empty, forecastempl.Plc ?? string.Empty, hours, forecastempl.Month, forecastempl.Year, forecastempl.EmplId ?? string.Empty);
                                //forecastempl.Fees = forecastempl.Revenue * (laborFees / 100);
                                //forecastempl.Revenue = forecastempl.Revenue + forecastempl.Fees; 
                                break;

                            case "LLRCINBF":
                                if (revenueSetup != null && OverrideFRevenueSettings)
                                {
                                    if (revenueSetup.LabTmFl.GetValueOrDefault())
                                        forecastempl.Revenue = await financeHelper.CalculateRevenueTNMAsync(forecastempl.ProjId ?? string.Empty, forecastempl.Plc ?? string.Empty, hours, forecastempl.Month, forecastempl.Year, forecastempl.EmplId ?? string.Empty);
                                }
                                else
                                {
                                    if (ProjRevDef != null)
                                    {
                                        forecastempl.Revenue = await financeHelper.CalculateRevenueTNMAsync(forecastempl.ProjId ?? string.Empty, forecastempl.Plc ?? string.Empty, hours, forecastempl.Month, forecastempl.Year, forecastempl.EmplId ?? string.Empty);
                                    }
                                }
                                break;

                            case "CPFF":
                                if (revenueSetup != null && OverrideFRevenueSettings)
                                {
                                    if (revenueSetup.LabCostFl.GetValueOrDefault() && revenueSetup.LabBurdFl.GetValueOrDefault())
                                    {
                                        //forecastempl.Fees = (forecastempl.Cost + forecastempl.Burden) * (laborFees / 100);
                                        //forecastempl.Revenue = (forecastempl.Cost + forecastempl.Burden) * (1 + laborFees / 100);
                                        forecastempl.Revenue = (forecastempl.Cost + forecastempl.Burden);
                                    }
                                    else
                                    {
                                        if (revenueSetup.LabCostFl.GetValueOrDefault())
                                        {
                                            //forecastempl.Fees = forecastempl.Cost * (laborFees / 100);
                                            //forecastempl.Revenue = forecastempl.Cost * (1 + laborFees / 100);
                                            forecastempl.Revenue = forecastempl.Cost;
                                        }
                                        else
                                        {
                                            if (revenueSetup.LabBurdFl.GetValueOrDefault())
                                            {
                                                //forecastempl.Fees = forecastempl.Burden * (laborFees / 100);
                                                //forecastempl.Revenue = forecastempl.Burden * (1 + laborFees / 100);
                                                forecastempl.Revenue = forecastempl.Burden;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (ProjRevDef != null)
                                    {
                                        forecastempl.Fees = (forecastempl.Cost + forecastempl.Burden) * (ProjRevDef.RevenueCalculationAmount.GetValueOrDefault() / 100);
                                        forecastempl.Revenue = forecastempl.Cost + forecastempl.Burden + forecastempl.Fees;
                                    }
                                }
                                break;

                            case "LLRCINLB":
                                //forecastempl.Revenue = await financeHelper.CalculateRevenueTNMAsync(forecastempl.ProjId ?? string.Empty, forecastempl.Plc ?? string.Empty, hours, forecastempl.Month, forecastempl.Year, forecastempl.EmplId ?? string.Empty);
                                //forecastempl.Fees = forecastempl.Revenue * (laborFees / 100);
                                //forecastempl.Revenue = forecastempl.Revenue + forecastempl.Fees; break;
                                if (revenueSetup != null && OverrideFRevenueSettings)
                                {
                                    if (revenueSetup.LabTmFl.GetValueOrDefault())
                                        forecastempl.Revenue = await financeHelper.CalculateRevenueTNMAsync(forecastempl.ProjId ?? string.Empty, forecastempl.Plc ?? string.Empty, hours, forecastempl.Month, forecastempl.Year, forecastempl.EmplId ?? string.Empty);
                                }
                                else
                                {
                                    if (ProjRevDef != null)
                                    {
                                        forecastempl.Revenue = await financeHelper.CalculateRevenueTNMAsync(forecastempl.ProjId ?? string.Empty, forecastempl.Plc ?? string.Empty, hours, forecastempl.Month, forecastempl.Year, forecastempl.EmplId ?? string.Empty);
                                    }
                                }
                                break;

                            case "LLRFNLBF":
                                if (revenueSetup != null && OverrideFRevenueSettings)
                                {
                                    if (revenueSetup.LabTmFl.GetValueOrDefault())
                                        forecastempl.Revenue = await financeHelper.CalculateRevenueTNMAsync(forecastempl.ProjId ?? string.Empty, forecastempl.Plc ?? string.Empty, hours, forecastempl.Month, forecastempl.Year, forecastempl.EmplId ?? string.Empty);
                                    //else
                                    //    forecastempl.Revenue += forecastempl.Cost;

                                    if (revenueSetup.LabFeeCostFl.GetValueOrDefault())
                                    {
                                        laborFees = revenueSetup.LabFeeRt.GetValueOrDefault();
                                        forecastempl.Fees = forecastempl.Revenue * (laborFees / 100);
                                        forecastempl.Revenue += forecastempl.Fees;
                                    }
                                }
                                else
                                {
                                    if (ProjRevDef != null)
                                    {
                                        forecastempl.Revenue = await financeHelper.CalculateRevenueTNMAsync(forecastempl.ProjId ?? string.Empty, forecastempl.Plc ?? string.Empty, hours, forecastempl.Month, forecastempl.Year, forecastempl.EmplId ?? string.Empty);
                                        forecastempl.Fees = forecastempl.Revenue * (ProjRevDef.RevenueCalculationAmount.GetValueOrDefault() / 100);
                                        forecastempl.Revenue += forecastempl.Fees;
                                    }
                                }
                                break;

                            case "RSBFNLBF":
                                if (revenueSetup != null && OverrideFRevenueSettings)
                                {
                                    if (revenueSetup.LabTmFl.GetValueOrDefault())
                                        forecastempl.Revenue = await financeHelper.CalculateRevenueTNMAsync(forecastempl.ProjId ?? string.Empty, forecastempl.Plc ?? string.Empty, hours, forecastempl.Month, forecastempl.Year, forecastempl.EmplId ?? string.Empty);

                                    if (revenueSetup.LabFeeCostFl.GetValueOrDefault())
                                        forecastempl.Revenue += forecastempl.Burden;
                                    //else
                                    //    forecastempl.Revenue += forecastempl.Cost;

                                    if (revenueSetup.LabFeeCostFl.GetValueOrDefault())
                                    {
                                        laborFees = revenueSetup.LabFeeRt.GetValueOrDefault();
                                        forecastempl.Fees = forecastempl.Revenue * (laborFees / 100);
                                        forecastempl.Revenue += forecastempl.Fees;
                                    }
                                }
                                else
                                {
                                    if (ProjRevDef != null)
                                    {
                                        forecastempl.Revenue = await financeHelper.CalculateRevenueTNMAsync(forecastempl.ProjId ?? string.Empty, forecastempl.Plc ?? string.Empty, hours, forecastempl.Month, forecastempl.Year, forecastempl.EmplId ?? string.Empty);
                                        forecastempl.Revenue += forecastempl.Burden;
                                        forecastempl.Fees = forecastempl.Revenue * (ProjRevDef.RevenueCalculationAmount.GetValueOrDefault() / 100);
                                        forecastempl.Revenue += forecastempl.Fees;
                                    }
                                }
                                break;


                        }
                    }
                    else
                        forecastempl.Revenue = 0;
                }
                //else
                //{
                //    forecastempl.Revenue = 0;
                //}

            }

            /////////////////Get Ceiling Configuration For Burdens

            var ceilingEmplHours = _context.PlCeilHrEmpls.AsNoTracking().Where(p => p.ProjectId == projId).ToList();
            var ceilingLabCatHours = _context.PlCeilHrCats.AsNoTracking().Where(p => p.ProjectId == projId).ToList();

            var combinedList = forecasts
                            .Concat(forecastsDirectCosts)
                            .OrderBy(x => x.Year)
                            .ThenBy(x => x.Month)
                            .ToList();

            if (projPlan.PlType.ToUpper() == "NBBUD")
            {
                fundingValue = 100000000;
            }

            var Adjusted = financeHelper.AdjustRevenue(combinedList, fundingValue, projPlan);
            var swTotal = Stopwatch.StartNew();

            await _context.BulkUpdateAsync(
                    Adjusted,
                    new BulkConfig
                    {
                        BatchSize = 1000,
                        SetOutputIdentity = false
                    });

            //foreach (var chunk in Adjusted.Chunk(300))
            //{
            //    var chunkIds = chunk.Select(f => f.Forecastid).ToList();
            //    await _context.BulkUpdateAsync(chunk, new BulkConfig { SetOutputIdentity = true });
            //    await _context.SaveChangesAsync();
            //}
            swTotal.Stop();
            Console.WriteLine($"🎯 Bulk update completed in {swTotal.Elapsed.TotalSeconds:F2}s");


            planForecastSummary.Version = projPlan.Version.GetValueOrDefault();
            planForecastSummary.Proj_Id = projPlan.ProjId;
            planForecastSummary.Type = projPlan.PlType;
        }
        catch (Exception ex)
        {

        }
        return planForecastSummary;
    }

    public async Task<PlanForecastSummary> CalculateBurdenCost(
    int planID,
    int templateId,
    string type)
    {
        PlanForecastSummary planForecastSummary = new()
        {
            EmployeeForecastSummary = new List<EmployeeForecastSummary>()
        };

        try
        {
            // =========================================================
            // LOAD CONFIGURATION
            // =========================================================

            var configValues = await _context.PlConfigValues
                .AsNoTracking()
                .ToListAsync();

            var configLookup = configValues
                .GroupBy(x => $"{x.Name.ToLower()}|{x.ProjId ?? ""}")
                .ToDictionary(
                    g => g.Key,
                    g => g.First().Value);

            DateTime currentMonth;

            if (!DateTime.TryParse(
                    configLookup.GetValueOrDefault("closing_period|xxxxx"),
                    out currentMonth))
            {
                throw new Exception("Invalid closing period configuration");
            }

            // =========================================================
            // PROJECT PLAN
            // =========================================================

            var projPlan = await _context.PlProjectPlans
                .Include(p => p.Proj)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PlId == planID);

            if (projPlan == null)
                return planForecastSummary;

            string projId = projPlan.ProjId;
            string projType = projPlan.Proj?.ProjTypeDc ?? "";

            // =========================================================
            // ACCOUNT GROUPS
            // =========================================================

            var allPools = await _context.AccountGroups
                .AsNoTracking()
                .ToListAsync();

            var poolTypeLookup = allPools
                .ToDictionary(x => x.Code, x => x.Type);

            // =========================================================
            // VALID ORGS
            // =========================================================

            var emplOrgs = await _context.PlEmployeees
                .AsNoTracking()
                .Where(e => e.PlId == planID && e.OrgId != null)
                .Select(e => e.OrgId)
                .ToListAsync();

            var dctOrgs = await _context.PlDcts
                .AsNoTracking()
                .Where(d => d.PlId == planID && d.OrgId != null)
                .Select(d => d.OrgId)
                .ToListAsync();

            var validOrgs = emplOrgs
                .Union(dctOrgs)
                .Distinct()
                .ToList();

            // =========================================================
            // ACCOUNT POOL
            // =========================================================

            var acctPool = await _context.PlOrgAcctPoolMappings
                .AsNoTracking()
                .Where(x => validOrgs.Contains(x.OrgId))
                .ToListAsync();

            var acctPoolLookup = acctPool
                .GroupBy(x => $"{x.AccountId}|{x.OrgId}|{x.Year}")
                .ToDictionary(g => g.Key, g => g.ToList());

            // =========================================================
            // TEMPLATE BURDENS
            // =========================================================

            var burdensByTemplate = await _context.PlTemplatePoolRates
                .AsNoTracking()
                .Where(r => r.TemplateId == templateId)
                .ToListAsync();

            var burdenLookup = burdensByTemplate
                .GroupBy(x => $"{x.Month}|{x.Year}")
                .ToDictionary(g => g.Key, g => g.ToList());

            // =========================================================
            // FORECASTS
            // =========================================================

            var forecasts = await _context.PlForecasts
                .Include(p => p.Emple)
                .Where(f =>
                    f.Emple != null &&
                    f.ProjId == projId &&
                    f.PlId == planID)
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            var directForecasts = await _context.PlForecasts
                .Include(p => p.DirectCost)
                .Where(f =>
                    f.DirectCost != null &&
                    f.ProjId == projId &&
                    f.PlId == planID)
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            // =========================================================
            // PRE-GROUP FORECASTS
            // =========================================================

            var groupedForecasts = forecasts
                .GroupBy(x =>
                    $"{x.EmplId}|{x.Year}|{x.OrgId}|{x.AcctId}|{x.Plc}")
                .ToDictionary(g => g.Key,
                    g => g.OrderBy(x => x.Month).ToList());

            var groupedDirectForecasts = directForecasts
                .GroupBy(x =>
                    $"{x.EmplId}|{x.Year}|{x.OrgId}|{x.AcctId}|{x.Plc}")
                .ToDictionary(g => g.Key,
                    g => g.OrderBy(x => x.Month).ToList());

            FinanceHelper financeHelper = new FinanceHelper(_context, projId);

            // =========================================================
            // EMPLOYEE FORECASTS
            // =========================================================

            foreach (var kv in groupedForecasts)
            {
                decimal runningCost = 0;
                decimal runningFringe = 0;
                decimal runningOH = 0;
                decimal runningGna = 0;
                decimal runningMnh = 0;

                foreach (var forecast in kv.Value)
                {
                    var emplType = forecast.Emple?.Type?
                        .ToUpperInvariant() ?? "";

                    decimal fringe = 0;
                    decimal gna = 0;
                    decimal overhead = 0;
                    decimal mnh = 0;
                    Boolean FringeOnOverhead = false;
                    Boolean FringeOnGna = false;
                    Boolean OverHeadOnGna = false;
                    // ---------------------------------------------

                    var burdenKey = $"{forecast.Month}|{forecast.Year}";

                    burdenLookup.TryGetValue(
                        burdenKey,
                        out var burdenRates);

                    var acctKey =
                        $"{forecast.AcctId}|{forecast.OrgId}|{forecast.Year}";

                    acctPoolLookup.TryGetValue(
                        acctKey,
                        out var pools);

                    if (pools != null && burdenRates != null)
                    {
                        var validBurdenRates =
                            financeHelper.GetBurdenRates(
                                pools.Select(x => x.PoolId).ToList(),
                                burdenRates,
                                type);

                        foreach (var pool in pools)
                        {
                            if (!poolTypeLookup.TryGetValue(
                                    pool.PoolId,
                                    out var poolType))
                                continue;

                            switch (poolType.ToUpperInvariant())
                            {
                                case "FRINGE":
                                    validBurdenRates.TryGetValue(
                                        pool.PoolId.ToUpper(),
                                        out fringe);
                                    break;

                                case "GNA":
                                    validBurdenRates.TryGetValue(
                                        pool.PoolId.ToUpper(),
                                        out gna);
                                    break;

                                case "OVERHEAD":
                                    validBurdenRates.TryGetValue(
                                        pool.PoolId.ToUpper(),
                                        out overhead);
                                    break;

                                case "MNH":
                                    validBurdenRates.TryGetValue(
                                        "MNH",
                                        out mnh);
                                    break;

                                case "FRINGEONOVERHEAD":
                                    FringeOnOverhead = true;
                                    break;
                                case "FRINGEONGNA":
                                    FringeOnGna = true;
                                    break;
                                case "OVERHEADONGNA":
                                    OverHeadOnGna = true;
                                    break;
                            }
                        }
                    }

                    // =================================================
                    // COST
                    // =================================================

                    decimal hours =
                        projPlan.PlType == "EAC"
                            ? forecast.Actualhours
                            : forecast.Forecastedhours;

                    decimal rate =
                        forecast.HrlyRate ??
                        forecast.Emple?.PerHourRate ??
                        0;

                    forecast.ForecastedCost = hours * rate;
                    forecast.Cost = forecast.ForecastedCost ?? 0;

                    // =================================================
                    // RUNNING YTD
                    // =================================================

                    runningCost += forecast.Cost;

                    forecast.YtdCost = runningCost;

                    // =================================================
                    // BURDENS
                    // =================================================

                    if (forecast.Emple?.IsBrd == true)
                    {
                        decimal ytdFringe =
                            runningCost * (fringe / 100);

                        decimal currentFringe =
                            ytdFringe - runningFringe;

                        runningFringe = ytdFringe;

                        decimal ytdOH =
                            (runningCost + ytdFringe)
                            * (overhead / 100);

                        decimal currentOH =
                            ytdOH - runningOH;

                        runningOH = ytdOH;

                        decimal ytdGna =
                            runningCost * (gna / 100);

                        decimal currentGna =
                            ytdGna - runningGna;

                        runningGna = ytdGna;

                        decimal ytdMnh =
                            runningCost * (mnh / 100);

                        decimal currentMnh =
                            ytdMnh - runningMnh;

                        runningMnh = ytdMnh;

                        forecast.Fringe = currentFringe;
                        forecast.Overhead = currentOH;
                        forecast.Gna = currentGna;
                        forecast.Materials = currentMnh;

                        forecast.YtdFringe = ytdFringe;
                        forecast.YtdOverhead = ytdOH;
                        forecast.YtdGna = ytdGna;
                        forecast.YtdMaterials = ytdMnh;

                        forecast.Burden =
                            currentFringe +
                            currentOH +
                            currentGna +
                            currentMnh;

                        forecast.TotalBurdenCost =
                            forecast.Cost +
                            forecast.Burden;
                    }
                    else
                    {
                        forecast.Fringe = 0;
                        forecast.Overhead = 0;
                        forecast.Gna = 0;
                        forecast.Materials = 0;
                        forecast.Burden = 0;

                        forecast.TotalBurdenCost =
                            forecast.Cost;
                    }
                }
            }

            // =========================================================
            // DIRECT COST FORECASTS
            // =========================================================

            foreach (var kv in groupedDirectForecasts)
            {
                decimal runningCost = 0;

                foreach (var forecast in kv.Value)
                {
                    forecast.Cost =
                        projPlan.PlType == "EAC"
                        ? forecast.Actualamt ?? 0
                        : forecast.Forecastedamt ?? 0;

                    runningCost += forecast.Cost;

                    forecast.YtdCost = runningCost;

                    forecast.TotalBurdenCost =
                        forecast.Cost + forecast.Burden;
                }
            }

            // =========================================================
            // SAVE
            // =========================================================

            var combinedList = forecasts
                .Concat(directForecasts)
                .ToList();

            await _context.BulkUpdateAsync(
                combinedList,
                new BulkConfig
                {
                    BatchSize = 1000,
                    SetOutputIdentity = false
                });

            // =========================================================
            // RESPONSE
            // =========================================================

            planForecastSummary.Version =
                projPlan.Version ?? 0;

            planForecastSummary.Proj_Id =
                projPlan.ProjId;

            planForecastSummary.Type =
                projPlan.PlType;

            return planForecastSummary;
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"Error calculating burden cost: {ex.Message}",
                ex);
        }
    }

    //public async Task<PlanForecastSummary> CalculateBurdenCost(int planID, int templateId, string type)
    //{
    //    int cost = 0;
    //    List<ProjRevWrkPd> adj = new List<ProjRevWrkPd>();
    //    string revenueFormula = "";
    //    string escallation_month = "0", escallation_percent = "0";


    //    string projId = string.Empty, organization = string.Empty, projType = string.Empty;
    //    List<EmployeeForecastSummary> employeeForecastSummary = new List<EmployeeForecastSummary>();
    //    List<EmployeeForecastSummary> directCostForecastSummary = new List<EmployeeForecastSummary>();
    //    List<MonthlyCostRevenue> MonthlyCostRevenue = new List<MonthlyCostRevenue>();
    //    bool OverrideFRevenueSettings = false;

    //    PlanForecastSummary planForecastSummary = new PlanForecastSummary();
    //    planForecastSummary.EmployeeForecastSummary = new List<EmployeeForecastSummary>();
    //    DateTime currentMonth;

    //    var closingPeriodConfig = _context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "closing_period");

    //    try
    //    {

    //        if (closingPeriodConfig != null && DateTime.TryParse(closingPeriodConfig.Value, out currentMonth))
    //        {
    //            // currentMonth is now safely parsed
    //        }
    //        else
    //        {
    //            // Handle the missing or invalid value here
    //            throw new Exception("Invalid or missing 'closing_period' configuration.");
    //        }

    //        var AllAvaialbepools = _context.AccountGroups.ToList();

    //        decimal fringe = 0, gna = 0, overHead = 0, laborFees = 0, hr = 0, mnh = 0, nonLaborFees = 0, RevenueAdj = 0, fundingValue = 0, ActualRevenue = 0;
    //        bool OverHeadOnGna = false, FringeOnOverhead = false, FringeOnGna = false;

    //        List<PlTemplatePoolRate> burdensByTemplate = new List<PlTemplatePoolRate>();
    //        List<pl_EmployeeBurdenCalculated> plEmployeeBurdenCalculated = new List<pl_EmployeeBurdenCalculated>();
    //        var projPlan = _context.PlProjectPlans.Where(p => p.PlId == planID).Include(p => p.Proj).AsNoTracking().FirstOrDefault();

    //        if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
    //        {
    //            projId = projPlan.Proj.ProjId;
    //            projType = projPlan.Proj.ProjTypeDc;
    //            //if (projPlan.ProjEndDt == null)
    //            //    projPlan.ProjEndDt = projPlan.Proj.ProjEndDt;

    //            //if (projPlan.ProjEndDt == null)
    //            //    projPlan.ProjStartDt = projPlan.Proj.ProjStartDt;
    //            fundingValue = projPlan.Proj.proj_f_tot_amt.GetValueOrDefault();
    //        }
    //        else
    //        {
    //            projId = projPlan.ProjId;
    //            var projPlanNBBUD = _context.NewBusinessBudgets.Where(p => p.BusinessBudgetId == projPlan.ProjId).FirstOrDefault();
    //            projPlan.ProjEndDt = projPlanNBBUD?.EndDate != null
    //                                    ? DateOnly.FromDateTime(projPlanNBBUD.EndDate)
    //                                    : (DateOnly?)null;
    //            projPlan.ProjStartDt = projPlanNBBUD?.StartDate != null
    //                                    ? DateOnly.FromDateTime(projPlanNBBUD.StartDate)
    //                                    : (DateOnly?)null;
    //            escallation_percent = projPlanNBBUD?.EscalationRate != null ? projPlanNBBUD.EscalationRate.ToString() : "3";
    //        }

    //        var revenueSetup = _context.ProjBgtRevSetups.FirstOrDefault(p => p.ProjId == projId && p.VersionNo == projPlan.Version && p.BgtType == projPlan.PlType);
    //        //var revenueSetup = _context.ProjBgtRevSetups.FirstOrDefault(p => p.PlId == planID);
    //        var validOrgs = _context.PlEmployeees
    //                            .Where(e => e.PlId == projPlan.PlId && e.OrgId != null)
    //                            .Select(e => e.OrgId)
    //                            .Union(
    //                                _context.PlDcts
    //                                    .Where(d => d.PlId == projPlan.PlId && d.OrgId != null)
    //                                    .Select(d => d.OrgId)
    //                            )
    //                            .Distinct()
    //                            .ToList();

    //        var acctPool = (from pool in _context.PlOrgAcctPoolMappings
    //                        join orgId in validOrgs on pool.OrgId equals orgId
    //                        select pool).ToList();

    //        FinanceHelper financeHelper = new FinanceHelper(_context, projId);
    //        List<string> prefixes = new List<string>();

    //        if (SystemName.ToUpper() != "UNANET")
    //        {
    //            var parts = projId.Split('.', StringSplitOptions.RemoveEmptyEntries);
    //            prefixes = Enumerable
    //                .Range(2, parts.Length - 1)
    //                .Select(i => string.Join('.', parts.Take(i)))
    //                .ToList();
    //        }
    //        else
    //        {
    //            prefixes.Add(projId);
    //        }
    //        var ProjRevDef = _context.ProjRevDefinitions
    //            .AsNoTracking()
    //            .Where(p => prefixes.Contains(p.ProjectId))
    //            .OrderByDescending(p => p.ProjectId.Length)
    //            .FirstOrDefault();

    //        if (ProjRevDef != null)
    //        {
    //            revenueFormula = ProjRevDef.RevenueFormulaCd;
    //            laborFees = ProjRevDef.RevenueCalculationAmount.GetValueOrDefault();
    //            nonLaborFees = ProjRevDef.RevenueCalculation1Amount.GetValueOrDefault();
    //        }


    //        if (revenueSetup != null)
    //        {
    //            revenueFormula = revenueSetup.RevType;
    //            if (revenueSetup.LabFeeCostFl.GetValueOrDefault())
    //                laborFees = revenueSetup.LabFeeRt.GetValueOrDefault();
    //            if (revenueSetup.NonLabFeeCostFl.GetValueOrDefault())
    //                nonLaborFees = revenueSetup.NonLabFeeRt.GetValueOrDefault();
    //            if (revenueSetup.OverrideRevAdjFl)
    //            {
    //                adj = _context.ProjRevWrkPds.Where(p => p.ProjId == projId && p.VersionNo == projPlan.Version && p.BgtType == projPlan.PlType).ToList();
    //                RevenueAdj = adj.Sum(p => p.RevAdj).GetValueOrDefault();
    //                ActualRevenue = adj.Sum(p => p.RevAmt).GetValueOrDefault();
    //            }
    //            if (revenueSetup.OverrideFundingCeilingFl)
    //            {
    //                if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
    //                    fundingValue = revenueSetup.AtRiskAmt + projPlan.Proj.proj_f_tot_amt.GetValueOrDefault();
    //            }
    //            else
    //            {
    //                if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
    //                    fundingValue = projPlan.Proj.proj_f_tot_amt.GetValueOrDefault();
    //            }
    //            if (revenueSetup.OverrideRevSettingFl)
    //            {
    //                OverrideFRevenueSettings = true;
    //            }

    //        }
    //        else
    //        {
    //            if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
    //            {
    //                revenueFormula = projType;
    //                laborFees = financeHelper.GetActiveFeesForProject(projId);
    //                nonLaborFees = laborFees;
    //                fundingValue = projPlan.Proj.proj_f_tot_amt.GetValueOrDefault();
    //            }
    //        }


    //        burdensByTemplate = await _context.PlTemplatePoolRates.Where(r => r.TemplateId == templateId).ToListAsync();
    //        var All_Config = _context.PlConfigValues.ToList();

    //        Account_Org_Helpercs account_Org_Helpercs = new Account_Org_Helpercs(_context);
    //        var templatePools = account_Org_Helpercs.GetPoolsByTemplateId(templateId).Select(p => p.PoolId).ToList();

    //        if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
    //        {
    //            if (projId != null)
    //            {
    //                organization = _context.PlProjects
    //                    .Where(p => p.ProjId == projId)
    //                    .Include(p => p.Org)
    //                    .Select(p => p.Org.OrgName)
    //                    .FirstOrDefault();
    //            }
    //        }

    //        var forecastsDirectCosts = await _context.PlForecasts
    //            .Include(p => p.DirectCost)
    //        .Include(p => p.Proj)
    //            .Where(f => f.Emple == null && f.DirectCost != null && f.ProjId == projId && f.PlId == planID).OrderBy(p => p.Month).ThenBy(q => q.Year).ToListAsync();

    //        /////////////////Get Ceiling Configuration For Burdens

    //        var ceilingPools = _context.PlCeilBurdens.Where(p => p.ProjectId == projId).ToList();


    //        //////////////////////////////////////////////////////////////

    //        foreach (var forecast in forecastsDirectCosts)
    //        {
    //            try
    //            {
    //                forecast.Revenue = 0;
    //                if (acctPool.Count() > 0)
    //                {
    //                    if (forecast.Year == 2026 && forecast.Month == 3 && forecast.AcctId == "50-400-002" && forecast.OrgId == "1.01.03.01" && forecast.EmplId == "TBD20024")
    //                    {

    //                    }
    //                    if (forecast.Year == 2026 && forecast.Month == 4 && forecast.EmplId.StartsWith("TBD"))
    //                    {

    //                    }
    //                    var burdens = burdensByTemplate.Where(r => r.Month == forecast.Month && r.Year == forecast.Year).ToList();
    //                    var validBurdens = financeHelper.GetBurdenRates(templatePools, burdens, type);

    //                    if (forecast.Year == 2025 && forecast.Month == 9 && forecast.EmplId == "S0019")
    //                    {

    //                    }
    //                    var pools = acctPool.Where(p => p.AccountId == forecast.AcctId && p.OrgId == forecast.OrgId);
    //                    fringe = 0; gna = 0; overHead = 0; mnh = 0; hr = 0;
    //                    OverHeadOnGna = false; FringeOnOverhead = false; FringeOnGna = false;
    //                    foreach (var pool in pools)
    //                    {
    //                        switch (pool.PoolId.ToUpper())
    //                        {
    //                            case "FRINGE":
    //                                validBurdens.TryGetValue("FRINGE", out fringe);
    //                                break;
    //                            case "GNA":
    //                                validBurdens.TryGetValue("GNA", out gna);
    //                                break;
    //                            case "OVERHEAD":
    //                                validBurdens.TryGetValue("OVERHEAD", out overHead);
    //                                break;
    //                            case "MNH":
    //                                validBurdens.TryGetValue("MNH", out mnh);
    //                                break;
    //                            case "HR":
    //                                validBurdens.TryGetValue("HR", out hr);
    //                                break;
    //                            case "FRINGEONOVERHEAD":
    //                                FringeOnOverhead = true;
    //                                break;
    //                            case "FRINGEONGNA":
    //                                FringeOnGna = true;
    //                                break;
    //                            case "OVERHEADONGNA":
    //                                OverHeadOnGna = true;
    //                                break;
    //                        }
    //                    }
    //                }

    //                //////////////////Calculate Burden 
    //                switch (projPlan.PlType)
    //                {
    //                    case "EAC":
    //                        forecast.Cost = (decimal)(forecast.Actualamt.GetValueOrDefault());
    //                        break;
    //                    case "BUD":
    //                        forecast.Cost = (decimal)(forecast.Forecastedamt.GetValueOrDefault());
    //                        break;
    //                    case "NBBUD":
    //                        forecast.Cost = (decimal)(forecast.Forecastedamt.GetValueOrDefault());
    //                        break;

    //                }
    //                if (forecast.DirectCost.IsBrd)
    //                {
    //                    //var ytdCost = forecastsDirectCosts.Where(p => p.EmplId == forecast.EmplId && p.Year == forecast.Year && p.Month <= forecast.Month && p.OrgId == forecast.OrgId && p.AcctId == forecast.AcctId && p.Plc == forecast.Plc).Sum(p => p.Cost);
    //                    //forecast.YtdCost = ytdCost;


    //                    decimal ytdCost = 0;
    //                    switch (forecast.DirectCost.Type.ToUpper())
    //                    {
    //                        case "EMPLOYEE":
    //                            ytdCost = forecastsDirectCosts.Where(p => p.EmplId == forecast.EmplId && p.Year == forecast.Year && p.Month <= forecast.Month && p.OrgId == forecast.OrgId && p.AcctId == forecast.AcctId && p.Plc == forecast.Plc && forecast.DirectCost.Type.ToUpper() == "EMPLOYEE").Sum(p => p.Cost);
    //                            break;
    //                        case "OTHER":
    //                            ytdCost = forecastsDirectCosts.Where(p => p.EmplId == forecast.EmplId && p.Year == forecast.Year && p.Month <= forecast.Month && p.OrgId == forecast.OrgId && p.AcctId == forecast.AcctId && p.Plc == forecast.Plc && forecast.DirectCost.Type.ToUpper() == "OTHER").Sum(p => p.Cost);
    //                            break;
    //                        case "VENDOR EMPLOYEE":
    //                        case "VENDOR":
    //                        case "VENDOREMPLOYEE":
    //                            ytdCost = forecastsDirectCosts.Where(p => p.EmplId == forecast.EmplId && p.Year == forecast.Year && p.Month <= forecast.Month && p.OrgId == forecast.OrgId && p.AcctId == forecast.AcctId && p.Plc == forecast.Plc && forecast.DirectCost.Type.ToUpper() == "VENDOR EMPLOYEE").Sum(p => p.Cost);
    //                            break;
    //                    }


    //                    forecast.YtdCost = ytdCost;


    //                    PlForecast prevFringe = new PlForecast();
    //                    decimal previousFringe = 0m, previousOverHead = 0m, previousGNA = 0m, previousMNH = 0m;

    //                    if (forecast.Year == 2025 && forecast.Month == 9 && forecast.AcctId == "50-400-002" && forecast.OrgId == "1.01.03.01" && forecast.EmplId == "1003435")
    //                    {

    //                    }

    //                    if (forecast.Month > 1)
    //                    {
    //                        prevFringe = forecastsDirectCosts
    //                            .FirstOrDefault(p =>
    //                                p.EmplId == forecast.EmplId &&
    //                                p.Year == forecast.Year &&
    //                                p.Month == forecast.Month - 1
    //                                 && p.OrgId == forecast.OrgId && p.AcctId == forecast.AcctId && p.Plc == forecast.Plc);
    //                    }
    //                    if (prevFringe != null)
    //                    {
    //                        previousFringe = prevFringe.YtdFringe;
    //                        previousOverHead = prevFringe.YtdOverhead;
    //                        previousGNA = prevFringe.YtdGna;
    //                        previousMNH = prevFringe.YtdMaterials;
    //                    }

    //                    var ytdFringe = ytdCost * (fringe / 100);
    //                    forecast.YtdFringe = ytdFringe;

    //                    if (fringe == 0)
    //                    {
    //                        forecast.Fringe = 0;
    //                    }
    //                    else
    //                    {
    //                        forecast.Fringe = ytdFringe - previousFringe;
    //                    }

    //                    var baseForOverhead = ytdCost + (FringeOnOverhead ? ytdFringe : 0);
    //                    var ytdOverHead = (decimal)(baseForOverhead * overHead / 100);
    //                    forecast.YtdOverhead = ytdOverHead;

    //                    if (overHead == 0)
    //                    {
    //                        forecast.Overhead = 0;
    //                    }
    //                    else
    //                    {
    //                        forecast.Overhead = ytdOverHead - previousOverHead;
    //                    }

    //                    var ytdMaterials = (decimal)(ytdCost * mnh / 100);
    //                    forecast.YtdMaterials = ytdMaterials;
    //                    forecast.Materials = ytdMaterials - previousMNH;


    //                    // Calculate Gna
    //                    decimal baseForGna = ytdCost;

    //                    var ytdGna = (decimal)(baseForGna * gna / 100);
    //                    forecast.YtdGna = ytdGna;

    //                    if (gna == 0)
    //                    {
    //                        forecast.Gna = 0;
    //                    }
    //                    else
    //                    {
    //                        forecast.Gna = ytdGna - previousGNA;
    //                    }

    //                    forecast.Burden = forecast.Fringe + forecast.Overhead + forecast.Gna + forecast.Materials;
    //                    // forecast.TotalBurdenCost = financeHelper.CalculateBurdenedCostBYRAPRate(new BurdenInput() { FringeRate = fringe, GnaRate = gna, HourlyRate = 0, Hours = forecast.Forecastedhours, OverheadRate = overHead, DirectCost = forecast.Forecastedamt.GetValueOrDefault(), plEmployee = forecast.Empl });
    //                    //forecast.TotalBurdenCost = financeHelper.CalculateBurdenedCost(new BurdenInput() { FringeRate = fringe, GnaRate = gna, HourlyRate = 0, Hours = forecast.Forecastedhours, OverheadRate = overHead, DirectCost = forecast.Forecastedamt.GetValueOrDefault(), plEmployee = forecast.Empl });
    //                }
    //                else
    //                {
    //                    forecast.Materials = 0;
    //                    forecast.Gna = 0;
    //                    forecast.Overhead = 0;
    //                    forecast.Fringe = 0;
    //                    forecast.Burden = 0;
    //                    forecast.Hr = 0;
    //                    //forecast.Cost = (decimal)(forecast.Forecastedamt.GetValueOrDefault());
    //                    forecast.TotalBurdenCost = forecast.Cost;

    //                }

    //            }
    //            catch (Exception ex)
    //            {
    //            }
    //        }

    //        if (organization == string.Empty || organization == null)
    //        {
    //            //projId = forecastsDirectCosts.FirstOrDefault()?.Proj?.ProjId;

    //            if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
    //            {
    //                if (projId != null)
    //                {
    //                    organization = _context.PlProjects
    //                        .Where(p => p.ProjId == projId)
    //                        .Include(p => p.Org)
    //                        .Select(p => p.Org.OrgName)
    //                        .FirstOrDefault();
    //                }
    //            }
    //        }

    //        var forecasts = await _context.PlForecasts
    //                        .Include(p => p.Emple)
    //                        .Include(p => p.Proj)
    //                        .Where(f => f.Emple != null && f.ProjId == projId && f.PlId == planID).OrderBy(p => p.Year).ThenBy(q => q.Month).ToListAsync();

    //        var organizationName = _context.PlProjects
    //                                .Include(p => p.Org)
    //                                .Where(p => p.ProjId == projId)
    //                                .Select(p => p.Org.OrgName)
    //                                .FirstOrDefault();

    //        var accountName = _context.Accounts
    //                          .Where(p => p.AcctId == projId)
    //                          .Select(p => p.AcctName)
    //                          .FirstOrDefault();

    //        if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
    //        {
    //            escallation_month = _context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "escallation_month" && r.ProjId == projId)?.Value ?? _context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "escallation_month" && r.ProjId == "xxxxx")?.Value ?? "3";
    //            escallation_percent = _context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "escallation_percent" && r.ProjId == projId)?.Value ?? _context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "escallation_percent" && r.ProjId == "xxxxx")?.Value ?? "3";
    //        }
    //        else
    //        {
    //            escallation_month = _context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "escallation_month" && r.ProjId == "xxxxx")?.Value ?? "7";
    //        }

    //        if (string.IsNullOrEmpty(escallation_month))
    //            escallation_month = "7";

    //        if (string.IsNullOrEmpty(escallation_percent))
    //            escallation_percent = "0";

    //        List<EmplSchedule> res = new List<EmplSchedule>();
    //        List<DirectCostSchedule> res1 = new List<DirectCostSchedule>();

    //        if (forecasts.Count() > 0)
    //        {
    //            res = financeHelper.CalculateSalaryPlanV1(forecasts, 0, DateTime.UtcNow.Year, Convert.ToInt32(escallation_month), Convert.ToDecimal(escallation_percent), projPlan);
    //            res1 = financeHelper.CalculateDirectCostPlan(forecastsDirectCosts);
    //        }

    //        foreach (var forecastempl in forecasts)
    //        {
    //            forecastempl.Revenue = 0;

    //            decimal hours = 0;

    //            fringe = 0; gna = 0; overHead = 0; ; mnh = 0; hr = 0;
    //            OverHeadOnGna = false; FringeOnOverhead = false; FringeOnGna = false;
    //            decimal perHrRate = 0;

    //            if (forecastempl.Year == 2026 && forecastempl.EmplId.StartsWith("PLC") && forecastempl.Month == 4)
    //            {

    //            }

    //            if (forecastempl.EmplId == "AMY.EWING" && forecastempl.Month == 1 && forecastempl.Year == 2026)
    //            {

    //            }

    //            if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
    //            {
    //                //commented for Unanet
    //                //perHrRate = res.FirstOrDefault(p => p.EmpId == forecastempl.EmplId && p.AccID == forecastempl.AcctId && Convert.ToString(p.PlcCode) == forecastempl.Plc && p.OrgID == forecastempl.OrgId)?.payrollSalary.FirstOrDefault(p => p.Month == forecastempl.Month && p.Year == forecastempl.Year)?.Salary ?? 0;
    //                perHrRate = res.FirstOrDefault(p => p.EmpId == forecastempl.EmplId)?.payrollSalary.FirstOrDefault(p => p.Month == forecastempl.Month && p.Year == forecastempl.Year)?.Salary ?? 0;

    //                if (forecastempl.ProjId.Contains("MANDH") || forecastempl.ProjId.Contains("FRNGE") || forecastempl.ProjId.Contains("GANDA") || forecastempl.ProjId.Contains("OVRHD") || forecastempl.ProjId.Contains("HRPAY"))
    //                    perHrRate = res.FirstOrDefault(p => p.EmpId == forecastempl.EmplId && p.AccID == forecastempl.AcctId && p.OrgID == forecastempl.OrgId)?.payrollSalary.FirstOrDefault(p => p.Month == forecastempl.Month && p.Year == forecastempl.Year)?.Salary ?? 0;
    //            }
    //            else
    //                perHrRate = forecastempl.Emple.PerHourRate.GetValueOrDefault();

    //            if (forecastempl.Emple.Type.ToUpper() == "VENDOR EMPLOYEE" || forecastempl.Emple.Type.ToUpper() == "VENDOR" || forecastempl.Emple.Type.ToUpper() == "VENDOREMPLOYEE")
    //            {
    //                if (forecastempl.HrlyRate.HasValue)
    //                    forecastempl.ForecastedCost = hours * forecastempl.HrlyRate.GetValueOrDefault();
    //                else
    //                    forecastempl.ForecastedCost = hours * forecastempl.Emple.PerHourRate.GetValueOrDefault();
    //            }
    //            else
    //            {

    //                DateTime forecastDay = new DateTime(forecastempl.Year, forecastempl.Month, DateTime.DaysInMonth(forecastempl.Year, forecastempl.Month));
    //                if (forecastDay <= currentMonth && projPlan.PlType.ToUpper() == "EAC")
    //                {
    //                    if (forecastempl.HrlyRate.HasValue)
    //                        forecastempl.ForecastedCost = hours * forecastempl.HrlyRate.GetValueOrDefault();
    //                    else
    //                        forecastempl.ForecastedCost = hours * perHrRate;
    //                }
    //                else
    //                {
    //                    forecastempl.ForecastedCost = hours * perHrRate;
    //                }
    //            }
    //            forecastempl.Cost = forecastempl.ForecastedCost.GetValueOrDefault();

    //            if (forecastempl.Year == 2026 && forecastempl.Month == 3 && forecastempl.EmplId == "BETH.MALONEY")
    //            {

    //            }

    //            if (acctPool.Count() > 0)
    //            {
    //                var burdens = burdensByTemplate.Where(r => r.Month == forecastempl.Month && r.Year == forecastempl.Year).ToList();
    //                var validBurdens = financeHelper.GetBurdenRates(templatePools, burdens, type);
    //                var pools = acctPool.Where(p => p.AccountId == forecastempl.AcctId && p.OrgId == forecastempl.OrgId && p.Year == forecastempl.Year);
    //                foreach (var pool in pools)
    //                {
    //                    //AllAvaialbepools.FirstOrDefault(p => p.Code == pool.PoolId).Type;
    //                    //switch (pool.PoolId.ToUpper())
    //                    switch (AllAvaialbepools.FirstOrDefault(p => p.Code == pool.PoolId).Type.ToUpper())
    //                    {
    //                        case "FRINGE":
    //                            validBurdens.TryGetValue(pool.PoolId.ToUpper(), out fringe);
    //                            break;
    //                        case "GNA":
    //                            validBurdens.TryGetValue(pool.PoolId.ToUpper(), out gna);
    //                            break;
    //                        case "MNH":
    //                            validBurdens.TryGetValue("MNH", out mnh);
    //                            break;
    //                        case "HR":
    //                            validBurdens.TryGetValue("HR", out hr);
    //                            break;
    //                        case "OVERHEAD":
    //                            validBurdens.TryGetValue(pool.PoolId.ToUpper(), out overHead);
    //                            break;
    //                        case "FRINGEONOVERHEAD":
    //                            FringeOnOverhead = true;
    //                            break;
    //                        case "FRINGEONGNA":
    //                            FringeOnGna = true;
    //                            break;
    //                        case "OVERHEADONGNA":
    //                            OverHeadOnGna = true;
    //                            break;
    //                    }
    //                }
    //            }


    //            if (ceilingPools.Count > 0)
    //            {

    //                var ceilingFringe = ceilingPools.FirstOrDefault(p => p.PoolCode == "FRINGE" && p.AccountId == forecastempl.AcctId && Convert.ToInt16(p.FiscalYear) == forecastempl.Year)?.RateCeiling.GetValueOrDefault();
    //                var ceilingGNA = ceilingPools.FirstOrDefault(p => p.PoolCode == "GNA" && p.AccountId == forecastempl.AcctId && Convert.ToInt16(p.FiscalYear) == forecastempl.Year)?.RateCeiling.GetValueOrDefault();
    //                var ceilingOVERHEAD = ceilingPools.FirstOrDefault(p => p.PoolCode == "OVERHEAD" && p.AccountId == forecastempl.AcctId && Convert.ToInt16(p.FiscalYear) == forecastempl.Year)?.RateCeiling.GetValueOrDefault();


    //                if (ceilingFringe.HasValue)
    //                    fringe = Math.Min(fringe, ceilingFringe.GetValueOrDefault());

    //                if (ceilingGNA.HasValue)
    //                    gna = Math.Min(gna, ceilingGNA.GetValueOrDefault());


    //                if (ceilingOVERHEAD.HasValue)
    //                    overHead = Math.Min(overHead, ceilingOVERHEAD.GetValueOrDefault());

    //            }
    //            //////////////////////////////////Calculate Cost and Burden


    //            switch (projPlan.PlType)
    //            {
    //                case "EAC":
    //                    hours = (decimal)(forecastempl.Actualhours);
    //                    break;
    //                case "BUD":
    //                    hours = (decimal)(forecastempl.Forecastedhours);
    //                    break;
    //                case "NBBUD":
    //                    hours = (decimal)(forecastempl.Forecastedhours);
    //                    break;
    //            }

    //            if (forecastempl.Emple.Type.ToUpper() == "VENDOR EMPLOYEE" || forecastempl.Emple.Type.ToUpper() == "VENDOR" || forecastempl.Emple.Type.ToUpper() == "VENDOREMPLOYEE")
    //            {
    //                if (forecastempl.HrlyRate.HasValue && forecastempl.HrlyRate != 0)
    //                    forecastempl.ForecastedCost = hours * forecastempl.HrlyRate.GetValueOrDefault();
    //                else
    //                    forecastempl.ForecastedCost = hours * forecastempl.Emple.PerHourRate.GetValueOrDefault();
    //            }
    //            else
    //            {

    //                DateTime forecastDay = new DateTime(forecastempl.Year, forecastempl.Month, DateTime.DaysInMonth(forecastempl.Year, forecastempl.Month));
    //                if (forecastDay <= currentMonth && projPlan.PlType.ToUpper() == "EAC")
    //                {
    //                    if (forecastempl.HrlyRate.HasValue)
    //                        forecastempl.ForecastedCost = hours * forecastempl.HrlyRate.GetValueOrDefault();
    //                    else
    //                        forecastempl.ForecastedCost = hours * perHrRate;
    //                }
    //                else
    //                {
    //                    forecastempl.ForecastedCost = hours * perHrRate;
    //                }
    //            }
    //            forecastempl.Cost = forecastempl.ForecastedCost.GetValueOrDefault();
    //            if (forecastempl.Emple.IsBrd.GetValueOrDefault())
    //            {
    //                if (forecastempl.Year == 2026 && forecastempl.Month == 1)
    //                {

    //                }
    //                decimal ytdCost = 0;
    //                switch (forecastempl.Emple.Type.ToUpper())
    //                {
    //                    case "EMPLOYEE":
    //                        ytdCost = forecasts.Where(p => p.EmplId == forecastempl.EmplId && p.Year == forecastempl.Year && p.Month <= forecastempl.Month && p.OrgId == forecastempl.OrgId && p.AcctId == forecastempl.AcctId && p.Plc == forecastempl.Plc && forecastempl.Emple.Type.ToUpper() == "EMPLOYEE").Sum(p => p.Cost);
    //                        break;
    //                    case "OTHER":
    //                        ytdCost = forecasts.Where(p => p.EmplId == forecastempl.EmplId && p.Year == forecastempl.Year && p.Month <= forecastempl.Month && p.OrgId == forecastempl.OrgId && p.AcctId == forecastempl.AcctId && p.Plc == forecastempl.Plc && forecastempl.Emple.Type.ToUpper() == "OTHER").Sum(p => p.Cost);
    //                        break;
    //                    case "PLC":
    //                        ytdCost = forecasts.Where(p => p.EmplId == forecastempl.EmplId && p.Year == forecastempl.Year && p.Month <= forecastempl.Month && p.OrgId == forecastempl.OrgId && p.AcctId == forecastempl.AcctId && p.Plc == forecastempl.Plc && forecastempl.Emple.Type.ToUpper() == "PLC").Sum(p => p.Cost);
    //                        break;
    //                    case "VENDOR EMPLOYEE":
    //                    case "VENDOR":
    //                    case "VENDOREMPLOYEE":
    //                        ytdCost = forecasts.Where(p => p.EmplId == forecastempl.EmplId && p.Year == forecastempl.Year && p.Month <= forecastempl.Month && p.OrgId == forecastempl.OrgId && p.AcctId == forecastempl.AcctId && p.Plc == forecastempl.Plc && (forecastempl.Emple.Type.ToUpper() == "VENDOR EMPLOYEE" || forecastempl.Emple.Type.ToUpper() == "VENDOREMPLOYEE")).Sum(p => p.Cost);
    //                        break;
    //                }


    //                //if (forecastempl.Emple.Type.ToUpper() == "EMPLOYEE" || forecastempl.Emple.Type.ToUpper() == "OTHER")
    //                //    ytdCost = forecasts.Where(p => p.EmplId == forecastempl.EmplId && p.Year == forecastempl.Year && p.Month <= forecastempl.Month && p.OrgId == forecastempl.OrgId && p.AcctId == forecastempl.AcctId && p.Plc == forecastempl.Plc && forecastempl.Emple.Type.ToUpper() == "EMPLOYEE").Sum(p => p.Cost);
    //                //else
    //                //    ytdCost = forecasts.Where(p => p.EmplId == forecastempl.EmplId && p.Year == forecastempl.Year && p.Month <= forecastempl.Month && p.OrgId == forecastempl.OrgId && p.AcctId == forecastempl.AcctId && p.Plc == forecastempl.Plc && forecastempl.Emple.Type.ToUpper() == "VENDOR EMPLOYEE").Sum(p => p.Cost);
    //                forecastempl.YtdCost = ytdCost;
    //                var ytdSubContractorCost = forecasts.Where(p => p.EmplId == forecastempl.EmplId && p.Year == forecastempl.Year && p.Month <= forecastempl.Month && p.OrgId == forecastempl.OrgId && p.AcctId == forecastempl.AcctId && p.Plc == forecastempl.Plc && forecastempl.Emple.Type.ToUpper() == "VENDOR EMPLOYEE").Sum(p => p.Cost);
    //                //var prevFringe = forecasts.FirstOrDefault(p => p.EmplId == forecastempl.EmplId && p.Year == forecastempl.Year && p.Month == forecastempl.Month - 1);
    //                PlForecast prevFringe = new PlForecast();
    //                List<PlForecast> ytdPrev = new List<PlForecast>();
    //                decimal previousFringe = 0m, previousOverHead = 0m, previousGNA = 0m, previousMNH = 0m;

    //                if (forecastempl.Month > 1)
    //                {
    //                    prevFringe = forecasts
    //                        .FirstOrDefault(p =>
    //                            p.EmplId == forecastempl.EmplId &&
    //                            p.Year == forecastempl.Year &&
    //                            p.Month == forecastempl.Month - 1
    //                            && p.OrgId == forecastempl.OrgId && p.AcctId == forecastempl.AcctId && p.Plc == forecastempl.Plc);
    //                }
    //                if (prevFringe != null)
    //                {
    //                    previousFringe = prevFringe.YtdFringe;
    //                    previousOverHead = prevFringe.YtdOverhead;
    //                    previousGNA = prevFringe.YtdGna;
    //                    previousMNH = prevFringe.YtdMaterials;
    //                }

    //                var ytdFringe = ytdCost * (fringe / 100);
    //                forecastempl.YtdFringe = ytdFringe;
    //                forecastempl.Fringe = ytdFringe - previousFringe;


    //                var baseForOverhead = ytdCost + (FringeOnOverhead ? ytdFringe : 0);
    //                var ytdOverHead = (decimal)(baseForOverhead * overHead / 100);
    //                forecastempl.YtdOverhead = ytdOverHead;

    //                forecastempl.Overhead = ytdOverHead - previousOverHead;



    //                var ytdMaterials = (decimal)((ytdCost) * mnh / 100);
    //                forecastempl.YtdMaterials = ytdMaterials;

    //                forecastempl.Materials = ytdMaterials - previousMNH;


    //                // Calculate Gna
    //                decimal baseForGna = ytdCost;
    //                if (FringeOnGna) baseForGna += ytdFringe;
    //                if (OverHeadOnGna) baseForGna += ytdOverHead;

    //                var ytdGna = (decimal)(baseForGna * gna / 100);
    //                forecastempl.YtdGna = ytdGna;

    //                forecastempl.Gna = ytdGna - previousGNA;

    //                forecastempl.Burden = forecastempl.Fringe + forecastempl.Overhead + forecastempl.Gna + forecastempl.Materials;
    //                //forecastempl.TotalBurdenCost = financeHelper.CalculateBurdenedCostBYRAPRate(new BurdenInput() { FringeRate = fringe, GnaRate = gna, HourlyRate = perHrRate, Hours = hours, OverheadRate = overHead, plEmployee = forecastempl.Empl });
    //                //forecastempl.TotalBurdenCost = financeHelper.CalculateBurdenedCost(new BurdenInput() { FringeRate = fringe, GnaRate = gna, HourlyRate = perHrRate, Hours = hours, OverheadRate = overHead, plEmployee = forecastempl.Empl });
    //                forecastempl.TotalBurdenCost = forecastempl.ForecastedCost.GetValueOrDefault() + forecastempl.Burden;

    //            }
    //            else
    //            {
    //                //Need to discuss whether to calculate burden for non burden employees  for actuals?
    //                if (projPlan.PlType.ToUpper() == "EAC" && new DateOnly(forecastempl.Year, forecastempl.Month, 1) < projPlan.ClosedPeriod.GetValueOrDefault())
    //                {
    //                    forecastempl.Cost = forecastempl.Cost;

    //                    decimal ytdCost = 0;
    //                    //var ytdCost = forecasts.Where(p => p.EmplId == forecastempl.EmplId && (new DateOnly(p.Year, p.Month, 1) <= new DateOnly(forecastempl.Year, forecastempl.Month, 1))).Sum(p => p.Cost);
    //                    if (forecastempl.Emple.Type.ToUpper() == "EMPLOYEE")
    //                        ytdCost = forecasts.Where(p => p.EmplId == forecastempl.EmplId && p.Year == forecastempl.Year && p.Month <= forecastempl.Month && p.OrgId == forecastempl.OrgId && p.AcctId == forecastempl.AcctId && p.Plc == forecastempl.Plc && forecastempl.Emple.Type.ToUpper() == "EMPLOYEE").Sum(p => p.Cost);
    //                    else
    //                        ytdCost = forecasts.Where(p => p.EmplId == forecastempl.EmplId && p.Year == forecastempl.Year && p.Month <= forecastempl.Month && p.OrgId == forecastempl.OrgId && p.AcctId == forecastempl.AcctId && p.Plc == forecastempl.Plc && forecastempl.Emple.Type.ToUpper() == "VENDOR EMPLOYEE").Sum(p => p.Cost);
    //                    forecastempl.YtdCost = ytdCost;
    //                    var ytdSubContractorCost = forecasts.Where(p => p.EmplId == forecastempl.EmplId && p.Year == forecastempl.Year && p.Month <= forecastempl.Month && p.OrgId == forecastempl.OrgId && p.AcctId == forecastempl.AcctId && p.Plc == forecastempl.Plc && forecastempl.Emple.Type.ToUpper() == "VENDOR EMPLOYEE").Sum(p => p.Cost);
    //                    //var prevFringe = forecasts.FirstOrDefault(p => p.EmplId == forecastempl.EmplId && p.Year == forecastempl.Year && p.Month == forecastempl.Month - 1);
    //                    PlForecast prevFringe = new PlForecast();
    //                    List<PlForecast> ytdPrev = new List<PlForecast>();
    //                    decimal previousFringe = 0m, previousOverHead = 0m, previousGNA = 0m, previousMNH = 0m;

    //                    if (forecastempl.Month > 1)
    //                    {
    //                        prevFringe = forecasts
    //                            .FirstOrDefault(p =>
    //                                p.EmplId == forecastempl.EmplId &&
    //                                p.Year == forecastempl.Year &&
    //                                p.Month == forecastempl.Month - 1
    //                                && p.OrgId == forecastempl.OrgId && p.AcctId == forecastempl.AcctId && p.Plc == forecastempl.Plc);


    //                        //ytdPrev = forecasts.Where(p => p.EmplId == forecastempl.EmplId && p.Year == forecastempl.Year && p.Month <= forecastempl.Month && p.OrgId == forecastempl.OrgId && p.AcctId == forecastempl.AcctId && p.Plc == forecastempl.Plc).ToList();
    //                    }
    //                    if (prevFringe != null)
    //                    {
    //                        previousFringe = prevFringe.YtdFringe;
    //                        previousOverHead = prevFringe.YtdOverhead;
    //                        previousGNA = prevFringe.YtdGna;
    //                        previousMNH = prevFringe.YtdMaterials;
    //                        //previousFringe = ytdPrev.Sum(p=>p.Fringe);
    //                        //previousOverHead = ytdPrev.Sum(p => p.Overhead);
    //                        //previousGNA = ytdPrev.Sum(p => p.Gna);
    //                        //previousMNH = ytdPrev.Sum(p => p.Materials);
    //                    }

    //                    var ytdFringe = ytdCost * (fringe / 100);
    //                    forecastempl.YtdFringe = ytdFringe;
    //                    forecastempl.Fringe = ytdFringe - previousFringe;


    //                    var baseForOverhead = ytdCost + (FringeOnOverhead ? ytdFringe : 0);
    //                    var ytdOverHead = (decimal)(baseForOverhead * overHead / 100);
    //                    forecastempl.YtdOverhead = ytdOverHead;

    //                    forecastempl.Overhead = ytdOverHead - previousOverHead;



    //                    var ytdMaterials = (decimal)((ytdCost) * mnh / 100);
    //                    forecastempl.YtdMaterials = ytdMaterials;

    //                    forecastempl.Materials = ytdMaterials - previousMNH;


    //                    // Calculate Gna
    //                    decimal baseForGna = ytdCost;
    //                    if (FringeOnGna) baseForGna += ytdFringe;
    //                    if (OverHeadOnGna) baseForGna += ytdOverHead;

    //                    var ytdGna = (decimal)(baseForGna * gna / 100);
    //                    forecastempl.YtdGna = ytdGna;

    //                    forecastempl.Gna = ytdGna - previousGNA;


    //                    forecastempl.Burden = forecastempl.Fringe + forecastempl.Overhead + forecastempl.Gna + forecastempl.Materials;
    //                    forecastempl.TotalBurdenCost = forecastempl.Cost + forecastempl.Burden;
    //                }
    //                else
    //                {
    //                    forecastempl.Cost = forecastempl.ForecastedCost.GetValueOrDefault();
    //                    forecastempl.Materials = 0;
    //                    forecastempl.Gna = 0;
    //                    forecastempl.Overhead = 0;
    //                    forecastempl.Fringe = 0;
    //                    forecastempl.Burden = 0;
    //                    forecastempl.Hr = 0;
    //                    // forecastempl.TotalBurdenCost = forecastempl.Cost;
    //                }
    //            }
    //        }

    //        /////////////////Get Ceiling Configuration For Burdens

    //        var ceilingEmplHours = _context.PlCeilHrEmpls.Where(p => p.ProjectId == projId).ToList();
    //        var ceilingLabCatHours = _context.PlCeilHrCats.Where(p => p.ProjectId == projId).ToList();

    //        var combinedList = forecasts
    //                        .Concat(forecastsDirectCosts)
    //                        .OrderBy(x => x.Year)
    //                        .ThenBy(x => x.Month)
    //                        .ToList();

    //        if (projPlan.PlType.ToUpper() == "NBBUD")
    //        {
    //            fundingValue = 100000000;
    //        }

    //        //var Adjusted = financeHelper.AdjustRevenue(combinedList, fundingValue, projPlan);
    //        var swTotal = Stopwatch.StartNew();

    //        foreach (var chunk in combinedList.Chunk(300))
    //        {
    //            var chunkIds = chunk.Select(f => f.Forecastid).ToList();
    //            await _context.BulkUpdateAsync(chunk, new BulkConfig { SetOutputIdentity = true });
    //            await _context.SaveChangesAsync();
    //        }
    //        swTotal.Stop();
    //        Console.WriteLine($"🎯 Bulk update completed in {swTotal.Elapsed.TotalSeconds:F2}s");


    //        planForecastSummary.Version = projPlan.Version.GetValueOrDefault();
    //        planForecastSummary.Proj_Id = projPlan.ProjId;
    //        planForecastSummary.Type = projPlan.PlType;
    //    }
    //    catch (Exception ex)
    //    {

    //    }
    //    return planForecastSummary;
    //}
    public async Task<PlanForecastSummary> CalculateRevenueCostForSelectedHours(int planID, int templateId, string type, List<PlForecast> hoursForecast)
    {
        int cost = 0;

        string revenueFormula = "";
        string escallation_month = "0", escallation_percent = "0";


        string projId = string.Empty, organization = string.Empty, projType = string.Empty;
        List<EmployeeForecastSummary> employeeForecastSummary = new List<EmployeeForecastSummary>();
        List<EmployeeForecastSummary> directCostForecastSummary = new List<EmployeeForecastSummary>();
        List<MonthlyCostRevenue> MonthlyCostRevenue = new List<MonthlyCostRevenue>();
        bool OverrideFRevenueSettings = false;

        PlanForecastSummary planForecastSummary = new PlanForecastSummary();
        planForecastSummary.EmployeeForecastSummary = new List<EmployeeForecastSummary>();


        try
        {
            var AllAvaialbepools = _context.AccountGroups.ToList();

            decimal fringe = 0, gna = 0, overHead = 0, laborFees = 0, hr = 0, mnh = 0, nonLaborFees = 0, RevenueAdj = 0, fundingValue = 0, ActualRevenue = 0;
            bool OverHeadOnGna = false, FringeOnOverhead = false, FringeOnGna = false;

            List<PlTemplatePoolRate> burdensByTemplate = new List<PlTemplatePoolRate>();
            List<pl_EmployeeBurdenCalculated> plEmployeeBurdenCalculated = new List<pl_EmployeeBurdenCalculated>();
            var projPlan = _context.PlProjectPlans.Where(p => p.PlId == planID).Include(p => p.Proj).FirstOrDefault();
            if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
            {
                projId = projPlan.Proj.ProjId;
                projType = projPlan.Proj.ProjTypeDc;
                projPlan.ProjEndDt = projPlan.Proj.ProjEndDt;
                projPlan.ProjStartDt = projPlan.Proj.ProjStartDt;
                fundingValue = projPlan.Proj.proj_f_tot_amt.GetValueOrDefault();
            }
            else
            {
                projId = projPlan.ProjId;
                var projPlanNBBUD = _context.NewBusinessBudgets.Where(p => p.BusinessBudgetId == projPlan.ProjId).FirstOrDefault();
                projPlan.ProjEndDt = projPlanNBBUD?.EndDate != null
                                        ? DateOnly.FromDateTime(projPlanNBBUD.EndDate)
                                        : (DateOnly?)null;
                projPlan.ProjStartDt = projPlanNBBUD?.StartDate != null
                                        ? DateOnly.FromDateTime(projPlanNBBUD.StartDate)
                                        : (DateOnly?)null;
                escallation_percent = projPlanNBBUD?.EscalationRate != null ? projPlanNBBUD.EscalationRate.ToString() : "3";
            }

            var revenueSetup = _context.ProjBgtRevSetups.FirstOrDefault(p => p.ProjId == projId && p.VersionNo == projPlan.Version && p.BgtType == projPlan.PlType);
            var validOrgs = _context.PlEmployeees
                                .Where(e => e.PlId == projPlan.PlId && e.OrgId != null)
                                .Select(e => e.OrgId)
                                .Union(
                                    _context.PlDcts
                                        .Where(d => d.PlId == projPlan.PlId && d.OrgId != null)
                                        .Select(d => d.OrgId)
                                )
                                .Distinct()
                                .ToList();

            var acctPool = (from pool in _context.PlOrgAcctPoolMappings
                            join orgId in validOrgs on pool.OrgId equals orgId
                            select pool).ToList();

            FinanceHelper financeHelper = new FinanceHelper(_context, projId);
            if (revenueSetup != null)
            {
                revenueFormula = revenueSetup.RevType;
                if (revenueSetup.LabFeeCostFl.GetValueOrDefault())
                    laborFees = revenueSetup.LabFeeRt.GetValueOrDefault();
                if (revenueSetup.NonLabFeeCostFl.GetValueOrDefault())
                    nonLaborFees = revenueSetup.NonLabFeeRt.GetValueOrDefault();
                if (revenueSetup.OverrideRevAdjFl)
                {
                    var adj = _context.ProjRevWrkPds.Where(p => p.ProjId == projId && p.VersionNo == projPlan.Version && p.BgtType == projPlan.PlType).ToList();
                    RevenueAdj = adj.Sum(p => p.RevAdj).GetValueOrDefault();
                    ActualRevenue = adj.Sum(p => p.RevAmt).GetValueOrDefault();
                }
                if (revenueSetup.OverrideFundingCeilingFl)
                {
                    if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
                        fundingValue = revenueSetup.AtRiskAmt + projPlan.Proj.proj_f_tot_amt.GetValueOrDefault();
                }
                else
                {
                    if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
                        fundingValue = projPlan.Proj.proj_f_tot_amt.GetValueOrDefault();
                }
                if (revenueSetup.OverrideRevSettingFl)
                {
                    OverrideFRevenueSettings = true;
                }

            }
            else
            {
                if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
                {
                    revenueFormula = projType;
                    laborFees = financeHelper.GetActiveFeesForProject(projId);
                    nonLaborFees = laborFees;
                    fundingValue = projPlan.Proj.proj_f_tot_amt.GetValueOrDefault();
                }
            }


            //if (revenueFormula == "LLRCINBF" || revenueFormula == "LLRCINLB" || revenueFormula == "LLRFNLBF" || revenueFormula == "CPFF" || revenueFormula == "CPFH")
            {
                burdensByTemplate = await _context.PlTemplatePoolRates.Where(r => r.TemplateId == templateId).ToListAsync();
            }
            var All_Config = _context.PlConfigValues.ToList();

            Account_Org_Helpercs account_Org_Helpercs = new Account_Org_Helpercs(_context);
            var templatePools = account_Org_Helpercs.GetPoolsByTemplateId(templateId).Select(p => p.PoolId).ToList();

            if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
            {
                if (projId != null)
                {
                    organization = _context.PlProjects
                        .Where(p => p.ProjId == projId)
                        .Include(p => p.Org)
                        .Select(p => p.Org.OrgName)
                        .FirstOrDefault();
                }
            }


            /////////////////Get Ceiling Configuration For Burdens

            var ceilingPools = _context.PlCeilBurdens.Where(p => p.ProjectId == projId).ToList();


            //////////////////////////////////////////////////////////////

            if (organization == string.Empty || organization == null)
            {
                projId = hoursForecast.FirstOrDefault()?.Proj?.ProjId;

                if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
                {
                    if (projId != null)
                    {
                        organization = _context.PlProjects
                            .Where(p => p.ProjId == projId)
                            .Include(p => p.Org)
                            .Select(p => p.Org.OrgName)
                            .FirstOrDefault();
                    }
                }
            }

            var organizationName = _context.PlProjects
                                    .Include(p => p.Org)
                                    .Where(p => p.ProjId == projId)
                                    .Select(p => p.Org.OrgName)
                                    .FirstOrDefault();

            var accountName = _context.Accounts
                              .Where(p => p.AcctId == projId)
                              .Select(p => p.AcctName)
                              .FirstOrDefault();

            if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
            {
                escallation_month = _context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "escallation_month" && r.ProjId == projId)?.Value ?? _context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "escallation_month" && r.ProjId == "xxxxx")?.Value ?? "3";
                escallation_percent = _context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "escallation_percent" && r.ProjId == projId)?.Value ?? _context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "escallation_percent" && r.ProjId == "xxxxx")?.Value ?? "3";
            }
            else
            {
                escallation_month = _context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "escallation_month" && r.ProjId == "xxxxx")?.Value ?? "7";
            }

            if (string.IsNullOrEmpty(escallation_month))
                escallation_month = "7";

            List<EmplSchedule> res = new List<EmplSchedule>();
            List<DirectCostSchedule> res1 = new List<DirectCostSchedule>();

            //if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
            if (hoursForecast.Count() > 0)
            {
                //res = financeHelper.CalculateSalaryPlan(hoursForecast, 0, 2025, Convert.ToInt32(escallation_month), Convert.ToDecimal(escallation_percent), projPlan);
                //res1 = financeHelper.CalculateDirectCostPlan(forecastsDirectCosts);
            }

            foreach (var forecastempl in hoursForecast)
            {

                if (forecastempl.Forecastid == 254007)
                {

                }
                decimal hours = 0;
                if (forecastempl.Year == 2025 && forecastempl.Month == 10 && forecastempl.EmplId == "1002220")
                {
                }

                fringe = 0; gna = 0; overHead = 0; ; mnh = 0; hr = 0;
                OverHeadOnGna = false; FringeOnOverhead = false; FringeOnGna = false;
                if (forecastempl.Year == 2025 && forecastempl.Month == 10)
                {

                }
                decimal perHrRate = 0;

                if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
                    //perHrRate = res.FirstOrDefault(p => p.EmpId == forecastempl.EmplId && p.AccID == forecastempl.AcctId && p.PlcCode == forecastempl.Plc && p.OrgID == forecastempl.OrgId)?.payrollSalary.FirstOrDefault(p => p.Month == forecastempl.Month && p.Year == forecastempl.Year)?.Salary ?? 0;
                    perHrRate = forecastempl.HrlyRate.HasValue ? forecastempl.HrlyRate.GetValueOrDefault() : perHrRate;
                else
                    perHrRate = forecastempl.Emple.PerHourRate.GetValueOrDefault();

                if (acctPool.Count() > 0)
                {
                    var burdens = burdensByTemplate.Where(r => r.Month == forecastempl.Month && r.Year == forecastempl.Year).ToList();
                    var validBurdens = financeHelper.GetBurdenRates(templatePools, burdens, type);
                    var pools = acctPool.Where(p => p.AccountId == forecastempl.AcctId && p.OrgId == forecastempl.OrgId);
                    foreach (var pool in pools)
                    {
                        //AllAvaialbepools.FirstOrDefault(p => p.Code == pool.PoolId).Type;
                        //switch (pool.PoolId.ToUpper())
                        switch (AllAvaialbepools.FirstOrDefault(p => p.Code == pool.PoolId).Type.ToUpper())
                        {
                            case "FRINGE":
                                validBurdens.TryGetValue(pool.PoolId.ToUpper(), out fringe);
                                break;
                            case "GNA":
                                validBurdens.TryGetValue(pool.PoolId.ToUpper(), out gna);
                                break;
                            case "MNH":
                                validBurdens.TryGetValue("MNH", out mnh);
                                break;
                            case "HR":
                                validBurdens.TryGetValue("HR", out hr);
                                break;
                            case "OVERHEAD":
                                validBurdens.TryGetValue(pool.PoolId.ToUpper(), out overHead);
                                break;
                            case "FRINGEONOVERHEAD":
                                FringeOnOverhead = true;
                                break;
                            case "FRINGEONGNA":
                                FringeOnGna = true;
                                break;
                            case "OVERHEADONGNA":
                                OverHeadOnGna = true;
                                break;
                        }
                    }
                }

                if (ceilingPools.Count > 0)
                {

                    var ceilingFringe = ceilingPools.FirstOrDefault(p => p.PoolCode == "FRINGE" && p.AccountId == forecastempl.AcctId && Convert.ToInt16(p.FiscalYear) == forecastempl.Year)?.RateCeiling.GetValueOrDefault();
                    var ceilingGNA = ceilingPools.FirstOrDefault(p => p.PoolCode == "GNA" && p.AccountId == forecastempl.AcctId && Convert.ToInt16(p.FiscalYear) == forecastempl.Year)?.RateCeiling.GetValueOrDefault();
                    var ceilingOVERHEAD = ceilingPools.FirstOrDefault(p => p.PoolCode == "OVERHEAD" && p.AccountId == forecastempl.AcctId && Convert.ToInt16(p.FiscalYear) == forecastempl.Year)?.RateCeiling.GetValueOrDefault();


                    if (ceilingFringe.HasValue)
                        fringe = Math.Min(fringe, ceilingFringe.GetValueOrDefault());

                    if (ceilingGNA.HasValue)
                        gna = Math.Min(gna, ceilingGNA.GetValueOrDefault());


                    if (ceilingOVERHEAD.HasValue)
                        overHead = Math.Min(overHead, ceilingOVERHEAD.GetValueOrDefault());

                }

                if (OverrideFRevenueSettings)
                {
                    switch (projPlan.PlType)
                    {
                        case "EAC":
                            hours = (decimal)(forecastempl.Actualhours);
                            break;
                        case "BUD":
                            hours = (decimal)(forecastempl.Forecastedhours);
                            break;
                        case "NBBUD":
                            hours = (decimal)(forecastempl.Forecastedhours);
                            break;
                    }
                    if (forecastempl.Emple.Type.ToUpper() == "VENDOR EMPLOYEE" || forecastempl.Emple.Type.ToUpper() == "VENDOR" || forecastempl.Emple.Type.ToUpper() == "VENDOREMPLOYEE")
                    {
                        if (forecastempl.HrlyRate.HasValue)
                            forecastempl.ForecastedCost = hours * forecastempl.HrlyRate.GetValueOrDefault();
                        else
                            forecastempl.ForecastedCost = hours * forecastempl.Emple.PerHourRate.GetValueOrDefault();
                    }
                    else
                    {
                        if (new DateOnly(forecastempl.Year, forecastempl.Month, 1) < projPlan.ClosedPeriod.GetValueOrDefault())
                        {
                            if (forecastempl.HrlyRate.HasValue)
                                forecastempl.ForecastedCost = hours * forecastempl.HrlyRate.GetValueOrDefault();
                            else
                                forecastempl.ForecastedCost = hours * perHrRate;
                        }
                        else
                        {
                            forecastempl.ForecastedCost = hours * perHrRate;
                        }
                    }
                    forecastempl.Cost = forecastempl.ForecastedCost.GetValueOrDefault();
                    //if (revenueSetup.LabBurdFl.GetValueOrDefault() && forecastempl.Emple.IsBrd.GetValueOrDefault())///Had discussion on 7th June 2024 with Abdul that we need to calculate burden
                    if (forecastempl.Emple.IsBrd.GetValueOrDefault())
                    {
                        //forecastempl.ForecastedCost = (decimal)(forecastempl.Forecastedhours * perHrRate);
                        forecastempl.Fringe = (decimal)(forecastempl.ForecastedCost * fringe / 100);

                        var baseForOverhead = forecastempl.ForecastedCost + (FringeOnOverhead ? forecastempl.Fringe : 0);
                        forecastempl.Overhead = (decimal)(baseForOverhead * overHead / 100);
                        forecastempl.Materials = (decimal)(forecastempl.ForecastedCost * mnh / 100);

                        // Calculate Gna
                        decimal baseForGna = forecastempl.ForecastedCost.GetValueOrDefault();
                        if (FringeOnGna) baseForGna += forecastempl.Fringe;
                        if (OverHeadOnGna) baseForGna += forecastempl.Overhead;

                        forecastempl.Gna = (decimal)(baseForGna * gna / 100);

                        forecastempl.Burden = forecastempl.Fringe + forecastempl.Overhead + forecastempl.Gna + forecastempl.Materials;
                        forecastempl.TotalBurdenCost = financeHelper.CalculateBurdenedCostBYRAPRate(new BurdenInput() { FringeRate = fringe, GnaRate = gna, HourlyRate = perHrRate, Hours = hours, OverheadRate = overHead, plEmployee = forecastempl.Empl });
                        forecastempl.TotalBurdenCost = financeHelper.CalculateBurdenedCost(new BurdenInput() { FringeRate = fringe, GnaRate = gna, HourlyRate = perHrRate, Hours = hours, OverheadRate = overHead, plEmployee = forecastempl.Empl });
                        forecastempl.TotalBurdenCost = forecastempl.ForecastedCost.GetValueOrDefault() + forecastempl.Burden;

                        if (projPlan.PlType.ToUpper() == "EAC" && new DateOnly(forecastempl.Year, forecastempl.Month, 1) < projPlan.ClosedPeriod.GetValueOrDefault())
                        {
                            forecastempl.Cost = forecastempl.Cost;

                            forecastempl.Fringe = (decimal)(forecastempl.Cost * fringe / 100);

                            baseForOverhead = forecastempl.Cost + (FringeOnOverhead ? forecastempl.Fringe : 0);
                            forecastempl.Overhead = (decimal)(baseForOverhead * overHead / 100);

                            // Calculate Gna
                            baseForGna = forecastempl.Cost;
                            if (FringeOnGna) baseForGna += forecastempl.Fringe;
                            if (OverHeadOnGna) baseForGna += forecastempl.Overhead;

                            forecastempl.Gna = (decimal)(baseForGna * gna / 100);
                            forecastempl.Burden = forecastempl.Fringe + forecastempl.Overhead + forecastempl.Gna + forecastempl.Materials;
                            forecastempl.TotalBurdenCost = forecastempl.Cost + forecastempl.Burden;
                        }
                        //else
                        //{
                        //    forecastempl.Cost = forecastempl.ForecastedCost.GetValueOrDefault();
                        //    //forecastempl.TotalBurdenCost = forecastempl.Cost;
                        //}

                    }
                    else
                    {
                        //Need to discuss whether to calculate burden for non burden employees  for actuals?
                        if (projPlan.PlType.ToUpper() == "EAC" && new DateOnly(forecastempl.Year, forecastempl.Month, 1) < projPlan.ClosedPeriod.GetValueOrDefault())
                        {
                            forecastempl.Cost = forecastempl.Cost;

                            forecastempl.Fringe = (decimal)(forecastempl.Cost * fringe / 100);

                            var baseForOverhead = forecastempl.Cost + (FringeOnOverhead ? forecastempl.Fringe : 0);
                            forecastempl.Overhead = (decimal)(baseForOverhead * overHead / 100);

                            forecastempl.Materials = (decimal)(forecastempl.Cost * mnh / 100);

                            // Calculate Gna
                            var baseForGna = forecastempl.Cost;
                            if (FringeOnGna) baseForGna += forecastempl.Fringe;
                            if (OverHeadOnGna) baseForGna += forecastempl.Overhead;
                            forecastempl.Gna = (decimal)(baseForGna * gna / 100);

                            forecastempl.Burden = forecastempl.Fringe + forecastempl.Overhead + forecastempl.Gna + forecastempl.Materials;
                            forecastempl.TotalBurdenCost = forecastempl.Cost + forecastempl.Burden;
                        }
                        else
                        {
                            forecastempl.Cost = forecastempl.ForecastedCost.GetValueOrDefault();
                            forecastempl.Materials = 0;
                            forecastempl.Gna = 0;
                            forecastempl.Overhead = 0;
                            forecastempl.Fringe = 0;
                            forecastempl.Burden = 0;
                            forecastempl.Hr = 0;
                            // forecastempl.TotalBurdenCost = forecastempl.Cost;
                        }
                    }

                    //if (revenueSetup.LabCostFl.GetValueOrDefault())  Commented FOr correction on 7th June 2024 to fix revenue calculation
                    {
                        if (projPlan.PlType.ToUpper() == "EAC")
                        {
                            hours = forecastempl.Actualhours;
                        }
                        else
                        {
                            hours = forecastempl.Forecastedhours;

                        }
                        switch (revenueFormula)
                        {
                            case "CPFH":
                                if (projPlan.PlType.ToUpper() == "EAC" && new DateOnly(forecastempl.Year, forecastempl.Month, 1) < projPlan.ClosedPeriod.GetValueOrDefault())
                                {
                                    forecastempl.Cost = forecastempl.Cost;

                                    forecastempl.Fringe = (decimal)(forecastempl.Cost * fringe / 100);
                                    forecastempl.Materials = (decimal)(forecastempl.Cost * mnh / 100);

                                    var baseForOverhead = forecastempl.Cost + (FringeOnOverhead ? forecastempl.Fringe : 0);
                                    forecastempl.Overhead = (decimal)(baseForOverhead * overHead / 100);

                                    // Calculate Gna
                                    var baseForGna = forecastempl.Cost;
                                    if (FringeOnGna) baseForGna += forecastempl.Fringe;
                                    if (OverHeadOnGna) baseForGna += forecastempl.Overhead;
                                    forecastempl.Gna = (decimal)(baseForGna * gna / 100);

                                    forecastempl.Burden = forecastempl.Fringe + forecastempl.Overhead + forecastempl.Gna + forecastempl.Materials;
                                    forecastempl.TotalBurdenCost = forecastempl.Cost + forecastempl.Burden;
                                }
                                else
                                {
                                    forecastempl.Cost = forecastempl.ForecastedCost.GetValueOrDefault();
                                    // forecastempl.TotalBurdenCost = forecastempl.Cost;
                                }
                                //forecastempl.Revenue = hours * laborFees;
                                forecastempl.Revenue = forecastempl.Cost + forecastempl.Burden + (hours * laborFees);
                                break;

                            case "LLRCINL":
                                forecastempl.Revenue = await financeHelper.CalculateRevenueTNMAsync(forecastempl.ProjId ?? string.Empty, forecastempl.Plc ?? string.Empty, hours, forecastempl.Month, forecastempl.Year, forecastempl.EmplId ?? string.Empty);
                                forecastempl.Fees = forecastempl.Revenue * (laborFees / 100);
                                forecastempl.Revenue = forecastempl.Revenue + forecastempl.Fees;
                                break;

                            case "LLR":
                                forecastempl.Revenue = await financeHelper.CalculateRevenueTNMAsync(forecastempl.ProjId ?? string.Empty, forecastempl.Plc ?? string.Empty, hours, forecastempl.Month, forecastempl.Year, forecastempl.EmplId ?? string.Empty);
                                forecastempl.Fees = forecastempl.Revenue * (laborFees / 100);
                                forecastempl.Revenue = forecastempl.Revenue + forecastempl.Fees; break;

                            case "LLRCINBF":
                                forecastempl.Revenue = await financeHelper.CalculateRevenueTNMAsync(forecastempl.ProjId ?? string.Empty, forecastempl.Plc ?? string.Empty, hours, forecastempl.Month, forecastempl.Year, forecastempl.EmplId ?? string.Empty);
                                forecastempl.Fees = forecastempl.Revenue * (laborFees / 100);
                                forecastempl.Revenue = forecastempl.Revenue + forecastempl.Fees; break;

                            case "CPFF":

                                if (forecastempl.Year == 2025 && forecastempl.Month == 11 && forecastempl.EmplId == "1002530")
                                {
                                }
                                if (revenueSetup.LabCostFl.GetValueOrDefault() && revenueSetup.LabBurdFl.GetValueOrDefault())
                                {
                                    forecastempl.Fees = forecastempl.TotalBurdenCost * (laborFees / 100);
                                    forecastempl.Revenue = forecastempl.TotalBurdenCost * (1 + laborFees / 100);
                                }
                                else
                                {
                                    if (revenueSetup.LabCostFl.GetValueOrDefault())
                                    {
                                        forecastempl.Fees = forecastempl.Cost * (laborFees / 100);
                                        forecastempl.Revenue = forecastempl.Cost * (1 + laborFees / 100);
                                    }
                                    else
                                    {
                                        if (revenueSetup.LabBurdFl.GetValueOrDefault())
                                        {
                                            forecastempl.Fees = forecastempl.Burden * (laborFees / 100);
                                            forecastempl.Revenue = forecastempl.Burden * (1 + laborFees / 100);
                                        }

                                    }
                                }
                                break;

                            case "LLRCINLB":
                                forecastempl.Revenue = await financeHelper.CalculateRevenueTNMAsync(forecastempl.ProjId ?? string.Empty, forecastempl.Plc ?? string.Empty, hours, forecastempl.Month, forecastempl.Year, forecastempl.EmplId ?? string.Empty);
                                forecastempl.Fees = forecastempl.Revenue * (laborFees / 100);
                                forecastempl.Revenue = forecastempl.Revenue + forecastempl.Fees; break;

                            case "LLRFNLBF":
                                forecastempl.Revenue = await financeHelper.CalculateRevenueTNMAsync(forecastempl.ProjId ?? string.Empty, forecastempl.Plc ?? string.Empty, hours, forecastempl.Month, forecastempl.Year, forecastempl.EmplId ?? string.Empty);
                                forecastempl.Fees = forecastempl.Revenue * (laborFees / 100);
                                forecastempl.Revenue = forecastempl.Revenue * (1 + laborFees / 100);
                                break;

                        }
                    }
                }
                else
                {
                    if (projPlan.PlType.ToUpper() == "EAC")
                    {
                        hours = forecastempl.Actualhours;
                    }
                    else
                    {
                        hours = forecastempl.Forecastedhours;

                    }

                    forecastempl.ForecastedCost = (decimal)(hours * perHrRate);
                    forecastempl.Forecastedamt = (decimal)(hours * perHrRate);
                    forecastempl.Cost = (decimal)(hours * perHrRate);

                    if (forecastempl.Emple.IsBrd.GetValueOrDefault())
                    {

                        forecastempl.Materials = (decimal)(forecastempl.ForecastedCost * mnh / 100);
                        forecastempl.Fringe = (decimal)(forecastempl.ForecastedCost * fringe / 100);

                        var baseForOverhead = forecastempl.ForecastedCost + (FringeOnOverhead ? forecastempl.Fringe : 0);
                        forecastempl.Overhead = (decimal)(baseForOverhead * overHead / 100);

                        // Calculate Gna
                        var baseForGna = forecastempl.ForecastedCost;
                        if (FringeOnGna) baseForGna += forecastempl.Fringe;
                        if (OverHeadOnGna) baseForGna += forecastempl.Overhead;
                        forecastempl.Gna = (decimal)(baseForGna * gna / 100);

                        forecastempl.Burden = forecastempl.Fringe + forecastempl.Overhead + forecastempl.Gna + forecastempl.Materials;
                        forecastempl.TotalBurdenCost = financeHelper.CalculateBurdenedCostBYRAPRate(new BurdenInput() { FringeRate = fringe, GnaRate = gna, HourlyRate = perHrRate, Hours = hours, OverheadRate = overHead, plEmployee = forecastempl.Empl });
                        forecastempl.TotalBurdenCost = financeHelper.CalculateBurdenedCost(new BurdenInput() { FringeRate = fringe, GnaRate = gna, HourlyRate = perHrRate, Hours = hours, OverheadRate = overHead, plEmployee = forecastempl.Empl });
                        forecastempl.TotalBurdenCost = forecastempl.ForecastedCost.GetValueOrDefault() + forecastempl.Burden;

                        if (projPlan.PlType.ToUpper() == "EAC" && new DateOnly(forecastempl.Year, forecastempl.Month, 1) < projPlan.ClosedPeriod.GetValueOrDefault())
                        {
                            forecastempl.Cost = forecastempl.Cost;

                            forecastempl.Fringe = (decimal)(forecastempl.Cost * fringe / 100);

                            baseForOverhead = forecastempl.Cost + (FringeOnOverhead ? forecastempl.Fringe : 0);
                            forecastempl.Overhead = (decimal)(baseForOverhead * overHead / 100);

                            // Calculate Gna
                            baseForGna = forecastempl.Cost;
                            if (FringeOnGna) baseForGna += forecastempl.Fringe;
                            if (OverHeadOnGna) baseForGna += forecastempl.Overhead;
                            forecastempl.Gna = (decimal)(baseForGna * gna / 100);

                            forecastempl.Burden = forecastempl.Fringe + forecastempl.Overhead + forecastempl.Gna;
                            forecastempl.TotalBurdenCost = forecastempl.Cost + forecastempl.Burden;
                        }
                        else
                        {
                            forecastempl.Cost = forecastempl.ForecastedCost.GetValueOrDefault();
                            forecastempl.TotalBurdenCost = forecastempl.Cost;
                        }
                    }
                    else
                    {
                        forecastempl.Materials = 0;
                        forecastempl.Gna = 0;
                        forecastempl.Overhead = 0;
                        forecastempl.Fringe = 0;
                        forecastempl.Burden = 0;
                        forecastempl.Hr = 0;
                    }
                    if (forecastempl.Emple.IsRev.GetValueOrDefault())
                    {


                        switch (revenueFormula)
                        {
                            case "CPFH":
                                forecastempl.Revenue = hours * laborFees;
                                forecastempl.Revenue = forecastempl.Cost + forecastempl.Burden + (hours * laborFees);
                                break;

                            case "LLRCINL":
                                forecastempl.Revenue = await financeHelper.CalculateRevenueTNMAsync(forecastempl.ProjId ?? string.Empty, forecastempl.Plc ?? string.Empty, hours, forecastempl.Month, forecastempl.Year, forecastempl.EmplId ?? string.Empty);
                                break;

                            case "LLR":
                                forecastempl.Revenue = await financeHelper.CalculateRevenueTNMAsync(forecastempl.ProjId ?? string.Empty, forecastempl.Plc ?? string.Empty, hours, forecastempl.Month, forecastempl.Year, forecastempl.EmplId ?? string.Empty);
                                break;

                            case "LLRCINBF":
                                forecastempl.Revenue = await financeHelper.CalculateRevenueTNMAsync(forecastempl.ProjId ?? string.Empty, forecastempl.Plc ?? string.Empty, hours, forecastempl.Month, forecastempl.Year, forecastempl.EmplId ?? string.Empty);
                                break;

                            case "CPFF":
                                forecastempl.Fees = (forecastempl.Cost + forecastempl.Burden) * (laborFees / 100);
                                forecastempl.Revenue = (forecastempl.Cost + forecastempl.Burden) * (1 + laborFees / 100);
                                break;

                            case "LLRCINLB":
                                forecastempl.Revenue = await financeHelper.CalculateRevenueTNMAsync(forecastempl.ProjId ?? string.Empty, forecastempl.Plc ?? string.Empty, hours, forecastempl.Month, forecastempl.Year, forecastempl.EmplId ?? string.Empty);
                                break;

                            case "LLRFNLBF":
                                forecastempl.Revenue = await financeHelper.CalculateRevenueTNMAsync(forecastempl.ProjId ?? string.Empty, forecastempl.Plc ?? string.Empty, hours, forecastempl.Month, forecastempl.Year, forecastempl.EmplId ?? string.Empty);
                                forecastempl.Fees = forecastempl.Revenue * (laborFees / 100);
                                forecastempl.Revenue = forecastempl.Revenue * (1 + laborFees / 100);
                                break;
                            default:
                                forecastempl.Revenue = 0;
                                forecastempl.Fees = 0;
                                break;

                        }

                    }
                    else
                    {
                        forecastempl.Revenue = 0;
                        forecastempl.Fees = 0;
                    }
                }

            }

            /////////////////Get Ceiling Configuration For Burdens

            var ceilingEmplHours = _context.PlCeilHrEmpls.Where(p => p.ProjectId == projId).ToList();
            var ceilingLabCatHours = _context.PlCeilHrCats.Where(p => p.ProjectId == projId).ToList();

            var combinedList = hoursForecast
                            .OrderBy(x => x.Year)
                            .ThenBy(x => x.Month)
                            .ToList();

            if (projPlan.PlType.ToUpper() == "NBBUD")
            {
                fundingValue = 100000000;
            }

            var Adjusted = financeHelper.AdjustRevenue(combinedList, fundingValue, projPlan);
            var swTotal = Stopwatch.StartNew();

            foreach (var chunk in Adjusted.Chunk(300))
            {
                var chunkIds = chunk.Select(f => f.Forecastid).ToList();
                await _context.BulkUpdateAsync(chunk, new BulkConfig { SetOutputIdentity = true });
                await _context.SaveChangesAsync();
            }
            swTotal.Stop();
            Console.WriteLine($"🎯 Bulk update completed in {swTotal.Elapsed.TotalSeconds:F2}s");


            planForecastSummary.Version = projPlan.Version.GetValueOrDefault();
            planForecastSummary.Proj_Id = projPlan.ProjId;
            planForecastSummary.Type = projPlan.PlType;
        }
        catch (Exception ex)
        {

        }
        return planForecastSummary;
    }



    //public async Task CalculateBurdenCost(int planID, int templateId, string type)
    //{
    //    int cost = 0;

    //    string revenueFormula = "";
    //    string escallation_month = "0", escallation_percent = "0";


    //    string projId = string.Empty, organization = string.Empty, projType = string.Empty;

    //    try
    //    {
    //        var AllAvaialbepools = _context.AccountGroups.ToList();

    //        decimal fringe = 0, gna = 0, overHead = 0, laborFees = 0, hr = 0, mnh = 0, nonLaborFees = 0, RevenueAdj = 0, fundingValue = 0, ActualRevenue = 0;
    //        bool OverHeadOnGna = false, FringeOnOverhead = false, FringeOnGna = false;

    //        List<PlTemplatePoolRate> burdensByTemplate = new List<PlTemplatePoolRate>();
    //        List<pl_EmployeeBurdenCalculated> plEmployeeBurdenCalculated = new List<pl_EmployeeBurdenCalculated>();
    //        var projPlan = _context.PlProjectPlans.Where(p => p.PlId == planID).Include(p => p.Proj).FirstOrDefault();
    //        if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
    //        {
    //            projId = projPlan.Proj.ProjId;
    //            projType = projPlan.Proj.ProjTypeDc;
    //            projPlan.ProjEndDt = projPlan.Proj.ProjEndDt;
    //            projPlan.ProjStartDt = projPlan.Proj.ProjStartDt;
    //            fundingValue = projPlan.Proj.proj_f_tot_amt.GetValueOrDefault();
    //        }
    //        else
    //        {
    //            projId = projPlan.ProjId;
    //            var projPlanNBBUD = _context.NewBusinessBudgets.Where(p => p.BusinessBudgetId == projPlan.ProjId).FirstOrDefault();
    //            projPlan.ProjEndDt = projPlanNBBUD?.EndDate != null
    //                                    ? DateOnly.FromDateTime(projPlanNBBUD.EndDate)
    //                                    : (DateOnly?)null;
    //            projPlan.ProjStartDt = projPlanNBBUD?.StartDate != null
    //                                    ? DateOnly.FromDateTime(projPlanNBBUD.StartDate)
    //                                    : (DateOnly?)null;
    //            escallation_percent = projPlanNBBUD?.EscalationRate != null ? projPlanNBBUD.EscalationRate.ToString() : "3";
    //        }

    //        var revenueSetup = _context.ProjBgtRevSetups.FirstOrDefault(p => p.ProjId == projId && p.VersionNo == projPlan.Version && p.BgtType == projPlan.PlType);
    //        var validOrgs = _context.PlEmployeees
    //                            .Where(e => e.PlId == projPlan.PlId && e.OrgId != null)
    //                            .Select(e => e.OrgId)
    //                            .Union(
    //                                _context.PlDcts
    //                                    .Where(d => d.PlId == projPlan.PlId && d.OrgId != null)
    //                                    .Select(d => d.OrgId)
    //                            )
    //                            .Distinct()
    //                            .ToList();

    //        var acctPool = (from pool in _context.PlOrgAcctPoolMappings
    //                        join orgId in validOrgs on pool.OrgId equals orgId
    //                        select pool).ToList();

    //        FinanceHelper financeHelper = new FinanceHelper(_context, projId);

    //        burdensByTemplate = await _context.PlTemplatePoolRates.Where(r => r.TemplateId == templateId).ToListAsync();
    //        var All_Config = _context.PlConfigValues.ToList();

    //        Account_Org_Helpercs account_Org_Helpercs = new Account_Org_Helpercs(_context);
    //        var templatePools = account_Org_Helpercs.GetPoolsByTemplateId(templateId).Select(p => p.PoolId).ToList();

    //        if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
    //        {
    //            if (projId != null)
    //            {
    //                organization = _context.PlProjects
    //                    .Where(p => p.ProjId == projId)
    //                    .Include(p => p.Org)
    //                    .Select(p => p.Org.OrgName)
    //                    .FirstOrDefault();
    //            }
    //        }

    //        var forecastsDirectCosts = await _context.PlForecasts
    //            .Include(p => p.DirectCost)
    //        .Include(p => p.Proj)
    //            .Where(f => f.Emple == null && f.PlId == planID).ToListAsync();

    //        /////////////////Get Ceiling Configuration For Burdens

    //        var ceilingPools = _context.PlCeilBurdens.Where(p => p.ProjectId == projId).ToList();


    //        //////////////////////////////////////////////////////////////

    //        foreach (var forecast in forecastsDirectCosts)
    //        {

    //            if (acctPool.Count() > 0)
    //            {
    //                var burdens = burdensByTemplate.Where(r => r.Month == forecast.Month && r.Year == forecast.Year).ToList();
    //                var validBurdens = financeHelper.GetBurdenRates(templatePools, burdens, type);
    //                var pools = acctPool.Where(p => p.AccountId == forecast.AcctId && p.OrgId == forecast.OrgId);
    //                fringe = 0; gna = 0; overHead = 0; mnh = 0; hr = 0;
    //                OverHeadOnGna = false; FringeOnOverhead = false; FringeOnGna = false;
    //                foreach (var pool in pools)
    //                {
    //                    switch (pool.PoolId.ToUpper())
    //                    {
    //                        case "FRINGE":
    //                            validBurdens.TryGetValue("FRINGE", out fringe);
    //                            break;
    //                        case "GNA":
    //                            validBurdens.TryGetValue("GNA", out gna);
    //                            break;
    //                        case "OVERHEAD":
    //                            validBurdens.TryGetValue("OVERHEAD", out overHead);
    //                            break;
    //                        case "MNH":
    //                            validBurdens.TryGetValue("MNH", out mnh);
    //                            break;
    //                        case "HR":
    //                            validBurdens.TryGetValue("HR", out hr);
    //                            break;
    //                        case "FRINGEONOVERHEAD":
    //                            FringeOnOverhead = true;
    //                            break;
    //                        case "FRINGEONGNA":
    //                            FringeOnGna = true;
    //                            break;
    //                        case "OVERHEADONGNA":
    //                            OverHeadOnGna = true;
    //                            break;
    //                    }
    //                }
    //            }
    //            if (forecast.Year == 2025 && forecast.Month == 1 && forecast.AcctId == "51-300-000" && forecast.OrgId == "1.01.03.01")
    //            {

    //            }


    //            if (projPlan.PlType.ToUpper() == "EAC" && new DateOnly(forecast.Year, forecast.Month, 1) < projPlan.ClosedPeriod.GetValueOrDefault())
    //            {
    //                forecast.TotalBurdenCost = forecast.Cost + forecast.Burden;
    //                continue;
    //            }

    //            {
    //                if (forecast.DirectCost.IsBrd)
    //                {
    //                    switch (projPlan.PlType)
    //                    {
    //                        case "EAC":
    //                            forecast.Cost = (decimal)(forecast.Actualamt.GetValueOrDefault());
    //                            break;
    //                        case "BUD":
    //                            forecast.Cost = (decimal)(forecast.Forecastedamt.GetValueOrDefault());
    //                            break;
    //                        case "NBBUD":
    //                            forecast.Cost = (decimal)(forecast.Forecastedamt.GetValueOrDefault());
    //                            break;
    //                    }
    //                    forecast.Materials = (decimal)(forecast.Cost * mnh / 100);
    //                    forecast.Fringe = (decimal)(forecast.Cost * fringe / 100);
    //                    forecast.Overhead = (decimal)((forecast.Cost + forecast.Fringe) * overHead / 100);
    //                    forecast.Gna = (decimal)((forecast.Cost + forecast.Fringe + forecast.Overhead) * gna / 100);
    //                    forecast.Burden = forecast.Fringe + forecast.Overhead + forecast.Gna + forecast.Materials;
    //                    forecast.TotalBurdenCost = financeHelper.CalculateBurdenedCostBYRAPRate(new BurdenInput() { FringeRate = fringe, GnaRate = gna, HourlyRate = 0, Hours = forecast.Forecastedhours, OverheadRate = overHead, DirectCost = forecast.Forecastedamt.GetValueOrDefault(), plEmployee = forecast.Empl });
    //                    forecast.TotalBurdenCost = financeHelper.CalculateBurdenedCost(new BurdenInput() { FringeRate = fringe, GnaRate = gna, HourlyRate = 0, Hours = forecast.Forecastedhours, OverheadRate = overHead, DirectCost = forecast.Forecastedamt.GetValueOrDefault(), plEmployee = forecast.Empl });
    //                }
    //                else
    //                {
    //                    switch (projPlan.PlType)
    //                    {
    //                        case "EAC":
    //                            forecast.Cost = (decimal)(forecast.Actualamt.GetValueOrDefault());
    //                            break;
    //                        case "BUD":
    //                            forecast.Cost = (decimal)(forecast.Forecastedamt.GetValueOrDefault());
    //                            break;
    //                        case "NBBUD":
    //                            forecast.Cost = (decimal)(forecast.Forecastedamt.GetValueOrDefault());
    //                            break;
    //                    }
    //                    //forecast.Cost = (decimal)(forecast.Forecastedamt.GetValueOrDefault());
    //                    forecast.TotalBurdenCost = forecast.Cost;
    //                }
    //            }

    //        }

    //        if (organization == string.Empty || organization == null)
    //        {
    //            projId = forecastsDirectCosts.FirstOrDefault()?.Proj?.ProjId;

    //            if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
    //            {
    //                if (projId != null)
    //                {
    //                    organization = _context.PlProjects
    //                        .Where(p => p.ProjId == projId)
    //                        .Include(p => p.Org)
    //                        .Select(p => p.Org.OrgName)
    //                        .FirstOrDefault();
    //                }
    //            }
    //        }

    //        var forecasts = await _context.PlForecasts
    //                        .Include(p => p.Emple)
    //                        .Include(p => p.Proj)
    //                        .Where(f => f.Emple != null && f.PlId == planID).ToListAsync();

    //        if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
    //        {
    //            escallation_month = _context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "escallation_month" && r.ProjId == forecasts.FirstOrDefault().ProjId)?.Value ?? _context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "escallation_month" && r.ProjId == "xxxxx")?.Value ?? "3";
    //            escallation_percent = _context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "escallation_percent" && r.ProjId == forecasts.FirstOrDefault().ProjId)?.Value ?? _context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "escallation_percent" && r.ProjId == "xxxxx")?.Value ?? "3";
    //        }
    //        else
    //        {
    //            escallation_month = _context.PlConfigValues.FirstOrDefault(r => r.Name.ToLower() == "escallation_month" && r.ProjId == "xxxxx")?.Value ?? "7";
    //        }

    //        if (string.IsNullOrEmpty(escallation_month))
    //            escallation_month = "7";

    //        List<EmplSchedule> res = new List<EmplSchedule>();
    //        List<DirectCostSchedule> res1 = new List<DirectCostSchedule>();

    //        res = financeHelper.CalculateSalaryPlan(forecasts, 0, 2025, Convert.ToInt32(escallation_month), Convert.ToDecimal(escallation_percent), projPlan);
    //        res1 = financeHelper.CalculateDirectCostPlan(forecastsDirectCosts);

    //        foreach (var forecastempl in forecasts)
    //        {

    //            if (forecastempl.Forecastid == 254007)
    //            {

    //            }
    //            decimal hours = 0;
    //            if (forecastempl.Year == 2025 && forecastempl.Month == 1 && forecastempl.EmplId == "000830")
    //            {
    //            }

    //            fringe = 0; gna = 0; overHead = 0; ; mnh = 0; hr = 0;
    //            OverHeadOnGna = false; FringeOnOverhead = false; FringeOnGna = false;
    //            if (forecastempl.Year == 2025 && forecastempl.Month == 10)
    //            {

    //            }
    //            decimal perHrRate = 0;

    //            if (projPlan != null && projPlan.PlType.ToUpper() != "NBBUD")
    //                perHrRate = res.FirstOrDefault(p => p.EmpId == forecastempl.EmplId && p.AccID == forecastempl.AcctId && p.PlcCode == forecastempl.Plc && p.OrgID == forecastempl.OrgId)?.payrollSalary.FirstOrDefault(p => p.Month == forecastempl.Month && p.Year == forecastempl.Year)?.Salary ?? 0;
    //            else
    //                perHrRate = forecastempl.Emple.PerHourRate.GetValueOrDefault();

    //            //if (revenueFormula == "CPFF" || revenueFormula == "CPFH")
    //            if (acctPool.Count() > 0)
    //            {
    //                var burdens = burdensByTemplate.Where(r => r.Month == forecastempl.Month && r.Year == forecastempl.Year).ToList();
    //                var validBurdens = financeHelper.GetBurdenRates(templatePools, burdens, type);
    //                var pools = acctPool.Where(p => p.AccountId == forecastempl.AcctId && p.OrgId == forecastempl.OrgId);
    //                foreach (var pool in pools)
    //                {
    //                    //AllAvaialbepools.FirstOrDefault(p => p.Code == pool.PoolId).Type;
    //                    //switch (pool.PoolId.ToUpper())
    //                    switch (AllAvaialbepools.FirstOrDefault(p => p.Code == pool.PoolId).Type.ToUpper())
    //                    {
    //                        case "FRINGE":
    //                            validBurdens.TryGetValue(pool.PoolId.ToUpper(), out fringe);
    //                            break;
    //                        case "GNA":
    //                            validBurdens.TryGetValue(pool.PoolId.ToUpper(), out gna);
    //                            break;
    //                        case "MNH":
    //                            validBurdens.TryGetValue("MNH", out mnh);
    //                            break;
    //                        case "HR":
    //                            validBurdens.TryGetValue("HR", out hr);
    //                            break;
    //                        case "OVERHEAD":
    //                            validBurdens.TryGetValue(pool.PoolId.ToUpper(), out overHead);
    //                            break;
    //                        case "FRINGEONOVERHEAD":
    //                            FringeOnOverhead = true;
    //                            break;
    //                        case "FRINGEONGNA":
    //                            FringeOnGna = true;
    //                            break;
    //                        case "OVERHEADONGNA":
    //                            OverHeadOnGna = true;
    //                            break;
    //                    }
    //                }
    //            }

    //            if (ceilingPools.Count > 0)
    //            {

    //                var ceilingFringe = ceilingPools.FirstOrDefault(p => p.PoolCode == "FRINGE" && p.AccountId == forecastempl.AcctId && Convert.ToInt16(p.FiscalYear) == forecastempl.Year)?.RateCeiling.GetValueOrDefault();
    //                var ceilingGNA = ceilingPools.FirstOrDefault(p => p.PoolCode == "GNA" && p.AccountId == forecastempl.AcctId && Convert.ToInt16(p.FiscalYear) == forecastempl.Year)?.RateCeiling.GetValueOrDefault();
    //                var ceilingOVERHEAD = ceilingPools.FirstOrDefault(p => p.PoolCode == "OVERHEAD" && p.AccountId == forecastempl.AcctId && Convert.ToInt16(p.FiscalYear) == forecastempl.Year)?.RateCeiling.GetValueOrDefault();


    //                if (ceilingFringe.HasValue)
    //                    fringe = Math.Min(fringe, ceilingFringe.GetValueOrDefault());

    //                if (ceilingGNA.HasValue)
    //                    gna = Math.Min(gna, ceilingGNA.GetValueOrDefault());


    //                if (ceilingOVERHEAD.HasValue)
    //                    overHead = Math.Min(overHead, ceilingOVERHEAD.GetValueOrDefault());

    //            }
    //            {

    //                if (forecastempl.Emple.IsBrd.GetValueOrDefault())
    //                {
    //                    hours = 0;
    //                    if (projPlan.PlType.ToUpper() == "EAC")
    //                    {
    //                        hours = forecastempl.Actualhours;
    //                    }
    //                    else
    //                    {
    //                        hours = forecastempl.Forecastedhours;

    //                    }
    //                    forecastempl.ForecastedCost = (decimal)(hours * perHrRate);
    //                    forecastempl.Forecastedamt = (decimal)(hours * perHrRate);
    //                    forecastempl.Cost = (decimal)(hours * perHrRate);
    //                    forecastempl.Materials = (decimal)(forecastempl.ForecastedCost * mnh / 100);
    //                    forecastempl.Fringe = (decimal)(forecastempl.ForecastedCost * fringe / 100);

    //                    var baseForOverhead = forecastempl.ForecastedCost + (FringeOnOverhead ? forecastempl.Fringe : 0);
    //                    forecastempl.Overhead = (decimal)(baseForOverhead * overHead / 100);

    //                    // Calculate Gna
    //                    var baseForGna = forecastempl.ForecastedCost;
    //                    if (FringeOnGna) baseForGna += forecastempl.Fringe;
    //                    if (OverHeadOnGna) baseForGna += forecastempl.Overhead;
    //                    forecastempl.Gna = (decimal)(baseForGna * gna / 100);

    //                    forecastempl.Burden = forecastempl.Fringe + forecastempl.Overhead + forecastempl.Gna + forecastempl.Materials;
    //                    forecastempl.TotalBurdenCost = financeHelper.CalculateBurdenedCostBYRAPRate(new BurdenInput() { FringeRate = fringe, GnaRate = gna, HourlyRate = perHrRate, Hours = hours, OverheadRate = overHead, plEmployee = forecastempl.Empl });
    //                    forecastempl.TotalBurdenCost = financeHelper.CalculateBurdenedCost(new BurdenInput() { FringeRate = fringe, GnaRate = gna, HourlyRate = perHrRate, Hours = hours, OverheadRate = overHead, plEmployee = forecastempl.Empl });
    //                    forecastempl.TotalBurdenCost = forecastempl.ForecastedCost.GetValueOrDefault() + forecastempl.Burden;

    //                    if (projPlan.PlType.ToUpper() == "EAC" && new DateOnly(forecastempl.Year, forecastempl.Month, 1) < projPlan.ClosedPeriod.GetValueOrDefault())
    //                    {
    //                        forecastempl.Cost = forecastempl.Cost;

    //                        forecastempl.Fringe = (decimal)(forecastempl.Cost * fringe / 100);

    //                        baseForOverhead = forecastempl.Cost + (FringeOnOverhead ? forecastempl.Fringe : 0);
    //                        forecastempl.Overhead = (decimal)(baseForOverhead * overHead / 100);

    //                        // Calculate Gna
    //                        baseForGna = forecastempl.Cost;
    //                        if (FringeOnGna) baseForGna += forecastempl.Fringe;
    //                        if (OverHeadOnGna) baseForGna += forecastempl.Overhead;
    //                        forecastempl.Gna = (decimal)(baseForGna * gna / 100);

    //                        forecastempl.Burden = forecastempl.Fringe + forecastempl.Overhead + forecastempl.Gna;
    //                        forecastempl.TotalBurdenCost = forecastempl.Cost + forecastempl.Burden;
    //                    }
    //                    else
    //                    {
    //                        forecastempl.Cost = forecastempl.ForecastedCost.GetValueOrDefault();
    //                        forecastempl.TotalBurdenCost = forecastempl.Cost;
    //                    }
    //                }
    //                else
    //                {

    //                    if (projPlan.PlType.ToUpper() == "EAC" && new DateOnly(forecastempl.Year, forecastempl.Month, 1) < projPlan.ClosedPeriod.GetValueOrDefault())
    //                    {
    //                        forecastempl.Cost = forecastempl.Cost;
    //                        forecastempl.TotalBurdenCost = forecastempl.Cost;
    //                    }
    //                    else
    //                    {
    //                        hours = 0;
    //                        if (projPlan.PlType.ToUpper() == "EAC")
    //                        {
    //                            hours = forecastempl.Actualhours;
    //                        }
    //                        else
    //                        {
    //                            hours = forecastempl.Forecastedhours;

    //                        }
    //                        forecastempl.ForecastedCost = (decimal)(hours * perHrRate);
    //                        forecastempl.Forecastedamt = (decimal)(hours * perHrRate);
    //                        forecastempl.TotalBurdenCost = forecastempl.ForecastedCost.GetValueOrDefault();
    //                    }

    //                }

    //            }

    //        }

    //        /////////////////Get Ceiling Configuration For Burdens

    //        var ceilingEmplHours = _context.PlCeilHrEmpls.Where(p => p.ProjectId == projId).ToList();
    //        var ceilingLabCatHours = _context.PlCeilHrCats.Where(p => p.ProjectId == projId).ToList();

    //        var combinedList = forecasts
    //                        .Concat(forecastsDirectCosts)
    //                        .OrderBy(x => x.Year)
    //                        .ThenBy(x => x.Month)
    //                        .ToList();

    //        if (projPlan.PlType.ToUpper() == "NBBUD")
    //        {
    //            fundingValue = 100000000;
    //        }

    //        var swTotal = Stopwatch.StartNew();

    //        foreach (var chunk in combinedList.Chunk(300))
    //        {
    //            var chunkIds = chunk.Select(f => f.Forecastid).ToList();

    //            await _context.BulkUpdateAsync(chunk, new BulkConfig { SetOutputIdentity = true });
    //            await _context.SaveChangesAsync();

    //            //Console.WriteLine($"✅ updated chunk {index + 1}/{chunks.Count} ({chunk.Count()} rows)");
    //        }
    //        swTotal.Stop();
    //        Console.WriteLine($"🎯 Bulk update completed in {swTotal.Elapsed.TotalSeconds:F2}s");
    //    }
    //    catch (Exception ex)
    //    {

    //    }
    //}



    public Task<List<PlForecast>> GetForecastByProjectIDAndVersion(string projId, int version, string type)
    {
        return _context.PlForecasts
                    .Include(p => p.Emple)
                    .Include(p => p.Proj)
                    .Include(p => p.DirectCost)
                    .Include(p => p.Pl)
            .Where(f => f.ProjId == projId && f.Pl.Version == version && f.Pl.PlType.ToLower() == type.ToLower()).ToListAsync();
    }

}