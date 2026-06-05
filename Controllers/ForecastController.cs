namespace WebApi.Controllers;

using EFCore.BulkExtensions;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol;
using NetTopologySuite.Mathematics;
using Newtonsoft.Json;
using Npgsql;
using NPOI.HSSF.Record;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.Streaming.Values;
using NPOI.XSSF.UserModel;
using NPOI.XWPF.UserModel;
using PlanningAPI.Helpers;
using PlanningAPI.Models;
using PlanningAPI.Repositories;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Text.Json;
using System.Text.RegularExpressions;
using WebApi.DTO;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Repositories;
using WebApi.Services;
using static NPOI.HSSF.Util.HSSFColor;
using static QuestPDF.Helpers.Colors;

[ApiController]
[Route("[controller]")]
public class ForecastController : ControllerBase
{
    private IPl_ForecastService _pl_ForecastService;
    private IProjPlanService _projPlanService;
    private IProjRevWrkPdRepository _projRevWrkPdRepository;

    private readonly MydatabaseContext _context;
    private readonly ILogger<ForecastController> _logger;
    IOrgService _orgService;
    private readonly IAiService _aiService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly IConfiguration _config;
    public ForecastController(ILogger<ForecastController> logger, IPl_ForecastService pl_ForecastService, IProjPlanService projPlanService, IOrgService orgService, IProjRevWrkPdRepository projRevWrkPdRepository, MydatabaseContext context, IAiService aiService, IServiceProvider serviceProvider, IBackgroundTaskQueue taskQueue, IConfiguration config)
    {
        _pl_ForecastService = pl_ForecastService;
        _projPlanService = projPlanService;
        _context = context;
        _logger = logger;
        _config = config;
        _orgService = orgService;
        _projRevWrkPdRepository = projRevWrkPdRepository;
        _aiService = aiService;
        _serviceProvider = serviceProvider;
        _taskQueue = taskQueue;
    }
    [HttpPost("ValidateForecast")]
    public IActionResult ValidateForecast(int planid)
    {

        _taskQueue.QueueBackgroundWorkItem(async token =>
        {
            using var scope = _serviceProvider.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<MydatabaseContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ForecastController>>();

            try
            {
                var projPlan = db.PlProjectPlans.Where(p => p.PlId == planid).FirstOrDefault();
                if (projPlan == null)
                    return;
                //var totalEmployeeHours = await (
                //        from f in _context.PlForecasts
                //        join pp in _context.PlProjectPlans on f.PlId equals pp.PlId
                //        where f.EmplId == forecast.EmplId
                //              && f.Year == forecast.Year
                //              && f.Month == forecast.Month
                //              && (pp.FinalVersion == true || pp.PlId == forecast.PlId)
                //        select new
                //        {
                //            Hours =
                //                pp.PlId == forecast.PlId && pp.PlType == "EAC" ? f.Actualhours :
                //                pp.PlId == forecast.PlId && pp.PlType == "BUD" ? f.Forecastedhours :
                //                pp.PlId != forecast.PlId && pp.PlType == "EAC" && pp.FinalVersion == true ? f.Actualhours :
                //                pp.PlId != forecast.PlId && pp.PlType == "BUD" && pp.FinalVersion == true &&
                //                !_context.PlProjectPlans.Any(pp2 => pp2.ProjId == pp.ProjId && pp2.PlType == "EAC" && pp2.FinalVersion == true)
                //                    ? f.Forecastedhours : 0
                //        })
                //        .SumAsync(x => x.Hours);



                var validator = new ForecastValidator(db, logger);
                var forecasts = await db.PlForecasts
                    .Where(p => p.PlId == planid && p.Year == projPlan.ClosedPeriod.GetValueOrDefault().Year)
                    .ToListAsync(token);



                var result = forecasts
                    .GroupBy(f => new { f.EmplId, f.Month, f.Year })
                    .Select(g => new PlForecast
                    {
                        PlId = planid,
                        ProjId = g.First().ProjId,
                        EmplId = g.Key.EmplId,
                        Month = g.Key.Month,
                        Year = g.Key.Year,
                        Forecastedhours = g.Sum(x => x.Forecastedhours),
                        Actualhours = g.Sum(x => x.Actualamt ?? 0)
                    })
                    .ToList();

                await validator.ValidateForecastsAsync(result);

                logger.LogInformation("Background forecast validation completed for PlanId {PlanId}", planid);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error validating forecast for PlanId {PlanId}", planid);
            }
        });

        return Accepted(new { Message = $"Forecast validation for PlanId {planid} started in background." });

        //_ = Task.Run(async () =>
        //{
        //    try
        //    {
        //        using var scope = _serviceProvider.CreateScope();
        //        var context = scope.ServiceProvider.GetRequiredService<MydatabaseContext>();
        //        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ForecastController>>();


        //        ForecastValidator forecastValidator = new ForecastValidator(_context, _logger);
        //        forecastValidator.ValidateForecastsAsync(_context.PlForecasts.Where(p => p.PlId == planid && p.Year == 2025).ToList()).Wait();
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Background forecast validation failed for plan {PlanId}", planid);
        //    }
        //});

        //return Accepted(new { Message = "Forecast validation started in background." });
    }

    [HttpGet("GetAllForecasts")]
    public async Task<IActionResult> GetAllForecasts()
    {
        _logger.LogInformation("GetAllForecasts called at {Time}", DateTime.UtcNow);
        try
        {
            var forecasts = await _pl_ForecastService.GetAllAsync();
            return Ok(forecasts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all forecasts");
            return StatusCode(500, "An error occurred while fetching forecasts.");
        }
    }

    [HttpGet("GetForecastById/{forecastId}")]
    public async Task<IActionResult> GetForecastById(int forecastId)
    {
        _logger.LogInformation("GetForecastById called with ID {ForecastId}", forecastId);
        try
        {
            var forecast = await _pl_ForecastService.GetByIdAsync(forecastId);
            if (forecast == null)
                return NotFound($"Forecast with ID {forecastId} not found.");

            return Ok(forecast);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get forecast with ID {ForecastId}", forecastId);
            return StatusCode(500, "An error occurred while fetching the forecast.");
        }
    }

    [HttpDelete("DeleteForecastById/{forecastId}")]
    public async Task<IActionResult> DeleteForecastById(int forecastId)
    {
        _logger.LogInformation("DeleteForecastById called with ID {ForecastId}", forecastId);
        try
        {
            await _pl_ForecastService.DeleteAsync(forecastId);
            return Ok("Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete forecast with ID {ForecastId}", forecastId);
            return StatusCode(500, "An error occurred while deleting the forecast.");
        }
    }

    [HttpPut("UpdateForecastById")]
    public async Task<IActionResult> UpdateForecastById([FromBody] PlForecast plForecast)
    {
        _logger.LogInformation("UpdateForecastById called for forecast ID {ForecastId}", plForecast?.Forecastid);
        try
        {
            await _pl_ForecastService.UpdateAsync(plForecast);
            return Ok("Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update forecast ID {ForecastId}", plForecast?.Forecastid);
            return StatusCode(500, "An error occurred while updating the forecast.");
        }
    }

    [HttpPut("UpdateForecastAmount")]
    public async Task<IActionResult> UpdateForecastAmount([FromBody] PlForecast plForecast)
    {
        _logger.LogInformation("UpdateForecastAmount called for forecast ID {ForecastId}", plForecast?.Forecastid);
        try
        {
            await _pl_ForecastService.UpdateAmountAsync(plForecast);
            return Ok("Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update forecast amount for ID {ForecastId}", plForecast?.Forecastid);
            return StatusCode(500, "An error occurred while updating the forecast amount.");
        }
    }

    [HttpPut("UpdateForecastHours")]
    public async Task<IActionResult> UpdateForecastHours([FromBody] PlForecast plForecast)
    {
        _logger.LogInformation("UpdateForecastHours called for forecast ID {ForecastId}", plForecast?.Forecastid);
        try
        {
            await _pl_ForecastService.UpdateHoursAsync(plForecast);
            return Ok("Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update forecast hours for ID {ForecastId}", plForecast?.Forecastid);
            return StatusCode(500, "An error occurred while updating the forecast hours.");
        }
    }

    [HttpPut("UpdateForecastAmount/{type}")]
    public async Task<IActionResult> UpdateForecastAmount([FromBody] PlForecast plForecast, string type)
    {
        _logger.LogInformation("UpdateForecastAmount called for forecast ID {ForecastId}", plForecast?.Forecastid);
        try
        {
            await _pl_ForecastService.UpdateAmountAsync(plForecast, type);
            return Ok("Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update forecast amount for ID {ForecastId}", plForecast?.Forecastid);
            return StatusCode(500, "An error occurred while updating the forecast amount.");
        }
    }

    [HttpPut("BulkUpdateForecastAmount/{type}")]
    public async Task<IActionResult> BulkUpdateForecastAmount([FromBody] List<PlForecast> plForecast, string type)
    {
        _logger.LogInformation("UpdateForecastAmount called for forecast");
        try
        {
            await _pl_ForecastService.UpdateAmountAsync(plForecast, type);
            return Ok("Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update forecast amount");
            return StatusCode(500, "An error occurred while updating the forecast amount.");
        }
    }

    [HttpPut("BulkUpdateForecastAmountV1/{type}")]
    public async Task<IActionResult> BulkUpdateForecastAmountV1([FromBody] List<PlForecast> plForecast, int plid, int templateid, string type)
    {
        _logger.LogInformation("UpdateForecastAmount called for forecast");
        try
        {
            await _pl_ForecastService.UpdateAmountAsync(plForecast, type);
            await _pl_ForecastService.CalculateRevenueCost(plid, templateid, type);
            //await _taskQueue.QueueBackgroundWorkItemAsync(async (sp, ct) =>
            //{
            //    await _pl_ForecastService.CalculateRevenueCost(plid, templateid, type);
            //});

            return Ok("Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update forecast amount");
            return StatusCode(500, "An error occurred while updating the forecast amount.");
        }
    }

    [HttpPut("UpdateForecastHours/{type}")]
    public async Task<IActionResult> UpdateForecastHours([FromBody] PlForecast plForecast, string type)
    {
        _logger.LogInformation("UpdateForecastHours called for forecast ID {ForecastId}", plForecast?.Forecastid);
        try
        {
            await _pl_ForecastService.UpdateHoursAsync(plForecast, type);
            return Ok("Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update forecast hours for ID {ForecastId}", plForecast?.Forecastid);
            return StatusCode(500, "An error occurred while updating the forecast hours.");
        }
    }

    [HttpPut("BulkUpdateForecastHours/{type}")]
    public async Task<IActionResult> BulkUpdateForecastHours([FromBody] List<PlForecast> plForecast, string type)
    {
        _logger.LogInformation("UpdateForecastHours called");
        try
        {
            await _pl_ForecastService.UpdateHoursAsync(plForecast, type);
            return Ok("Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update forecast hours");
            return StatusCode(500, "An error occurred while updating the forecast hours.");
        }
    }

    [HttpPut("BulkUpdateForecastHoursV1/{type}")]
    public async Task<IActionResult> BulkUpdateForecastHours([FromBody] List<PlForecast> plForecast, int plid, int templateid, string type)
    {
        _logger.LogInformation("UpdateForecastHours called");
        try
        {
            await _pl_ForecastService.UpdateHoursAsync(plForecast, type);
            try
            {
                await _pl_ForecastService.CalculateRevenueCost(plid, templateid, type);
            }
            catch (Exception ex)
            {
                //logger.LogError(ex, "Error Calculation Revenue for {PlanId}", result.PlId);
            }

            return Ok("Success");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update forecast hours");
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("ExportPlan")]
    public async Task<IActionResult> ExportPlan(string projId, int version, string type)
    {
        _logger.LogInformation("ExportPlan called");
        ScheduleHelper helper = new ScheduleHelper();
        try
        {


            var forecasts = await _pl_ForecastService.GetForecastByProjectIDAndVersion(projId, version, type);

            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("Data");

            IRow budgetInfo = sheet.CreateRow(0);
            budgetInfo.CreateCell(0).SetCellValue("Project - ");
            budgetInfo.CreateCell(1).SetCellValue(projId);
            budgetInfo.CreateCell(2).SetCellValue("Type - ");
            budgetInfo.CreateCell(3).SetCellValue(type);
            budgetInfo.CreateCell(4).SetCellValue("Version - ");
            budgetInfo.CreateCell(5).SetCellValue(version);

            var months = helper.GetMonthsBetween(forecasts.FirstOrDefault(p => p.ProjId == projId).Proj.ProjStartDt.GetValueOrDefault(), forecasts.FirstOrDefault(p => p.ProjId == projId).Proj.ProjEndDt.GetValueOrDefault());

            {
                var projectPlans = forecasts
                                .Select(f => f.PlId).Distinct()
                                .ToList();

                string[] baseHeaders = { "Project_ID", "ID_Type", "ID", "Pool_Org_ID", "Account_ID", "GLC/PLC", "Hourly_Rate", "Burden", "Revenue" };
                List<string> headers = new List<string>(baseHeaders);

                // Append dynamic headers
                foreach (var (year, month) in months)
                {
                    var dateTime = new DateTime(year, month, 1);

                    headers.Add($"{dateTime.ToString("MMM").Replace("Sept", "Sep")} {year}");
                }


                {

                    var forecastsForPlIdTest = forecasts
                          .Select(f => new
                          {
                              ProjId = f.ProjId,
                              EmplId = f.EmplId,
                              Type = f.Empl.Type,
                              PlanId = f.PlId,
                              OrgId = f.Proj?.OrgId ?? string.Empty,
                              AccId = f.Empl?.AccId ?? string.Empty,
                              PlcGlcCode = f.Empl?.PlcGlcCode ?? string.Empty,
                              PerHourRate = f.Empl?.PerHourRate ?? 0,
                              IsBrd = f.Empl?.IsBrd == true ? "TRUE" : "FALSE",
                              Revenue = f.Empl?.IsBrd == true ? "TRUE" : "FALSE"
                          }).Distinct()
                          .ToList();
                    // Header

                    foreach (var (year, month) in months)
                    {
                        headers.Append($"Year: {year}, Month: {month}");
                    }
                    IRow headerRow = sheet.CreateRow(1);
                    for (int i = 0; i < headers.Count; i++)
                    {
                        headerRow.CreateCell(i).SetCellValue(headers[i]);
                    }

                    var distinctPlIds = forecasts.Select(f => f.PlId).Distinct();

                    int rowIndex = 2;

                    foreach (var forecast in forecastsForPlIdTest)
                    {
                        IRow row = sheet.CreateRow(rowIndex++);
                        row.CreateCell(0).SetCellValue(forecast.ProjId);
                        row.CreateCell(1).SetCellValue(forecast.Type);
                        row.CreateCell(2).SetCellValue(forecast.EmplId);
                        row.CreateCell(3).SetCellValue(forecast.OrgId);
                        row.CreateCell(4).SetCellValue(forecast.AccId);
                        row.CreateCell(5).SetCellValue(forecast.PlcGlcCode);
                        row.CreateCell(6).SetCellValue(forecast.PerHourRate.ToString());
                        row.CreateCell(7).SetCellValue(forecast.IsBrd);
                        row.CreateCell(8).SetCellValue(forecast.Revenue);

                        var hours = forecasts
                                    .Where(p => p.EmplId == forecast.EmplId && p.PlId == forecast.PlanId)
                                    .OrderBy(p => p.Year)
                                    .ThenBy(p => p.Month)
                                    .ToList();
                        for (int i = 0; i < hours.Count; i++)
                        {
                            row.CreateCell(8 + i + 1).SetCellValue(hours[i].Forecastedhours.ToString());
                        }

                    }
                }

            }

            using var stream = new MemoryStream();
            workbook.Write(stream);
            var content = stream.ToArray();

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ExportedData.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export plan");
            return StatusCode(500, "An error occurred while exporting the plan." + ex.InnerException.Message);
        }
    }


    [HttpGet("ExportPlanDirectCostV1")]
    public async Task<IActionResult> ExportPlanDirectCostV1(string projId, int version, string type)
    {
        _logger.LogInformation("ExportPlan called");
        ScheduleHelper helper = new ScheduleHelper();
        PlProject project = new PlProject();
        try
        {
            IWorkbook workbook = new XSSFWorkbook();

            //var plan = _context.PlProjectPlans.Where(p => p.ProjId == projId && p.Version == version && p.PlType.ToUpper() == type.ToUpper()).Include(p => p.Proj).FirstOrDefault();
            var plan = _context.PlProjectPlans.Where(p => p.ProjId == projId && p.Version == version && p.PlType.ToUpper() == type.ToUpper()).Include(p => p.Proj).FirstOrDefault();
            var Allforecasts = _context.PlForecasts.Where(p => p.PlId == plan.PlId && p.ProjId == plan.ProjId).Include(p => p.Emple).Include(p => p.DirectCost).ToList();
            //var Allforecasts = await _pl_ForecastService.GetForecastByProjectIDAndVersion(projId, version, type);

            var forecasts = Allforecasts.Where(p => p.Emple != null).ToList();

            ISheet sheet = workbook.CreateSheet("Hours");

            IRow budgetInfo = sheet.CreateRow(0);
            budgetInfo.CreateCell(0).SetCellValue("Project - ");
            budgetInfo.CreateCell(1).SetCellValue(projId);
            budgetInfo.CreateCell(2).SetCellValue("Type - ");
            budgetInfo.CreateCell(3).SetCellValue(type);
            budgetInfo.CreateCell(4).SetCellValue("Version - ");
            budgetInfo.CreateCell(5).SetCellValue(version);
            List<(int Year, int Month)> months = new List<(int Year, int Month)>();

            DateTime projectStartDate = DateTime.MinValue, projectEndDate = DateTime.MaxValue;
            if (Allforecasts.Count > 0)
            {
                if (plan.Proj != null)
                {
                    project = plan.Proj;
                    projectEndDate = project.ProjEndDt.GetValueOrDefault().ToDateTime(TimeOnly.MaxValue);
                    projectStartDate = project.ProjStartDt.GetValueOrDefault().ToDateTime(TimeOnly.MinValue);
                    projectEndDate = plan.ProjEndDt.GetValueOrDefault().ToDateTime(TimeOnly.MaxValue);
                    projectStartDate = plan.ProjStartDt.GetValueOrDefault().ToDateTime(TimeOnly.MinValue);
                }
                else
                {
                    var NBBud = _context.NewBusinessBudgets.FirstOrDefault(p => p.BusinessBudgetId == projId);
                    if (NBBud != null)
                    {
                        project.ProjId = NBBud.BusinessBudgetId;
                        projectEndDate = NBBud.EndDate;
                        projectStartDate = NBBud.StartDate;
                    }
                }
                months = helper.GetMonthsBetween(DateOnly.FromDateTime(projectStartDate), DateOnly.FromDateTime(projectEndDate));
            }

            var projectPlans = forecasts
                            .Select(f => f.PlId).Distinct()
                            .ToList();

            string[] baseHeaders = { "Project_ID", "ID_Type", "ID", "Pool_Org_ID", "Account_ID", "PLC", "Hourly_Rate", "Burden", "Revenue" };
            List<string> headers = new List<string>(baseHeaders);

            // Append dynamic headers
            foreach (var (year, month) in months)
            {
                var dateTime = new DateTime(year, month, 1);

                headers.Add($"{dateTime.ToString("MMM").Replace("Sept", "Sep")} {year}");
            }
            var forecastsForPlIdTest = forecasts
                  .Select(f => new
                  {
                      ProjId = f.ProjId,
                      //EmplId = f.EmplId,
                      EmplId = f.Emple?.EmplId ?? string.Empty,
                      Type = f.Emple.Type,
                      PlanId = f.PlId,
                      OrgId = f.Emple?.OrgId ?? string.Empty,
                      AccId = f.Emple?.AccId ?? string.Empty,
                      PlcGlcCode = f.Emple?.PlcGlcCode ?? string.Empty,
                      PerHourRate = f.Emple?.PerHourRate ?? 0,
                      IsBrd = f.Emple?.IsBrd == true ? "TRUE" : "FALSE",
                      Revenue = f.Emple?.IsBrd == true ? "TRUE" : "FALSE"
                  }).Distinct()
                  .ToList();
            // Header

            foreach (var (year, month) in months)
            {
                headers.Append($"Year: {year}, Month: {month}");
            }
            IRow headerRow = sheet.CreateRow(1);
            for (int i = 0; i < headers.Count; i++)
            {
                headerRow.CreateCell(i).SetCellValue(headers[i]);
            }

            var distinctPlIds = forecasts.Select(f => f.PlId).Distinct();

            int rowIndex = 2;

            foreach (var forecast in forecastsForPlIdTest)
            {
                IRow row = sheet.CreateRow(rowIndex++);
                row.CreateCell(0).SetCellValue(forecast.ProjId);
                row.CreateCell(1).SetCellValue(forecast.Type);
                row.CreateCell(2).SetCellValue(forecast.EmplId);
                row.CreateCell(3).SetCellValue(forecast.OrgId);
                row.CreateCell(4).SetCellValue(forecast.AccId);
                row.CreateCell(5).SetCellValue(forecast.PlcGlcCode);
                //row.CreateCell(6).SetCellValue(forecast.PerHourRate.ToString());
                row.CreateCell(6).SetCellValue(Convert.ToDouble(forecast.PerHourRate));
                row.CreateCell(7).SetCellValue(forecast.IsBrd);
                row.CreateCell(8).SetCellValue(forecast.Revenue);

                //var hours = forecasts
                //            .Where(p => p.EmplId == forecast.EmplId && p.PlId == forecast.PlanId && p.OrgId == forecast.OrgId && p.AcctId == forecast.AccId && p.Plc == forecast.PlcGlcCode && p.Month <= project.ProjEndDt.GetValueOrDefault().Month && p.Year <= project.ProjEndDt.GetValueOrDefault().Year)
                //            .OrderBy(p => p.Year)
                //            .ThenBy(p => p.Month)
                //            .ToList();

                if (forecast.EmplId == "0016")
                {

                }

                //var hours = forecasts
                //    .Where(p =>
                //        p.EmplId == forecast.EmplId &&
                //         p.PlId == forecast.PlanId &&
                //         p.OrgId == forecast.OrgId &&
                //         p.AcctId == forecast.AccId &&
                //         string.Equals(Convert.ToString(p.Plc), forecast.PlcGlcCode, StringComparison.OrdinalIgnoreCase) &&
                //         new DateTime(p.Year, p.Month, 1) <= projectEndDate &&
                //         new DateTime(p.Year, p.Month, 1) >= projectStartDate
                //    )
                //    .OrderBy(p => p.Year)
                //    .ThenBy(p => p.Month)
                //    .ToList();
                var projectStartKey = projectStartDate.Year * 100 + projectStartDate.Month;
                var projectEndKey = projectEndDate.Year * 100 + projectEndDate.Month;
                //var hours = forecasts
                //            .Where(p =>
                //                p.EmplId == forecast.EmplId &&
                //                p.PlId == forecast.PlanId &&
                //                p.OrgId == forecast.OrgId &&
                //                p.AcctId == forecast.AccId &&
                //                // Only compare Plc if both are not null/empty
                //                (string.IsNullOrWhiteSpace(Convert.ToString(p.Plc)) || string.IsNullOrWhiteSpace(forecast.PlcGlcCode)
                //                    || string.Equals(Convert.ToString(p.Plc), forecast.PlcGlcCode, StringComparison.OrdinalIgnoreCase)) &&
                //                new DateTime(p.Year, p.Month, DateTime.DaysInMonth(p.Year, p.Month)) <= projectEndDate &&
                //                new DateTime(p.Year, p.Month, 1) >= projectStartDate
                //            )
                //            .OrderBy(p => p.Year)
                //            .ThenBy(p => p.Month)
                //            .ToList();

                //var hours = forecasts
                //            .Where(p =>
                //                p.EmplId == forecast.EmplId &&
                //                p.PlId == forecast.PlanId &&
                //                p.OrgId == forecast.OrgId &&
                //                p.AcctId == forecast.AccId &&
                //                (string.IsNullOrWhiteSpace(Convert.ToString(p.Plc)) ||
                //                 string.IsNullOrWhiteSpace(forecast.PlcGlcCode) ||
                //                 string.Equals(Convert.ToString(p.Plc), forecast.PlcGlcCode, StringComparison.OrdinalIgnoreCase)) &&
                //                (p.Year * 100 + p.Month) >= projectStartKey &&
                //                (p.Year * 100 + p.Month) <= projectEndKey
                //            )
                //            .OrderBy(p => p.Year)
                //            .ThenBy(p => p.Month)
                //            .ToList();

                var hours = forecasts
                            .Where(p =>
                                p.EmplId == forecast.EmplId &&
                                p.PlId == forecast.PlanId &&
                                p.OrgId == forecast.OrgId &&
                                p.AcctId == forecast.AccId &&
                                p.Plc == forecast.PlcGlcCode &&
                                (p.Year * 100 + p.Month) >= projectStartKey &&
                                (p.Year * 100 + p.Month) <= projectEndKey
                            )
                            .OrderBy(p => p.Year)
                            .ThenBy(p => p.Month)
                            .ToList();

                for (int i = 0; i < hours.Count; i++)
                {
                    if (type.ToUpper() == "EAC")
                    {
                        if (i > 45)
                        {

                        }
                        //row.CreateCell(8 + i + 1).SetCellValue(hours[i].Actualhours.ToString());

                        row.CreateCell(8 + i + 1).SetCellValue(Convert.ToDouble(hours[i].Actualhours));
                    }
                    else
                    {
                        //row.CreateCell(8 + i + 1).SetCellValue(hours[i].Forecastedhours.ToString());
                        row.CreateCell(8 + i + 1).SetCellValue(Convert.ToDouble(hours[i].Forecastedhours));

                    }
                }

            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////////
            var DirectCostforecasts = Allforecasts.Where(p => p.DirectCost != null).ToList();

            ISheet DirectCostSheet = workbook.CreateSheet("Direct Cost");

            IRow budgetDirectCostInfo = DirectCostSheet.CreateRow(0);
            budgetDirectCostInfo.CreateCell(0).SetCellValue("Project - ");
            budgetDirectCostInfo.CreateCell(1).SetCellValue(projId);
            budgetDirectCostInfo.CreateCell(2).SetCellValue("Type - ");
            budgetDirectCostInfo.CreateCell(3).SetCellValue(type);
            budgetDirectCostInfo.CreateCell(4).SetCellValue("Version - ");
            budgetDirectCostInfo.CreateCell(5).SetCellValue(version);

            List<(int Year, int Month)> DirectCostMonths = new List<(int Year, int Month)>();

            if (DirectCostforecasts.Count > 0)
                DirectCostMonths = helper.GetMonthsBetween(DateOnly.FromDateTime(projectStartDate), DateOnly.FromDateTime(projectEndDate));

            //DirectCostMonths = helper.GetMonthsBetween(DirectCostforecasts.FirstOrDefault(p => p.ProjId == projId).Proj.ProjStartDt.GetValueOrDefault(), DirectCostforecasts.FirstOrDefault(p => p.ProjId == projId).Proj.ProjEndDt.GetValueOrDefault());
            //var projectPlans = Allforecasts
            //                .Select(f => f.PlId).Distinct()
            //                .ToList();

            string[] baseHeadersDirectCost = { "Project_ID", "Type", "Dct_ID", "Org_ID", "Account_ID", "PLC", "EMPL_ID", "Burden", "Revenue" };
            List<string> headersDirectCost = new List<string>(baseHeadersDirectCost);

            // Append dynamic headers
            foreach (var (year, month) in months)
            {
                var dateTime = new DateTime(year, month, 1);

                headersDirectCost.Add($"{dateTime.ToString("MMM").Replace("Sept", "Sep")} {year}");
            }


            var forecastsForDirectCostPlId = DirectCostforecasts
                  .Select(f => new
                  {
                      ProjId = f.ProjId,
                      dctId = f.DctId,
                      Type = f.DirectCost?.Type,
                      PlanId = f.PlId,
                      OrgId = f.DirectCost?.OrgId ?? string.Empty,
                      AccId = f.DirectCost?.AcctId ?? string.Empty,
                      PlcGlcCode = f.DirectCost?.PlcGlc ?? string.Empty,
                      EmplID = f.DirectCost?.Id ?? string.Empty,
                      IsBrd = f.DirectCost?.IsBrd == true ? "TRUE" : "FALSE",
                      Revenue = f.DirectCost?.IsBrd == true ? "TRUE" : "FALSE"
                  }).Distinct()
                  .ToList();

            foreach (var (year, month) in DirectCostMonths)
            {
                headersDirectCost.Append($"Year: {year}, Month: {month}");
            }
            IRow DirectCostheaderRow = DirectCostSheet.CreateRow(1);
            for (int i = 0; i < headersDirectCost.Count; i++)
            {
                DirectCostheaderRow.CreateCell(i).SetCellValue(headersDirectCost[i]);
            }

            distinctPlIds = DirectCostforecasts.Select(f => f.PlId).Distinct();

            rowIndex = 2;

            foreach (var forecast in forecastsForDirectCostPlId)
            {
                IRow row = DirectCostSheet.CreateRow(rowIndex++);
                row.CreateCell(0).SetCellValue(forecast.ProjId);
                row.CreateCell(1).SetCellValue(forecast.Type);
                row.CreateCell(2).SetCellValue(forecast.dctId.GetValueOrDefault());
                row.CreateCell(3).SetCellValue(forecast.OrgId);
                row.CreateCell(4).SetCellValue(forecast.AccId);
                row.CreateCell(5).SetCellValue(forecast.PlcGlcCode);
                row.CreateCell(6).SetCellValue(forecast.EmplID);
                row.CreateCell(7).SetCellValue(forecast.IsBrd);
                row.CreateCell(8).SetCellValue(forecast.Revenue);

                var hours = DirectCostforecasts
                            .Where(p => p.DctId == forecast.dctId && p.PlId == forecast.PlanId && new DateTime(p.Year, p.Month, DateTime.DaysInMonth(p.Year, p.Month)) <= projectEndDate &&
                         new DateTime(p.Year, p.Month, 1) >= projectStartDate)
                            .OrderBy(p => p.Year)
                            .ThenBy(p => p.Month)
                            .ToList();

                int i = 0;
                foreach (var (year, month) in DirectCostMonths)
                {
                    if (type.ToUpper() == "EAC")
                    {
                        //row.CreateCell(8 + i + 1).SetCellValue(hours.FirstOrDefault(p => p.Month == month && p.Year == year)?.Actualamt.ToString());

                        row.CreateCell(8 + i + 1).SetCellValue(Convert.ToDouble(hours.FirstOrDefault(p => p.Month == month && p.Year == year)?.Actualamt));

                    }
                    else
                    {
                        //row.CreateCell(8 + i + 1).SetCellValue(hours.FirstOrDefault(p => p.Month == month && p.Year == year)?.Forecastedamt.ToString());
                        row.CreateCell(8 + i + 1).SetCellValue(Convert.ToDouble(hours.FirstOrDefault(p => p.Month == month && p.Year == year)?.Forecastedamt));
                    }

                    i++;
                }


            }
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            using var stream = new MemoryStream();
            workbook.Write(stream);
            var content = stream.ToArray();

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ExportedData.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export plan");
            return StatusCode(500, "An error occurred while exporting the plan." + ex.InnerException.Message);
        }
    }

    [HttpGet("ExportPlanDirectCost")]
    public async Task<IActionResult> ExportPlanDirectCost(string projId, int version, string type)
    {
        _logger.LogInformation("ExportPlan called");
        ScheduleHelper helper = new ScheduleHelper();
        PlProject project = new PlProject();
        try
        {
            IWorkbook workbook = new XSSFWorkbook();

            var plan = _context.PlProjectPlans.Where(p => p.ProjId == projId && p.Version == version && p.PlType.ToUpper() == type.ToUpper()).Include(p => p.Proj).FirstOrDefault();
            var Allforecasts = _context.PlForecasts.Where(p => p.PlId == plan.PlId && p.ProjId == plan.ProjId).Include(p => p.Emple).Include(p => p.DirectCost).ToList();

            var forecasts = Allforecasts.Where(p => p.Emple != null).ToList();

            ISheet sheet = workbook.CreateSheet("Hours");

            IRow budgetInfo = sheet.CreateRow(0);
            budgetInfo.CreateCell(0).SetCellValue("Project - ");
            budgetInfo.CreateCell(1).SetCellValue(projId);
            budgetInfo.CreateCell(2).SetCellValue("Type - ");
            budgetInfo.CreateCell(3).SetCellValue(type);
            budgetInfo.CreateCell(4).SetCellValue("Version - ");
            budgetInfo.CreateCell(5).SetCellValue(version);
            List<(int Year, int Month)> months = new List<(int Year, int Month)>();

            DateTime projectStartDate = DateTime.MinValue, projectEndDate = DateTime.MaxValue;
            if (Allforecasts.Count > 0)
            {
                if (plan.Proj != null)
                {
                    project = plan.Proj;
                    projectEndDate = project.ProjEndDt.GetValueOrDefault().ToDateTime(TimeOnly.MaxValue);
                    projectStartDate = project.ProjStartDt.GetValueOrDefault().ToDateTime(TimeOnly.MinValue);
                    projectEndDate = plan.ProjEndDt.GetValueOrDefault().ToDateTime(TimeOnly.MaxValue);
                    projectStartDate = plan.ProjStartDt.GetValueOrDefault().ToDateTime(TimeOnly.MinValue);
                }
                else
                {
                    var NBBud = _context.NewBusinessBudgets.FirstOrDefault(p => p.BusinessBudgetId == projId);
                    if (NBBud != null)
                    {
                        project.ProjId = NBBud.BusinessBudgetId;
                        projectEndDate = NBBud.EndDate;
                        projectStartDate = NBBud.StartDate;
                    }
                }
                months = helper.GetMonthsBetween(DateOnly.FromDateTime(projectStartDate), DateOnly.FromDateTime(projectEndDate));
            }

            var projectPlans = forecasts
                            .Select(f => f.PlId).Distinct()
                            .ToList();

            string[] baseHeaders = { "Project_ID", "ID_Type", "ID", "Pool_Org_ID", "Account_ID", "PLC", "Hourly_Rate", "Burden", "Revenue" };
            List<string> headers = new List<string>(baseHeaders);

            // Append dynamic headers
            foreach (var (year, month) in months)
            {
                var dateTime = new DateTime(year, month, 1);

                headers.Add($"{dateTime.ToString("MMM").Replace("Sept", "Sep")} {year}");
            }
            var forecastsForPlIdTest = forecasts
                  .Select(f => new
                  {
                      ProjId = f.ProjId,
                      //EmplId = f.EmplId,
                      EmplId = f.Emple?.EmplId ?? string.Empty,
                      Type = f.Emple.Type,
                      PlanId = f.PlId,
                      OrgId = f.Emple?.OrgId ?? string.Empty,
                      AccId = f.Emple?.AccId ?? string.Empty,
                      PlcGlcCode = f.Emple?.PlcGlcCode ?? string.Empty,
                      PerHourRate = f.Emple?.PerHourRate ?? 0,
                      IsBrd = f.Emple?.IsBrd == true ? "TRUE" : "FALSE",
                      Revenue = f.Emple?.IsBrd == true ? "TRUE" : "FALSE"
                  }).Distinct()
                  .ToList();
            // Header

            foreach (var (year, month) in months)
            {
                headers.Append($"Year: {year}, Month: {month}");
            }
            IRow headerRow = sheet.CreateRow(1);
            for (int i = 0; i < headers.Count; i++)
            {
                headerRow.CreateCell(i).SetCellValue(headers[i]);
            }

            var distinctPlIds = forecasts.Select(f => f.PlId).Distinct();

            int rowIndex = 2;

            foreach (var forecast in forecastsForPlIdTest)
            {
                IRow row = sheet.CreateRow(rowIndex++);
                row.CreateCell(0).SetCellValue(forecast.ProjId);
                row.CreateCell(1).SetCellValue(forecast.Type);
                row.CreateCell(2).SetCellValue(forecast.EmplId);
                row.CreateCell(3).SetCellValue(forecast.OrgId);
                row.CreateCell(4).SetCellValue(forecast.AccId);
                row.CreateCell(5).SetCellValue(forecast.PlcGlcCode);
                row.CreateCell(6).SetCellValue(Convert.ToDouble(forecast.PerHourRate));
                row.CreateCell(7).SetCellValue(forecast.IsBrd);
                row.CreateCell(8).SetCellValue(forecast.Revenue);

                if (forecast.EmplId == "0016")
                {

                }
                var projectStartKey = projectStartDate.Year * 100 + projectStartDate.Month;
                var projectEndKey = projectEndDate.Year * 100 + projectEndDate.Month;

                var hours = forecasts
                    .Where(p =>
                        p.EmplId == forecast.EmplId &&
                        p.PlId == forecast.PlanId &&
                        p.OrgId == forecast.OrgId &&
                        p.AcctId == forecast.AccId &&
                        (
                            p.Plc == forecast.PlcGlcCode ||
                            (string.IsNullOrEmpty(p.Plc) && string.IsNullOrEmpty(forecast.PlcGlcCode))
                        ) &&
                        (p.Year * 100 + p.Month) >= projectStartKey &&
                        (p.Year * 100 + p.Month) <= projectEndKey
                    )
                    .OrderBy(p => p.Year)
                    .ThenBy(p => p.Month)
                    .ToList();
                //var hours = forecasts
                //            .Where(p =>
                //                p.EmplId == forecast.EmplId &&
                //                p.PlId == forecast.PlanId &&
                //                p.OrgId == forecast.OrgId &&
                //                p.AcctId == forecast.AccId &&
                //                //p.Plc == forecast.PlcGlcCode &&
                //                (p.Year * 100 + p.Month) >= projectStartKey &&
                //                (p.Year * 100 + p.Month) <= projectEndKey
                //            )
                //            .OrderBy(p => p.Year)
                //            .ThenBy(p => p.Month)
                //            .ToList();

                //for (int i = 0; i < hours.Count; i++)
                //{
                //    if (type.ToUpper() == "EAC")
                //    {
                //        if (i > 45)
                //        {

                //        }
                //        row.CreateCell(8 + i + 1).SetCellValue(Convert.ToDouble(hours[i].Actualhours));
                //    }
                //    else
                //    {
                //        row.CreateCell(8 + i + 1).SetCellValue(Convert.ToDouble(hours[i].Forecastedhours));
                //    }
                //}

                int i = 0;

                foreach (var (year, month) in months)
                {
                    var matchingHour = hours
                        .FirstOrDefault(h => h.Year == year && h.Month == month);

                    double value = 0;

                    if (matchingHour != null)
                    {
                        value = type.ToUpper() == "EAC"
                            ? Convert.ToDouble(matchingHour.Actualhours)
                            : Convert.ToDouble(matchingHour.Forecastedhours);
                    }

                    row.CreateCell(9 + i).SetCellValue(value);

                    i++;
                }
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////////
            var DirectCostforecasts = Allforecasts.Where(p => p.DirectCost != null).ToList();

            ISheet DirectCostSheet = workbook.CreateSheet("Direct Cost");

            IRow budgetDirectCostInfo = DirectCostSheet.CreateRow(0);
            budgetDirectCostInfo.CreateCell(0).SetCellValue("Project - ");
            budgetDirectCostInfo.CreateCell(1).SetCellValue(projId);
            budgetDirectCostInfo.CreateCell(2).SetCellValue("Type - ");
            budgetDirectCostInfo.CreateCell(3).SetCellValue(type);
            budgetDirectCostInfo.CreateCell(4).SetCellValue("Version - ");
            budgetDirectCostInfo.CreateCell(5).SetCellValue(version);

            List<(int Year, int Month)> DirectCostMonths = new List<(int Year, int Month)>();

            if (DirectCostforecasts.Count > 0)
                DirectCostMonths = helper.GetMonthsBetween(DateOnly.FromDateTime(projectStartDate), DateOnly.FromDateTime(projectEndDate));

            string[] baseHeadersDirectCost = { "Project_ID", "Type", "Dct_ID", "Org_ID", "Account_ID", "PLC", "EMPL_ID", "Burden", "Revenue" };
            List<string> headersDirectCost = new List<string>(baseHeadersDirectCost);

            // Append dynamic headers
            foreach (var (year, month) in months)
            {
                var dateTime = new DateTime(year, month, 1);

                headersDirectCost.Add($"{dateTime.ToString("MMM").Replace("Sept", "Sep")} {year}");
            }

            var forecastsForDirectCostPlId = DirectCostforecasts
                  .Select(f => new
                  {
                      ProjId = f.ProjId,
                      dctId = f.DctId,
                      Type = f.DirectCost?.Type,
                      PlanId = f.PlId,
                      OrgId = f.DirectCost?.OrgId ?? string.Empty,
                      AccId = f.DirectCost?.AcctId ?? string.Empty,
                      PlcGlcCode = f.DirectCost?.PlcGlc ?? string.Empty,
                      EmplID = f.DirectCost?.Id ?? string.Empty,
                      IsBrd = f.DirectCost?.IsBrd == true ? "TRUE" : "FALSE",
                      Revenue = f.DirectCost?.IsBrd == true ? "TRUE" : "FALSE"
                  }).Distinct()
                  .ToList();

            foreach (var (year, month) in DirectCostMonths)
            {
                headersDirectCost.Append($"Year: {year}, Month: {month}");
            }
            IRow DirectCostheaderRow = DirectCostSheet.CreateRow(1);
            for (int i = 0; i < headersDirectCost.Count; i++)
            {
                DirectCostheaderRow.CreateCell(i).SetCellValue(headersDirectCost[i]);
            }

            distinctPlIds = DirectCostforecasts.Select(f => f.PlId).Distinct();

            rowIndex = 2;

            foreach (var forecast in forecastsForDirectCostPlId)
            {
                IRow row = DirectCostSheet.CreateRow(rowIndex++);
                row.CreateCell(0).SetCellValue(forecast.ProjId);
                row.CreateCell(1).SetCellValue(forecast.Type);
                row.CreateCell(2).SetCellValue(forecast.dctId.GetValueOrDefault());
                row.CreateCell(3).SetCellValue(forecast.OrgId);
                row.CreateCell(4).SetCellValue(forecast.AccId);
                row.CreateCell(5).SetCellValue(forecast.PlcGlcCode);
                row.CreateCell(6).SetCellValue(forecast.EmplID);
                row.CreateCell(7).SetCellValue(forecast.IsBrd);
                row.CreateCell(8).SetCellValue(forecast.Revenue);

                var hours = DirectCostforecasts
                            .Where(p => p.DctId == forecast.dctId && p.PlId == forecast.PlanId && new DateTime(p.Year, p.Month, DateTime.DaysInMonth(p.Year, p.Month)) <= projectEndDate &&
                         new DateTime(p.Year, p.Month, 1) >= projectStartDate)
                            .OrderBy(p => p.Year)
                            .ThenBy(p => p.Month)
                            .ToList();

                int i = 0;
                foreach (var (year, month) in DirectCostMonths)
                {
                    if (type.ToUpper() == "EAC")
                    {
                        row.CreateCell(8 + i + 1).SetCellValue(Convert.ToDouble(hours.FirstOrDefault(p => p.Month == month && p.Year == year)?.Actualamt));
                    }
                    else
                    {
                        row.CreateCell(8 + i + 1).SetCellValue(Convert.ToDouble(hours.FirstOrDefault(p => p.Month == month && p.Year == year)?.Forecastedamt));
                    }

                    i++;
                }
            }
            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            using var stream = new MemoryStream();
            workbook.Write(stream);
            var content = stream.ToArray();

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ExportedData.xlsx");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export plan");
            return StatusCode(500, "An error occurred while exporting the plan." + ex.InnerException.Message);
        }
    }

    [HttpPost("ImportDirectCostPlanLastWorking")]
    public async Task<IActionResult> ImportDirectCostPlanLastWorking(IFormFile file)
    {
        bool newImport = false; int closingMonth = 0, closingYear = 0;
        _logger.LogInformation("ImportPlan called");

        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var plForecastData = new List<PlForecast>();

        PlProjectPlan plan = new PlProjectPlan();
        string projId = string.Empty, type = string.Empty, version = string.Empty;
        try
        {
            List<PlEmployeee> plEmployees = new List<PlEmployeee>();
            List<PlDct> plDcts = new List<PlDct>();
            var random = new Random();

            using var stream = file.OpenReadStream();
            var workbook = new XSSFWorkbook(stream);
            var sheet = workbook.GetSheet("Hours");

            var infoRow = sheet.GetRow(0);
            if (infoRow != null)
            {
                projId = infoRow.GetCell(1)?.ToString() ?? string.Empty;
                type = infoRow.GetCell(3)?.ToString() ?? string.Empty;
                version = infoRow.GetCell(5)?.ToString() ?? string.Empty;

            }
            if (string.IsNullOrEmpty(version))
            {
                var proj = _context.PlProjects.FirstOrDefault(p => p.ProjId == projId);
                if (proj == null)
                {
                    return NotFound("Project Not Found - " + projId);
                }

                newImport = true;
                plan = await _projPlanService.AddProjectPlanAsync(new PlProjectPlan
                {
                    TemplateId = 1,
                    ProjId = projId,
                    Status = "In Progress",
                    PlType = type,
                    Type = "A",
                    ProjStartDt = proj.ProjStartDt,
                    ProjEndDt = proj.ProjEndDt,
                    Source = "EXCEL"
                }, "Excel");
                version = plan.Version.ToString();
            }

            plan = _context.PlProjectPlans.Where(p => p.ProjId == projId && p.Version == Convert.ToInt32(version) && p.PlType == type).Include(p => p.Proj).FirstOrDefault();

            if (plan.ClosedPeriod.HasValue)
            {
                closingMonth = plan.ClosedPeriod.Value.Month;
                closingYear = plan.ClosedPeriod.Value.Year;
            }

            if (plan != null)
            {
                if (plan?.Status.ToUpper() != "IN PROGRESS")
                {
                    return StatusCode(500, $"Import failed: Budget status is '{plan.Status}' for Project '{projId}' with Version '{version}'. If you want to Import update status to 'Working'");
                }

                plForecastData = _context.PlForecasts.Where(p => p.PlId == plan.PlId).ToList();
            }
            else
            {
                return StatusCode(500, "An error occurred while importing the plan.");
            }

            var project = plan.Proj;
            //int projectDurationMonths = (project.ProjEndDt.GetValueOrDefault().Year -
            //                project.ProjStartDt.GetValueOrDefault().Year) * 12 +
            //                project.ProjEndDt.GetValueOrDefault().Month -
            //                project.ProjStartDt.GetValueOrDefault().Month + 1;

            int projectDurationMonths = (plan.ProjEndDt.GetValueOrDefault().Year -
                plan.ProjStartDt.GetValueOrDefault().Year) * 12 +
                plan.ProjEndDt.GetValueOrDefault().Month -
                plan.ProjStartDt.GetValueOrDefault().Month + 1;

            var emplPeriod = new Dictionary<string, string>();
            var plForecasts = new List<PlForecast>();

            var headerRow = sheet.GetRow(1);

            //Get EMployee List
            var emplList = _context.PlEmployeees.Where(p => p.PlId == plan.PlId).ToList();
            for (int rowIndex = 2; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                var row = sheet.GetRow(rowIndex);

                if (row.GetCell(2)?.ToString() == "9030910")
                {

                }
                PlEmployeee employee = new PlEmployeee()
                {
                    EmplId = row.GetCell(2)?.ToString() ?? string.Empty,
                    AccId = row.GetCell(4)?.ToString() ?? string.Empty,
                    IsBrd = bool.TryParse(row.GetCell(7)?.ToString(), out bool result) && result,
                    IsRev = bool.TryParse(row.GetCell(8)?.ToString(), out result) && result,
                    PlcGlcCode = row.GetCell(5)?.ToString() ?? string.Empty,
                    OrgId = row.GetCell(3)?.ToString() ?? string.Empty,
                    Type = row.GetCell(1)?.ToString() ?? string.Empty,
                    PlId = plan.PlId,
                    PerHourRate = double.TryParse(row.GetCell(6)?.ToString() ?? string.Empty, out double d) ? (decimal)d : 0m
                };
                //plEmployees.Add(employee);

                var existingEmployee = emplList.Where(p => p.EmplId == employee.EmplId && p.OrgId == employee.OrgId && p.AccId == employee.AccId && p.PlcGlcCode == employee.PlcGlcCode).ToList();

                if (existingEmployee == null || existingEmployee.Count() == 0)
                {
                    string sql1 = "";

                    switch (row.GetCell(1)?.ToString().ToUpper())
                    {

                        case "VENDOREMPLOYEE":
                            employee.Type = "VENDOR EMPLOYEE";
                            sql1 = $@"Select vend_empl_id as EmplId,vend_empl_status as Status, vend_empl_name as FirstName, 0 AS Salary,0 AS PerHourRate, null AS EffectiveDate  
                            from public.vendor_employee where vend_empl_status = 'A' and
                            vend_empl_id = '{employee.EmplId}';";
                            break;
                        case "VENDOR EMPLOYEE":
                            employee.Type = "VENDOR EMPLOYEE";
                            sql1 = $@"Select vend_empl_id as EmplId,vend_empl_status as Status, vend_empl_name as FirstName, 0 AS Salary,0 AS PerHourRate, null AS EffectiveDate  
                            from public.vendor_employee where vend_empl_status = 'A' and
                            vend_empl_id = '{employee.EmplId}';";
                            break;
                        case "VENDOR":
                            employee.Type = "VENDOR";
                            sql1 = $@"Select vend_id as EmplId,vend_empl_status as Status, vend_empl_name as FirstName, 0 AS Salary,0 AS PerHourRate,null AS EffectiveDate  
                            from public.vendor_employee where vend_empl_status = 'A' and
                            vend_id = '{employee.EmplId}';";
                            break;
                        case "OTHER":
                            employee.Type = row.GetCell(1)?.ToString().ToUpper();
                            break;
                        case "EMPLOYEE":
                            employee.Type = "EMPLOYEE";
                            sql1 = $@"
                                SELECT empl.empl_id AS EmplId, 
                                       s_empl_status_cd AS Status, 
                                       last_first_name AS FirstName, 
                                       sal_amt AS Salary,
                                       effect_dt AS EffectiveDate,
                                       hrly_amt AS PerHourRate
                                FROM empl
                                JOIN public.empl_lab_info 
                                    ON empl.empl_id = public.empl_lab_info.empl_id
                                WHERE empl.s_empl_status_cd = 'ACT' and empl.empl_id = '{employee.EmplId}' 
                                  AND public.empl_lab_info.end_dt = '2078-12-31';";
                            break;
                    }

                    if (row.GetCell(1)?.ToString().ToUpper() == "OTHER" || row.GetCell(1)?.ToString().ToUpper() == "PLC")
                    {

                        int number = random.Next(1, 100000); // 1 to 99999

                        if (row.GetCell(1)?.ToString().ToUpper() == "PLC")
                        {
                            employee.EmplId = employee.Type + "_" + number.ToString("D5");
                        }
                        else
                        {
                            employee.EmplId = "TBD_" + number.ToString("D5");
                            employee.FirstName = employee.EmplId;

                        }

                        ////////////////////////////////////////////////////////////////////////////////////////////
                        var entry = _context.PlEmployeees.Add(employee);
                        _context.SaveChanges();
                        employee.Id = entry.Entity.Id;
                    }
                    else
                    {
                        var employeeDetails = _context.Empl_Master
                                            .FromSqlRaw(sql1)
                                            .ToList().FirstOrDefault();

                        if (employeeDetails != null)
                        {
                            if (!string.IsNullOrWhiteSpace(employeeDetails.FirstName))
                            {
                                var names = employeeDetails.FirstName.Split(',', 2);

                                employee.LastName = names[0];
                                employee.FirstName = names.Length > 1 ? names[1] : names[0];
                            }

                            if (employee.Type.ToUpper() == "EMPLOYEE")
                            {
                                employee.Salary = employeeDetails.Salary;
                                employee.PerHourRate = employeeDetails.PerHourRate;
                            }
                            var entry = _context.PlEmployeees.Add(employee);
                            _context.SaveChanges();
                            employee.Id = entry.Entity.Id;
                        }
                        else
                        {
                            //_projPlanService.DeleteProjectPlanAsync(plan.PlId.GetValueOrDefault()).Wait();
                            //_context.PlProjectPlans.Remove(plan);
                            //_context.SaveChanges();
                            throw new Exception("Employee (" + employee.EmplId + ") not found.");
                        }
                    }

                }

                if (employee.Id == 0 && existingEmployee.Count() > 0)
                {
                    employee = existingEmployee.FirstOrDefault();
                }
                //else
                //{
                //    if(employee.Type.ToUpper() == "OTHER")
                //    {
                //        var entry = _context.PlEmployeees.Add(employee);
                //        _context.SaveChanges();
                //        employee.Id = entry.Entity.Id;
                //    }
                //}
                for (int i = 9; i < (9 + projectDurationMonths); i++)
                {
                    try
                    {
                        var period = headerRow.GetCell(i)?.ToString() ?? string.Empty;

                        var cell = row.GetCell(i);
                        decimal forecastedHours = 0;

                        if (cell != null)
                        {
                            switch (cell.CellType)
                            {
                                case NPOI.SS.UserModel.CellType.Numeric:
                                    forecastedHours = (decimal)cell.NumericCellValue;
                                    break;

                                case NPOI.SS.UserModel.CellType.String:
                                    decimal.TryParse(cell.StringCellValue, out forecastedHours);
                                    break;

                                case NPOI.SS.UserModel.CellType.Formula:
                                    if (cell.CachedFormulaResultType == NPOI.SS.UserModel.CellType.Numeric)
                                        forecastedHours = (decimal)cell.NumericCellValue;
                                    else if (cell.CachedFormulaResultType == NPOI.SS.UserModel.CellType.String)
                                        decimal.TryParse(cell.StringCellValue, out forecastedHours);
                                    break;
                            }
                        }


                        DateTime parsedDate = DateTime.ParseExact(period, "MMM yyyy", System.Globalization.CultureInfo.InvariantCulture);
                        if (parsedDate.Month == 8 && parsedDate.Year == 2025)
                        {

                        }
                        if (type.ToUpper() == "EAC")
                        {
                            int month = parsedDate.Month; // 6
                            int year = parsedDate.Year;

                            if (plan.ClosedPeriod < DateOnly.FromDateTime(parsedDate))
                            {

                                if (!string.IsNullOrEmpty(period))
                                {
                                    plForecasts.Add(new PlForecast() { AcctId = employee.AccId, OrgId = employee.OrgId, PlId = plan.PlId.GetValueOrDefault(), empleId = employee.Id, Plc = employee.PlcGlcCode, EmplId = employee.EmplId, ProjId = projId, Year = year, Month = month, Actualhours = Convert.ToDecimal(forecastedHours) });
                                }
                                else
                                {
                                    plForecasts.Add(new PlForecast() { AcctId = employee.AccId, OrgId = employee.OrgId, PlId = plan.PlId.GetValueOrDefault(), empleId = employee.Id, Plc = employee.PlcGlcCode, EmplId = employee.EmplId, ProjId = projId, Year = year, Month = month, Actualhours = Convert.ToDecimal(0) });
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(period))
                                {
                                    plForecasts.Add(new PlForecast() { AcctId = employee.AccId, OrgId = employee.OrgId, PlId = plan.PlId.GetValueOrDefault(), empleId = employee.Id, Plc = employee.PlcGlcCode, EmplId = employee.EmplId, ProjId = projId, Year = year, Month = month, Actualhours = forecastedHours, Forecastedhours = forecastedHours });
                                    //plForecasts.Add(new PlForecast() { PlId = plan.PlId.GetValueOrDefault(), EmplId = employee.EmplId, ProjId = projId, Year = year, Month = month, Forecastedhours = Convert.ToDecimal(row.GetCell(i).ToString()) });
                                }
                                else
                                {
                                    plForecasts.Add(new PlForecast() { AcctId = employee.AccId, OrgId = employee.OrgId, PlId = plan.PlId.GetValueOrDefault(), empleId = employee.Id, Plc = employee.PlcGlcCode, EmplId = employee.EmplId, ProjId = projId, Year = year, Month = month, Actualhours = Convert.ToDecimal(0), Forecastedhours = forecastedHours });
                                    //plForecasts.Add(new PlForecast() { PlId = plan.PlId.GetValueOrDefault(), EmplId = employee.EmplId, ProjId = projId, Year = year, Month = month, Forecastedhours = Convert.ToDecimal(0) });
                                }
                            }
                        }
                        else
                        {
                            int month = parsedDate.Month; // 6
                            int year = parsedDate.Year;

                            if (!string.IsNullOrEmpty(period))
                            {
                                plForecasts.Add(new PlForecast() { AcctId = employee.AccId, OrgId = employee.OrgId, PlId = plan.PlId.GetValueOrDefault(), empleId = employee.Id, Plc = employee.PlcGlcCode, EmplId = employee.EmplId, ProjId = projId, Year = year, Month = month, Forecastedhours = forecastedHours });
                            }
                            else
                            {
                                plForecasts.Add(new PlForecast() { AcctId = employee.AccId, OrgId = employee.OrgId, PlId = plan.PlId.GetValueOrDefault(), empleId = employee.Id, Plc = employee.PlcGlcCode, EmplId = employee.EmplId, ProjId = projId, Year = year, Month = month, Forecastedhours = Convert.ToDecimal(0) });
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }

            ////////////////////////////////////////////////////////Direct COst
            random = new Random();
            var dctList = _context.PlDcts.Select(p => p.DctId).ToList();
            List<PlForecast> plForecastsDirectCost = new List<PlForecast>();

            sheet = workbook.GetSheet("Direct Cost");
            headerRow = sheet.GetRow(1);
            for (int rowIndex = 2; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                var row = sheet.GetRow(rowIndex);
                PlDct plDct = new PlDct()
                {
                    PlId = plan.PlId.GetValueOrDefault(),
                    DctId = newImport ? 0 : Convert.ToInt32(row.GetCell(2)?.ToString()),
                    AcctId = row.GetCell(4)?.ToString() ?? string.Empty,
                    OrgId = row.GetCell(3)?.ToString() ?? string.Empty,
                    AmountType = row.GetCell(1)?.ToString() ?? string.Empty,
                    IsBrd = bool.TryParse(row.GetCell(7)?.ToString(), out bool result) && result,
                    IsRev = bool.TryParse(row.GetCell(8)?.ToString(), out result) && result,
                    PlcGlc = row.GetCell(5)?.ToString() ?? string.Empty,
                    Id = row.GetCell(6)?.ToString() ?? string.Empty
                };

                if (plDct.DctId == 0)
                {
                    string sql1 = "";
                    switch (row.GetCell(1)?.ToString().ToUpper())
                    {
                        case "VENDOREMPLOYEE":
                            plDct.Type = "VENDOR EMPLOYEE";
                            sql1 = $@"Select vend_empl_id as EmplId,vend_empl_status as Status, vend_empl_name as FirstName, 0 AS Salary,0 AS PerHourRate,null AS EffectiveDate  
                            from public.vendor_employee where vend_empl_status = 'A' and
                            vend_empl_id = '{plDct.Id}';";
                            break;
                        case "VENDOR EMPLOYEE":
                            plDct.Type = "VENDOR EMPLOYEE";
                            sql1 = $@"Select vend_empl_id as EmplId,vend_empl_status as Status, vend_empl_name as FirstName, 0 AS Salary,0 AS PerHourRate,null AS EffectiveDate  
                            from public.vendor_employee where vend_empl_status = 'A' and
                            vend_empl_id = '{plDct.Id}';";
                            break;
                        case "VENDOR":
                            plDct.Type = "VENDOR";
                            sql1 = $@"Select vend_id as EmplId,vend_empl_status as Status, vend_empl_name as FirstName, 0 AS Salary,0 AS PerHourRate,null AS EffectiveDate 
                            from public.vendor_employee where vend_empl_status = 'A' and
                            vend_id = '{plDct.Id}';";
                            break;
                        case "OTHER":
                            plDct.Type = row.GetCell(1)?.ToString().ToUpper();
                            break;
                        case "EMPLOYEE":
                            plDct.Type = "EMPLOYEE";
                            sql1 = $@"
                                SELECT empl.empl_id AS EmplId, 
                                       s_empl_status_cd AS Status, 
                                       last_first_name AS FirstName, 
                                       sal_amt AS Salary,
                                       effect_dt AS EffectiveDate,
                                       hrly_amt AS PerHourRate
                                FROM empl
                                JOIN public.empl_lab_info 
                                    ON empl.empl_id = public.empl_lab_info.empl_id
                                WHERE empl.s_empl_status_cd = 'ACT' and empl.empl_id = '{plDct.Id}'
                                  AND public.empl_lab_info.end_dt = '2078-12-31';";
                            break;
                    }

                    ///////////////////////////////////////////////////////////////////////////////////
                    //var sql1 = $@"
                    //    SELECT empl.empl_id AS EmplId, 
                    //           s_empl_status_cd AS Status, 
                    //           last_first_name AS FirstName, 
                    //           sal_amt AS Salary,
                    //           hrly_amt AS PerHourRate
                    //    FROM empl
                    //    JOIN public.empl_lab_info 
                    //        ON empl.empl_id = public.empl_lab_info.empl_id
                    //    WHERE empl.empl_id = '{plDct.Id}'
                    //      AND public.empl_lab_info.end_dt = '2078-12-31';";

                    if (row.GetCell(1)?.ToString().ToUpper() != "OTHER" && row.GetCell(1)?.ToString().ToUpper() != "PLC")
                    {
                        var employeeDetails = _context.Empl_Master
                        .FromSqlRaw(sql1)
                        .ToList().FirstOrDefault();
                        if (employeeDetails != null && !string.IsNullOrWhiteSpace(employeeDetails.FirstName))
                            plDct.Category = employeeDetails.FirstName;
                        else
                        {
                            //_context.PlProjectPlans.Remove(plan);
                            //_context.SaveChanges();
                            //_projPlanService.DeleteProjectPlanAsync(plan.PlId.GetValueOrDefault()).Wait();
                            throw new Exception("Direct Cost Employee (" + plDct.Id + ") not found.");
                        }
                    }
                    else
                    {
                        //var random = new Random();
                        int number = random.Next(1, 100000); // 1 to 99999

                        if (row.GetCell(1)?.ToString().ToUpper() == "PLC")
                        {
                            plDct.Category = plDct.Type + number.ToString("D5");
                            plDct.Id = plDct.Type + number.ToString("D5");
                        }
                        else
                        {
                            plDct.Category = "TBD_" + number.ToString("D5");
                            plDct.Id = "TBD_" + number.ToString("D5");
                        }
                    }

                }
                plDcts.Add(plDct);

                if (plDct.DctId == 0)
                {
                    for (int i = 9; i < (9 + projectDurationMonths); i++)
                    {
                        try
                        {

                            var period = headerRow.GetCell(i)?.ToString() ?? string.Empty;

                            if (!string.IsNullOrEmpty(period))
                            {
                                DateTime parsedDate = DateTime.ParseExact(period, "MMM yyyy", System.Globalization.CultureInfo.InvariantCulture);

                                if (type.ToUpper() == "EAC")
                                {

                                    if (plan.ClosedPeriod <= DateOnly.FromDateTime(parsedDate))
                                    {
                                        int month = parsedDate.Month; // 6
                                        int year = parsedDate.Year;
                                        var cell = row.GetCell(i);
                                        decimal? cellValue = cell != null && !string.IsNullOrWhiteSpace(cell.ToString())
                                            ? Convert.ToDecimal(cell.ToString())
                                            : null;
                                        if (cellValue != null)
                                            plDct.PlForecasts.Add(new PlForecast() { AcctId = plDct.AcctId, OrgId = plDct.OrgId, PlId = plan.PlId.GetValueOrDefault(), DctId = plDct.DctId, ProjId = projId, Year = year, Month = month, Forecastedamt = cellValue });
                                        else
                                            plDct.PlForecasts.Add(new PlForecast() { AcctId = plDct.AcctId, OrgId = plDct.OrgId, PlId = plan.PlId.GetValueOrDefault(), DctId = plDct.DctId, ProjId = projId, Year = year, Month = month, Forecastedamt = cellValue });
                                    }

                                }
                                else
                                {
                                    int month = parsedDate.Month; // 6
                                    int year = parsedDate.Year;
                                    var cell = row.GetCell(i);
                                    decimal? cellValue = cell != null && !string.IsNullOrWhiteSpace(cell.ToString())
                                        ? Convert.ToDecimal(cell.ToString())
                                        : null;
                                    if (cellValue != null)
                                        plDct.PlForecasts.Add(new PlForecast() { AcctId = plDct.AcctId, OrgId = plDct.OrgId, PlId = plan.PlId.GetValueOrDefault(), DctId = plDct.DctId, ProjId = projId, Year = year, Month = month, Forecastedamt = cellValue });
                                    else
                                        plDct.PlForecasts.Add(new PlForecast() { AcctId = plDct.AcctId, OrgId = plDct.OrgId, PlId = plan.PlId.GetValueOrDefault(), DctId = plDct.DctId, ProjId = projId, Year = year, Month = month, Forecastedamt = cellValue });
                                }
                            }

                        }
                        catch (Exception ex)
                        {

                        }
                    }

                    var entry = _context.PlDcts.Add(plDct);
                    plDct.DctId = entry.Entity.DctId;
                    _context.SaveChanges();
                    continue;
                }


                for (int i = 9; i < (9 + projectDurationMonths); i++)
                {

                    try
                    {
                        var period = headerRow.GetCell(i)?.ToString() ?? string.Empty;

                        if (!string.IsNullOrEmpty(period))
                        {
                            DateTime parsedDate = DateTime.ParseExact(period, "MMM yyyy", System.Globalization.CultureInfo.InvariantCulture);

                            if (type.ToUpper() == "EAC")
                            {
                                if (plan.ClosedPeriod < DateOnly.FromDateTime(parsedDate))
                                {
                                    int month = parsedDate.Month; // 6
                                    int year = parsedDate.Year;

                                    if (parsedDate.Month == 8 && parsedDate.Year == 2025)
                                    {

                                    }
                                    var cell = row.GetCell(i);
                                    decimal? cellValue = cell != null && !string.IsNullOrWhiteSpace(cell.ToString())
                                        ? Convert.ToDecimal(cell.ToString())
                                        : null;
                                    if (cellValue != null)
                                        plForecastsDirectCost.Add(new PlForecast() { AcctId = plDct.AcctId, OrgId = plDct.OrgId, PlId = plan.PlId.GetValueOrDefault(), DctId = plDct.DctId, ProjId = projId, Year = year, Month = month, Forecastedamt = cellValue, Actualamt = cellValue });
                                    else
                                        plForecastsDirectCost.Add(new PlForecast() { AcctId = plDct.AcctId, OrgId = plDct.OrgId, PlId = plan.PlId.GetValueOrDefault(), DctId = plDct.DctId, ProjId = projId, Year = year, Month = month, Forecastedamt = cellValue, Actualamt = 0 });
                                }
                            }
                            else
                            {

                                int month = parsedDate.Month; // 6
                                int year = parsedDate.Year;
                                if (month == 2 && year == 2026)
                                {

                                }

                                var cell = row.GetCell(i);
                                decimal? cellValue = cell != null && !string.IsNullOrWhiteSpace(cell.ToString())
                                    ? Convert.ToDecimal(cell.ToString())
                                    : null;
                                if (cellValue != null)
                                    plForecastsDirectCost.Add(new PlForecast() { AcctId = plDct.AcctId, OrgId = plDct.OrgId, PlId = plan.PlId.GetValueOrDefault(), DctId = plDct.DctId, ProjId = projId, Year = year, Month = month, Forecastedamt = cellValue });
                                else
                                    plForecastsDirectCost.Add(new PlForecast() { AcctId = plDct.AcctId, OrgId = plDct.OrgId, PlId = plan.PlId.GetValueOrDefault(), DctId = plDct.DctId, ProjId = projId, Year = year, Month = month, Forecastedamt = cellValue });
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }

            }

            var dctsToRemove = dctList.Except(plDcts.Select(p => p.DctId).ToList()).ToList();

            if (!newImport)
            {
                List<PlForecast> newFOrcast = new List<PlForecast>();

                var updatedList = plForecastData
                            .Select(t =>
                            {
                                if (type.ToUpper() == "EAC")
                                {
                                    var source = plForecasts.FirstOrDefault(s => s.Plc == t.Plc && s.AcctId == t.AcctId && s.OrgId == t.OrgId && s.PlId == t.PlId && s.EmplId == t.EmplId && s.Month == t.Month && s.Year == t.Year && s.ProjId == t.ProjId && (new DateOnly(s.Year, s.Month, 1) >= plan.ClosedPeriod.GetValueOrDefault()));
                                    if (source != null)
                                    {
                                        t.Actualhours = source.Actualhours;
                                        t.Forecastedhours = source.Actualhours;
                                    }
                                }
                                else
                                {
                                    var source = plForecasts.FirstOrDefault(s => s.Plc == t.Plc && s.AcctId == t.AcctId && s.OrgId == t.OrgId && s.PlId == t.PlId && s.EmplId == t.EmplId && s.Month == t.Month && s.Year == t.Year && s.ProjId == t.ProjId);
                                    if (source != null)
                                    {
                                        t.Forecastedhours = source.Forecastedhours;
                                        t.Actualhours = source.Actualhours;
                                    }
                                }
                                return t;
                            }).ToList();


                var updatedListForDirectCost = updatedList
                           .Select(t =>
                           {
                               if (type.ToUpper() == "EAC")
                               {
                                   var source = plForecastsDirectCost.FirstOrDefault(s => s.PlId == t.PlId && s.DctId == t.DctId && s.Month == t.Month && s.Year == t.Year && s.ProjId == t.ProjId && (new DateOnly(s.Year, s.Month, 1) >= plan.ClosedPeriod.GetValueOrDefault()));
                                   if (source != null)
                                   {
                                       t.Actualamt = source.Actualamt;
                                       t.Forecastedamt = source.Actualamt;

                                   }
                               }
                               else
                               {
                                   var source = plForecastsDirectCost.FirstOrDefault(s => s.PlId == t.PlId && s.DctId == t.DctId && s.Month == t.Month && s.Year == t.Year && s.ProjId == t.ProjId);
                                   if (source != null)
                                   {
                                       t.Forecastedamt = source.Forecastedamt;
                                       t.Actualamt = source.Actualamt;

                                   }
                               }
                               return t;
                           }).ToList();


                List<PlForecast> newHoursForecast = new List<PlForecast>();
                newHoursForecast = updatedList.Where(p => p.Forecastid == 0 && p.EmplId != null).ToList();

                var test = plForecasts
                            .Select(t =>
                            {
                                var source = updatedList.Where(p => p.EmplId != null).FirstOrDefault(s => s.PlId == t.PlId && s.EmplId == t.EmplId && s.Month == t.Month && s.Year == t.Year && s.ProjId == t.ProjId && s.Plc == t.Plc);
                                if (source == null)
                                {
                                    newHoursForecast.Add(t);
                                }
                                return t;
                            }).ToList();


                test = plForecastsDirectCost
               .Select(t =>
               {
                   var source = updatedList.Where(p => p.EmplId == null).FirstOrDefault(s => s.PlId == t.PlId && s.DctId == t.DctId && s.Month == t.Month && s.Year == t.Year && s.ProjId == t.ProjId);
                   if (source == null)
                   {
                       newFOrcast.Add(t);
                   }
                   return t;
               }).ToList();
                _context.PlForecasts.AddRange(newHoursForecast);
                _context.PlForecasts.AddRange(newFOrcast);

                _context.PlForecasts.UpdateRange(updatedListForDirectCost);
            }
            else
            {
                _context.PlForecasts.UpdateRange(plForecasts);
            }
            _context.SaveChanges();
            var itemsToRemove = _context.PlDcts.Where(p => dctsToRemove.Contains(p.DctId)).ToList();
            var forecastsToRemove = _context.PlForecasts.Where(p => dctsToRemove.Contains(p.DctId.GetValueOrDefault())).ToList();

            if (itemsToRemove.Count > 0)
            {
                //_context.PlForecasts.RemoveRange(forecastsToRemove);
                //_context.PlDcts.RemoveRange(itemsToRemove);
            }
            //_context.SaveChanges();

            PlForecastRepository plForecastRepository = new PlForecastRepository(_context, _config);
            //await plForecastRepository.CalculateRevenueCost(plan.PlId.GetValueOrDefault(), plan.TemplateId.GetValueOrDefault(), plan.PlType);

            if (newImport)
            {
                var responseMessage = "Successfully Imported and Created new '" + ((type == "BUD") ? "Budget" : "EAC") + "' for Project - '" + project.ProjName + "' with Version - '" + plan?.Version + "'";
                _logger.LogInformation(responseMessage);
                return Ok(responseMessage);
            }
            else
            {
                var responseMessage = "Successfully Imported and Updated existing '" + ((type == "BUD") ? "Budget" : "EAC") + "' for Project - '" + project.ProjName + "' Having version - '" + version + "'";
                _logger.LogInformation(responseMessage);
                return Ok(responseMessage);
            }
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pgEx &&
                pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                var formatted = FormatPgDetail(pgEx.Detail);
                if (newImport)
                    _projPlanService.DeleteProjectPlanAsync(plan.PlId.GetValueOrDefault()).Wait();
                return Conflict(formatted);
            }

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import plan" + ex.Message);
            if (newImport)
                _projPlanService.DeleteProjectPlanAsync(plan.PlId.GetValueOrDefault()).Wait();
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("ImportDirectCostPlangadvbad")]
    public async Task<IActionResult> ImportDirectCostPlangadbad(IFormFile file)
    {
        bool newImport = false;
        int closingMonth = 0, closingYear = 0;

        _logger.LogInformation("ImportPlan called");

        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var plForecastData = new List<PlForecast>();

        PlProjectPlan plan = new PlProjectPlan();

        string projId = string.Empty;
        string type = string.Empty;
        string version = string.Empty;

        try
        {
            List<PlEmployeee> plEmployees = new();
            List<PlDct> plDcts = new();

            var random = new Random();

            using var stream = file.OpenReadStream();

            var workbook = new XSSFWorkbook(stream);

            var sheet = workbook.GetSheet("Hours");

            //-------------------------------------------------------
            // HEADER
            //-------------------------------------------------------

            var infoRow = sheet.GetRow(0);

            if (infoRow != null)
            {
                projId = infoRow.GetCell(1)?.ToString() ?? string.Empty;
                type = infoRow.GetCell(3)?.ToString() ?? string.Empty;
                version = infoRow.GetCell(5)?.ToString() ?? string.Empty;
            }

            //-------------------------------------------------------
            // CREATE NEW PLAN
            //-------------------------------------------------------

            if (string.IsNullOrEmpty(version))
            {
                var proj = _context.PlProjects
                    .FirstOrDefault(p => p.ProjId == projId);

                if (proj == null)
                {
                    return NotFound("Project Not Found - " + projId);
                }

                newImport = true;

                plan = await _projPlanService.AddProjectPlanAsync(
                    new PlProjectPlan
                    {
                        TemplateId = 1,
                        ProjId = projId,
                        Status = "In Progress",
                        PlType = type,
                        Type = "A",
                        ProjStartDt = proj.ProjStartDt,
                        ProjEndDt = proj.ProjEndDt,
                        Source = "EXCEL"
                    },
                    "Excel");
            }

            //-------------------------------------------------------
            // GET PLAN
            //-------------------------------------------------------

            plan = await _context.PlProjectPlans
                .Include(p => p.Proj)
                .FirstOrDefaultAsync(p =>
                    p.ProjId == projId &&
                    p.Version == Convert.ToInt32(version == "" ? plan.Version.ToString() : version) &&
                    p.PlType == type);

            if (plan == null)
                return BadRequest("Plan not found.");

            if (plan.Status.ToUpper() != "IN PROGRESS")
            {
                return StatusCode(
                    500,
                    $"Import failed: Budget status is '{plan.Status}'");
            }

            //-------------------------------------------------------
            // EXISTING FORECASTS
            //-------------------------------------------------------

            plForecastData = await _context.PlForecasts
                .Where(p => p.PlId == plan.PlId)
                .ToListAsync();

            //-------------------------------------------------------
            // PROJECT DURATION
            //-------------------------------------------------------

            int projectDurationMonths =
                (plan.ProjEndDt.GetValueOrDefault().Year -
                 plan.ProjStartDt.GetValueOrDefault().Year) * 12 +
                plan.ProjEndDt.GetValueOrDefault().Month -
                plan.ProjStartDt.GetValueOrDefault().Month + 1;

            //-------------------------------------------------------
            // FORECASTS
            //-------------------------------------------------------

            var plForecasts = new List<PlForecast>();

            var headerRow = sheet.GetRow(1);

            //-------------------------------------------------------
            // EMPLOYEE CACHE
            //-------------------------------------------------------

            var emplList = await _context.PlEmployeees
                .Where(p => p.PlId == plan.PlId)
                .ToListAsync();

            //-------------------------------------------------------
            // HOURS SHEET
            //-------------------------------------------------------

            for (int rowIndex = 2; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                var row = sheet.GetRow(rowIndex);

                if (row == null)
                    continue;

                var employee = new PlEmployeee
                {
                    EmplId = row.GetCell(2)?.ToString() ?? "",
                    AccId = row.GetCell(4)?.ToString() ?? "",
                    OrgId = row.GetCell(3)?.ToString() ?? "",
                    PlcGlcCode = row.GetCell(5)?.ToString() ?? "",
                    Type = row.GetCell(1)?.ToString() ?? "",
                    PlId = plan.PlId,
                    IsBrd = bool.TryParse(row.GetCell(7)?.ToString(), out bool brd) && brd,
                    IsRev = bool.TryParse(row.GetCell(8)?.ToString(), out bool rev) && rev,
                    PerHourRate = decimal.TryParse(
                        row.GetCell(6)?.ToString(),
                        out decimal rate)
                        ? rate
                        : 0
                };

                //-------------------------------------------------------
                // EXISTING EMPLOYEE
                //-------------------------------------------------------

                var existingEmployee = emplList.FirstOrDefault(p =>
                    p.EmplId == employee.EmplId &&
                    p.OrgId == employee.OrgId &&
                    p.AccId == employee.AccId &&
                    p.PlcGlcCode == employee.PlcGlcCode);

                if (existingEmployee != null)
                {
                    employee = existingEmployee;
                }
                else
                {

                    ///////////////////////////////////////////////////////

                    if (existingEmployee == null)
                    {
                        string sql1 = "";

                        switch (row.GetCell(1)?.ToString().ToUpper())
                        {

                            case "VENDOREMPLOYEE":
                                employee.Type = "VENDOR EMPLOYEE";
                                sql1 = $@"Select vend_empl_id as EmplId,vend_empl_status as Status, vend_empl_name as FirstName, 0 AS Salary,0 AS PerHourRate, null AS EffectiveDate  
                            from public.vendor_employee where vend_empl_status = 'A' and
                            vend_empl_id = '{employee.EmplId}';";
                                break;
                            case "VENDOR EMPLOYEE":
                                employee.Type = "VENDOR EMPLOYEE";
                                sql1 = $@"Select vend_empl_id as EmplId,vend_empl_status as Status, vend_empl_name as FirstName, 0 AS Salary,0 AS PerHourRate, null AS EffectiveDate  
                            from public.vendor_employee where vend_empl_status = 'A' and
                            vend_empl_id = '{employee.EmplId}';";
                                break;
                            case "VENDOR":
                                employee.Type = "VENDOR";
                                sql1 = $@"Select vend_id as EmplId,vend_empl_status as Status, vend_empl_name as FirstName, 0 AS Salary,0 AS PerHourRate,null AS EffectiveDate  
                            from public.vendor_employee where vend_empl_status = 'A' and
                            vend_id = '{employee.EmplId}';";
                                break;
                            case "OTHER":
                                employee.Type = row.GetCell(1)?.ToString().ToUpper();
                                break;
                            case "EMPLOYEE":
                                employee.Type = "EMPLOYEE";
                                sql1 = $@"
                                SELECT empl.empl_id AS EmplId, 
                                       s_empl_status_cd AS Status, 
                                       last_first_name AS FirstName, 
                                       sal_amt AS Salary,
                                       effect_dt AS EffectiveDate,
                                       hrly_amt AS PerHourRate
                                FROM empl
                                JOIN public.empl_lab_info 
                                    ON empl.empl_id = public.empl_lab_info.empl_id
                                WHERE empl.s_empl_status_cd = 'ACT' and empl.empl_id = '{employee.EmplId}' 
                                  AND public.empl_lab_info.end_dt = '2078-12-31';";
                                break;
                        }

                        if (row.GetCell(1)?.ToString().ToUpper() == "OTHER" || row.GetCell(1)?.ToString().ToUpper() == "PLC")
                        {

                            int number = random.Next(1, 100000); // 1 to 99999

                            if (row.GetCell(1)?.ToString().ToUpper() == "PLC")
                            {
                                employee.EmplId = employee.Type + "_" + number.ToString("D5");
                            }
                            else
                            {
                                employee.EmplId = "TBD_" + number.ToString("D5");
                                employee.FirstName = employee.EmplId;

                            }

                            ////////////////////////////////////////////////////////////////////////////////////////////
                            var entry = _context.PlEmployeees.Add(employee);
                            _context.SaveChanges();
                            employee.Id = entry.Entity.Id;
                        }
                        else
                        {
                            var employeeDetails = _context.Empl_Master
                                                .FromSqlRaw(sql1)
                                                .ToList().FirstOrDefault();

                            if (employeeDetails != null)
                            {
                                if (!string.IsNullOrWhiteSpace(employeeDetails.FirstName))
                                {
                                    var names = employeeDetails.FirstName.Split(',', 2);

                                    employee.LastName = names[0];
                                    employee.FirstName = names.Length > 1 ? names[1] : names[0];
                                }

                                if (employee.Type.ToUpper() == "EMPLOYEE")
                                {
                                    employee.Salary = employeeDetails.Salary;
                                    employee.PerHourRate = employeeDetails.PerHourRate;
                                }
                                var entry = _context.PlEmployeees.Add(employee);
                                _context.SaveChanges();
                                employee.Id = entry.Entity.Id;
                            }
                            else
                            {
                                //_projPlanService.DeleteProjectPlanAsync(plan.PlId.GetValueOrDefault()).Wait();
                                //_context.PlProjectPlans.Remove(plan);
                                //_context.SaveChanges();
                                throw new Exception("Employee (" + employee.EmplId + ") not found.");
                            }
                        }

                    }



                    //////////////////////////////////////////////////////////////////
                    _context.PlEmployeees.Add(employee);
                    await _context.SaveChangesAsync();

                    emplList.Add(employee);
                }

                //-------------------------------------------------------
                // MONTHS
                //-------------------------------------------------------

                for (int i = 9; i < (9 + projectDurationMonths); i++)
                {
                    try
                    {
                        var period =
                            headerRow.GetCell(i)?.ToString() ?? "";

                        if (string.IsNullOrWhiteSpace(period))
                            continue;

                        DateTime parsedDate =
                            DateTime.ParseExact(
                                period,
                                "MMM yyyy",
                                System.Globalization.CultureInfo.InvariantCulture);

                        var cell = row.GetCell(i);

                        decimal hours = 0;

                        if (cell != null)
                        {
                            switch (cell.CellType)
                            {
                                case CellType.Numeric:
                                    hours = (decimal)cell.NumericCellValue;
                                    break;

                                case CellType.String:
                                    decimal.TryParse(
                                        cell.StringCellValue,
                                        out hours);
                                    break;

                                case CellType.Formula:
                                    if (cell.CachedFormulaResultType ==
                                        CellType.Numeric)
                                    {
                                        hours = (decimal)cell.NumericCellValue;
                                    }
                                    break;
                            }
                        }

                        var forecast = new PlForecast
                        {
                            AcctId = employee.AccId,
                            OrgId = employee.OrgId,
                            PlId = plan.PlId.GetValueOrDefault(),
                            empleId = employee.Id,
                            Plc = employee.PlcGlcCode,
                            EmplId = employee.EmplId,
                            ProjId = projId,
                            Year = parsedDate.Year,
                            Month = parsedDate.Month
                        };

                        //-------------------------------------------------------
                        // BUD / EAC
                        //-------------------------------------------------------

                        if (type.ToUpper() == "EAC")
                        {
                            forecast.Actualhours = hours;
                            forecast.Forecastedhours = hours;
                        }
                        else
                        {
                            forecast.Forecastedhours = hours;
                            forecast.Actualhours = 0;
                        }

                        plForecasts.Add(forecast);
                    }
                    catch
                    {
                    }
                }
            }

            //-------------------------------------------------------
            // VALIDATION
            //-------------------------------------------------------
            var closingPeriod = DateOnly.FromDateTime(
            DateTime.Parse(
                _context.PlConfigValues
                    .First(x => x.Name == "closing_period")
                    .Value));

            bool isEac = type.Equals("EAC", StringComparison.OrdinalIgnoreCase);

            var filteredForecasts = isEac
                ? plForecasts.Where(x =>
                    new DateOnly(x.Year, x.Month, 1) >= closingPeriod)
                : plForecasts;

            var importedHours = filteredForecasts
                    .GroupBy(x => new
                    {
                        x.EmplId,
                        x.Year,
                        x.Month
                    })
                    .Select(g => new
                    {
                        g.Key.EmplId,
                        g.Key.Year,
                        g.Key.Month,

                        Hours = g.Sum(x =>
                            type.ToUpper() == "EAC"
                                ? x.Actualhours
                                : x.Forecastedhours)
                    })
                    .ToList();



            //var importedHours = plForecasts
            //    .GroupBy(x => new
            //    {
            //        x.EmplId,
            //        x.Year,
            //        x.Month
            //    })
            //    .Select(g => new
            //    {
            //        g.Key.EmplId,
            //        g.Key.Year,
            //        g.Key.Month,
            //        Hours = g.Sum(x =>
            //            type.ToUpper() == "EAC"
            //                ? (x.Actualhours)
            //                : (x.Forecastedhours))
            //    })
            //    .ToList();

            //-------------------------------------------------------
            // EXISTING HOURS
            //-------------------------------------------------------

            var employeeIds = importedHours
                .Select(x => x.EmplId)
                .Distinct()
                .ToList();

            var existingHours = await _context.PlForecasts
                .Where(x =>
                    employeeIds.Contains(x.EmplId) &&
                    x.PlId != plan.PlId)
                .GroupBy(x => new
                {
                    x.EmplId,
                    x.Year,
                    x.Month
                })
                .Select(g => new
                {
                    g.Key.EmplId,
                    g.Key.Year,
                    g.Key.Month,
                    Hours = g.Sum(x =>
                        type.ToUpper() == "EAC"
                            ? (x.Actualhours)
                            : (x.Forecastedhours))
                })
                .ToListAsync();

            //-------------------------------------------------------
            // STANDARD SCHEDULE
            //-------------------------------------------------------

            Helper scheduleHelper = new Helper(_context, _config);

            var startDate = new DateOnly(
                importedHours.Min(x => x.Year),
                importedHours.Min(x => x.Month),
                1);

            var endDate = new DateOnly(
                importedHours.Max(x => x.Year),
                importedHours.Max(x => x.Month),
                DateTime.DaysInMonth(
                    importedHours.Max(x => x.Year),
                    importedHours.Max(x => x.Month)));

            var standardSchedule =
                scheduleHelper.GetWorkingDaysForDuration(
                    startDate,
                    endDate);

            var standardLookup = standardSchedule
                .ToDictionary(
                    x => (x.Year, x.MonthNo),
                    x => x.WorkingDays * 8m);

            //-------------------------------------------------------
            // VALIDATE 130%
            //-------------------------------------------------------

            var validationErrors = new List<string>();

            List<AlternateEmployeeDto> alternateEmployees = new List<AlternateEmployeeDto>();

            foreach (var imported in importedHours)
            {

                if (type.ToUpper() == "EAC")
                {
                    if (plan.ClosedPeriod.HasValue)
                    {
                        var periodDate = new DateOnly(imported.Year, imported.Month, 1);

                        // skip validation for closed periods
                        if (periodDate <= plan.ClosedPeriod.Value || imported.Hours == 0)
                        {
                            continue;
                        }
                    }
                }
                var existing = existingHours
                    .FirstOrDefault(x =>
                        x.EmplId == imported.EmplId &&
                        x.Year == imported.Year &&
                        x.Month == imported.Month);

                decimal existingTotal =
                    existing?.Hours ?? 0;

                decimal totalHours =
                    existingTotal + imported.Hours;

                decimal standardHours =
                    standardLookup.TryGetValue(
                        (imported.Year, imported.Month),
                        out var std)
                        ? std
                        : 0;

                decimal allowedHours =
                    standardHours * 1.30m;

                if (totalHours > allowedHours)
                {

                    //// --------------------------------------------------
                    //// FIND ALTERNATE EMPLOYEES
                    //// --------------------------------------------------

                    //var employeeHours = await _context.PlForecasts
                    //    .Where(x =>
                    //        x.Year == imported.Year &&
                    //        x.Month == imported.Month &&
                    //        x.EmplId != null)
                    //    .GroupBy(x => x.EmplId)
                    //    .Select(g => new
                    //    {
                    //        EmplId = g.Key,
                    //        Hours = g.Sum(x =>
                    //            (x.Actualhours) +
                    //            (x.Forecastedhours))
                    //    })
                    //    .ToListAsync();

                    //alternateEmployees = employeeHours
                    //    .Where(x => x.Hours < allowedHours)
                    //    .Select(x => new AlternateEmployeeDto
                    //    {
                    //        EmployeeId = x.EmplId,
                    //        AssignedHours = x.Hours,
                    //        AvailableHours = allowedHours - x.Hours
                    //    })
                    //    .OrderByDescending(x => x.AvailableHours)
                    //    .Take(10)
                    //    .ToList();

                    //var alternateEmployeeMessage =
                    //    alternateEmployees.Any()
                    //        ? "\nAlternative Employees:\n" +
                    //          string.Join(
                    //              "\n",
                    //              alternateEmployees.Select((x, i) =>
                    //                  $"{i + 1}. {x.EmployeeId} | " +
                    //                  $"Assigned: {x.AssignedHours:N2} | " +
                    //                  $"Available: {x.AvailableHours:N2}"
                    //              ))
                    //        : "\nNo alternative employees available.";

                    validationErrors.Add(
                        $"Employee '{imported.EmplId}' exceeds allowed hours for " +
                        $"{imported.Month}/{imported.Year}. " +
                        $"Planned: {totalHours:N2}, " +
                        $"Allowed: {allowedHours:N2}");

                    //validationErrors.Add(
                    //    $"Employee '{imported.EmplId}' exceeds allowed hours for " +
                    //    $"{imported.Month}/{imported.Year}. " +
                    //    $"Planned: {totalHours:N2}, " +
                    //    $"Allowed: {allowedHours:N2}" +
                    //    $"{alternateEmployeeMessage}"
                    //);
                }
            }

            //-------------------------------------------------------
            // STOP IMPORT
            //-------------------------------------------------------

            if (validationErrors.Any())
            {
                return BadRequest(new
                {
                    Message = "Hours validation failed:\n" +
              string.Join("\n", validationErrors),
                    Errors = validationErrors,

                });
            }












            //-------------------------------------------------------
            // SAVE
            //-------------------------------------------------------

            if (!newImport)
            {
                _context.PlForecasts.RemoveRange(plForecastData);
            }

            await _context.PlForecasts.AddRangeAsync(plForecasts);

            await _context.SaveChangesAsync();

            //-------------------------------------------------------
            // CALCULATE
            //-------------------------------------------------------

            PlForecastRepository repo =
                new PlForecastRepository(_context, _config);

            await repo.CalculateRevenueCost(
                plan.PlId.GetValueOrDefault(),
                plan.TemplateId.GetValueOrDefault(),
                plan.PlType);

            //-------------------------------------------------------
            // RESPONSE
            //-------------------------------------------------------

            if (newImport)
            {
                return Ok(
                    $"Successfully Imported and Created new '{type}' plan.");
            }

            return Ok(
                $"Successfully Imported and Updated existing '{type}' plan.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import plan");

            if (newImport && plan?.PlId != null)
            {
                await _projPlanService.DeleteProjectPlanAsync(
                    plan.PlId.GetValueOrDefault());
            }

            return StatusCode(500, ex.Message);
        }
    }


    [HttpPost("ImportDirectCostPlanGadbad")]
    public async Task<IActionResult> ImportDirectCostPlanGadbad(IFormFile file)
    {
        bool newImport = false;

        _logger.LogInformation("ImportPlan called");

        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var plForecastData = new List<PlForecast>();

        PlProjectPlan plan = new PlProjectPlan();

        string projId = string.Empty;
        string type = string.Empty;
        string version = string.Empty;

        try
        {
            List<PlEmployeee> plEmployees = new();
            List<PlDct> plDcts = new();

            var random = new Random();

            using var stream = file.OpenReadStream();

            var workbook = new XSSFWorkbook(stream);

            var sheet = workbook.GetSheet("Hours");

            //-------------------------------------------------------
            // HEADER
            //-------------------------------------------------------

            var infoRow = sheet.GetRow(0);

            if (infoRow != null)
            {
                projId = infoRow.GetCell(1)?.ToString() ?? string.Empty;
                type = infoRow.GetCell(3)?.ToString() ?? string.Empty;
                version = infoRow.GetCell(5)?.ToString() ?? string.Empty;
            }

            //-------------------------------------------------------
            // CREATE NEW PLAN
            //-------------------------------------------------------

            if (string.IsNullOrEmpty(version))
            {
                var proj = _context.PlProjects
                    .FirstOrDefault(p => p.ProjId == projId);

                if (proj == null)
                {
                    return NotFound("Project Not Found - " + projId);
                }

                newImport = true;

                plan = await _projPlanService.AddProjectPlanAsync(
                    new PlProjectPlan
                    {
                        TemplateId = 1,
                        ProjId = projId,
                        Status = "In Progress",
                        PlType = type,
                        Type = "A",
                        ProjStartDt = proj.ProjStartDt,
                        ProjEndDt = proj.ProjEndDt,
                        Source = "EXCEL"
                    },
                    "Excel");
            }

            //-------------------------------------------------------
            // GET PLAN
            //-------------------------------------------------------

            plan = await _context.PlProjectPlans
                .Include(p => p.Proj)
                .FirstOrDefaultAsync(p =>
                    p.ProjId == projId &&
                    p.Version == Convert.ToInt32(
                        version == ""
                            ? plan.Version.ToString()
                            : version) &&
                    p.PlType == type);

            if (plan == null)
                return BadRequest("Plan not found.");

            if (plan.Status.ToUpper() != "IN PROGRESS")
            {
                return StatusCode(
                    500,
                    $"Import failed: Budget status is '{plan.Status}'");
            }

            //-------------------------------------------------------
            // EXISTING FORECASTS
            //-------------------------------------------------------

            plForecastData = await _context.PlForecasts
                .Where(p => p.PlId == plan.PlId)
                .ToListAsync();

            //-------------------------------------------------------
            // PROJECT DURATION
            //-------------------------------------------------------

            int projectDurationMonths =
                (plan.ProjEndDt.GetValueOrDefault().Year -
                 plan.ProjStartDt.GetValueOrDefault().Year) * 12 +
                plan.ProjEndDt.GetValueOrDefault().Month -
                plan.ProjStartDt.GetValueOrDefault().Month + 1;

            //-------------------------------------------------------
            // FORECASTS
            //-------------------------------------------------------

            var plForecasts = new List<PlForecast>();

            var headerRow = sheet.GetRow(1);

            //-------------------------------------------------------
            // EMPLOYEE CACHE
            //-------------------------------------------------------

            var emplList = await _context.PlEmployeees
                .Where(p => p.PlId == plan.PlId)
                .ToListAsync();

            //-------------------------------------------------------
            // HOURS SHEET
            //-------------------------------------------------------

            for (int rowIndex = 2; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                var row = sheet.GetRow(rowIndex);

                if (row == null)
                    continue;

                var employee = new PlEmployeee
                {
                    EmplId = row.GetCell(2)?.ToString() ?? "",
                    AccId = row.GetCell(4)?.ToString() ?? "",
                    OrgId = row.GetCell(3)?.ToString() ?? "",
                    PlcGlcCode = row.GetCell(5)?.ToString() ?? "",
                    Type = row.GetCell(1)?.ToString() ?? "",
                    PlId = plan.PlId,
                    IsBrd = bool.TryParse(
                        row.GetCell(7)?.ToString(),
                        out bool brd) && brd,

                    IsRev = bool.TryParse(
                        row.GetCell(8)?.ToString(),
                        out bool rev) && rev,

                    PerHourRate = decimal.TryParse(
                        row.GetCell(6)?.ToString(),
                        out decimal rate)
                        ? rate
                        : 0
                };

                //-------------------------------------------------------
                // EXISTING EMPLOYEE
                //-------------------------------------------------------

                var existingEmployee = emplList.FirstOrDefault(p =>
                    p.EmplId == employee.EmplId &&
                    p.OrgId == employee.OrgId &&
                    p.AccId == employee.AccId &&
                    p.PlcGlcCode == employee.PlcGlcCode);

                if (existingEmployee != null)
                {
                    employee = existingEmployee;
                }
                else
                {
                    _context.PlEmployeees.Add(employee);

                    await _context.SaveChangesAsync();

                    emplList.Add(employee);
                }

                //-------------------------------------------------------
                // MONTHS
                //-------------------------------------------------------

                for (int i = 9; i < (9 + projectDurationMonths); i++)
                {
                    try
                    {
                        var period =
                            headerRow.GetCell(i)?.ToString() ?? "";

                        if (string.IsNullOrWhiteSpace(period))
                            continue;

                        DateTime parsedDate =
                            DateTime.ParseExact(
                                period,
                                "MMM yyyy",
                                System.Globalization.CultureInfo.InvariantCulture);

                        //-------------------------------------------------------
                        // SKIP CLOSED PERIODS FOR EAC
                        //-------------------------------------------------------

                        var forecastPeriod =
                            new DateOnly(
                                parsedDate.Year,
                                parsedDate.Month,
                                1);

                        if (type.Equals(
                                "EAC",
                                StringComparison.OrdinalIgnoreCase) &&
                            plan.ClosedPeriod.HasValue &&
                            forecastPeriod <= plan.ClosedPeriod.Value)
                        {
                            continue;
                        }

                        //-------------------------------------------------------
                        // HOURS
                        //-------------------------------------------------------

                        var cell = row.GetCell(i);

                        decimal hours = 0;

                        if (cell != null)
                        {
                            switch (cell.CellType)
                            {
                                case CellType.Numeric:
                                    hours = (decimal)cell.NumericCellValue;
                                    break;

                                case CellType.String:
                                    decimal.TryParse(
                                        cell.StringCellValue,
                                        out hours);
                                    break;

                                case CellType.Formula:

                                    if (cell.CachedFormulaResultType ==
                                        CellType.Numeric)
                                    {
                                        hours = (decimal)cell.NumericCellValue;
                                    }

                                    break;
                            }
                        }

                        //-------------------------------------------------------
                        // FORECAST
                        //-------------------------------------------------------

                        var forecast = new PlForecast
                        {
                            AcctId = employee.AccId,
                            OrgId = employee.OrgId,
                            PlId = plan.PlId.GetValueOrDefault(),
                            empleId = employee.Id,
                            Plc = employee.PlcGlcCode,
                            EmplId = employee.EmplId,
                            ProjId = projId,
                            Year = parsedDate.Year,
                            Month = parsedDate.Month,
                            DisplayText = employee.Type
                        };

                        //-------------------------------------------------------
                        // BUDGET / EAC
                        //-------------------------------------------------------

                        if (type.Equals(
                            "EAC",
                            StringComparison.OrdinalIgnoreCase))
                        {
                            forecast.Actualhours = hours;
                            forecast.Forecastedhours = hours;
                        }
                        else
                        {
                            forecast.Forecastedhours = hours;
                            forecast.Actualhours = 0;
                        }

                        plForecasts.Add(forecast);
                    }
                    catch
                    {
                    }
                }
            }

            //-------------------------------------------------------
            // VALIDATION
            //-------------------------------------------------------

            var importedHours = plForecasts
                .GroupBy(x => new
                {
                    x.EmplId,
                    x.Year,
                    x.Month
                })
                .Select(g => new
                {
                    g.Key.EmplId,
                    g.Key.Year,
                    g.Key.Month,
                    Type = g.FirstOrDefault().DisplayText,
                    Hours = g.Sum(x =>
                        type.ToUpper() == "EAC"
                            ? x.Actualhours
                            : x.Forecastedhours)
                })
                .ToList();

            //-------------------------------------------------------
            // EXISTING HOURS
            //-------------------------------------------------------

            var employeeIds = importedHours
                .Select(x => x.EmplId)
                .Distinct()
                .ToList();

            var existingHours = await _context.PlForecasts
                .Where(x =>
                    employeeIds.Contains(x.EmplId) &&
                    x.PlId != plan.PlId)
                .GroupBy(x => new
                {
                    x.EmplId,
                    x.Year,
                    x.Month
                })
                .Select(g => new
                {
                    g.Key.EmplId,
                    g.Key.Year,
                    g.Key.Month,

                    Hours = g.Sum(x =>
                        type.ToUpper() == "EAC"
                            ? x.Actualhours
                            : x.Forecastedhours)
                })
                .ToListAsync();

            //-------------------------------------------------------
            // STANDARD SCHEDULE
            //-------------------------------------------------------

            Helper scheduleHelper =
                new Helper(_context, _config);

            var startDate = new DateOnly(
                importedHours.Min(x => x.Year),
                importedHours.Min(x => x.Month),
                1);

            var endDate = new DateOnly(
                importedHours.Max(x => x.Year),
                importedHours.Max(x => x.Month),
                DateTime.DaysInMonth(
                    importedHours.Max(x => x.Year),
                    importedHours.Max(x => x.Month)));

            var standardSchedule =
                scheduleHelper.GetWorkingDaysForDuration(
                    startDate,
                    endDate);

            var standardLookup =
                standardSchedule.ToDictionary(
                    x => (x.Year, x.MonthNo),
                    x => x.WorkingDays * 8m);

            //-------------------------------------------------------
            // VALIDATE 130%
            //-------------------------------------------------------

            var validationErrors = new List<string>();

            foreach (var imported in importedHours)
            {
                var existing = existingHours
                    .FirstOrDefault(x =>
                        x.EmplId == imported.EmplId &&
                        x.Year == imported.Year &&
                        x.Month == imported.Month);

                decimal existingTotal =
                    existing?.Hours ?? 0;

                decimal totalHours =
                    existingTotal + imported.Hours;

                decimal standardHours =
                    standardLookup.TryGetValue(
                        (imported.Year, imported.Month),
                        out var std)
                        ? std
                        : 0;

                decimal allowedHours =
                    standardHours * 1.30m;

                if (imported.Type.ToUpper() != "PLC" &&
                   totalHours > allowedHours &&
                   imported.Hours > 0)
                {
                    validationErrors.Add(
                        $"Employee '{imported.EmplId}' exceeds allowed hours for " +
                        $"{imported.Month}/{imported.Year}. " +
                        $"Planned: {totalHours:N2}, " +
                        $"Allowed: {allowedHours:N2}");
                }

                //if (totalHours > allowedHours && imported.Hours > 0)
                //{
                //    validationErrors.Add(
                //        $"Employee '{imported.EmplId}' exceeds allowed hours for " +
                //        $"{imported.Month}/{imported.Year}. " +
                //        $"Planned: {totalHours:N2}, " +
                //        $"Allowed: {allowedHours:N2}");
                //}
            }

            //-------------------------------------------------------
            // STOP IMPORT
            //-------------------------------------------------------

            if (validationErrors.Any())
            {
                return BadRequest(new
                {
                    Message =
                        "Hours validation failed:\n" +
                        string.Join("\n", validationErrors),

                    Errors = validationErrors
                });
            }

            //-------------------------------------------------------
            // SAVE
            //-------------------------------------------------------

            if (!newImport)
            {
                var forecastsToRemove = plForecastData;

                ////---------------------------------------------------
                //// DO NOT REMOVE CLOSED PERIODS FOR EAC
                ////---------------------------------------------------

                //if (type.Equals(
                //    "EAC",
                //    StringComparison.OrdinalIgnoreCase) &&
                //    plan.ClosedPeriod.HasValue)
                //{
                //    forecastsToRemove = plForecastData
                //        .Where(x =>
                //            new DateOnly(
                //                x.Year,
                //                x.Month,
                //                1) >
                //            plan.ClosedPeriod.Value)
                //        .ToList();
                //}

                //_context.PlForecasts.RemoveRange(
                //    forecastsToRemove);
            }

            await _context.PlForecasts.AddRangeAsync(
                plForecasts);

            await _context.SaveChangesAsync();


            ////////////////////////////////////////////////////////////////////////////////

            random = new Random();
            var dctList = _context.PlDcts.Select(p => p.DctId).ToList();
            List<PlForecast> plForecastsDirectCost = new List<PlForecast>();

            sheet = workbook.GetSheet("Direct Cost");
            headerRow = sheet.GetRow(1);
            for (int rowIndex = 2; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                var row = sheet.GetRow(rowIndex);
                PlDct plDct = new PlDct()
                {
                    PlId = plan.PlId.GetValueOrDefault(),
                    DctId = newImport ? 0 : Convert.ToInt32(row.GetCell(2)?.ToString()),
                    AcctId = row.GetCell(4)?.ToString() ?? string.Empty,
                    OrgId = row.GetCell(3)?.ToString() ?? string.Empty,
                    AmountType = row.GetCell(1)?.ToString() ?? string.Empty,
                    IsBrd = bool.TryParse(row.GetCell(7)?.ToString(), out bool result) && result,
                    IsRev = bool.TryParse(row.GetCell(8)?.ToString(), out result) && result,
                    PlcGlc = row.GetCell(5)?.ToString() ?? string.Empty,
                    Id = row.GetCell(6)?.ToString() ?? string.Empty
                };

                if (plDct.DctId == 0)
                {
                    string sql1 = "";
                    switch (row.GetCell(1)?.ToString().ToUpper())
                    {
                        case "VENDOREMPLOYEE":
                            plDct.Type = "VENDOR EMPLOYEE";
                            sql1 = $@"Select vend_empl_id as EmplId,vend_empl_status as Status, vend_empl_name as FirstName, 0 AS Salary,0 AS PerHourRate,null AS EffectiveDate  
                            from public.vendor_employee where vend_empl_status = 'A' and
                            vend_empl_id = '{plDct.Id}';";
                            break;
                        case "VENDOR EMPLOYEE":
                            plDct.Type = "VENDOR EMPLOYEE";
                            sql1 = $@"Select vend_empl_id as EmplId,vend_empl_status as Status, vend_empl_name as FirstName, 0 AS Salary,0 AS PerHourRate,null AS EffectiveDate  
                            from public.vendor_employee where vend_empl_status = 'A' and
                            vend_empl_id = '{plDct.Id}';";
                            break;
                        case "VENDOR":
                            plDct.Type = "VENDOR";
                            sql1 = $@"Select vend_id as EmplId,vend_empl_status as Status, vend_empl_name as FirstName, 0 AS Salary,0 AS PerHourRate,null AS EffectiveDate 
                            from public.vendor_employee where vend_empl_status = 'A' and
                            vend_id = '{plDct.Id}';";
                            break;
                        case "OTHER":
                            plDct.Type = row.GetCell(1)?.ToString().ToUpper();
                            break;
                        case "EMPLOYEE":
                            plDct.Type = "EMPLOYEE";
                            sql1 = $@"
                                SELECT empl.empl_id AS EmplId, 
                                       s_empl_status_cd AS Status, 
                                       last_first_name AS FirstName, 
                                       sal_amt AS Salary,
                                       effect_dt AS EffectiveDate,
                                       hrly_amt AS PerHourRate
                                FROM empl
                                JOIN public.empl_lab_info 
                                    ON empl.empl_id = public.empl_lab_info.empl_id
                                WHERE empl.s_empl_status_cd = 'ACT' and empl.empl_id = '{plDct.Id}'
                                  AND public.empl_lab_info.end_dt = '2078-12-31';";
                            break;
                    }

                    ///////////////////////////////////////////////////////////////////////////////////
                    //var sql1 = $@"
                    //    SELECT empl.empl_id AS EmplId, 
                    //           s_empl_status_cd AS Status, 
                    //           last_first_name AS FirstName, 
                    //           sal_amt AS Salary,
                    //           hrly_amt AS PerHourRate
                    //    FROM empl
                    //    JOIN public.empl_lab_info 
                    //        ON empl.empl_id = public.empl_lab_info.empl_id
                    //    WHERE empl.empl_id = '{plDct.Id}'
                    //      AND public.empl_lab_info.end_dt = '2078-12-31';";

                    if (row.GetCell(1)?.ToString().ToUpper() != "OTHER" && row.GetCell(1)?.ToString().ToUpper() != "PLC")
                    {
                        var employeeDetails = _context.Empl_Master
                        .FromSqlRaw(sql1)
                        .ToList().FirstOrDefault();
                        if (employeeDetails != null && !string.IsNullOrWhiteSpace(employeeDetails.FirstName))
                            plDct.Category = employeeDetails.FirstName;
                        else
                        {
                            //_context.PlProjectPlans.Remove(plan);
                            //_context.SaveChanges();
                            //_projPlanService.DeleteProjectPlanAsync(plan.PlId.GetValueOrDefault()).Wait();
                            throw new Exception("Direct Cost Employee (" + plDct.Id + ") not found.");
                        }
                    }
                    else
                    {
                        //var random = new Random();
                        int number = random.Next(1, 100000); // 1 to 99999

                        if (row.GetCell(1)?.ToString().ToUpper() == "PLC")
                        {
                            plDct.Category = plDct.Type + number.ToString("D5");
                            plDct.Id = plDct.Type + number.ToString("D5");
                        }
                        else
                        {
                            plDct.Category = "TBD_" + number.ToString("D5");
                            plDct.Id = "TBD_" + number.ToString("D5");
                        }
                    }

                }
                plDcts.Add(plDct);

                if (plDct.DctId == 0)
                {
                    for (int i = 9; i < (9 + projectDurationMonths); i++)
                    {
                        try
                        {

                            var period = headerRow.GetCell(i)?.ToString() ?? string.Empty;

                            if (!string.IsNullOrEmpty(period))
                            {
                                DateTime parsedDate = DateTime.ParseExact(period, "MMM yyyy", System.Globalization.CultureInfo.InvariantCulture);

                                if (type.ToUpper() == "EAC")
                                {

                                    if (plan.ClosedPeriod <= DateOnly.FromDateTime(parsedDate))
                                    {
                                        int month = parsedDate.Month; // 6
                                        int year = parsedDate.Year;
                                        var cell = row.GetCell(i);
                                        decimal? cellValue = cell != null && !string.IsNullOrWhiteSpace(cell.ToString())
                                            ? Convert.ToDecimal(cell.ToString())
                                            : null;
                                        if (cellValue != null)
                                            plDct.PlForecasts.Add(new PlForecast() { AcctId = plDct.AcctId, OrgId = plDct.OrgId, PlId = plan.PlId.GetValueOrDefault(), DctId = plDct.DctId, ProjId = projId, Year = year, Month = month, Forecastedamt = cellValue });
                                        else
                                            plDct.PlForecasts.Add(new PlForecast() { AcctId = plDct.AcctId, OrgId = plDct.OrgId, PlId = plan.PlId.GetValueOrDefault(), DctId = plDct.DctId, ProjId = projId, Year = year, Month = month, Forecastedamt = cellValue });
                                    }

                                }
                                else
                                {
                                    int month = parsedDate.Month; // 6
                                    int year = parsedDate.Year;
                                    var cell = row.GetCell(i);
                                    decimal? cellValue = cell != null && !string.IsNullOrWhiteSpace(cell.ToString())
                                        ? Convert.ToDecimal(cell.ToString())
                                        : null;
                                    if (cellValue != null)
                                        plDct.PlForecasts.Add(new PlForecast() { AcctId = plDct.AcctId, OrgId = plDct.OrgId, PlId = plan.PlId.GetValueOrDefault(), DctId = plDct.DctId, ProjId = projId, Year = year, Month = month, Forecastedamt = cellValue });
                                    else
                                        plDct.PlForecasts.Add(new PlForecast() { AcctId = plDct.AcctId, OrgId = plDct.OrgId, PlId = plan.PlId.GetValueOrDefault(), DctId = plDct.DctId, ProjId = projId, Year = year, Month = month, Forecastedamt = cellValue });
                                }
                            }

                        }
                        catch (Exception ex)
                        {

                        }
                    }

                    var entry = _context.PlDcts.Add(plDct);
                    plDct.DctId = entry.Entity.DctId;
                    _context.SaveChanges();
                    continue;
                }


                for (int i = 9; i < (9 + projectDurationMonths); i++)
                {

                    try
                    {
                        var period = headerRow.GetCell(i)?.ToString() ?? string.Empty;

                        if (!string.IsNullOrEmpty(period))
                        {
                            DateTime parsedDate = DateTime.ParseExact(period, "MMM yyyy", System.Globalization.CultureInfo.InvariantCulture);

                            if (type.ToUpper() == "EAC")
                            {
                                if (plan.ClosedPeriod < DateOnly.FromDateTime(parsedDate))
                                {
                                    int month = parsedDate.Month; // 6
                                    int year = parsedDate.Year;

                                    if (parsedDate.Month == 8 && parsedDate.Year == 2025)
                                    {

                                    }
                                    var cell = row.GetCell(i);
                                    decimal? cellValue = cell != null && !string.IsNullOrWhiteSpace(cell.ToString())
                                        ? Convert.ToDecimal(cell.ToString())
                                        : null;
                                    if (cellValue != null)
                                        plForecastsDirectCost.Add(new PlForecast() { AcctId = plDct.AcctId, OrgId = plDct.OrgId, PlId = plan.PlId.GetValueOrDefault(), DctId = plDct.DctId, ProjId = projId, Year = year, Month = month, Forecastedamt = cellValue, Actualamt = cellValue });
                                    else
                                        plForecastsDirectCost.Add(new PlForecast() { AcctId = plDct.AcctId, OrgId = plDct.OrgId, PlId = plan.PlId.GetValueOrDefault(), DctId = plDct.DctId, ProjId = projId, Year = year, Month = month, Forecastedamt = cellValue, Actualamt = 0 });
                                }
                            }
                            else
                            {

                                int month = parsedDate.Month; // 6
                                int year = parsedDate.Year;
                                if (month == 2 && year == 2026)
                                {

                                }

                                var cell = row.GetCell(i);
                                decimal? cellValue = cell != null && !string.IsNullOrWhiteSpace(cell.ToString())
                                    ? Convert.ToDecimal(cell.ToString())
                                    : null;
                                if (cellValue != null)
                                    plForecastsDirectCost.Add(new PlForecast() { AcctId = plDct.AcctId, OrgId = plDct.OrgId, PlId = plan.PlId.GetValueOrDefault(), DctId = plDct.DctId, ProjId = projId, Year = year, Month = month, Forecastedamt = cellValue });
                                else
                                    plForecastsDirectCost.Add(new PlForecast() { AcctId = plDct.AcctId, OrgId = plDct.OrgId, PlId = plan.PlId.GetValueOrDefault(), DctId = plDct.DctId, ProjId = projId, Year = year, Month = month, Forecastedamt = cellValue });
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }

            }

            var dctsToRemove = dctList.Except(plDcts.Select(p => p.DctId).ToList()).ToList();

            if (!newImport)
            {
                List<PlForecast> newFOrcast = new List<PlForecast>();

                var updatedList = plForecastData
                            .Select(t =>
                            {
                                if (type.ToUpper() == "EAC")
                                {
                                    var source = plForecasts.FirstOrDefault(s => s.Plc == t.Plc && s.AcctId == t.AcctId && s.OrgId == t.OrgId && s.PlId == t.PlId && s.EmplId == t.EmplId && s.Month == t.Month && s.Year == t.Year && s.ProjId == t.ProjId && (new DateOnly(s.Year, s.Month, 1) >= plan.ClosedPeriod.GetValueOrDefault()));
                                    if (source != null)
                                    {
                                        t.Actualhours = source.Actualhours;
                                        t.Forecastedhours = source.Actualhours;
                                    }
                                }
                                else
                                {
                                    var source = plForecasts.FirstOrDefault(s => s.Plc == t.Plc && s.AcctId == t.AcctId && s.OrgId == t.OrgId && s.PlId == t.PlId && s.EmplId == t.EmplId && s.Month == t.Month && s.Year == t.Year && s.ProjId == t.ProjId);
                                    if (source != null)
                                    {
                                        t.Forecastedhours = source.Forecastedhours;
                                        t.Actualhours = source.Actualhours;
                                    }
                                }
                                return t;
                            }).ToList();


                var updatedListForDirectCost = updatedList
                           .Select(t =>
                           {
                               if (type.ToUpper() == "EAC")
                               {
                                   var source = plForecastsDirectCost.FirstOrDefault(s => s.PlId == t.PlId && s.DctId == t.DctId && s.Month == t.Month && s.Year == t.Year && s.ProjId == t.ProjId && (new DateOnly(s.Year, s.Month, 1) >= plan.ClosedPeriod.GetValueOrDefault()));
                                   if (source != null)
                                   {
                                       t.Actualamt = source.Actualamt;
                                       t.Forecastedamt = source.Actualamt;

                                   }
                               }
                               else
                               {
                                   var source = plForecastsDirectCost.FirstOrDefault(s => s.PlId == t.PlId && s.DctId == t.DctId && s.Month == t.Month && s.Year == t.Year && s.ProjId == t.ProjId);
                                   if (source != null)
                                   {
                                       t.Forecastedamt = source.Forecastedamt;
                                       t.Actualamt = source.Actualamt;

                                   }
                               }
                               return t;
                           }).ToList();


                List<PlForecast> newHoursForecast = new List<PlForecast>();
                newHoursForecast = updatedList.Where(p => p.Forecastid == 0 && p.EmplId != null).ToList();

                var test = plForecasts
                            .Select(t =>
                            {
                                var source = updatedList.Where(p => p.EmplId != null).FirstOrDefault(s => s.PlId == t.PlId && s.EmplId == t.EmplId && s.Month == t.Month && s.Year == t.Year && s.ProjId == t.ProjId && s.Plc == t.Plc);
                                if (source == null)
                                {
                                    newHoursForecast.Add(t);
                                }
                                return t;
                            }).ToList();


                test = plForecastsDirectCost
               .Select(t =>
               {
                   var source = updatedList.Where(p => p.EmplId == null).FirstOrDefault(s => s.PlId == t.PlId && s.DctId == t.DctId && s.Month == t.Month && s.Year == t.Year && s.ProjId == t.ProjId);
                   if (source == null)
                   {
                       newFOrcast.Add(t);
                   }
                   return t;
               }).ToList();
                _context.PlForecasts.AddRange(newHoursForecast);
                _context.PlForecasts.AddRange(newFOrcast);

                _context.PlForecasts.UpdateRange(updatedListForDirectCost);
            }
            else
            {
                _context.PlForecasts.UpdateRange(plForecasts);
            }
            _context.SaveChanges();


            ////////////////////////////////////////////////////////////////////////////////////
            //-------------------------------------------------------
            // CALCULATE
            //-------------------------------------------------------

            PlForecastRepository repo =
                new PlForecastRepository(
                    _context,
                    _config);

            await repo.CalculateRevenueCost(
                plan.PlId.GetValueOrDefault(),
                plan.TemplateId.GetValueOrDefault(),
                plan.PlType);

            //-------------------------------------------------------
            // RESPONSE
            //-------------------------------------------------------

            if (newImport)
            {
                return Ok(
                    $"Successfully Imported and Created new '{type}' plan.");
            }

            return Ok(
                $"Successfully Imported and Updated existing '{type}' plan.");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to import plan");

            if (newImport && plan?.PlId != null)
            {
                await _projPlanService.DeleteProjectPlanAsync(
                    plan.PlId.GetValueOrDefault());
            }

            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("ImportDirectCostPlanV1")]
    public async Task<IActionResult> ImportDirectCostPlanV1(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        _context.ChangeTracker.AutoDetectChangesEnabled = false;

        using var trx = await _context.Database.BeginTransactionAsync();

        try
        {
            // 🔹 READ EXCEL
            using var stream = file.OpenReadStream();
            var workbook = new XSSFWorkbook(stream);
            var sheet = workbook.GetSheet("Hours");

            var infoRow = sheet.GetRow(0);

            string projId = infoRow?.GetCell(1)?.ToString() ?? "";
            string type = infoRow?.GetCell(3)?.ToString() ?? "";
            string versionStr = infoRow?.GetCell(5)?.ToString() ?? "";

            if (string.IsNullOrEmpty(projId))
                return BadRequest("Invalid Project Id");

            // 🔹 LOAD PLAN
            var plan = await _context.PlProjectPlans
                .Include(p => p.Proj)
                .FirstOrDefaultAsync(p =>
                    p.ProjId == projId &&
                    p.PlType == type &&
                    (string.IsNullOrEmpty(versionStr) || p.Version == Convert.ToInt32(versionStr)));

            if (plan == null)
                return NotFound("Plan not found");

            if (plan.Status.ToUpper() != "IN PROGRESS")
                return BadRequest($"Plan is not editable. Status: {plan.Status}");

            int durationMonths =
                (plan.ProjEndDt.Value.Year - plan.ProjStartDt.Value.Year) * 12 +
                plan.ProjEndDt.Value.Month - plan.ProjStartDt.Value.Month + 1;

            // 🔹 PRELOAD DATA
            var employeesDb = await _context.PlEmployeees
                .Where(x => x.PlId == plan.PlId)
                .ToListAsync();

            //var empDict = employeesDb.ToDictionary(
            //    x => $"{x.EmplId}|{x.OrgId}|{x.AccId}|{x.PlcGlcCode}");

            var empDict = employeesDb
                .GroupBy(x => $"{x.EmplId}|{x.OrgId}|{x.AccId}|{x.PlcGlcCode}")
                .ToDictionary(g => g.Key, g => g.First());

            var forecastsDb = await _context.PlForecasts
                .Where(x => x.PlId == plan.PlId)
                .ToListAsync();

            var forecastDict = forecastsDb.ToDictionary(
                x => $"{x.EmplId}|{x.Month}|{x.Year}|{x.Plc}|{x.AcctId}|{x.OrgId}");

            // 🔹 BATCH COLLECTIONS
            var newEmployees = new List<PlEmployeee>();
            var newForecasts = new List<PlForecast>();
            var updateForecasts = new List<PlForecast>();

            var headerRow = sheet.GetRow(1);

            // 🔹 LOOP ROWS
            for (int r = 2; r <= sheet.LastRowNum; r++)
            {
                var row = sheet.GetRow(r);
                if (row == null) continue;

                var employee = new PlEmployeee
                {
                    EmplId = row.GetCell(2)?.ToString() ?? "",
                    OrgId = row.GetCell(3)?.ToString() ?? "",
                    AccId = row.GetCell(4)?.ToString() ?? "",
                    PlcGlcCode = row.GetCell(5)?.ToString() ?? "",
                    Type = row.GetCell(1)?.ToString() ?? "",
                    PlId = plan.PlId
                };

                var key = $"{employee.EmplId}|{employee.OrgId}|{employee.AccId}|{employee.PlcGlcCode}";

                if (!empDict.TryGetValue(key, out var existingEmp))
                {
                    newEmployees.Add(employee);
                    empDict[key] = employee;
                }
                else
                {
                    employee = existingEmp;
                }

                // 🔹 MONTH LOOP
                for (int i = 9; i < 9 + durationMonths; i++)
                {
                    var period = headerRow.GetCell(i)?.ToString();
                    if (string.IsNullOrEmpty(period)) continue;

                    DateTime parsedDate;
                    if (!DateTime.TryParseExact(period, "MMM yyyy",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out parsedDate))
                        continue;

                    var cell = row.GetCell(i);
                    decimal value = 0;

                    if (cell != null)
                    {
                        if (cell.CellType == NPOI.SS.UserModel.CellType.Numeric)
                            value = (decimal)cell.NumericCellValue;
                        else
                            decimal.TryParse(cell.ToString(), out value);
                    }

                    var fKey = $"{employee.EmplId}|{parsedDate.Month}|{parsedDate.Year}|{employee.PlcGlcCode}|{employee.AccId}|{employee.OrgId}";

                    if (forecastDict.TryGetValue(fKey, out var existingForecast))
                    {
                        existingForecast.Forecastedhours = value;
                        updateForecasts.Add(existingForecast);
                    }
                    else
                    {
                        newForecasts.Add(new PlForecast
                        {
                            PlId = plan.PlId.Value,
                            EmplId = employee.EmplId,
                            empleId = employee.Id,
                            ProjId = projId,
                            Month = parsedDate.Month,
                            Year = parsedDate.Year,
                            Forecastedhours = value,
                            OrgId = employee.OrgId,
                            AcctId = employee.AccId,
                            Plc = employee.PlcGlcCode
                        });
                    }
                }
            }

            // 🔹 SAVE BATCHES
            if (newEmployees.Any())
                await _context.PlEmployeees.AddRangeAsync(newEmployees);

            if (newForecasts.Any())
                await _context.PlForecasts.AddRangeAsync(newForecasts);

            if (updateForecasts.Any())
                _context.PlForecasts.UpdateRange(updateForecasts);

            await _context.SaveChangesAsync();
            await trx.CommitAsync();

            return Ok(new
            {
                Message = "Import completed",
                EmployeesAdded = newEmployees.Count,
                ForecastsAdded = newForecasts.Count,
                ForecastsUpdated = updateForecasts.Count
            });
        }
        catch (Exception ex)
        {
            await trx.RollbackAsync();
            return StatusCode(500, ex.Message);
        }
        finally
        {
            _context.ChangeTracker.AutoDetectChangesEnabled = true;
        }
    }

    //[HttpPost("ImportDirectCostPlan-Bulk")]
    //public async Task<IActionResult> ImportDirectCostPlanBulk(IFormFile file)
    //{
    //    if (file == null || file.Length == 0)
    //        return BadRequest("No file uploaded.");

    //    using var trx = await _context.Database.BeginTransactionAsync();

    //    _context.ChangeTracker.AutoDetectChangesEnabled = false;

    //    try
    //    {
    //        using var stream = file.OpenReadStream();
    //        var workbook = new XSSFWorkbook(stream);
    //        var sheet = workbook.GetSheet("Hours");

    //        var infoRow = sheet.GetRow(0);

    //        string projId = infoRow?.GetCell(1)?.ToString() ?? "";
    //        string type = infoRow?.GetCell(3)?.ToString() ?? "";
    //        string versionStr = infoRow?.GetCell(5)?.ToString() ?? "";

    //        var plan = await _context.PlProjectPlans
    //            .FirstOrDefaultAsync(p =>
    //                p.ProjId == projId &&
    //                p.PlType == type &&
    //                (string.IsNullOrEmpty(versionStr) || p.Version == Convert.ToInt32(versionStr)));

    //        if (plan == null)
    //            return NotFound("Plan not found");

    //        int durationMonths =
    //            (plan.ProjEndDt.Value.Year - plan.ProjStartDt.Value.Year) * 12 +
    //            plan.ProjEndDt.Value.Month - plan.ProjStartDt.Value.Month + 1;

    //        // 🔹 PRELOAD EXISTING (NO TRACKING)
    //        var existingEmployees = await _context.PlEmployeees
    //            .AsNoTracking()
    //            .Where(x => x.PlId == plan.PlId)
    //            .ToListAsync();

    //        var empDict = existingEmployees
    //            .GroupBy(x => $"{x.EmplId}|{x.OrgId}|{x.AccId}|{x.PlcGlcCode}")
    //            .ToDictionary(g => g.Key, g => g.First());

    //        var existingForecasts = await _context.PlForecasts
    //            .AsNoTracking()
    //            .Where(x => x.PlId == plan.PlId)
    //            .ToListAsync();

    //        var forecastDict = existingForecasts
    //            .GroupBy(x => $"{x.EmplId}|{x.Month}|{x.Year}|{x.Plc}|{x.AcctId}|{x.OrgId}")
    //            .ToDictionary(g => g.Key, g => g.First());

    //        var headerRow = sheet.GetRow(1);

    //        var employeesToInsert = new List<PlEmployeee>();
    //        var forecastsToUpsert = new List<PlForecast>();

    //        // 🔹 PROCESS EXCEL
    //        for (int r = 2; r <= sheet.LastRowNum; r++)
    //        {
    //            var row = sheet.GetRow(r);
    //            if (row == null) continue;

    //            var employee = new PlEmployeee
    //            {
    //                EmplId = row.GetCell(2)?.ToString() ?? "",
    //                OrgId = row.GetCell(3)?.ToString() ?? "",
    //                AccId = row.GetCell(4)?.ToString() ?? "",
    //                PlcGlcCode = row.GetCell(5)?.ToString() ?? "",
    //                Type = row.GetCell(1)?.ToString() ?? "",
    //                PlId = plan.PlId
    //            };

    //            var empKey = $"{employee.EmplId}|{employee.OrgId}|{employee.AccId}|{employee.PlcGlcCode}";

    //            if (!empDict.ContainsKey(empKey))
    //            {
    //                employeesToInsert.Add(employee);
    //                empDict[empKey] = employee;
    //            }
    //            else
    //            {
    //                employee = empDict[empKey];
    //            }

    //            for (int i = 9; i < 9 + durationMonths; i++)
    //            {
    //                var period = headerRow.GetCell(i)?.ToString();
    //                if (string.IsNullOrEmpty(period)) continue;

    //                if (!DateTime.TryParseExact(period, "MMM yyyy",
    //                    CultureInfo.InvariantCulture,
    //                    DateTimeStyles.None,
    //                    out var parsedDate))
    //                    continue;

    //                var cell = row.GetCell(i);
    //                decimal value = 0;

    //                if (cell != null)
    //                {
    //                    if (cell.CellType == CellType.Numeric)
    //                        value = (decimal)cell.NumericCellValue;
    //                    else
    //                        decimal.TryParse(cell.ToString(), out value);
    //                }

    //                var fKey = $"{employee.EmplId}|{parsedDate.Month}|{parsedDate.Year}|{employee.PlcGlcCode}|{employee.AccId}|{employee.OrgId}";

    //                forecastsToUpsert.Add(new PlForecast
    //                {
    //                    PlId = plan.PlId.Value,
    //                    EmplId = employee.EmplId,
    //                    empleId = employee.Id,
    //                    ProjId = projId,
    //                    Month = parsedDate.Month,
    //                    Year = parsedDate.Year,
    //                    Forecastedhours = value,
    //                    OrgId = employee.OrgId,
    //                    AcctId = employee.AccId,
    //                    Plc = employee.PlcGlcCode
    //                });
    //            }
    //        }

    //        // 🔹 BULK INSERT EMPLOYEES
    //        if (employeesToInsert.Any())
    //        {
    //            await _context.BulkInsertAsync(employeesToInsert, new BulkConfig
    //            {
    //                BatchSize = 5000,
    //                SetOutputIdentity = true
    //            });
    //        }

    //        // 🔹 REMOVE DUPLICATES (IMPORTANT)
    //        forecastsToUpsert = forecastsToUpsert
    //            .GroupBy(x => new
    //            {
    //                x.PlId,
    //                x.EmplId,
    //                x.Month,
    //                x.Year,
    //                x.ProjId,
    //                x.Plc,
    //                x.AcctId,
    //                x.OrgId
    //            })
    //            .Select(g => g.Last())
    //            .ToList();

    //        // 🔥 BULK UPSERT FORECASTS
    //        await _context.BulkInsertOrUpdateAsync(forecastsToUpsert, new BulkConfig
    //        {
    //            BatchSize = 5000,
    //            UpdateByProperties = new List<string>
    //        {
    //            nameof(PlForecast.PlId),
    //            nameof(PlForecast.EmplId),
    //            nameof(PlForecast.Month),
    //            nameof(PlForecast.Year),
    //            nameof(PlForecast.ProjId),
    //            nameof(PlForecast.Plc),
    //            nameof(PlForecast.AcctId),
    //            nameof(PlForecast.OrgId)
    //        }
    //        });

    //        await trx.CommitAsync();

    //        return Ok(new
    //        {
    //            Message = "Bulk import completed successfully",
    //            EmployeesInserted = employeesToInsert.Count,
    //            ForecastsProcessed = forecastsToUpsert.Count
    //        });
    //    }
    //    catch (Exception ex)
    //    {
    //        await trx.RollbackAsync();
    //        return StatusCode(500, ex.Message);
    //    }
    //    finally
    //    {
    //        _context.ChangeTracker.AutoDetectChangesEnabled = true;
    //    }
    //}

    [HttpPost("ImportDirectCostPlan")]
    public async Task<IActionResult> ImportDirectCostPlan(IFormFile file)
    {
        bool newImport = false; int closingMonth = 0, closingYear = 0;
        _logger.LogInformation("ImportPlan called");

        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var plForecastData = new List<PlForecast>();

        PlProjectPlan plan = new PlProjectPlan();
        string projId = string.Empty, type = string.Empty, version = string.Empty;
        try
        {
            List<PlEmployeee> plEmployees = new List<PlEmployeee>();
            List<PlDct> plDcts = new List<PlDct>();
            var random = new Random();

            using var stream = file.OpenReadStream();
            var workbook = new XSSFWorkbook(stream);
            var sheet = workbook.GetSheet("Hours");

            var infoRow = sheet.GetRow(0);
            if (infoRow != null)
            {
                projId = infoRow.GetCell(1)?.ToString() ?? string.Empty;
                type = infoRow.GetCell(3)?.ToString() ?? string.Empty;
                version = infoRow.GetCell(5)?.ToString() ?? string.Empty;

            }
            if (string.IsNullOrEmpty(version))
            {
                var proj = _context.PlProjects.FirstOrDefault(p => p.ProjId == projId);
                if (proj == null)
                {
                    return NotFound("Project Not Found - " + projId);
                }

                newImport = true;
                plan = await _projPlanService.AddProjectPlanAsync(new PlProjectPlan
                {
                    TemplateId = 1,
                    ProjId = projId,
                    Status = "In Progress",
                    PlType = type,
                    Type = "A",
                    ProjStartDt = proj.ProjStartDt,
                    ProjEndDt = proj.ProjEndDt,
                    Source = "EXCEL"
                }, "Excel");
                version = plan.Version.ToString();
            }

            plan = _context.PlProjectPlans.Where(p => p.ProjId == projId && p.Version == Convert.ToInt32(version) && p.PlType == type).Include(p => p.Proj).FirstOrDefault();

            if (plan.ClosedPeriod.HasValue)
            {
                closingMonth = plan.ClosedPeriod.Value.Month;
                closingYear = plan.ClosedPeriod.Value.Year;
            }

            if (plan != null)
            {
                if (plan?.Status.ToUpper() != "IN PROGRESS")
                {
                    return StatusCode(500, $"Import failed: Budget status is '{plan.Status}' for Project '{projId}' with Version '{version}'. If you want to Import update status to 'Working'");
                }

                plForecastData = _context.PlForecasts.Where(p => p.PlId == plan.PlId).ToList();
            }
            else
            {
                return StatusCode(500, "An error occurred while importing the plan.");
            }

            var project = plan.Proj;
            projId = plan.ProjId;
            //int projectDurationMonths = (project.ProjEndDt.GetValueOrDefault().Year -
            //                project.ProjStartDt.GetValueOrDefault().Year) * 12 +
            //                project.ProjEndDt.GetValueOrDefault().Month -
            //                project.ProjStartDt.GetValueOrDefault().Month + 1;

            int projectDurationMonths = (plan.ProjEndDt.GetValueOrDefault().Year -
                plan.ProjStartDt.GetValueOrDefault().Year) * 12 +
                plan.ProjEndDt.GetValueOrDefault().Month -
                plan.ProjStartDt.GetValueOrDefault().Month + 1;

            var emplPeriod = new Dictionary<string, string>();
            var plForecasts = new List<PlForecast>();

            var headerRow = sheet.GetRow(1);

            //Get EMployee List
            var emplList = _context.PlEmployeees.Where(p => p.PlId == plan.PlId).ToList();
            for (int rowIndex = 2; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                var row = sheet.GetRow(rowIndex);

                if (row.GetCell(2)?.ToString() == "9030910")
                {

                }
                PlEmployeee employee = new PlEmployeee()
                {
                    EmplId = row.GetCell(2)?.ToString() ?? string.Empty,
                    AccId = row.GetCell(4)?.ToString() ?? string.Empty,
                    IsBrd = bool.TryParse(row.GetCell(7)?.ToString(), out bool result) && result,
                    IsRev = bool.TryParse(row.GetCell(8)?.ToString(), out result) && result,
                    PlcGlcCode = row.GetCell(5)?.ToString() ?? string.Empty,
                    OrgId = row.GetCell(3)?.ToString() ?? string.Empty,
                    Type = row.GetCell(1)?.ToString() ?? string.Empty,
                    PlId = plan.PlId,
                    PerHourRate = double.TryParse(row.GetCell(6)?.ToString() ?? string.Empty, out double d) ? (decimal)d : 0m
                };
                //plEmployees.Add(employee);

                var existingEmployee = emplList.Where(p => p.EmplId == employee.EmplId && p.OrgId == employee.OrgId && p.AccId == employee.AccId && p.PlcGlcCode == employee.PlcGlcCode).ToList();

                if (existingEmployee == null || existingEmployee.Count() == 0)
                {
                    string sql1 = "";

                    if (employee.EmplId == "MIKE.KITTREDGE")
                    {

                    }

                    switch (row.GetCell(1)?.ToString().ToUpper())
                    {

                        case "VENDOREMPLOYEE":
                            employee.Type = "VENDOR EMPLOYEE";
                            sql1 = $@"Select vend_empl_id as EmplId,vend_empl_status as Status, vend_empl_name as FirstName, 0 AS Salary,0 AS PerHourRate, null AS EffectiveDate  
                            from public.vendor_employee where vend_empl_status = 'A' and
                            vend_empl_id = '{employee.EmplId}';";
                            break;
                        case "VENDOR EMPLOYEE":
                            employee.Type = "VENDOR EMPLOYEE";
                            sql1 = $@"Select vend_empl_id as EmplId,vend_empl_status as Status, vend_empl_name as FirstName, 0 AS Salary,0 AS PerHourRate, null AS EffectiveDate  
                            from public.vendor_employee where vend_empl_status = 'A' and
                            vend_empl_id = '{employee.EmplId}';";
                            break;
                        case "VENDOR":
                            employee.Type = "VENDOR";
                            sql1 = $@"Select vend_id as EmplId,vend_empl_status as Status, vend_empl_name as FirstName, 0 AS Salary,0 AS PerHourRate,null AS EffectiveDate  
                            from public.vendor_employee where vend_empl_status = 'A' and
                            vend_id = '{employee.EmplId}';";
                            break;
                        case "OTHER":
                            employee.Type = row.GetCell(1)?.ToString().ToUpper();
                            break;
                        case "EMPLOYEE":
                            employee.Type = "EMPLOYEE";
                            sql1 = $@"
                                SELECT empl.empl_id AS EmplId, 
                                       s_empl_status_cd AS Status, 
                                       last_first_name AS FirstName, 
                                       sal_amt AS Salary,
                                       effect_dt AS EffectiveDate,
                                       hrly_amt AS PerHourRate
                                FROM empl
                                JOIN public.empl_lab_info 
                                    ON empl.empl_id = public.empl_lab_info.empl_id
                                WHERE empl.s_empl_status_cd = 'ACT' and empl.empl_id = '{employee.EmplId}' 
                                  AND public.empl_lab_info.end_dt = '2078-12-31';";
                            break;
                    }

                    if (row.GetCell(1)?.ToString().ToUpper() == "OTHER" || row.GetCell(1)?.ToString().ToUpper() == "PLC")
                    {

                        int number = random.Next(1, 100000); // 1 to 99999

                        if (row.GetCell(1)?.ToString().ToUpper() == "PLC")
                        {
                            employee.EmplId = employee.Type + "_" + number.ToString("D5");
                        }
                        else
                        {
                            employee.EmplId = "TBD_" + number.ToString("D5");
                            employee.FirstName = employee.EmplId;

                        }

                        ////////////////////////////////////////////////////////////////////////////////////////////
                        var entry = _context.PlEmployeees.Add(employee);
                        _context.SaveChanges();
                        employee.Id = entry.Entity.Id;
                    }
                    else
                    {
                        var employeeDetails = _context.Empl_Master
                                            .FromSqlRaw(sql1)
                                            .ToList().FirstOrDefault();

                        if (employeeDetails != null)
                        {
                            if (!string.IsNullOrWhiteSpace(employeeDetails.FirstName))
                            {
                                var names = employeeDetails.FirstName.Split(',', 2);

                                employee.LastName = names[0];
                                employee.FirstName = names.Length > 1 ? names[1] : names[0];
                            }

                            if (employee.Type.ToUpper() == "EMPLOYEE")
                            {
                                employee.Salary = employeeDetails.Salary;
                                employee.PerHourRate = employeeDetails.PerHourRate;
                            }
                            if (existingEmployee == null)
                            {
                                var entry = _context.PlEmployeees.Add(employee);
                                _context.SaveChanges();
                                employee.Id = entry.Entity.Id;
                            }
                        }
                        else
                        {

                            sql1 = $@"
                                SELECT empl.empl_id AS EmplId, 
                                       s_empl_status_cd AS Status, 
                                       last_first_name AS FirstName, 
                                       sal_amt AS Salary,
                                       effect_dt AS EffectiveDate,
                                       hrly_amt AS PerHourRate
                                FROM empl
                                JOIN public.empl_lab_info 
                                    ON empl.empl_id = public.empl_lab_info.empl_id
                                WHERE empl.s_empl_status_cd = 'ACT' and empl.empl_id = '{employee.EmplId}' 
                                  AND public.empl_lab_info.end_dt = '2078-12-31';";

                            employeeDetails = _context.Empl_Master
                                            .FromSqlRaw(sql1)
                                            .ToList().FirstOrDefault();

                            if (employeeDetails != null)
                            {
                                if (!string.IsNullOrWhiteSpace(employeeDetails.FirstName))
                                {
                                    var names = employeeDetails.FirstName.Split(',', 2);

                                    employee.LastName = names[0];
                                    employee.FirstName = names.Length > 1 ? names[1] : names[0];
                                }

                                if (employee.Type.ToUpper() == "EMPLOYEE")
                                {
                                    employee.Salary = employeeDetails.Salary;
                                    employee.PerHourRate = employeeDetails.PerHourRate;
                                }

                                var entry = _context.PlEmployeees.Add(employee);
                                _context.SaveChanges();
                                employee.Id = entry.Entity.Id;
                            }
                            else
                                throw new Exception("Employee (" + employee.EmplId + ") not found.");

                            //_projPlanService.DeleteProjectPlanAsync(plan.PlId.GetValueOrDefault()).Wait();
                            //_context.PlProjectPlans.Remove(plan);
                            //_context.SaveChanges();
                        }
                    }

                }

                if (employee.Id == 0 && existingEmployee.Count() > 0)
                {
                    employee = existingEmployee.FirstOrDefault();
                }
                //else
                //{
                //    if(employee.Type.ToUpper() == "OTHER")
                //    {
                //        var entry = _context.PlEmployeees.Add(employee);
                //        _context.SaveChanges();
                //        employee.Id = entry.Entity.Id;
                //    }
                //}
                for (int i = 9; i < (9 + projectDurationMonths); i++)
                {
                    try
                    {
                        var period = headerRow.GetCell(i)?.ToString() ?? string.Empty;

                        var cell = row.GetCell(i);
                        decimal forecastedHours = 0;

                        if (cell != null)
                        {
                            switch (cell.CellType)
                            {
                                case NPOI.SS.UserModel.CellType.Numeric:
                                    forecastedHours = (decimal)cell.NumericCellValue;
                                    break;

                                case NPOI.SS.UserModel.CellType.String:
                                    decimal.TryParse(cell.StringCellValue, out forecastedHours);
                                    break;

                                case NPOI.SS.UserModel.CellType.Formula:
                                    if (cell.CachedFormulaResultType == NPOI.SS.UserModel.CellType.Numeric)
                                        forecastedHours = (decimal)cell.NumericCellValue;
                                    else if (cell.CachedFormulaResultType == NPOI.SS.UserModel.CellType.String)
                                        decimal.TryParse(cell.StringCellValue, out forecastedHours);
                                    break;
                            }
                        }


                        DateTime parsedDate = DateTime.ParseExact(period, "MMM yyyy", System.Globalization.CultureInfo.InvariantCulture);
                        if (parsedDate.Month == 8 && parsedDate.Year == 2025)
                        {

                        }
                        if (type.ToUpper() == "EAC")
                        {
                            int month = parsedDate.Month; // 6
                            int year = parsedDate.Year;

                            if (plan.ClosedPeriod < DateOnly.FromDateTime(parsedDate))
                            {

                                if (!string.IsNullOrEmpty(period))
                                {
                                    plForecasts.Add(new PlForecast() { AcctId = employee.AccId, OrgId = employee.OrgId, PlId = plan.PlId.GetValueOrDefault(), empleId = employee.Id, Plc = employee.PlcGlcCode, EmplId = employee.EmplId, ProjId = projId, Year = year, Month = month, Actualhours = Convert.ToDecimal(forecastedHours) });
                                }
                                else
                                {
                                    plForecasts.Add(new PlForecast() { AcctId = employee.AccId, OrgId = employee.OrgId, PlId = plan.PlId.GetValueOrDefault(), empleId = employee.Id, Plc = employee.PlcGlcCode, EmplId = employee.EmplId, ProjId = projId, Year = year, Month = month, Actualhours = Convert.ToDecimal(0) });
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(period))
                                {
                                    plForecasts.Add(new PlForecast() { AcctId = employee.AccId, OrgId = employee.OrgId, PlId = plan.PlId.GetValueOrDefault(), empleId = employee.Id, Plc = employee.PlcGlcCode, EmplId = employee.EmplId, ProjId = projId, Year = year, Month = month, Actualhours = forecastedHours, Forecastedhours = forecastedHours });
                                    //plForecasts.Add(new PlForecast() { PlId = plan.PlId.GetValueOrDefault(), EmplId = employee.EmplId, ProjId = projId, Year = year, Month = month, Forecastedhours = Convert.ToDecimal(row.GetCell(i).ToString()) });
                                }
                                else
                                {
                                    plForecasts.Add(new PlForecast() { AcctId = employee.AccId, OrgId = employee.OrgId, PlId = plan.PlId.GetValueOrDefault(), empleId = employee.Id, Plc = employee.PlcGlcCode, EmplId = employee.EmplId, ProjId = projId, Year = year, Month = month, Actualhours = Convert.ToDecimal(0), Forecastedhours = forecastedHours });
                                    //plForecasts.Add(new PlForecast() { PlId = plan.PlId.GetValueOrDefault(), EmplId = employee.EmplId, ProjId = projId, Year = year, Month = month, Forecastedhours = Convert.ToDecimal(0) });
                                }
                            }
                        }
                        else
                        {
                            int month = parsedDate.Month; // 6
                            int year = parsedDate.Year;

                            if (!string.IsNullOrEmpty(period))
                            {
                                plForecasts.Add(new PlForecast() { AcctId = employee.AccId, OrgId = employee.OrgId, PlId = plan.PlId.GetValueOrDefault(), empleId = employee.Id, Plc = employee.PlcGlcCode, EmplId = employee.EmplId, ProjId = projId, Year = year, Month = month, Forecastedhours = forecastedHours });
                            }
                            else
                            {
                                plForecasts.Add(new PlForecast() { AcctId = employee.AccId, OrgId = employee.OrgId, PlId = plan.PlId.GetValueOrDefault(), empleId = employee.Id, Plc = employee.PlcGlcCode, EmplId = employee.EmplId, ProjId = projId, Year = year, Month = month, Forecastedhours = Convert.ToDecimal(0) });
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }


            /////////////////////////Validation Code
            ///

            // -------------------------------------------------------
            // VALIDATE 130% UTILIZATION
            // -------------------------------------------------------

            var validationErrors = new List<string>();

            var pl_ids = _context.PlProjectPlans.Where(x => x.FinalVersion == true).Select(p => p.PlId).ToList();

            Helper scheduleHelper = new Helper(_context, _config);

            // IMPORTED HOURS
            var importedHours = plForecasts
                .GroupBy(x => new
                {
                    x.EmplId,
                    x.Year,
                    x.Month
                })
                .Select(g => new
                {
                    g.Key.EmplId,
                    g.Key.Year,
                    g.Key.Month,
                    Hours = g.Sum(x =>
                        type.ToUpper() == "EAC"
                            ? x.Actualhours
                            : x.Forecastedhours)
                })
                .ToList();

            // EMPLOYEE IDS
            var employeeIds = importedHours
                .Where(x => !string.IsNullOrWhiteSpace(x.EmplId))
                .Select(x => x.EmplId)
                .Distinct()
                .ToList();

            // EXISTING HOURS FROM OTHER PLANS
            var existingHours = await _context.PlForecasts
                .Where(x =>
                    employeeIds.Contains(x.EmplId) &&
                    x.PlId != plan.PlId && pl_ids.Contains(x.PlId))
                .GroupBy(x => new
                {
                    x.EmplId,
                    x.Year,
                    x.Month
                })
                .Select(g => new
                {
                    g.Key.EmplId,
                    g.Key.Year,
                    g.Key.Month,
                    Hours = g.Sum(x =>
                        type.ToUpper() == "EAC"
                            ? x.Actualhours
                            : x.Forecastedhours)
                })
                .ToListAsync();

            // DATE RANGE
            var minYear = importedHours.Min(x => x.Year);
            var minMonth = importedHours.Min(x => x.Month);

            var maxYear = importedHours.Max(x => x.Year);
            var maxMonth = importedHours.Max(x => x.Month);

            var startDate = new DateOnly(minYear, minMonth, 1);

            var endDate = new DateOnly(
                maxYear,
                maxMonth,
                DateTime.DaysInMonth(maxYear, maxMonth));

            // STANDARD SCHEDULE
            var schedules = scheduleHelper
                .GetWorkingDaysForDuration(startDate, endDate);

            var standardLookup = schedules
                .ToDictionary(
                    x => (x.Year, x.MonthNo),
                    x => x.WorkingHours);

            // VALIDATE EACH EMPLOYEE/MONTH
            foreach (var imported in importedHours)
            {
                if (imported.EmplId.StartsWith("PLC_"))
                {
                    continue;
                }

                if (imported.Hours == 0)
                    continue;
                // -------------------------------------------------------
                // SKIP CLOSED PERIOD FOR EAC
                // -------------------------------------------------------

                if (type.ToUpper() == "EAC" &&
                    plan.ClosedPeriod.HasValue)
                {
                    var currentPeriod =
                        new DateOnly(imported.Year, imported.Month, 1);

                    if (currentPeriod <= plan.ClosedPeriod.Value)
                    {
                        continue;
                    }
                }

                // -------------------------------------------------------
                // EXISTING HOURS
                // -------------------------------------------------------

                var existing = existingHours
                    .FirstOrDefault(x =>
                        x.EmplId == imported.EmplId &&
                        x.Year == imported.Year &&
                        x.Month == imported.Month);

                decimal existingTotal =
                    existing?.Hours ?? 0;

                decimal totalHours =
                    existingTotal + imported.Hours;

                // -------------------------------------------------------
                // STANDARD HOURS
                // -------------------------------------------------------

                decimal standardHours =
                    standardLookup.TryGetValue(
                        (imported.Year, imported.Month),
                        out var stdHours)
                        ? Convert.ToDecimal(stdHours)
                        : 160m;

                decimal allowedHours =
                    standardHours * 1.30m;

                // -------------------------------------------------------
                // VALIDATION
                // -------------------------------------------------------

                if (totalHours > allowedHours)
                {
                    // -------------------------------------------------------
                    // GET ALTERNATE EMPLOYEES
                    // -------------------------------------------------------

                    var alternateEmployees =
                        await scheduleHelper.GetAlternateEmployees(
                            imported.Year,
                            imported.Month,
                            totalHours - allowedHours,
                            allowedHours,
                            standardHours,
                            null,
                            null,
                            null);

                    var alternateEmployeeMessage =
                        alternateEmployees.Take(5).Any()
                            ? "\nAlternative Employees:\n" +
                              string.Join(
                                  "\n",
                                  alternateEmployees.Select((x, i) =>
                                      $"{i + 1}. {x.EmployeeId} | " +
                                      $"Assigned: {x.AssignedHours:N2} | " +
                                      $"Available: {x.AvailableHours:N2}"
                                  ))
                            : "\nNo alternative employees available.";

                    validationErrors.Add(
                        $"Employee '{imported.EmplId}' exceeds allowed hours for " +
                        $"{imported.Month}/{imported.Year}. " +
                        $"Planned: {totalHours:N2}, " +
                        $"Allowed: {allowedHours:N2}" +
                        $"{alternateEmployeeMessage}");
                }
            }

            // -------------------------------------------------------
            // STOP IMPORT IF VALIDATION FAILED
            // -------------------------------------------------------

            if (validationErrors.Any())
            {
                return BadRequest(new
                {
                    Message =
                        "Hours validation failed:\n\n" +
                        string.Join("\n\n", validationErrors),

                    Errors = validationErrors
                });
            }

            ////////////////////////////////////////////////////////Direct COst
            random = new Random();
            var dctList = _context.PlDcts.Select(p => p.DctId).ToList();
            List<PlForecast> plForecastsDirectCost = new List<PlForecast>();

            sheet = workbook.GetSheet("Direct Cost");
            headerRow = sheet.GetRow(1);
            for (int rowIndex = 2; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                var row = sheet.GetRow(rowIndex);
                PlDct plDct = new PlDct()
                {
                    PlId = plan.PlId.GetValueOrDefault(),
                    DctId = newImport ? 0 : Convert.ToInt32(row.GetCell(2)?.ToString()),
                    AcctId = row.GetCell(4)?.ToString() ?? string.Empty,
                    OrgId = row.GetCell(3)?.ToString() ?? string.Empty,
                    AmountType = row.GetCell(1)?.ToString() ?? string.Empty,
                    IsBrd = bool.TryParse(row.GetCell(7)?.ToString(), out bool result) && result,
                    IsRev = bool.TryParse(row.GetCell(8)?.ToString(), out result) && result,
                    PlcGlc = row.GetCell(5)?.ToString() ?? string.Empty,
                    Id = row.GetCell(6)?.ToString() ?? string.Empty
                };

                if (plDct.DctId == 0)
                {
                    string sql1 = "";
                    switch (row.GetCell(1)?.ToString().ToUpper())
                    {
                        case "VENDOREMPLOYEE":
                            plDct.Type = "VENDOR EMPLOYEE";
                            sql1 = $@"Select vend_empl_id as EmplId,vend_empl_status as Status, vend_empl_name as FirstName, 0 AS Salary,0 AS PerHourRate,null AS EffectiveDate  
                            from public.vendor_employee where vend_empl_status = 'A' and
                            vend_empl_id = '{plDct.Id}';";
                            break;
                        case "VENDOR EMPLOYEE":
                            plDct.Type = "VENDOR EMPLOYEE";
                            sql1 = $@"Select vend_empl_id as EmplId,vend_empl_status as Status, vend_empl_name as FirstName, 0 AS Salary,0 AS PerHourRate,null AS EffectiveDate  
                            from public.vendor_employee where vend_empl_status = 'A' and
                            vend_empl_id = '{plDct.Id}';";
                            break;
                        case "VENDOR":
                            plDct.Type = "VENDOR";
                            sql1 = $@"Select vend_id as EmplId,vend_empl_status as Status, vend_empl_name as FirstName, 0 AS Salary,0 AS PerHourRate,null AS EffectiveDate 
                            from public.vendor_employee where vend_empl_status = 'A' and
                            vend_id = '{plDct.Id}';";
                            break;
                        case "OTHER":
                            plDct.Type = row.GetCell(1)?.ToString().ToUpper();
                            break;
                        case "EMPLOYEE":
                            plDct.Type = "EMPLOYEE";
                            sql1 = $@"
                                SELECT empl.empl_id AS EmplId, 
                                       s_empl_status_cd AS Status, 
                                       last_first_name AS FirstName, 
                                       sal_amt AS Salary,
                                       effect_dt AS EffectiveDate,
                                       hrly_amt AS PerHourRate
                                FROM empl
                                JOIN public.empl_lab_info 
                                    ON empl.empl_id = public.empl_lab_info.empl_id
                                WHERE empl.s_empl_status_cd = 'ACT' and empl.empl_id = '{plDct.Id}'
                                  AND public.empl_lab_info.end_dt = '2078-12-31';";
                            break;
                    }

                    ///////////////////////////////////////////////////////////////////////////////////
                    //var sql1 = $@"
                    //    SELECT empl.empl_id AS EmplId, 
                    //           s_empl_status_cd AS Status, 
                    //           last_first_name AS FirstName, 
                    //           sal_amt AS Salary,
                    //           hrly_amt AS PerHourRate
                    //    FROM empl
                    //    JOIN public.empl_lab_info 
                    //        ON empl.empl_id = public.empl_lab_info.empl_id
                    //    WHERE empl.empl_id = '{plDct.Id}'
                    //      AND public.empl_lab_info.end_dt = '2078-12-31';";

                    if (row.GetCell(1)?.ToString().ToUpper() != "OTHER" && row.GetCell(1)?.ToString().ToUpper() != "PLC")
                    {
                        var employeeDetails = _context.Empl_Master
                        .FromSqlRaw(sql1)
                        .ToList().FirstOrDefault();
                        if (employeeDetails != null && !string.IsNullOrWhiteSpace(employeeDetails.FirstName))
                            plDct.Category = employeeDetails.FirstName;
                        else
                        {
                            //_context.PlProjectPlans.Remove(plan);
                            //_context.SaveChanges();
                            //_projPlanService.DeleteProjectPlanAsync(plan.PlId.GetValueOrDefault()).Wait();
                            throw new Exception("Direct Cost Employee (" + plDct.Id + ") not found.");
                        }
                    }
                    else
                    {
                        //var random = new Random();
                        int number = random.Next(1, 100000); // 1 to 99999

                        if (row.GetCell(1)?.ToString().ToUpper() == "PLC")
                        {
                            plDct.Category = plDct.Type + number.ToString("D5");
                            plDct.Id = plDct.Type + number.ToString("D5");
                        }
                        else
                        {
                            plDct.Category = "TBD_" + number.ToString("D5");
                            plDct.Id = "TBD_" + number.ToString("D5");
                        }
                    }

                }
                plDcts.Add(plDct);

                if (plDct.DctId == 0)
                {
                    for (int i = 9; i < (9 + projectDurationMonths); i++)
                    {
                        try
                        {

                            var period = headerRow.GetCell(i)?.ToString() ?? string.Empty;

                            if (!string.IsNullOrEmpty(period))
                            {
                                DateTime parsedDate = DateTime.ParseExact(period, "MMM yyyy", System.Globalization.CultureInfo.InvariantCulture);

                                if (type.ToUpper() == "EAC")
                                {

                                    if (plan.ClosedPeriod <= DateOnly.FromDateTime(parsedDate))
                                    {
                                        int month = parsedDate.Month; // 6
                                        int year = parsedDate.Year;
                                        var cell = row.GetCell(i);
                                        decimal? cellValue = cell != null && !string.IsNullOrWhiteSpace(cell.ToString())
                                            ? Convert.ToDecimal(cell.ToString())
                                            : null;
                                        if (cellValue != null)
                                            plDct.PlForecasts.Add(new PlForecast() { AcctId = plDct.AcctId, OrgId = plDct.OrgId, PlId = plan.PlId.GetValueOrDefault(), DctId = plDct.DctId, ProjId = projId, Year = year, Month = month, Forecastedamt = cellValue });
                                        else
                                            plDct.PlForecasts.Add(new PlForecast() { AcctId = plDct.AcctId, OrgId = plDct.OrgId, PlId = plan.PlId.GetValueOrDefault(), DctId = plDct.DctId, ProjId = projId, Year = year, Month = month, Forecastedamt = cellValue });
                                    }

                                }
                                else
                                {
                                    int month = parsedDate.Month; // 6
                                    int year = parsedDate.Year;
                                    var cell = row.GetCell(i);
                                    decimal? cellValue = cell != null && !string.IsNullOrWhiteSpace(cell.ToString())
                                        ? Convert.ToDecimal(cell.ToString())
                                        : null;
                                    if (cellValue != null)
                                        plDct.PlForecasts.Add(new PlForecast() { AcctId = plDct.AcctId, OrgId = plDct.OrgId, PlId = plan.PlId.GetValueOrDefault(), DctId = plDct.DctId, ProjId = projId, Year = year, Month = month, Forecastedamt = cellValue });
                                    else
                                        plDct.PlForecasts.Add(new PlForecast() { AcctId = plDct.AcctId, OrgId = plDct.OrgId, PlId = plan.PlId.GetValueOrDefault(), DctId = plDct.DctId, ProjId = projId, Year = year, Month = month, Forecastedamt = cellValue });
                                }
                            }

                        }
                        catch (Exception ex)
                        {

                        }
                    }

                    var entry = _context.PlDcts.Add(plDct);
                    plDct.DctId = entry.Entity.DctId;
                    _context.SaveChanges();
                    continue;
                }


                for (int i = 9; i < (9 + projectDurationMonths); i++)
                {

                    try
                    {
                        var period = headerRow.GetCell(i)?.ToString() ?? string.Empty;

                        if (!string.IsNullOrEmpty(period))
                        {
                            DateTime parsedDate = DateTime.ParseExact(period, "MMM yyyy", System.Globalization.CultureInfo.InvariantCulture);

                            if (type.ToUpper() == "EAC")
                            {
                                if (plan.ClosedPeriod < DateOnly.FromDateTime(parsedDate))
                                {
                                    int month = parsedDate.Month; // 6
                                    int year = parsedDate.Year;

                                    if (parsedDate.Month == 8 && parsedDate.Year == 2025)
                                    {

                                    }
                                    var cell = row.GetCell(i);
                                    decimal? cellValue = cell != null && !string.IsNullOrWhiteSpace(cell.ToString())
                                        ? Convert.ToDecimal(cell.ToString())
                                        : null;
                                    if (cellValue != null)
                                        plForecastsDirectCost.Add(new PlForecast() { AcctId = plDct.AcctId, OrgId = plDct.OrgId, PlId = plan.PlId.GetValueOrDefault(), DctId = plDct.DctId, ProjId = projId, Year = year, Month = month, Forecastedamt = cellValue, Actualamt = cellValue });
                                    else
                                        plForecastsDirectCost.Add(new PlForecast() { AcctId = plDct.AcctId, OrgId = plDct.OrgId, PlId = plan.PlId.GetValueOrDefault(), DctId = plDct.DctId, ProjId = projId, Year = year, Month = month, Forecastedamt = cellValue, Actualamt = 0 });
                                }
                            }
                            else
                            {

                                int month = parsedDate.Month; // 6
                                int year = parsedDate.Year;
                                if (month == 2 && year == 2026)
                                {

                                }

                                var cell = row.GetCell(i);
                                decimal? cellValue = cell != null && !string.IsNullOrWhiteSpace(cell.ToString())
                                    ? Convert.ToDecimal(cell.ToString())
                                    : null;
                                if (cellValue != null)
                                    plForecastsDirectCost.Add(new PlForecast() { AcctId = plDct.AcctId, OrgId = plDct.OrgId, PlId = plan.PlId.GetValueOrDefault(), DctId = plDct.DctId, ProjId = projId, Year = year, Month = month, Forecastedamt = cellValue });
                                else
                                    plForecastsDirectCost.Add(new PlForecast() { AcctId = plDct.AcctId, OrgId = plDct.OrgId, PlId = plan.PlId.GetValueOrDefault(), DctId = plDct.DctId, ProjId = projId, Year = year, Month = month, Forecastedamt = cellValue });
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }

            }

            var dctsToRemove = dctList.Except(plDcts.Select(p => p.DctId).ToList()).ToList();

            if (!newImport)
            {
                List<PlForecast> newFOrcast = new List<PlForecast>();

                var updatedList = plForecastData
                            .Select(t =>
                            {
                                if (type.ToUpper() == "EAC")
                                {
                                    var source = plForecasts.FirstOrDefault(s => s.Plc == t.Plc && s.AcctId == t.AcctId && s.OrgId == t.OrgId && s.PlId == t.PlId && s.EmplId == t.EmplId && s.Month == t.Month && s.Year == t.Year && s.ProjId == t.ProjId && (new DateOnly(s.Year, s.Month, 1) >= plan.ClosedPeriod.GetValueOrDefault()));
                                    if (source != null)
                                    {
                                        t.Actualhours = source.Actualhours;
                                        t.Forecastedhours = source.Actualhours;
                                    }
                                }
                                else
                                {
                                    var source = plForecasts.FirstOrDefault(s => s.Plc == t.Plc && s.AcctId == t.AcctId && s.OrgId == t.OrgId && s.PlId == t.PlId && s.EmplId == t.EmplId && s.Month == t.Month && s.Year == t.Year && s.ProjId == t.ProjId);
                                    if (source != null)
                                    {
                                        t.Forecastedhours = source.Forecastedhours;
                                        t.Actualhours = source.Actualhours;
                                    }
                                }
                                return t;
                            }).ToList();


                var updatedListForDirectCost = updatedList
                           .Select(t =>
                           {
                               if (type.ToUpper() == "EAC")
                               {
                                   var source = plForecastsDirectCost.FirstOrDefault(s => s.PlId == t.PlId && s.DctId == t.DctId && s.Month == t.Month && s.Year == t.Year && s.ProjId == t.ProjId && (new DateOnly(s.Year, s.Month, 1) >= plan.ClosedPeriod.GetValueOrDefault()));
                                   if (source != null)
                                   {
                                       t.Actualamt = source.Actualamt;
                                       t.Forecastedamt = source.Actualamt;

                                   }
                               }
                               else
                               {
                                   var source = plForecastsDirectCost.FirstOrDefault(s => s.PlId == t.PlId && s.DctId == t.DctId && s.Month == t.Month && s.Year == t.Year && s.ProjId == t.ProjId);
                                   if (source != null)
                                   {
                                       t.Forecastedamt = source.Forecastedamt;
                                       t.Actualamt = source.Actualamt;

                                   }
                               }
                               return t;
                           }).ToList();


                List<PlForecast> newHoursForecast = new List<PlForecast>();
                newHoursForecast = updatedList.Where(p => p.Forecastid == 0 && p.EmplId != null).ToList();

                var test = plForecasts
                            .Select(t =>
                            {
                                var source = updatedList.Where(p => p.EmplId != null).FirstOrDefault(s => s.PlId == t.PlId && s.EmplId == t.EmplId && s.Month == t.Month && s.Year == t.Year && s.ProjId == t.ProjId && s.Plc == t.Plc);
                                if (source == null)
                                {
                                    newHoursForecast.Add(t);
                                }
                                return t;
                            }).ToList();


                test = plForecastsDirectCost
               .Select(t =>
               {
                   var source = updatedList.Where(p => p.EmplId == null).FirstOrDefault(s => s.PlId == t.PlId && s.DctId == t.DctId && s.Month == t.Month && s.Year == t.Year && s.ProjId == t.ProjId);
                   if (source == null)
                   {
                       newFOrcast.Add(t);
                   }
                   return t;
               }).ToList();
                _context.PlForecasts.AddRange(newHoursForecast);
                _context.PlForecasts.AddRange(newFOrcast);

                _context.PlForecasts.UpdateRange(updatedListForDirectCost);
            }
            else
            {
                _context.PlForecasts.UpdateRange(plForecasts);
            }
            _context.SaveChanges();
            var itemsToRemove = _context.PlDcts.Where(p => dctsToRemove.Contains(p.DctId)).ToList();
            var forecastsToRemove = _context.PlForecasts.Where(p => dctsToRemove.Contains(p.DctId.GetValueOrDefault())).ToList();

            if (itemsToRemove.Count > 0)
            {
                //_context.PlForecasts.RemoveRange(forecastsToRemove);
                //_context.PlDcts.RemoveRange(itemsToRemove);
            }
            //_context.SaveChanges();

            PlForecastRepository plForecastRepository = new PlForecastRepository(_context, _config);
            //await plForecastRepository.CalculateRevenueCost(plan.PlId.GetValueOrDefault(), plan.TemplateId.GetValueOrDefault(), plan.PlType);

            if (newImport)
            {
                var responseMessage = "Successfully Imported and Created new '" + ((type == "BUD") ? "Budget" : "EAC") + "' for Project - '" + projId + "' with Version - '" + plan?.Version + "'";
                _logger.LogInformation(responseMessage);
                return Ok(responseMessage);
            }
            else
            {
                var responseMessage = "Successfully Imported and Updated existing '" + ((type == "BUD") ? "Budget" : "EAC") + "' for Project - '" + projId + "' Having version - '" + version + "'";
                _logger.LogInformation(responseMessage);
                return Ok(responseMessage);
            }
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pgEx &&
                pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                var formatted = FormatPgDetail(pgEx.Detail);
                if (newImport)
                    _projPlanService.DeleteProjectPlanAsync(plan.PlId.GetValueOrDefault()).Wait();
                return Conflict(formatted);
            }

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import plan" + ex.Message);
            if (newImport)
                _projPlanService.DeleteProjectPlanAsync(plan.PlId.GetValueOrDefault()).Wait();
            return StatusCode(500, ex.Message);
        }
    }


    [HttpPost("ImportDirectCostPlan-Merge")]
    public async Task<IActionResult> ImportDirectCostPlanMerge(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        bool newImport = false; int closingMonth = 0, closingYear = 0;
        _logger.LogInformation("ImportPlan called");

        var plForecastData = new List<PlForecast>();

        PlProjectPlan plan = new PlProjectPlan();
        string projId = string.Empty, type = string.Empty, version = string.Empty;

        var conn = new NpgsqlConnection(_context.Database.GetConnectionString());
        await conn.OpenAsync();

        await using var trx = await conn.BeginTransactionAsync();

        try
        {
            // 🔹 1. CREATE TEMP TABLE
            var createTemp = @"
            CREATE TABLE temp_pl_forecast (
                pl_id INT,
                empl_id TEXT,
                dct_id INT,
                month INT,
                year INT,
                proj_id TEXT,
                plc TEXT,
                acct_id TEXT,
                org_id TEXT,
                forecasted_hours NUMERIC,
                actual_hours NUMERIC,
                forecasted_amt NUMERIC,
                actual_amt NUMERIC
            ) --ON COMMIT DROP;
        ";

            await using (var cmd = new NpgsqlCommand(createTemp, conn, trx))
                await cmd.ExecuteNonQueryAsync();

            // 🔹 2. PARSE EXCEL → STREAM INTO COPY
            using var stream = file.OpenReadStream();
            var workbook = new XSSFWorkbook(stream);
            var sheet = workbook.GetSheet("Hours");

            var infoRow = sheet.GetRow(0);
            if (infoRow != null)
            {
                projId = infoRow.GetCell(1)?.ToString() ?? string.Empty;
                type = infoRow.GetCell(3)?.ToString() ?? string.Empty;
                version = infoRow.GetCell(5)?.ToString() ?? string.Empty;

            }
            if (string.IsNullOrEmpty(version))
            {
                //var proj = _context.PlProjects.FirstOrDefault(p => p.ProjId == projId);
                //if (proj == null)
                //{
                //    return NotFound("Project Not Found - " + projId);
                //}

                //newImport = true;
                //plan = await _projPlanService.AddProjectPlanAsync(new PlProjectPlan
                //{
                //    TemplateId = 1,
                //    ProjId = projId,
                //    Status = "In Progress",
                //    PlType = type,
                //    Type = "A",
                //    ProjStartDt = proj.ProjStartDt,
                //    ProjEndDt = proj.ProjEndDt,
                //    Source = "EXCEL"
                //}, "Excel");
                //version = plan.Version.ToString();
            }

            plan = _context.PlProjectPlans.Where(p => p.ProjId == projId && p.Version == Convert.ToInt32(version) && p.PlType == type).Include(p => p.Proj).FirstOrDefault();

            if (plan.ClosedPeriod.HasValue)
            {
                closingMonth = plan.ClosedPeriod.Value.Month;
                closingYear = plan.ClosedPeriod.Value.Year;
            }

            if (plan != null)
            {
                if (plan?.Status.ToUpper() != "IN PROGRESS")
                {
                    return StatusCode(500, $"Import failed: Budget status is '{plan.Status}' for Project '{projId}' with Version '{version}'. If you want to Import update status to 'Working'");
                }

                //plForecastData = _context.PlForecasts.Where(p => p.PlId == plan.PlId).ToList();
            }
            else
            {
                return StatusCode(500, "An error occurred while importing the plan.");
            }

            var project = plan.Proj;
            //int projectDurationMonths = (project.ProjEndDt.GetValueOrDefault().Year -
            //                project.ProjStartDt.GetValueOrDefault().Year) * 12 +
            //                project.ProjEndDt.GetValueOrDefault().Month -
            //                project.ProjStartDt.GetValueOrDefault().Month + 1;

            int projectDurationMonths = (plan.ProjEndDt.GetValueOrDefault().Year -
                plan.ProjStartDt.GetValueOrDefault().Year) * 12 +
                plan.ProjEndDt.GetValueOrDefault().Month -
                plan.ProjStartDt.GetValueOrDefault().Month + 1;

            var headerRow = sheet.GetRow(1);

            // 🔹 COPY BLOCK (separate scope)
            await using (var writer = conn.BeginBinaryImport(@"
            COPY temp_pl_forecast (
                pl_id, empl_id, dct_id, month, year,
                proj_id, plc, acct_id, org_id,
                forecasted_hours, actual_hours,
                forecasted_amt, actual_amt
            ) FROM STDIN (FORMAT BINARY)"))
            {
                for (int r = 2; r <= sheet.LastRowNum; r++)
                {
                    var row = sheet.GetRow(r);
                    if (row == null) continue;

                    string emplId = row.GetCell(2)?.ToString() ?? "";
                    string orgId = row.GetCell(3)?.ToString() ?? "";
                    string acctId = row.GetCell(4)?.ToString() ?? "";
                    string plc = row.GetCell(5)?.ToString() ?? "";

                    for (int i = 9; i < 9 + projectDurationMonths; i++)
                    {
                        var period = headerRow.GetCell(i)?.ToString();
                        if (string.IsNullOrEmpty(period)) continue;

                        if (!DateTime.TryParseExact(period, "MMM yyyy",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out var parsedDate))
                            continue;

                        var cell = row.GetCell(i);
                        decimal val = 0;
                        if (cell != null)
                            decimal.TryParse(cell.ToString(), out val);

                        await writer.StartRowAsync();
                        writer.Write(plan.PlId);
                        writer.Write(emplId);
                        writer.Write(DBNull.Value);
                        writer.Write(parsedDate.Month);
                        writer.Write(parsedDate.Year);
                        writer.Write(plan.ProjId);
                        writer.Write(plc);
                        writer.Write(acctId);
                        writer.Write(orgId);
                        writer.Write(val);
                        writer.Write(val);
                        writer.Write(DBNull.Value);
                        writer.Write(DBNull.Value);
                    }
                }

                await writer.CompleteAsync(); // ✅ finalize COPY
            } // ✅ writer DISPOSED HERE → connection FREE

            // 🔥 3. MERGE (UPSERT)
            var mergeSql = @"
            INSERT INTO pl_forecast (
                pl_id, empl_id, dct_id, month, year,
                proj_id, plc, acct_id, org_id,
                forecastedhours, actualhours,
                forecastedamt, actualamt
            )
            SELECT
                pl_id, empl_id, dct_id, month, year,
                proj_id, plc, acct_id, org_id,
                forecasted_hours, actual_hours,
                forecasted_amt, actual_amt
            FROM temp_pl_forecast
            ON CONFLICT (
                pl_id, empl_id, dct_id, month, year,
                proj_id, plc, acct_id, org_id
            )
            DO UPDATE SET
                forecastedhours = EXCLUDED.forecastedhours,
                actualhours = EXCLUDED.actualhours,
                forecastedamt = EXCLUDED.forecastedamt,
                actualamt = EXCLUDED.actualamt;
        ";

            await using (var cmd = new NpgsqlCommand(mergeSql, conn, trx))
                await cmd.ExecuteNonQueryAsync();

            await trx.CommitAsync();

            return Ok("MERGE import completed successfully 🚀");
        }
        catch (Exception ex)
        {
            await trx.RollbackAsync();
            return StatusCode(500, ex.Message);
        }
    }
    private static object FormatPgDetail(string detail)
    {
        // Key (id, acctid, orgid, pl_id)=(1003128, 50-400-000, 1.01.03.01, 645) already exists.

        var match = Regex.Match(detail, @"\((.*?)\)=\((.*?)\)");

        if (!match.Success)
        {
            return new
            {
                message = "Record already exists."
            };
        }

        var keys = match.Groups[1].Value.Split(", ");
        var values = match.Groups[2].Value.Split(", ");

        var dict = keys.Zip(values, (k, v) => new { k, v })
                       .Where(x => !string.Equals(x.k, "pl_id", StringComparison.OrdinalIgnoreCase))
                       .ToDictionary(x => x.k, x => x.v);

        return new
        {
            message = "A record already exists with the following values:",
            details = dict
        };
    }

    [HttpPost("ImportPlan")]
    public async Task<IActionResult> ImportPlan(IFormFile file)
    {
        bool newImport = false;
        _logger.LogInformation("ImportPlan called");

        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var plForecastData = new List<PlForecast>();

        PlProjectPlan plan = new PlProjectPlan();
        string projId = string.Empty, type = string.Empty, version = string.Empty;
        try
        {
            using var stream = file.OpenReadStream();
            var workbook = new XSSFWorkbook(stream);
            var sheet = workbook.GetSheetAt(0);

            var infoRow = sheet.GetRow(0);
            if (infoRow != null)
            {
                projId = infoRow.GetCell(1)?.ToString() ?? string.Empty;
                type = infoRow.GetCell(3)?.ToString() ?? string.Empty;
                version = infoRow.GetCell(5)?.ToString() ?? string.Empty;
            }
            if (string.IsNullOrEmpty(version))
            {
                newImport = true;
                plan = await _projPlanService.AddProjectPlanAsync(new PlProjectPlan
                {
                    ProjId = projId,
                    Status = "Working",
                    PlType = type,
                    Type = "EXCEL",
                    Source = "EXCEL"
                }, "excel");
                version = plan.Version.ToString();
            }

            plan = _context.PlProjectPlans.Where(p => p.ProjId == projId && p.Version == Convert.ToInt32(version) && p.PlType == type).Include(p => p.Proj).FirstOrDefault();

            if (plan != null)
            {
                plForecastData = _context.PlForecasts.Where(p => p.PlId == plan.PlId && p.EmplId != null).ToList();
            }
            else
            {
                return StatusCode(500, "An error occurred while importing the plan.");
            }

            var project = plan.Proj;
            int projectDurationMonths = (project.ProjEndDt.GetValueOrDefault().Year -
                            project.ProjStartDt.GetValueOrDefault().Year) * 12 +
                            project.ProjEndDt.GetValueOrDefault().Month -
                            project.ProjStartDt.GetValueOrDefault().Month + 1;

            var emplPeriod = new Dictionary<string, string>();
            var plForecasts = new List<PlForecast>();

            var headerRow = sheet.GetRow(1);

            //Get EMployee List
            List<PlEmployee> plEmployees = new List<PlEmployee>();
            for (int rowIndex = 2; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                var row = sheet.GetRow(rowIndex);
                PlEmployee employee = new PlEmployee()
                {
                    EmplId = row.GetCell(2)?.ToString() ?? string.Empty,
                    AccId = row.GetCell(4)?.ToString() ?? string.Empty,
                    OrgId = row.GetCell(3)?.ToString() ?? string.Empty
                };
                for (int i = 9; i < (9 + projectDurationMonths); i++)
                {
                    var period = headerRow.GetCell(i)?.ToString() ?? string.Empty;

                    if (!string.IsNullOrEmpty(period))
                    {
                        DateTime parsedDate = DateTime.ParseExact(period, "MMM yyyy", System.Globalization.CultureInfo.InvariantCulture);

                        int month = parsedDate.Month; // 6
                        int year = parsedDate.Year;
                        plForecasts.Add(new PlForecast() { PlId = plan.PlId.GetValueOrDefault(), EmplId = employee.EmplId, ProjId = projId, Year = year, Month = month, Forecastedhours = Convert.ToDecimal(row.GetCell(i).ToString()) });
                    }
                }
            }

            if (!newImport)
            {
                var updatedList = plForecastData
                            .Select(t =>
                            {
                                var source = plForecasts.FirstOrDefault(s => s.PlId == t.PlId && s.EmplId == t.EmplId && s.Month == t.Month && s.Year == t.Year && s.ProjId == t.ProjId);
                                if (source != null)
                                {
                                    t.Forecastedhours = source.Forecastedhours;
                                }
                                return t;
                            }).ToList();

                _context.PlForecasts.UpdateRange(updatedList);
            }
            else
            {
                _context.PlForecasts.UpdateRange(plForecasts);
            }
            _context.SaveChanges();
            if (newImport)
            {
                var responseMessage = "Successfully Imported and Created new '" + ((type == "BUD") ? "Budget" : "EAC") + "' for Project - '" + project.ProjName + "' with Version - '" + plan?.Version + "'";
                _logger.LogInformation(responseMessage);
                return Ok(responseMessage);
            }
            else
            {
                var responseMessage = "Successfully Imported and Updated existing '" + ((type == "BUD") ? "Budget" : "EAC") + "' for Project - '" + project.ProjName + "' Having version - '" + version + "'";
                _logger.LogInformation(responseMessage);
                return Ok(responseMessage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import plan" + ex.Message);
            return StatusCode(500, "An error occurred while importing the plan.");
        }
    }

    [HttpGet("GetAlternateEmployees")]
    public async Task<IActionResult> GetAlternateEmployees(
    int year,
    int month,
    decimal requiredHours,
    string? orgId = null,
    string? acctId = null,
    string? plc = null)
    {
        try
        {
            // --------------------------------------------------
            // GET STANDARD HOURS
            // --------------------------------------------------

            ScheduleHelper scheduleHelper = new ScheduleHelper();

            var startDate = new DateOnly(year, month, 1);

            var endDate = new DateOnly(
                year,
                month,
                DateTime.DaysInMonth(year, month));

            var schedule = scheduleHelper
                .GetWorkingDaysForDuration(
                    startDate,
                    endDate,
                    _orgService)
                .FirstOrDefault();

            decimal standardHours =
                Convert.ToDecimal(schedule?.WorkingHours ?? 160);

            decimal allowedHours = standardHours * 1.30m;

            // --------------------------------------------------
            // GET ALL ACTIVE EMPLOYEES
            // --------------------------------------------------

            //       var sql = $@"SELECT empl.empl_id AS EmplId, 
            //      s_empl_status_cd AS Status, 
            //      last_first_name AS FirstName, 
            //      effect_dt AS EffectiveDate,
            //      sal_amt AS Salary,
            //      hrly_amt AS PerHourRate,
            //bill_lab_cat_cd AS Bill_Lab_Cat_CD,
            //genl_lab_cat_cd AS Genl_Lab_Cat_CD
            //           FROM empl
            //           JOIN public.empl_lab_info 
            //               ON empl.empl_id = public.empl_lab_info.empl_id
            //       where public.empl_lab_info.end_dt = '2078-12-31' and bill_lab_cat_cd = '"+plc+"'";


            var conditions = new List<string>
                {
                    "public.empl_lab_info.end_dt = '2078-12-31'"
                };

            var parameters = new List<NpgsqlParameter>
                {
                    new NpgsqlParameter("@endDate", new DateOnly(2078, 12, 31))
                };

            if (!string.IsNullOrWhiteSpace(plc))
            {
                conditions.Add("bill_lab_cat_cd = @plc");

                parameters.Add(
                    new NpgsqlParameter("@plc", plc));
            }

            if (!string.IsNullOrWhiteSpace(acctId))
            {
                conditions.Add("acct_id = @acctId");

                parameters.Add(
                    new NpgsqlParameter("@acctId", acctId));
            }

            if (!string.IsNullOrWhiteSpace(orgId))
            {
                conditions.Add("org_id = @orgId");

                parameters.Add(
                    new NpgsqlParameter("@orgId", orgId));
            }

            var whereClause = string.Join(" AND ", conditions);

            var sql = $@"
                SELECT
                    empl.empl_id AS EmplId,
                    s_empl_status_cd AS Status,
                    last_first_name AS FirstName,
                    effect_dt AS EffectiveDate,
                    sal_amt AS Salary,
                    hrly_amt AS PerHourRate,
                    bill_lab_cat_cd AS Bill_Lab_Cat_CD,
                    genl_lab_cat_cd AS Genl_Lab_Cat_CD
                FROM empl
                JOIN public.empl_lab_info
                    ON empl.empl_id = public.empl_lab_info.empl_id
                WHERE {whereClause}";

            var employees = await _context.Empl_Master_Dto
            .FromSqlRaw(sql, parameters.ToArray())
            .ToListAsync();

            //var employees = _context.Empl_Master_Dto
            //    .FromSqlRaw(sql)
            //    .ToList();



            //var employeesQuery = _context.Empl_Master
            //    .AsQueryable();

            //// optional filters
            //if (!string.IsNullOrWhiteSpace(orgId))
            //{
            //    employeesQuery =
            //        employeesQuery.Where(x => x.OrgId == orgId);
            //}

            //if (!string.IsNullOrWhiteSpace(acctId))
            //{
            //    employeesQuery =
            //        employeesQuery.Where(x => x.AccId == acctId);
            //}

            //if (!string.IsNullOrWhiteSpace(plc))
            //{
            //    employeesQuery =
            //        employeesQuery.Where(x => x.PlcGlcCode == plc);
            //}

            //var employees = await employeesQuery
            //    .GroupBy(x => new
            //    {
            //        x.EmplId,
            //        x.FirstName,
            //        x.LastName,
            //        x.OrgId,
            //        x.AccId,
            //        x.PlcGlcCode,
            //        x.PerHourRate
            //    })
            //    .Select(g => new
            //    {
            //        g.Key.EmplId,
            //        Name =
            //            (g.Key.FirstName ?? "") + " " +
            //            (g.Key.LastName ?? ""),

            //        g.Key.OrgId,
            //        g.Key.AccId,
            //        g.Key.PlcGlcCode,
            //        g.Key.PerHourRate
            //    })
            //    .ToListAsync();

            // --------------------------------------------------
            // GET CURRENT UTILIZATION
            // --------------------------------------------------

            var employeeHours = await _context.PlForecasts
                .Where(x =>
                    x.Year == year &&
                    x.Month == month &&
                    x.EmplId != null)
                .GroupBy(x => x.EmplId)
                .Select(g => new
                {
                    EmplId = g.Key,

                    Hours =
                        g.Sum(x =>
                            (x.Actualhours) +
                            (x.Forecastedhours))
                })
                .ToListAsync();

            var hoursLookup =
                employeeHours.ToDictionary(
                    x => x.EmplId ?? "",
                    x => x.Hours);

            // --------------------------------------------------
            // BUILD RESPONSE
            // --------------------------------------------------

            var alternateEmployees = employees
                .Select(x =>
                {
                    hoursLookup.TryGetValue(
                        x.EmplId ?? "",
                        out var assignedHours);

                    decimal availableHours =
                        allowedHours - assignedHours;

                    return new
                    {
                        EmployeeId = x.EmplId,
                        EmployeeName = x.FirstName,

                        //x.OrgId,
                        //x.AccId,
                        Plc = x.Bill_Lab_Cat_CD,

                        AssignedHours = assignedHours,
                        AvailableHours = availableHours,

                        CanAllocate =
                            availableHours >= requiredHours,

                        UtilizationPercent =
                            standardHours == 0
                                ? 0
                                : Math.Round(
                                    (assignedHours / standardHours) * 100,
                                    2),

                        PerHourRate = x.PerHourRate
                    };
                })
                .Where(x => x.AvailableHours > 0)
                .OrderByDescending(x => x.AvailableHours)
                .ThenBy(x => x.PerHourRate)
                .Take(50)
                .ToList();

            return Ok(new
            {
                Year = year,
                Month = month,
                RequiredHours = requiredHours,
                StandardHours = standardHours,
                AllowedHours = allowedHours,

                AlternateEmployees = alternateEmployees
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to get alternate employees");

            return StatusCode(
                500,
                "An error occurred while fetching alternate employees.");
        }
    }

    [HttpGet("GetAlternateEmployeesV1")]
    public async Task<IActionResult> GetAlternateEmployeesV1(
    int year,
    int month,
    decimal requiredHours,
    string? orgId = null,
    string? acctId = null,
    string? plc = null)
    {
        try
        {
            //------------------------------------------------------
            // STANDARD HOURS
            //------------------------------------------------------

            ScheduleHelper scheduleHelper = new ScheduleHelper();
            Helper helper = new Helper(_context, _config);
            var startDate = new DateOnly(year, month, 1);

            var endDate = new DateOnly(
                year,
                month,
                DateTime.DaysInMonth(year, month));

            var schedule = scheduleHelper
                .GetWorkingDaysForDuration(
                    startDate,
                    endDate,
                    _orgService)
                .FirstOrDefault();

            decimal standardHours =
                Convert.ToDecimal(schedule?.WorkingHours ?? 160);

            decimal allowedHours =
                standardHours * 1.30m;

            //------------------------------------------------------
            // HELPER CALL
            //------------------------------------------------------

            var alternateEmployees =
                await helper.GetAlternateEmployees(
                    year,
                    month,
                    requiredHours,
                    allowedHours,
                    standardHours,
                    orgId,
                    acctId,
                    plc);

            //------------------------------------------------------
            // RESPONSE
            //------------------------------------------------------

            return Ok(new
            {
                Year = year,
                Month = month,
                RequiredHours = requiredHours,
                StandardHours = standardHours,
                AllowedHours = allowedHours,

                AlternateEmployees = alternateEmployees
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to get alternate employees");

            return StatusCode(
                500,
                "An error occurred while fetching alternate employees.");
        }
    }

    [HttpGet("CalculateCost")]
    public async Task<IActionResult> CalculateCost(int planID, int templateId, string type)
    {
        _logger.LogInformation("CalculateCost called for planID {PlanID}, templateId {TemplateId}, type {Type}", planID, templateId, type);

        try
        {
            var employeeCosts = await _pl_ForecastService.CalculateCost(planID, templateId, type);
            //await BulkUpsertProjForecastSummary(employeeCosts.Proj_Id, employeeCosts.Type, employeeCosts.Version);
            return Ok(employeeCosts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate cost");
            return StatusCode(500, "An error occurred while calculating the cost.");
        }
    }


    [HttpGet("CalculateCostAI")]
    public async Task<IActionResult> CalculateCostAI(string proj_Id, string type, int Year)
    {
        _logger.LogInformation("CalculateCostAI called for proj_Id {ProjId}", proj_Id);

        try
        {
            //var AllPlans = await _context.PlProjectPlans
            //    .Where(p =>
            //        p.ProjId == proj_Id)
            //    .ToListAsync();

            //var finalPlan = AllPlans
            //        .Where(p =>
            //            p.ProjId == proj_Id &&
            //            p.FinalVersion == true && p.PlType.ToUpper() == type.ToUpper())
            //        .FirstOrDefault();

            //var selectedPlan =
            //    finalPlan ??
            //    await _context.PlProjectPlans
            //        .Where(p => p.ProjId == proj_Id && p.Type.ToUpper() == type.ToUpper())
            //        .OrderByDescending(p => p.Version)
            //        .FirstOrDefaultAsync();

            var selectedPlan = await _context.PlProjectPlans
                    .Where(p =>
                        p.ProjId == proj_Id &&
                        (p.PlType == "EAC" || p.PlType == "BUD"))
                    .OrderByDescending(p =>
                        p.PlType == "EAC" && p.FinalVersion.Value)

                    .ThenByDescending(p =>
                        p.PlType == "EAC")

                    .ThenByDescending(p =>
                        p.PlType == "BUD" && p.FinalVersion.Value)

                    .ThenByDescending(p =>
                        p.Version)

                    .FirstOrDefaultAsync();

            var plId = selectedPlan?.PlId;

            var employeeCosts = await _pl_ForecastService.CalculateCostAI(selectedPlan.PlId.GetValueOrDefault(), selectedPlan.TemplateId.GetValueOrDefault(), type, Year);
            //await BulkUpsertProjForecastSummary(employeeCosts.Proj_Id, employeeCosts.Type, employeeCosts.Version);
            return Ok(employeeCosts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate cost");
            return StatusCode(500, "An error occurred while calculating the cost.");
        }
    }

    [HttpGet("CalculateRevenueCost")]
    public async Task<IActionResult> CalculateRevenueCost(int planID, int templateId, string type)
    {
        _logger.LogInformation("CalculateCost called for planID {PlanID}, templateId {TemplateId}, type {Type}", planID, templateId, type);

        try
        {
            var employeeCosts = await _pl_ForecastService.CalculateRevenueCost(planID, templateId, type);
            //await BulkUpsertProjForecastSummary(employeeCosts.Proj_Id, employeeCosts.Type, employeeCosts.Version);
            return Ok(employeeCosts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate cost");
            return StatusCode(500, "An error occurred while calculating the cost.");
        }
    }

    [HttpGet("CalculateBurdenCost")]
    public async Task<IActionResult> CalculateBurdenCost(int planID, int templateId, string type)
    {
        _logger.LogInformation("CalculateCost called for planID {PlanID}, templateId {TemplateId}, type {Type}", planID, templateId, type);

        try
        {
            var employeeCosts = await _pl_ForecastService.CalculateBurdenCost(planID, templateId, type);
            return Ok(employeeCosts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate cost");
            return StatusCode(500, "An error occurred while calculating the cost.");
        }
    }

    [HttpPost("CalculateRevenueCostForSelectedHours")]
    public async Task<IActionResult> CalculateRevenueCostForSelectedHours(int planID, int templateId, string type, List<PlForecast> hoursForecast)
    {
        _logger.LogInformation("CalculateCost called for planID {PlanID}, templateId {TemplateId}, type {Type}", planID, templateId, type);

        try
        {
            var employeeCosts = await _pl_ForecastService.CalculateRevenueCostForSelectedHours(planID, templateId, type, hoursForecast);
            await BulkUpsertProjForecastSummary(employeeCosts.Proj_Id, employeeCosts.Type, employeeCosts.Version);
            return Ok(employeeCosts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate cost");
            return StatusCode(500, "An error occurred while calculating the cost.");
        }
    }

    [HttpGet("GetMonthlyData")]
    public async Task<IActionResult> GetMonthlyData(int planID, string planType)
    {
        try
        {
            //var forecastsDirectCosts = await _context.PlForecasts
            //    .Where(f => f.PlId == planID)
            //    .GroupBy(p => new { p.Month, p.Year })
            //    .Select(g => new MonthlyData
            //    {
            //        Month = g.Key.Month,
            //        Year = g.Key.Year,
            //        LaborCost = g.Sum(x => x.DctId == null ? x.Cost : 0),
            //        NonLaborCost = g.Sum(x => x.DctId != null ? x.Cost : 0),
            //        Revenue = g.Sum(x => x.Revenue),
            //        Fringe = g.Sum(x => x.Fringe),
            //        Mnh = g.Sum(x => x.Materials),
            //        Overhead = g.Sum(x => x.Overhead),
            //        Gna = g.Sum(x => x.Gna),
            //        Hr = g.Sum(x => x.Hr)
            //    })
            //    .ToListAsync();

            var forecastsDirectCosts = await _context.PlForecasts
                .Where(f => f.PlId == planID)
                .GroupBy(p => new { p.Month, p.Year })
                .Select(g => new MonthlyData
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    LaborCost = g.Sum(x => x.DctId == null ? x.Cost : 0),
                    //NonLaborCost = g.Sum(x => x.DctId != null ? x.Cost : 0),
                    NonLaborCost = g.Sum(x => x.DctId != null
                    ? (planType.ToUpper() == "BUD" || planType.ToUpper() == "NBBUD"
                        ? x.Forecastedamt
                        : x.Actualamt)
                    : 0).GetValueOrDefault(),
                    Revenue = g.Sum(x => x.Revenue),
                    Fringe = g.Sum(x => x.Fringe),
                    Mnh = g.Sum(x => x.Materials),
                    Overhead = g.Sum(x => x.Overhead),
                    Gna = g.Sum(x => x.Gna),
                    Hr = g.Sum(x => x.Hr),
                    Hours = g.Sum(x => planType.ToUpper() == "BUD" || planType.ToUpper() == "NBBUD" ? x.Forecastedhours : x.Actualhours)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();
            return Ok(forecastsDirectCosts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate cost");
            return StatusCode(500, "An error occurred while calculating the cost.");
        }
    }


    [HttpGet("GetMonthlyDataV1")]
    public async Task<IActionResult> GetMonthlyDataV1(int planID, string planType)
    {
        try
        {
            var projPlan = _context.PlProjectPlans.Where(p => p.PlId == planID).FirstOrDefault();
            if (projPlan == null)
            {
                return NotFound("Project Plan not found.");
            }
            List<IndirectRates> indirectRates = new List<IndirectRates>();
            //var MonthlyData = await _context.PlForecasts
            //        .Where(f => f.PlId == planID)
            //        .GroupBy(p => new { p.Month, p.Year})
            //        .Select(g => new MonthlyDataV1
            //        {
            //            Month = g.Key.Month,
            //            Year = g.Key.Year,
            //            LaborCost = g.Sum(x => x.DctId == null ? x.Cost : 0),
            //            //NonLaborCost = g.Sum(x => x.DctId != null ? x.Cost : 0),
            //            NonLaborCost = g.Sum(x => x.DctId != null
            //            ? (planType.ToUpper() == "BUD" || planType.ToUpper() == "NBBUD"
            //                ? x.Forecastedamt
            //                : x.Actualamt)
            //            : 0).GetValueOrDefault(),
            //            Revenue = g.Sum(x => x.Revenue),
            //            Fringe = g.Sum(x => x.Fringe),
            //            Mnh = g.Sum(x => x.Materials),
            //            Overhead = g.Sum(x => x.Overhead),
            //            Gna = g.Sum(x => x.Gna),
            //            Hr = g.Sum(x => x.Hr),
            //            Hours = g.Sum(x => planType.ToUpper() == "BUD" || planType.ToUpper() == "NBBUD" ? x.Forecastedhours : x.Actualhours)
            //        })
            //        .OrderBy(x => x.Year)
            //        .ThenBy(x => x.Month)
            //        .ToListAsync();

            var forecastsDirectCosts = await _context.PlForecasts
                .Where(f => f.PlId == planID && f.ProjId == projPlan.ProjId)
                .GroupBy(p => new { p.Month, p.Year, p.OrgId, p.AcctId })
                .Select(g => new MonthlyDataV1
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    OrgId = g.Key.OrgId,
                    AcctId = g.Key.AcctId,
                    LaborCost = g.Sum(x => x.DctId == null ? x.Cost : 0),
                    //NonLaborCost = g.Sum(x => x.DctId != null ? x.Cost : 0),
                    NonLaborCost = g.Sum(x => x.DctId != null
                    ? (planType.ToUpper() == "BUD" || planType.ToUpper() == "NBBUD"
                        ? x.Forecastedamt
                        : x.Actualamt)
                    : 0).GetValueOrDefault(),
                    Revenue = g.Sum(x => x.Revenue),
                    Fringe = g.Sum(x => x.Fringe),
                    Mnh = g.Sum(x => x.Materials),
                    Overhead = g.Sum(x => x.Overhead),
                    Gna = g.Sum(x => x.Gna),
                    Hr = g.Sum(x => x.Hr),
                    Hours = g.Sum(x => planType.ToUpper() == "BUD" || planType.ToUpper() == "NBBUD" ? x.Forecastedhours : x.Actualhours)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            var MonthlyData = forecastsDirectCosts
               .GroupBy(p => new { p.Month, p.Year })
               .Select(g => new MonthlyDataV2
               {
                   Month = g.Key.Month,
                   Year = g.Key.Year,
                   LaborCost = g.Sum(x => x.LaborCost),
                   //NonLaborCost = g.Sum(x => x.DctId != null ? x.Cost : 0),
                   NonLaborCost = g.Sum(x => x.NonLaborCost),
                   Hours = g.Sum(x => x.Hours)
               })
               .OrderBy(x => x.Year)
               .ThenBy(x => x.Month)
               .ToList();

            var keys = forecastsDirectCosts
                    .Select(c => $"{c.OrgId}|{c.AcctId}")
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

            foreach (var employee in forecastsDirectCosts)
            {
                poolLookup.TryGetValue((employee.OrgId, employee.AcctId), out var actualPools);
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

            var MonthlyOverhead = forecastsDirectCosts
                .Where(p => !string.IsNullOrWhiteSpace(p.OverheadName))
                .GroupBy(p => new { p.Month, p.Year, p.OverheadName })
                .Select(g => new IndirectRates
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    Name = g.Key.OverheadName!,
                    Value = g.Sum(x => x.Overhead)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            var MonthlyFringe = forecastsDirectCosts
                .Where(p => !string.IsNullOrWhiteSpace(p.FringeName))
                .GroupBy(p => new { p.Month, p.Year, p.FringeName })
                .Select(g => new IndirectRates
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    Name = g.Key.FringeName!,
                    Value = g.Sum(x => x.Fringe)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            var MonthlyMnh = forecastsDirectCosts
                .Where(p => !string.IsNullOrWhiteSpace(p.MaterialsName))
                .GroupBy(p => new { p.Month, p.Year, p.MaterialsName })
                .Select(g => new IndirectRates
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    Name = g.Key.MaterialsName,
                    Value = g.Sum(x => x.Mnh)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            var MonthlyGna = forecastsDirectCosts
                .Where(p => !string.IsNullOrWhiteSpace(p.GnaName))
                .GroupBy(p => new { p.Month, p.Year, p.GnaName })
                .Select(g => new IndirectRates
                {
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    Name = g.Key.GnaName,
                    Value = g.Sum(x => x.Gna)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            foreach (var data in MonthlyData)
            {
                data.IndirectCost = new List<PlanningAPI.Helpers.Indirect>();
                if (MonthlyOverhead.Any())
                {
                    data.IndirectCost.AddRange(
                        MonthlyOverhead
                            .Where(p => p.Month == data.Month && p.Year == data.Year)
                            .Select(g => new PlanningAPI.Helpers.Indirect
                            {
                                Name = g.Name,
                                Value = g.Value
                            })
                            .ToList());
                }
                if (MonthlyFringe.Any())
                {
                    data.IndirectCost.AddRange(
                        MonthlyFringe
                            .Where(p => p.Month == data.Month && p.Year == data.Year)
                            .Select(g => new PlanningAPI.Helpers.Indirect
                            {
                                Name = g.Name,
                                Value = g.Value
                            })
                            .ToList());
                }
                if (MonthlyMnh.Any())
                {
                    data.IndirectCost.AddRange(
                        MonthlyMnh
                            .Where(p => p.Month == data.Month && p.Year == data.Year)
                            .Select(g => new PlanningAPI.Helpers.Indirect
                            {
                                Name = g.Name,
                                Value = g.Value
                            })
                            .ToList());
                }
                if (MonthlyGna.Any())
                {
                    data.IndirectCost.AddRange(
                        MonthlyGna
                            .Where(p => p.Month == data.Month && p.Year == data.Year)
                            .Select(g => new PlanningAPI.Helpers.Indirect
                            {
                                Name = g.Name,
                                Value = g.Value
                            })
                            .ToList());
                }
            }
            return Ok(MonthlyData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate cost");
            return StatusCode(500, "An error occurred while calculating the cost.");
        }
    }

    [HttpGet("GenerateForecastReport")]
    public async Task<IActionResult> GenerateForecastReport(int planID, int templateId, string type)
    {
        _logger.LogInformation("CalculateCost called for planID {PlanID}, templateId {TemplateId}, type {Type}", planID, templateId, type);
        try
        {
            var forecast = await _pl_ForecastService.CalculateCost(planID, templateId, type); ;

            var aiInsight = await _aiService.GetForecastInsightAsync(forecast);

            // 2. Generate PDF
            var report = new ForecastReport(forecast, aiInsight);
            var pdfBytes = report.GeneratePdf();

            // 3. Return PDF as downloadable file
            return File(pdfBytes, "application/pdf", $"{forecast.Proj_Id}_ForecastReport.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate cost");
            return StatusCode(500, "An error occurred while calculating the cost.");
        }
    }

    [HttpPost("generate-variance-report-V2")]
    public async Task<IActionResult> GenerateVarianceReport(
       [FromQuery] int versionA,
       [FromQuery] int versionB)
    {
        List<ProjForecastSummary> forecastList = _context.ProjForecastSummary
            .Where(f => f.ProjId == "22003.T.0069.00" && (f.Version == versionA || f.Version == versionB) && f.PlType.ToUpper() == "EAC")
            .ToList();
        if (forecastList == null || !forecastList.Any())
            return BadRequest("Forecast data is required.");

        // -------------------------------
        // 1️⃣ Calculate variance between versions
        // -------------------------------
        //var grouped = forecastList
        //    .GroupBy(f => new { f.PlType, f.ProjId, f.Month, f.Year});

        var grouped = forecastList
            .GroupBy(f => new { f.ProjId, f.Month, f.Year })
            .OrderBy(g => g.Key.Year)
            .ThenBy(g => g.Key.Month);

        var varianceData = new List<VarianceComparison>();

        foreach (var g in grouped)
        {
            var versions = g.ToDictionary(f => f.Version, f => f);
            if (versions.ContainsKey(versionA) && versions.ContainsKey(versionB))
            {
                var a = versions[versionA];
                var b = versions[versionB];

                //varianceData.Add(new VarianceComparison
                //{
                //    ProjId = a.ProjId,
                //    PlType = a.PlType,
                //    Month = a.Month,
                //    Year = a.Year,
                //    ForecastedCostDiff = b.MonthlyForecastedAmt - a.MonthlyForecastedAmt,
                //    ActualCostDiff = b.MonthlyActualAmt - a.MonthlyActualAmt,
                //    ForecastedHoursDiff = b.MonthlyForecastedHours - a.MonthlyForecastedHours,
                //    ActualHoursDiff = b.MonthlyActualHours - a.MonthlyActualHours,
                //    RevenueDiff = b.MonthlyRevenue - a.MonthlyRevenue
                //});
                varianceData.Add(new VarianceComparison
                {
                    ProjId = a.ProjId,
                    PlType = a.PlType,
                    Month = a.Month,
                    Year = a.Year,

                    // Just assign A & B values
                    ForecastedCostA = a.MonthlyForecastedAmt,
                    ActualCostA = a.MonthlyActualAmt,
                    ForecastedHoursA = a.MonthlyForecastedHours,
                    ActualHoursA = a.MonthlyActualHours,
                    RevenueA = a.MonthlyRevenue,

                    ForecastedCostB = b.MonthlyForecastedAmt,
                    ActualCostB = b.MonthlyActualAmt,
                    ForecastedHoursB = b.MonthlyForecastedHours,
                    ActualHoursB = b.MonthlyActualHours,
                    RevenueB = b.MonthlyRevenue
                });

            }
        }

        // -------------------------------
        // 2️⃣ AI summary per PlType
        // -------------------------------
        var aiSummaries = new Dictionary<string, string>();
        var byPlType = varianceData.GroupBy(v => v.PlType);

        foreach (var group in byPlType)
        {
            var summary = await _aiService.GetVarianceSummaryAsync(group.Key, versionA, versionB, group.ToList());
            aiSummaries[group.Key] = summary;
        }

        // -------------------------------
        // 3️⃣ Generate PDF (VarianceReport class handles table + AI summary)
        // -------------------------------
        var report = new VarianceReport(varianceData, aiSummaries, versionA, versionB);
        var pdfBytes = report.GeneratePdf();

        return File(pdfBytes, "application/pdf", $"VarianceReport_V{versionA}_V{versionB}.pdf");
    }

    [HttpPost("GenerateVarianceReportForBudgetVsEAC")]
    public async Task<IActionResult> GenerateVarianceReportForBudgetVsEAC([FromQuery] string ProjId,
   [FromQuery] int BudgetVersion,
   [FromQuery] int EACVersion)
    {
        List<ProjForecastSummary> forecastList = _context.ProjForecastSummary
            .Where(f => f.ProjId == ProjId && ((f.Version == BudgetVersion && f.PlType == "BUD") || (f.Version == EACVersion && f.PlType == "EAC")))
            .ToList();
        if (forecastList == null || !forecastList.Any())
            return BadRequest("Forecast data is required.");

        // -------------------------------
        // 1️⃣ Calculate variance between versions
        // -------------------------------
        //var grouped = forecastList
        //    .GroupBy(f => new {f.ProjId, f.Month, f.Year });
        var grouped = forecastList
                    .GroupBy(f => new { f.ProjId, f.Month, f.Year })
                    .OrderBy(g => g.Key.Year)
                    .ThenBy(g => g.Key.Month);

        var varianceData = new List<VarianceComparison>();

        foreach (var g in grouped)
        {
            var versions = g.ToDictionary(f => f.Version, f => f);
            if (versions.ContainsKey(BudgetVersion) && versions.ContainsKey(EACVersion))
            {
                var a = versions[BudgetVersion];
                var b = versions[EACVersion];

                //varianceData.Add(new VarianceComparison
                //{
                //    ProjId = a.ProjId,
                //    PlType = a.PlType,
                //    Month = a.Month,
                //    Year = a.Year,
                //    ForecastedCostDiff = b.MonthlyForecastedAmt - a.MonthlyForecastedAmt,
                //    ActualCostDiff = b.MonthlyActualAmt - a.MonthlyActualAmt,
                //    ForecastedHoursDiff = b.MonthlyForecastedHours - a.MonthlyForecastedHours,
                //    ActualHoursDiff = b.MonthlyActualHours - a.MonthlyActualHours,
                //    RevenueDiff = b.MonthlyRevenue - a.MonthlyRevenue
                //});
                varianceData.Add(new VarianceComparison
                {
                    ProjId = a.ProjId,
                    PlType = a.PlType,
                    Month = a.Month,
                    Year = a.Year,

                    // Just assign A & B values
                    ForecastedCostA = a.MonthlyForecastedAmt,
                    ActualCostA = a.MonthlyActualAmt,
                    ForecastedHoursA = a.MonthlyForecastedHours,
                    ActualHoursA = a.MonthlyActualHours,
                    RevenueA = a.MonthlyRevenue,

                    ForecastedCostB = b.MonthlyForecastedAmt,
                    ActualCostB = b.MonthlyActualAmt,
                    ForecastedHoursB = b.MonthlyForecastedHours,
                    ActualHoursB = b.MonthlyActualHours,
                    RevenueB = b.MonthlyRevenue
                });

            }
        }

        // -------------------------------
        // 2️⃣ AI summary per PlType
        // -------------------------------
        var aiSummaries = new Dictionary<string, string>();
        var byPlType = varianceData.GroupBy(v => v.PlType);

        foreach (var group in byPlType)
        {
            var summary = await _aiService.GetVarianceSummaryAsync(group.Key, BudgetVersion, EACVersion, group.ToList());
            aiSummaries[group.Key] = summary;
        }

        // -------------------------------
        // 3️⃣ Generate PDF (VarianceReport class handles table + AI summary)
        // -------------------------------
        var report = new VarianceReport(varianceData, aiSummaries, BudgetVersion, EACVersion);
        var pdfBytes = report.GeneratePdf();

        return File(pdfBytes, "application/pdf", $"VarianceReport_V{BudgetVersion}_V{EACVersion}.pdf");
    }


    [HttpPost("GenerateVarianceReportDynamic")]
    public async Task<IActionResult> GenerateVarianceReportDynamic(
        [FromQuery] string projId,
        [FromQuery] string sourceType,
        [FromQuery] int sourceVersion,
        [FromQuery] string compareType,
        [FromQuery] int compareVersion)
    {
        // -----------------------------------
        // VALIDATION
        // -----------------------------------

        if (string.IsNullOrWhiteSpace(projId))
            return BadRequest("Project Id is required.");

        if (string.IsNullOrWhiteSpace(sourceType))
            return BadRequest("Source type is required.");

        if (string.IsNullOrWhiteSpace(compareType))
            return BadRequest("Compare type is required.");

        // -----------------------------------
        // GET FORECAST DATA
        // -----------------------------------

        sourceVersion = _context.PlProjectPlans.FirstOrDefault(p => p.ProjId == projId && p.PlType == sourceType)?.PlId.GetValueOrDefault() ?? 0;
        compareVersion = _context.PlProjectPlans.FirstOrDefault(p => p.ProjId == projId && p.PlType == compareType)?.PlId.GetValueOrDefault() ?? 0;

        var forecastList = await _context.PlForecasts
            .AsNoTracking()
            .Where(f =>
                f.ProjId == projId &&
                (f.PlId == sourceVersion ||
                 f.PlId == compareVersion))
            .ToListAsync();


        if (!forecastList.Any())
        {
            return BadRequest(
                "Forecast data is required.");
        }

        // -----------------------------------
        // GROUP MONTHLY DATA
        // -----------------------------------

        var grouped = forecastList
            .GroupBy(f => new
            {
                f.ProjId,
                f.Month,
                f.Year
            })
            .OrderBy(g => g.Key.Year)
            .ThenBy(g => g.Key.Month);

        var varianceData =
            new List<VarianceComparison>();

        foreach (var g in grouped)
        {
            //---------------------------------
            // SOURCE DATA
            //---------------------------------

            var source = g
                .Where(x => x.PlId == sourceVersion)
                .ToList();

            //---------------------------------
            // COMPARISON DATA
            //---------------------------------

            var compare = g
                .Where(x => x.PlId == compareVersion)
                .ToList();

            if (!source.Any() || !compare.Any())
                continue;

            //---------------------------------
            // AGGREGATE SOURCE
            //---------------------------------

            var sourceForecastedCost =
                source.Sum(x => x.ForecastedCost ?? 0);

            var sourceActualCost =
                source.Sum(x => x.Actualamt ?? 0);

            var sourceForecastedHours =
                source.Sum(x => x.Forecastedhours);

            var sourceActualHours =
                source.Sum(x => x.Actualhours);

            var sourceRevenue =
                source.Sum(x => x.Revenue);

            //---------------------------------
            // AGGREGATE COMPARISON
            //---------------------------------

            var compareForecastedCost =
                compare.Sum(x => x.ForecastedCost ?? 0);

            var compareActualCost =
                compare.Sum(x => x.Actualamt ?? 0);

            var compareForecastedHours =
                compare.Sum(x => x.Forecastedhours);

            var compareActualHours =
                compare.Sum(x => x.Actualhours);

            var compareRevenue =
                compare.Sum(x => x.Revenue);

            //---------------------------------
            // BUILD VARIANCE ROW
            //---------------------------------

            varianceData.Add(new VarianceComparison
            {
                ProjId = g.Key.ProjId,

                PlType =
                    $"{sourceType.ToUpper()} vs {compareType.ToUpper()}",

                Month = g.Key.Month,
                Year = g.Key.Year,

                //---------------------------------
                // SOURCE VALUES (A)
                //---------------------------------

                ForecastedCostA =
                    sourceForecastedCost,

                ActualCostA =
                    sourceActualCost,

                ForecastedHoursA =
                    sourceForecastedHours,

                ActualHoursA =
                    sourceActualHours,

                RevenueA =
                    sourceRevenue,

                //---------------------------------
                // COMPARISON VALUES (B)
                //---------------------------------

                ForecastedCostB =
                    compareForecastedCost,

                ActualCostB =
                    compareActualCost,

                ForecastedHoursB =
                    compareForecastedHours,

                ActualHoursB =
                    compareActualHours,

                RevenueB =
                    compareRevenue
            });
        }

        //-----------------------------------
        // RETURN RESULT
        //-----------------------------------

        return Ok(varianceData);
    }


    [HttpPost("GenerateVarianceReportDataForUI")]
    public async Task<IActionResult> GenerateVarianceReportDataForUI(
        [FromQuery] string projId,
        [FromQuery] string sourceType,
        [FromQuery] int sourceVersion,
        [FromQuery] string compareType,
        [FromQuery] int compareVersion)
    {
        // =========================================================
        // VALIDATION
        // =========================================================

        if (string.IsNullOrWhiteSpace(projId))
            return BadRequest("Project Id is required.");

        // =========================================================
        // GET PLAN IDS
        // =========================================================

        string SourceName = string.Empty, compareName = string.Empty;

        SourceName = sourceVersion.ToString();
        compareName = compareVersion.ToString();

        sourceVersion = await _context.PlProjectPlans
            .Where(p => p.ProjId == projId &&
                        p.PlType == sourceType && p.Version == sourceVersion)
            .Select(p => p.PlId ?? 0)
            .FirstOrDefaultAsync();

        compareVersion = await _context.PlProjectPlans
            .Where(p => p.ProjId == projId &&
                        p.PlType == compareType && p.Version == compareVersion)
            .Select(p => p.PlId ?? 0)
            .FirstOrDefaultAsync();

        // =========================================================
        // PROJECT INFO
        // =========================================================

        var project = await _context.PlProjects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProjId == projId);

        // =========================================================
        // FORECAST DATA
        // =========================================================

        var forecastList = await _context.PlForecasts
            .AsNoTracking()
            .Where(f =>
                f.ProjId == projId &&
                (f.PlId == sourceVersion ||
                 f.PlId == compareVersion))
            .ToListAsync();

        if (!forecastList.Any())
            return BadRequest("Forecast data not found.");

        // =========================================================
        // GROUP BY VERSION
        // =========================================================

        var versionGroups = forecastList
            .GroupBy(x => x.PlId)
            .ToList();

        var versionsObject =
            new Dictionary<string, object>();

        foreach (var versionGroup in versionGroups)
        {
            var versionId = versionGroup.Key;

            string pl_type =
                versionId == sourceVersion
                    ? sourceType
                    : compareType;

            string versionName =
                versionId == sourceVersion
                    ? sourceType + "-" + SourceName
                    : compareType + "-" + compareName;

            // =====================================================
            // TOTALS
            // =====================================================

            decimal totalRevenue =
                versionGroup.Sum(x => x.Revenue);


            decimal totalLabor =
                versionGroup.Where(x => x.DctId == null).Sum(x =>
                    x.Cost);

            decimal totalNonLabor =
                versionGroup.Where(x => x.DctId != null).Sum(x =>
                    x.Cost);

            decimal totalIndirect =
                versionGroup.Sum(x =>
                    x.Burden);

            // =====================================================
            // MONTHLY DATA
            // =====================================================

            var monthlyData = versionGroup
                .GroupBy(x => new
                {
                    x.Year,
                    x.Month
                })
                .OrderBy(x => x.Key.Year)
                .ThenBy(x => x.Key.Month)
                .Select(g => new
                {
                    month = new DateTime(
                            g.Key.Year,
                            g.Key.Month,
                            1)
                        .ToString("MMM"),

                    cost = g.Where(x => x.DctId == null).Sum(x =>
                        x.Cost),

                    revenue = g.Sum(x =>
                        x.Revenue),

                    //laborHours = g.Sum(x =>
                    //    x.Forecastedhours),

                    laborHours = g.Sum(x =>
                        pl_type == "EAC"
                            ? (x.Actualhours)
                            : (x.Forecastedhours)
),

                    nonLabor = g.Where(x => x.DctId != null).Sum(x =>
                        x.Cost),

                    indirect = g.Sum(x =>
                        x.Burden)
                })
                .ToList();

            // =====================================================
            // VERSION OBJECT
            // =====================================================

            versionsObject[versionName] = new
            {
                revenue = totalRevenue,
                labor = totalLabor,
                nonLabor = totalNonLabor,
                indirect = totalIndirect,
                monthlyData
            };
        }

        // =========================================================
        // FINAL RESPONSE
        // =========================================================

        var result = new Dictionary<string, object>
        {
            [projId] = new
            {
                name = project?.ProjName ?? "",
                reportingPeriod =
                    $"FY{forecastList.Max(x => x.Year)}",

                versions = versionsObject
            }
        };

        return Ok(result);
    }

    //[HttpPost("GenerateVarianceReportForBudgetVsEACV2")]
    //public async Task<IActionResult> GenerateVarianceReportForBudgetVsEACV2(
    //[FromQuery] string projId,
    //[FromQuery] int budgetVersion,
    //[FromQuery] int eacVersion)
    //{
    //    // -----------------------------------
    //    // 1️⃣ Get Forecast Data
    //    // -----------------------------------
    //    var forecastList = await _context.PlForecasts
    //        .AsNoTracking()
    //        .Where(f =>
    //            f.ProjId == projId &&
    //            (f.PlId == budgetVersion || f.PlId == eacVersion))
    //        .ToListAsync();

    //    if (!forecastList.Any())
    //        return BadRequest("Forecast data is required.");

    //    // -----------------------------------
    //    // 2️⃣ Group Monthly Data
    //    // -----------------------------------
    //    var grouped = forecastList
    //        .GroupBy(f => new
    //        {
    //            f.ProjId,
    //            f.Month,
    //            f.Year
    //        })
    //        .OrderBy(g => g.Key.Year)
    //        .ThenBy(g => g.Key.Month);

    //    var varianceData = new List<VarianceComparison>();

    //    foreach (var g in grouped)
    //    {
    //        var budget = g
    //            .Where(x => x.PlId == budgetVersion)
    //            .ToList();

    //        var eac = g
    //            .Where(x => x.PlId == eacVersion)
    //            .ToList();

    //        if (!budget.Any() || !eac.Any())
    //            continue;

    //        // -----------------------------
    //        // Aggregate Budget
    //        // -----------------------------
    //        var budgetForecastedCost = budget.Sum(x => x.ForecastedCost ?? 0);
    //        var budgetActualCost = budget.Sum(x => x.Actualamt ?? 0);
    //        var budgetForecastedHours = budget.Sum(x => x.Forecastedhours);
    //        var budgetActualHours = budget.Sum(x => x.Actualhours);
    //        var budgetRevenue = budget.Sum(x => x.Revenue);

    //        // -----------------------------
    //        // Aggregate EAC
    //        // -----------------------------
    //        var eacForecastedCost = eac.Sum(x => x.ForecastedCost ?? 0);
    //        var eacActualCost = eac.Sum(x => x.Actualamt ?? 0);
    //        var eacForecastedHours = eac.Sum(x => x.Forecastedhours);
    //        var eacActualHours = eac.Sum(x => x.Actualhours);
    //        var eacRevenue = eac.Sum(x => x.Revenue);

    //        varianceData.Add(new VarianceComparison
    //        {
    //            ProjId = g.Key.ProjId,
    //            PlType = "BUD vs EAC",
    //            Month = g.Key.Month,
    //            Year = g.Key.Year,

    //            // -----------------------------
    //            // Budget Values (A)
    //            // -----------------------------
    //            ForecastedCostA = budgetForecastedCost,
    //            ActualCostA = budgetActualCost,
    //            ForecastedHoursA = budgetForecastedHours,
    //            ActualHoursA = budgetActualHours,
    //            RevenueA = budgetRevenue,

    //            // -----------------------------
    //            // EAC Values (B)
    //            // -----------------------------
    //            ForecastedCostB = eacForecastedCost,
    //            ActualCostB = eacActualCost,
    //            ForecastedHoursB = eacForecastedHours,
    //            ActualHoursB = eacActualHours,
    //            RevenueB = eacRevenue
    //        });
    //    }

    //    return Ok(varianceData);
    //    // -----------------------------------
    //    // 3️⃣ AI Summary
    //    // -----------------------------------
    //    //var aiSummaries = new Dictionary<string, string>();

    //    //var summary = await _aiService.GetVarianceSummaryAsync(
    //    //    "BUD vs EAC",
    //    //    budgetVersion,
    //    //    eacVersion,
    //    //    varianceData);

    //    //aiSummaries["BUD vs EAC"] = summary;

    //    //// -----------------------------------
    //    //// 4️⃣ Generate PDF
    //    //// -----------------------------------
    //    //var report = new VarianceReport(
    //    //    varianceData,
    //    //    aiSummaries,
    //    //    budgetVersion,
    //    //    eacVersion);

    //    //var pdfBytes = report.GeneratePdf();

    //    //return File(
    //    //    pdfBytes,
    //    //    "application/pdf",
    //    //    $"VarianceReport_V{budgetVersion}_V{eacVersion}.pdf");
    //}

    public class ForecastSummaryDto
    {
        public string ProjId { get; set; }
        public string PlType { get; set; }
        public int Version { get; set; }
        public string AcctId { get; set; }
        public string AccountName { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalBurden { get; set; }
    }


    [HttpPost("export-revenue-summary")]
    public async Task<IActionResult> ExportRevenueSummary(int planID, int templateId, string type)
    {

        var result = await (
            from f in _context.PlForecasts
            join a in _context.Accounts on f.AcctId equals a.AcctId
            join p in _context.PlProjectPlans on f.PlId equals p.PlId
            where p.ProjId == "22003.T.0069.00" && f.Month == 8 && f.Year == 2024 &&
                  (
                      (p.PlType == "EAC" && p.Version == 2) ||
                      (p.PlType == "BUD" && p.Version == 2)
                  )
            group new { f, a, p } by new
            {
                p.ProjId,
                p.PlType,
                p.Version,
                f.AcctId,
                a.AcctName
            } into g
            select new ForecastSummaryDto
            {
                ProjId = g.Key.ProjId,
                PlType = g.Key.PlType,
                Version = g.Key.Version.GetValueOrDefault(),
                AcctId = g.Key.AcctId,
                AccountName = g.Key.AcctName,
                TotalRevenue = g.Sum(x => x.f.Revenue),
                TotalCost = g.Sum(x => x.f.Cost),
                TotalBurden = g.Sum(x => x.f.Burden)
            }
        ).ToListAsync();






        var data = await _pl_ForecastService.CalculateCost(planID, templateId, type);
        var report = new RevenueAnalysisReport(data);
        var pdf = report.GeneratePdf();
        return File(pdf, "application/pdf", "RevenueSummary.pdf");
    }


    [HttpPost("ImportPlan_Ver1")]
    public async Task<IActionResult> ImportPlan_Ver1(IFormFile file)
    {
        _logger.LogInformation("ImportPlan_Ver1 called");

        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var plForecastData = new List<PlForecast>();
        int plID = 0;

        try
        {
            using var stream = file.OpenReadStream();
            var workbook = new XSSFWorkbook(stream);
            var sheet = workbook.GetSheetAt(0);

            var plForecasts = new List<PlForecast>();

            for (int rowIndex = 1; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                var row = sheet.GetRow(rowIndex);
                if (row == null) continue;

                var dateValue = DateOnly.Parse(row.GetCell(1)?.ToString() ?? DateTime.Now.ToString());

                var forecast = new PlForecast
                {
                    ProjId = row.GetCell(0)?.ToString() ?? string.Empty,
                    EmplId = row.GetCell(3)?.ToString() ?? string.Empty,
                    Forecastedhours = decimal.TryParse(row.GetCell(8)?.ToString(), out var hours) ? hours : 0,
                    Forecastedamt = decimal.TryParse(row.GetCell(9)?.ToString(), out var amt) ? (int)amt : 0,
                    Month = dateValue.Month,
                    Year = dateValue.Year,
                    PlId = plID // will be updated below
                };

                plForecasts.Add(forecast);
            }

            var distinctProjIds = plForecasts.Select(f => f.ProjId).Distinct();

            foreach (var projId in distinctProjIds)
            {
                plID = 0;

                var plan = await _projPlanService.AddProjectPlanAsync(new PlProjectPlan
                {
                    ProjId = projId,
                    Status = "Working",
                    PlType = "BUD",
                    Type = "BUD",
                    Source = "EXCEL"
                }, "excel");

                plID = plan?.PlId ?? 0;

                var projForecasts = plForecasts.Where(f => f.ProjId == projId).ToList();

                foreach (var projForecast in projForecasts)
                {
                    projForecast.PlId = plID;
                }

                plForecastData.AddRange(projForecasts);
            }

            await _pl_ForecastService.AddRangeAsync(plForecastData);

            return Ok("Excel file processed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import plan Ver1");
            return StatusCode(500, "An error occurred while importing the plan.");
        }
    }

    [HttpPost("RefreshForecastSummary")]
    public async Task<IActionResult> BulkUpsertProjForecastSummary(
            [FromQuery] string projId,
            [FromQuery] string plType,
            [FromQuery] int? version)
    {
        List<ProjForecastSummary> records = new List<ProjForecastSummary>();

        try
        {

            var ProjectPlan = _context.PlProjectPlans.FirstOrDefault(f => f.ProjId == projId && f.PlType == plType && f.Version == version);
            var plid = ProjectPlan.PlId;

            records = _context.PlForecasts
                        .Where(f => f.PlId == plid)
                        .GroupBy(f => new { f.ProjId, f.Year, f.Month })
                        .Select(g => new
                        {
                            g.Key.ProjId,
                            g.Key.Year,
                            g.Key.Month,
                            C = g.Sum(x => x.Cost),
                            FC = g.Sum(x => x.ForecastedCost),
                            R = g.Sum(x => x.Revenue),
                            B = g.Sum(x => x.Burden),
                            FH = g.Sum(x => x.Forecastedhours),
                            FA = g.Sum(x => x.Forecastedamt),
                            AH = g.Sum(x => x.Actualhours),
                            AA = g.Sum(x => x.Actualamt)
                        })
                        .OrderBy(r => r.ProjId).ThenBy(r => r.Year).ThenBy(r => r.Month)
                        .AsEnumerable()
                        .GroupBy(r => r.ProjId)
                        .SelectMany(g =>
                        {
                            Func<int, int, Func<dynamic, decimal>, decimal> ytd = (y, m, sel) => g.Where(z => z.Year == y && z.Month <= m).Sum(sel);
                            Func<int, Func<dynamic, decimal>, decimal> itd = (i, sel) => g.Take(i + 1).Sum(sel);
                            return g.Select((x, i) => new ProjForecastSummary
                            {
                                ProjId = x.ProjId,
                                PlType = plType.ToUpper(),
                                Version = version.GetValueOrDefault(),
                                Year = x.Year,
                                Month = x.Month,
                                MonthlyCost = x.C,
                                YtdCost = ytd(x.Year, x.Month, z => z.C),
                                ItdCost = itd(i, z => z.C),
                                ForecastedMonthlyCost = x.FC,
                                ForecastedYtdCost = ytd(x.Year, x.Month, z => z.FC),
                                ForecastedItdCost = itd(i, z => z.FC),
                                MonthlyRevenue = x.R,
                                YtdRevenue = ytd(x.Year, x.Month, z => z.R),
                                ItdRevenue = itd(i, z => z.R),
                                MonthlyBurden = x.B,
                                YtdBurden = ytd(x.Year, x.Month, z => z.B),
                                ItdBurden = itd(i, z => z.B),
                                MonthlyForecastedHours = x.FH,
                                YtdForecastedHours = ytd(x.Year, x.Month, z => z.FH),
                                ItdForecastedHours = itd(i, z => z.FH),
                                MonthlyForecastedAmt = x.FA,
                                YtdForecastedAmt = ytd(x.Year, x.Month, z => z.FA),
                                ItdForecastedAmt = itd(i, z => z.FA),
                                MonthlyActualHours = x.AH,
                                YtdActualHours = ytd(x.Year, x.Month, z => z.AH),
                                ItdActualHours = itd(i, z => z.AH),
                                MonthlyActualAmt = x.AA,
                                YtdActualAmt = ytd(x.Year, x.Month, z => z.AA),
                                ItdActualAmt = itd(i, z => z.AA)
                            });
                        })
                        .ToList();

            if (!(records?.Any() ?? false))
                return BadRequest("No records to process.");



            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            if (plType.ToUpper() == "EAC")
            {
                var actualMonthlySummary = await _context.PSRFinalData
                    .Where(p => p.ProjId == projId && (p.RateType == "A" || p.RateType == "N"))
                    .GroupBy(p => new { p.PdNo, p.FyCd, p.SubTotTypeNo, p.ProjId })
                    .Select(g => new MonthlyActualRevenueSummary
                    {
                        Month = g.Key.PdNo,
                        Year = Convert.ToInt16(g.Key.FyCd),
                        Revenue = g.Sum(x => x.PtdIncurAmt),
                        YtdRevenue = g.Sum(x => x.YtdIncurAmt),
                        ItdRevenue = g.Sum(x => x.PyIncurAmt),
                        subTotalType = g.Key.SubTotTypeNo
                        //Cost = (g.Key.SubTotTypeNo == 2 || g.Key.SubTotTypeNo == 3) ? g.Sum(x => x.PtdIncurAmt) : 0m

                    })
                    .ToListAsync();

                var summaryLookup = actualMonthlySummary
                    .ToDictionary(
                        x => (x.Month, x.Year, x.subTotalType),
                        x => new
                        {
                            x.Revenue,
                            x.YtdRevenue,
                            x.ItdRevenue
                        });

                var TargetMonthlySummary = await _context.PSRFinalData
                    .Where(p => p.ProjId == projId && (p.RateType == "T" || p.RateType == "N"))
                    .GroupBy(p => new { p.PdNo, p.FyCd, p.SubTotTypeNo, p.ProjId })
                    .Select(g => new MonthlyActualRevenueSummary
                    {
                        Month = g.Key.PdNo,
                        Year = Convert.ToInt16(g.Key.FyCd),
                        Revenue = g.Sum(x => x.PtdIncurAmt),
                        YtdRevenue = g.Sum(x => x.YtdIncurAmt),
                        ItdRevenue = g.Sum(x => x.PyIncurAmt),
                        subTotalType = g.Key.SubTotTypeNo
                        //Cost = (g.Key.SubTotTypeNo == 2 || g.Key.SubTotTypeNo == 3) ? g.Sum(x => x.PtdIncurAmt) : 0m

                    })
                    .ToListAsync();

                var TargetSummaryLookup = TargetMonthlySummary
                    .ToDictionary(
                        x => (x.Month, x.Year, x.subTotalType),
                        x => new
                        {
                            x.Revenue,
                            x.YtdRevenue,
                            x.ItdRevenue
                        });

                foreach (var temp in records)
                {
                    summaryLookup.TryGetValue((temp.Month, temp.Year, 1), out var revenue);

                    if (new DateOnly(temp.Year, temp.Month, 1) < ProjectPlan.ClosedPeriod.GetValueOrDefault())
                    {
                        if (revenue != null)
                        {
                            temp.MonthlyRevenue = revenue.Revenue;
                            temp.ItdRevenue = revenue.ItdRevenue;
                            temp.YtdRevenue = revenue.YtdRevenue;
                        }
                    }
                    else
                    {
                        temp.MonthlyCost = temp.MonthlyCost + temp.MonthlyBurden;
                    }

                    if (new DateOnly(temp.Year, temp.Month, 4) < ProjectPlan.ClosedPeriod.GetValueOrDefault())
                    {
                        if (revenue != null)
                        {
                            temp.MonthlyBurden = revenue.Revenue;
                            temp.ItdBurden = revenue.ItdRevenue;
                            temp.YtdBurden = revenue.YtdRevenue;
                        }
                    }

                    TargetSummaryLookup.TryGetValue((temp.Month, temp.Year, 1), out var TargetRevenue);

                    if (new DateOnly(temp.Year, temp.Month, 1) < ProjectPlan.ClosedPeriod.GetValueOrDefault())
                    {
                        if (TargetRevenue != null)
                        {
                            temp.MonthlyTargetRevenue = TargetRevenue.Revenue;
                            temp.ItdTargetRevenue = TargetRevenue.ItdRevenue;
                            temp.YtdTargetRevenue = TargetRevenue.YtdRevenue;
                        }
                        else
                        {
                            temp.MonthlyTargetRevenue = 0;
                            temp.ItdTargetRevenue = 0;
                            temp.YtdTargetRevenue = 0;
                        }
                    }


                    TargetSummaryLookup.TryGetValue((temp.Month, temp.Year, 4), out var TargetBurden);

                    if (new DateOnly(temp.Year, temp.Month, 1) < ProjectPlan.ClosedPeriod.GetValueOrDefault())
                    {
                        if (TargetRevenue != null)
                        {
                            temp.MonthlyTargetBurden = TargetBurden.Revenue;
                            temp.ItdTargetBurden = TargetBurden.ItdRevenue;
                            temp.YtdTargetBurden = TargetBurden.YtdRevenue;
                        }
                        else
                        {
                            temp.MonthlyTargetBurden = 0;
                            temp.ItdTargetBurden = 0;
                            temp.YtdTargetBurden = 0;
                        }
                    }
                }
            }


            var query = _context.ProjRevWrkPds.AsQueryable();

            query = query.Where(p => p.ProjId == projId);

            query = query.Where(p => p.VersionNo == version);

            query = query.Where(p => p.BgtType == plType);

            var allPds = await query.ToListAsync();

            foreach (var pd in allPds)
            {

                if (pd.EndDate.GetValueOrDefault().Year == 2024)
                {

                }
                //pd.RevAmt = records.FirstOrDefault(p => p.Month == pd.Period.GetValueOrDefault() && p.Year == pd.EndDate.GetValueOrDefault().Year)?.MonthlyRevenue;
                pd.RevAmt = records
                        .FirstOrDefault(p => p.Month == pd.Period.GetValueOrDefault()
                        && p.Year == pd.EndDate.GetValueOrDefault().Year)
                        ?.MonthlyRevenue ?? 0m;

                pd.TimeStamp = DateTime.UtcNow;
                pd.CreatedAt = pd.CreatedAt.ToLocalTime().ToUniversalTime();
            }



            _context.ProjRevWrkPds.UpdateRange(allPds);
            await _context.SaveChangesAsync();

            /////////////////////////////////////////////////////////////////////////////////////////////

            var props = typeof(ProjForecastSummary).GetProperties().Where(p => p.CanRead).ToList();
            var props1 = typeof(ProjForecastSummary)
              .GetProperties()
              .Where(p => p.CanRead)
              .Select(p =>
              {
                  var colAttr = p.GetCustomAttributes(typeof(ColumnAttribute), false)
                                 .Cast<ColumnAttribute>()
                                 .FirstOrDefault();
                  return colAttr?.Name ?? p.Name; // Fall back to property name if no attributeproj_id, pl_type, version, year, month
              })
              .ToList();

            var keyCols = new[] { "proj_id", "pl_type", "version", "month", "year" };

            var sql = $@"
                    INSERT INTO public.proj_forecast_summary ({string.Join(", ", props1.Select(p => p.ToLower()))})
                    VALUES {string.Join(", ", records.Select((r, ri) => $"({string.Join(", ", props.Select((p, pi) => $"@p{ri}_{pi}"))})"))}
                    ON CONFLICT ({string.Join(", ", keyCols.Select(c => c.ToLower()))})
                    DO UPDATE SET {string.Join(", ", props1
                                    .Where(p => !keyCols.Contains(p, StringComparer.OrdinalIgnoreCase))
                                    .Select(p => $"{p.ToLower()} = EXCLUDED.{p.ToLower()}"))};
                    ";

            var parameters = records
                .SelectMany((r, ri) => props.Select((p, pi) =>
                    new NpgsqlParameter($"@p{ri}_{pi}", p.GetValue(r) ?? DBNull.Value)))
                .ToArray();

            await _context.Database.ExecuteSqlRawAsync(sql, parameters);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
             $"Unexpected Error: {ex.Message}");
        }

        return Ok("Bulk upsert completed successfully.");
    }

    [HttpPost("GetEmployeeScheduleAsync/{emplId}/{year}")]
    public async Task<object> GetEmployeeScheduleAsync(string emplId, int year)
    {

        //var schedules = await _context.PlForecasts
        //    .Join(_context.PlProjectPlans,
        //          f => f.PlId,
        //          pp => pp.PlId,
        //          (f, pp) => new { f, pp })
        //    .Where(x => x.f.EmplId == emplId && x.pp.FinalVersion == true)
        //    .GroupBy(x => new { x.f.ProjId, x.f.Year, x.f.Month })
        //    .Select(g => new
        //    {
        //        g.Key.ProjId,
        //        g.Key.Year,
        //        g.Key.Month,
        //        Hours = g.Any(x => x.pp.PlType == "EAC")
        //                    ? g.Where(x => x.pp.PlType == "EAC").Sum(x => x.f.Actualhours)
        //                    : g.Where(x => x.pp.PlType == "BUD").Sum(x => x.f.Forecastedhours)
        //        //Source = g.Any(x => x.pp.PlType == "EAC") ? "EAC" : "BUD"
        //    })
        //    .OrderBy(r => r.ProjId)
        //    .ThenBy(r => r.Year)
        //    .ThenBy(r => r.Month)
        //    .ToListAsync();

        var latestPlanIds = await _context.PlProjectPlans.Where(p => p.FinalVersion == true)
            .GroupBy(p => p.ProjId)
            .Select(g => g.Max(x => x.PlId))
            .ToListAsync();

        var schedules = await _context.PlForecasts
            .Join(
                _context.PlProjectPlans,
                f => f.PlId,
                pp => pp.PlId,
                (f, pp) => new { f, pp }
            )
            .Where(x =>
                x.f.EmplId == emplId &&
                latestPlanIds.Contains(x.pp.PlId) &&
                (x.f.Year == year))
            .GroupBy(x => new
            {
                x.f.ProjId,
                x.f.Year,
                x.f.Month
            })
            .Select(g => new
            {
                g.Key.ProjId,
                g.Key.Year,
                g.Key.Month,

                Hours = g.Any(x => x.pp.PlType == "EAC")
                    ? g.Where(x => x.pp.PlType == "EAC")
                        .Sum(x => x.f.Actualhours)
                    : g.Where(x => x.pp.PlType == "BUD")
                        .Sum(x => x.f.Forecastedhours)
            })
            .OrderBy(r => r.ProjId)
            .ThenBy(r => r.Year)
            .ThenBy(r => r.Month)
            .ToListAsync();

        if (schedules.Count() == 0)
            return NotFound("N Schedule found for employee - " + emplId);

        // Compute global StartDate and EndDate across all schedules
        var minYear = schedules.Min(s => s.Year);
        var minMonth = schedules.Where(s => s.Year == minYear).Min(s => s.Month);
        var maxYear = schedules.Max(s => s.Year);
        var maxMonth = schedules.Where(s => s.Year == maxYear).Max(s => s.Month);

        var startDate = new DateOnly(minYear, minMonth, 1);
        var endDate = new DateOnly(
            maxYear,
            maxMonth,
            DateTime.DaysInMonth(maxYear, maxMonth)
        );

        startDate = new DateOnly(year, 1, 1);

        endDate = new DateOnly(year, 12, 31);

        ScheduleHelper scheduleHelper = new ScheduleHelper();

        var schedule = scheduleHelper.GetWorkingDaysForDuration(startDate, endDate, _orgService);

        // Break schedules by ProjId
        var projects = schedules
            .GroupBy(s => s.ProjId)
            .Select(pg => new
            {
                ProjId = pg.Key,
                Schedules = pg.ToList()
            })
            .ToList();

        return new
        {
            StandardSchedule = schedule,
            StartDate = startDate,
            EndDate = endDate,
            projects = projects
        };
    }

    [HttpPost("GetEmployeeScheduleAsyncV1/{emplId?}/{year?}")]
    public async Task<object> GetEmployeeScheduleAsyncV1(string? emplId = null, int? year = null)
    {
        // -------------------------------------------------------
        // GET LATEST PLAN IDS
        // -------------------------------------------------------
        var latestPlanIds = await _context.PlProjectPlans
            .GroupBy(p => p.ProjId)
            .Select(g => g.Max(x => x.PlId))
            .ToListAsync();

        // -------------------------------------------------------
        // BASE QUERY
        // -------------------------------------------------------
        var forecastQuery = _context.PlForecasts
            .Join(
                _context.PlProjectPlans,
                f => f.PlId,
                pp => pp.PlId,
                (f, pp) => new { f, pp }
            )
            .Where(x => latestPlanIds.Contains(x.pp.PlId) && (!year.HasValue || x.f.Year == year.Value));

        // -------------------------------------------------------
        // FILTER FOR SPECIFIC EMPLOYEE IF PROVIDED
        // -------------------------------------------------------
        if (!string.IsNullOrWhiteSpace(emplId))
        {
            forecastQuery = forecastQuery
                .Where(x => x.f.EmplId == emplId);
        }

        // -------------------------------------------------------
        // GET SCHEDULES
        // -------------------------------------------------------
        var schedules = await forecastQuery
            .GroupBy(x => new
            {
                x.f.EmplId,
                x.f.ProjId,
                x.f.Year,
                x.f.Month
            })
            .Select(g => new
            {
                g.Key.EmplId,
                g.Key.ProjId,
                g.Key.Year,
                g.Key.Month,

                Hours = g.Any(x => x.pp.PlType == "EAC")
                    ? g.Where(x => x.pp.PlType == "EAC")
                        .Sum(x => x.f.Actualhours)
                    : g.Where(x => x.pp.PlType == "BUD")
                        .Sum(x => x.f.Forecastedhours)

                // Source = g.Any(x => x.pp.PlType == "EAC")
                //     ? "EAC"
                //     : "BUD"
            })
            .OrderBy(r => r.EmplId)
            .ThenBy(r => r.ProjId)
            .ThenBy(r => r.Year)
            .ThenBy(r => r.Month)
            .ToListAsync();

        // -------------------------------------------------------
        // NO DATA
        // -------------------------------------------------------
        if (!schedules.Any())
        {
            return NotFound(
                string.IsNullOrWhiteSpace(emplId)
                    ? "No schedules found"
                    : $"No schedule found for employee - {emplId}"
            );
        }

        // -------------------------------------------------------
        // GLOBAL DATE RANGE
        // -------------------------------------------------------
        var minYear = schedules.Min(s => s.Year);

        var minMonth = schedules
            .Where(s => s.Year == minYear)
            .Min(s => s.Month);

        var maxYear = schedules.Max(s => s.Year);

        var maxMonth = schedules
            .Where(s => s.Year == maxYear)
            .Max(s => s.Month);

        var startDate = new DateOnly(minYear, minMonth, 1);

        var endDate = new DateOnly(
            maxYear,
            maxMonth,
            DateTime.DaysInMonth(maxYear, maxMonth)
        );

        // -------------------------------------------------------
        // STANDARD SCHEDULE
        // -------------------------------------------------------
        ScheduleHelper scheduleHelper = new ScheduleHelper();

        var standardSchedule =
            scheduleHelper.GetWorkingDaysForDuration(
                startDate,
                endDate,
                _orgService
            );

        // -------------------------------------------------------
        // EMPLOYEE -> PROJECT -> SCHEDULE HIERARCHY
        // -------------------------------------------------------
        var employees = schedules
            .GroupBy(e => e.EmplId)
            .Select(emp => new
            {
                EmplId = emp.Key,

                Projects = emp
                    .GroupBy(p => p.ProjId)
                    .Select(pg => new
                    {
                        ProjId = pg.Key,

                        Schedules = pg
                            .Select(s => new
                            {
                                s.Year,
                                s.Month,
                                s.Hours
                            })
                            .ToList()
                    })
                    .ToList()
            })
            .ToList();

        // -------------------------------------------------------
        // RETURN RESPONSE
        // -------------------------------------------------------
        return new
        {
            StandardSchedule = standardSchedule,
            StartDate = startDate,
            EndDate = endDate,
            Employees = employees
        };
    }

    [HttpPost("GetEmployeeOverUtilizedScheduleAsync/{plid}/{emplId}")]
    public async Task<object> GetEmployeeOverUtilizedScheduleAsync(int plid, string emplId)
    {
        CeilingHelper helper = new CeilingHelper(_context);
        var warnings = helper.GetWarningsByEmployee(plid, emplId);
        // assume warnings is a list of objects with Year, Month

        var warningPeriods = warnings
            .Select(w => new { w.Year, w.Month })
            .ToList().Distinct();

        // query schedules, restrict by warning periods
        var schedules = await _context.PlForecasts
            .Join(_context.PlProjectPlans,
                  f => f.PlId,
                  pp => pp.PlId,
                  (f, pp) => new { f, pp })
            .Where(x => x.f.EmplId == emplId && x.pp.FinalVersion == true)
            .GroupBy(x => new { x.f.ProjId, x.f.Year, x.f.Month, x.pp.PlType, x.pp.Version })
            .Select(g => new
            {
                g.Key.ProjId,
                g.Key.Year,
                g.Key.Month,
                g.Key.Version,
                g.Key.PlType,
                Hours = g.Any(x => x.pp.PlType == "EAC")
                            ? g.Where(x => x.pp.PlType == "EAC").Sum(x => x.f.Actualhours)
                            : g.Where(x => x.pp.PlType == "BUD").Sum(x => x.f.Forecastedhours)
                //Source = g.Any(x => x.pp.PlType == "EAC") ? "EAC" : "BUD"
            })
            .OrderBy(r => r.ProjId)
            .ThenBy(r => r.Year)
            .ThenBy(r => r.Month)
            .ToListAsync();

        if (!schedules.Any())
            return new { StandardSchedule = new List<object>(), Projects = new List<object>() };

        schedules = schedules.Where(x => warningPeriods.Any(p => p.Year == x.Year && p.Month == x.Month)).ToList();

        // restrict schedule helper only to warning periods
        var periods = warningPeriods
            .Select(p => new
            {
                StartDate = new DateOnly(p.Year, p.Month, 1),
                EndDate = new DateOnly(p.Year, p.Month, DateTime.DaysInMonth(p.Year, p.Month))
            })
            .ToList();

        ScheduleHelper scheduleHelper = new ScheduleHelper();

        var standardSchedule = new List<object>();
        foreach (var p in periods)
        {
            var sched = scheduleHelper.GetWorkingDaysForDuration(p.StartDate, p.EndDate, _orgService);
            standardSchedule.Add(new
            {
                //Year = p.StartDate.Year,
                //Month = p.StartDate.Month,
                Schedule = sched.FirstOrDefault()
            });
        }

        // break schedules by ProjId new { x.f.ProjId, x.f.Year, x.f.Month, x.pp.PlType, x.pp.Version }
        var projects = schedules
            .GroupBy(s => new { s.ProjId, s.PlType, s.Version })
            .Select(pg => new
            {
                proj_id = pg.Key.ProjId,
                Type = pg.Key.PlType,
                Version = pg.Key.Version,
                Schedules = pg.Select(p => new { p.Year, p.Month, p.Hours }).ToList()
            })
            .ToList();

        return new
        {
            StandardSchedule = standardSchedule,
            Projects = projects
        };
    }


    //[HttpPost("GetProjectFinancials")]
    //public async Task<List<ProjectFinancialSummaryDto>> GetProjectFinancials(string projId, string planType)
    //{

    //    var plids = await _context.PlProjectPlans
    //        .Where(p => p.ProjId.StartsWith(projId) && p.FinalVersion == true && p.PlType == planType)
    //        .Select(p => p.PlId)
    //        .ToListAsync();

    //    var result = await _context.PlForecasts
    //            .AsNoTracking()
    //            .Where(f => plids.Contains(f.PlId))
    //            .GroupBy(f => f.ProjId)
    //            .Select(g => new ProjectFinancialSummaryDto
    //            {
    //                ProjId = g.Key!,
    //                Revenue = g.Sum(x => x.Revenue),
    //                Cost = g.Sum(x => x.Cost),

    //                Profit = g.Sum(x => x.Revenue) - g.Sum(x => x.Cost),

    //                ProfitPercent =
    //                    g.Sum(x => x.Revenue) == 0
    //                        ? 0
    //                        : ((g.Sum(x => x.Revenue) - g.Sum(x => x.Cost))
    //                            / g.Sum(x => x.Revenue)) * 100,

    //                Backlog =
    //                    g.Sum(x => x.Forecastedamt ?? 0) - g.Sum(x => x.Revenue)
    //            })
    //            .ToListAsync();

    //    return result;

    //}

    [HttpPost("GetProjectFinancials")]
    public async Task<List<ProjectFinancialSummary1Dto>> GetProjectFinancials(
    string projId,
    string planType)
    {

        var ProjModfunding = _context.ProjectModifications
            .Where(p => p.ProjId.StartsWith(projId));

        decimal costFunding = 0, feeFunding = 0;

        if (ProjModfunding != null && ProjModfunding.Count() > 0)
        {
            costFunding = ProjModfunding.ToList().Sum(p => p.ProjFCstAmt).GetValueOrDefault();
            feeFunding = ProjModfunding.ToList().Sum(p => p.ProjFFeeAmt).GetValueOrDefault();
        }


        var baseQuery =
            from p in _context.PlProjectPlans
            join f in _context.PlForecasts on p.PlId equals f.PlId
            where p.ProjId.StartsWith(projId)
                  && p.FinalVersion == true
                  && p.PlType == planType
            select new { p.ProjId, f };

        // 🔹 Leaf-level (each project)
        var projectLevel =
            from x in baseQuery
            group x by x.ProjId into g
            select new ProjectFinancialSummary1Dto
            {
                ProjId = g.Key,
                IsRollup = false,

                Revenue = g.Sum(x => x.f.Revenue),
                Cost = g.Sum(x => x.f.Cost),

                Profit = g.Sum(x => x.f.Revenue) - g.Sum(x => x.f.Cost),

                ProfitPercent =
                    g.Sum(x => x.f.Revenue) == 0
                        ? 0
                        : ((g.Sum(x => x.f.Revenue) - g.Sum(x => x.f.Cost))
                            / g.Sum(x => x.f.Revenue)) * 100,

                Backlog =
                    g.Sum(x => x.f.Forecastedamt ?? 0)
                    - g.Sum(x => x.f.Revenue)
            };

        // 🔹 Roll-up (upper / parent level)
        var rollup =
            from x in baseQuery
            group x by 1 into g
            select new ProjectFinancialSummary1Dto
            {
                ProjId = projId,
                IsRollup = true,

                Revenue = g.Sum(x => x.f.Revenue),
                Cost = g.Sum(x => x.f.Cost),

                Profit = g.Sum(x => x.f.Revenue) - g.Sum(x => x.f.Cost),

                ProfitPercent =
                    g.Sum(x => x.f.Revenue) == 0
                        ? 0
                        : ((g.Sum(x => x.f.Revenue) - g.Sum(x => x.f.Cost))
                            / g.Sum(x => x.f.Revenue)) * 100,

                Backlog =
                    g.Sum(x => x.f.Forecastedamt ?? 0)
                    - g.Sum(x => x.f.Revenue)
            };

        var projectDetails = await projectLevel
            .Concat(rollup)
            .OrderByDescending(x => x.IsRollup)
            .ThenBy(x => x.ProjId)
            .ToListAsync();


        // Add funding info to all records
        foreach (var record in projectDetails)
        {
            //record.Funding = ProjModfunding.Where(p=>p.ProjId.StartsWith(record.ProjId)).Sum(x=>x.ProjFCstAmt).GetValueOrDefault() + ProjModfunding.Where(p => p.ProjId.StartsWith(record.ProjId)).Sum(x => x.ProjFFeeAmt).GetValueOrDefault();
            record.Funding = await ProjModfunding
                            .Where(p => p.ProjId.StartsWith(record.ProjId))
                            .SumAsync(p => (p.ProjFCstAmt ?? 0m) + (p.ProjFFeeAmt ?? 0m));

            record.Backlog = (record.Funding - record.Revenue);

        }

        return projectDetails;
    }

    [HttpGet("GetSummary")]
    public async Task<IActionResult> GetSummary()
    {
        _logger.LogInformation("GetAllForecasts called at {Time}", DateTime.UtcNow);
        try
        {

            var plids = await _context.PlProjectPlans
                .Where(p => p.FinalVersion == true && p.PlType == "EAC")
                .Select(p => p.PlId)
                .ToListAsync();

            var forecasts = await _context.PlForecasts.Where(p => plids.Contains(p.PlId)).ToListAsync();
            var groups = new Dictionary<string, FinancialNode>();

            foreach (var f in forecasts)
            {
                if (string.IsNullOrEmpty(f.AcctId)) continue;

                var groupKey = GetGroup(f.AcctId);
                var acctKey = f.AcctId;
                var projKey = f.ProjId ?? "NO-PROJ";
                var empKey = f.EmplId ?? "NO-EMP";

                var monthKey = GetMonthKey(f.Year, f.Month);

                var amount = f.Cost;
                var revenue = f.Revenue; // ✅ NEW

                // ---------------- GROUP ----------------
                if (!groups.TryGetValue(groupKey, out var groupNode))
                {
                    groupNode = new FinancialNode
                    {
                        Id = groupKey,
                        Name = GetGroupName(groupKey),
                        Type = "group"
                    };
                    groups[groupKey] = groupNode;
                }

                AddAmount(groupNode.MonthlyTotals, monthKey, amount);
                AddAmount(groupNode.RevenueTotals, monthKey, revenue); // ✅

                // ---------------- ACCOUNT ----------------
                if (!groupNode.Lookup.TryGetValue(acctKey, out var acctNode))
                {
                    acctNode = new FinancialNode
                    {
                        Id = acctKey,
                        Name = acctKey,
                        Type = "account"
                    };
                    groupNode.Lookup[acctKey] = acctNode;
                    groupNode.Children.Add(acctNode);
                }

                AddAmount(acctNode.MonthlyTotals, monthKey, amount);
                AddAmount(acctNode.RevenueTotals, monthKey, revenue); // ✅

                // ---------------- PROJECT ----------------
                if (!acctNode.Lookup.TryGetValue(projKey, out var projNode))
                {
                    projNode = new FinancialNode
                    {
                        Id = projKey,
                        Name = projKey,
                        Type = "project"
                    };
                    acctNode.Lookup[projKey] = projNode;
                    acctNode.Children.Add(projNode);
                }

                AddAmount(projNode.MonthlyTotals, monthKey, amount);
                AddAmount(projNode.RevenueTotals, monthKey, revenue); // ✅

                // ---------------- EMPLOYEE ----------------
                if (!projNode.Lookup.TryGetValue(empKey, out var empNode))
                {
                    empNode = new FinancialNode
                    {
                        Id = empKey,
                        Name = empKey,
                        Type = "employee"
                    };
                    projNode.Lookup[empKey] = empNode;
                    projNode.Children.Add(empNode);
                }

                AddAmount(empNode.MonthlyTotals, monthKey, amount);
                AddAmount(empNode.RevenueTotals, monthKey, revenue); // ✅
            }

            //var groups = new Dictionary<string, FinancialNode>();

            //foreach (var f in forecasts)
            //{
            //    if (string.IsNullOrEmpty(f.AcctId)) continue;

            //    var groupKey = GetGroup(f.AcctId);
            //    var acctKey = f.AcctId;
            //    var projKey = f.ProjId ?? "NO-PROJ";
            //    var empKey = f.EmplId ?? "NO-EMP";

            //    var monthKey = GetMonthKey(f.Year, f.Month);
            //    var amount = f.Forecastedamt ?? 0;

            //    // ---------------- GROUP ----------------
            //    if (!groups.TryGetValue(groupKey, out var groupNode))
            //    {
            //        groupNode = new FinancialNode
            //        {
            //            Id = groupKey,
            //            Name = GetGroupName(groupKey),
            //            Type = "group"
            //        };
            //        groups[groupKey] = groupNode;
            //    }

            //    AddAmount(groupNode, monthKey, amount);

            //    // ---------------- ACCOUNT ----------------
            //    if (!groupNode.Lookup.TryGetValue(acctKey, out var acctNode))
            //    {
            //        acctNode = new FinancialNode
            //        {
            //            Id = acctKey,
            //            Name = acctKey,
            //            Type = "account"
            //        };
            //        groupNode.Lookup[acctKey] = acctNode;
            //        groupNode.Children.Add(acctNode);
            //    }

            //    AddAmount(acctNode, monthKey, amount);

            //    // ---------------- PROJECT ----------------
            //    if (!acctNode.Lookup.TryGetValue(projKey, out var projNode))
            //    {
            //        projNode = new FinancialNode
            //        {
            //            Id = projKey,
            //            Name = projKey,
            //            Type = "project"
            //        };
            //        acctNode.Lookup[projKey] = projNode;
            //        acctNode.Children.Add(projNode);
            //    }

            //    AddAmount(projNode, monthKey, amount);

            //    // ---------------- EMPLOYEE ----------------
            //    if (!projNode.Lookup.TryGetValue(empKey, out var empNode))
            //    {
            //        empNode = new FinancialNode
            //        {
            //            Id = empKey,
            //            Name = empKey,
            //            Type = "employee"
            //        };
            //        projNode.Lookup[empKey] = empNode;
            //        projNode.Children.Add(empNode);
            //    }

            //    AddAmount(empNode, monthKey, amount);
            //}
            var result = groups.Values.ToList();

            //var group = result.First(x => x.Id == "40.01"); // Revenue group

            //var groupTotal = group.RevenueTotals.GetValueOrDefault("2026-01");

            //var accountTotal = group.Children
            //    .Sum(a => a.RevenueTotals.GetValueOrDefault("2026-01"));

            //var projectTotal = group.Children
            //    .SelectMany(a => a.Children)
            //    .Sum(p => p.RevenueTotals.GetValueOrDefault("2026-01"));

            //var employeeTotal = group.Children
            //    .SelectMany(a => a.Children)
            //    .SelectMany(p => p.Children)
            //    .Sum(e => e.RevenueTotals.GetValueOrDefault("2026-01"));

            //Console.WriteLine($"Group: {groupTotal}");
            //Console.WriteLine($"Account Sum: {accountTotal}");
            //Console.WriteLine($"Project Sum: {projectTotal}");
            //Console.WriteLine($"Employee Sum: {employeeTotal}");

            var revenueGroup = result.FirstOrDefault(x => x.Id == "40.01");

            if (revenueGroup == null)
            {
                revenueGroup = new FinancialNode
                {
                    Id = "40.01",
                    Name = "Revenue",
                    Type = "group"
                };

                result.Add(revenueGroup);
            }

            // exclude revenue itself
            var otherGroups = result.Where(x => x.Id != "40.01").ToList();

            // 🔥 PERIOD-WISE revenue aggregation
            revenueGroup.RevenueTotals = MergeTotals(
                otherGroups.Select(g => g.RevenueTotals)
            );

            // optional: same for cost/forecast
            revenueGroup.MonthlyTotals = MergeTotals(
                otherGroups.Select(g => g.MonthlyTotals)
            );

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all forecasts");
            return StatusCode(500, "An error occurred while fetching forecasts.");
        }
    }


    [HttpGet("GetSummaryV1Working")]
    public async Task<IActionResult> GetSummaryV1Working()
    {
        _logger.LogInformation("GetAllForecasts called at {Time}", DateTime.UtcNow);
        try
        {

            var actualMonthlySummary = await _context.PSRFinalData
                    .Where(p => p.SubTotTypeNo == 1)
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





            var plids = await _context.PlProjectPlans
                .Where(p => p.FinalVersion == true && p.PlType == "EAC")
                .Select(p => p.PlId)
                .ToListAsync();

            var forecasts = await _context.PlForecasts.Where(p => plids.Contains(p.PlId)).ToListAsync();
            var groups = new Dictionary<string, FinancialNode>();

            foreach (var f in forecasts)
            {
                if (string.IsNullOrEmpty(f.AcctId)) continue;

                var groupKey = GetGroup(f.AcctId);
                var acctKey = f.AcctId;
                var projKey = f.ProjId ?? "NO-PROJ";
                var empKey = f.EmplId ?? "NO-EMP";

                var monthKey = GetMonthKey(f.Year, f.Month);

                var amount = f.Cost;
                var revenue = f.Revenue; // ✅ NEW

                // ---------------- GROUP ----------------
                if (!groups.TryGetValue(groupKey, out var groupNode))
                {
                    groupNode = new FinancialNode
                    {
                        Id = groupKey,
                        Name = GetGroupName(groupKey),
                        Type = "group"
                    };
                    groups[groupKey] = groupNode;
                }

                AddAmount(groupNode.MonthlyTotals, monthKey, amount);
                AddAmount(groupNode.RevenueTotals, monthKey, revenue); // ✅

                // ---------------- ACCOUNT ----------------
                if (!groupNode.Lookup.TryGetValue(acctKey, out var acctNode))
                {
                    acctNode = new FinancialNode
                    {
                        Id = acctKey,
                        Name = acctKey,
                        Type = "account"
                    };
                    groupNode.Lookup[acctKey] = acctNode;
                    groupNode.Children.Add(acctNode);
                }

                AddAmount(acctNode.MonthlyTotals, monthKey, amount);
                AddAmount(acctNode.RevenueTotals, monthKey, revenue); // ✅

                // ---------------- PROJECT ----------------
                if (!acctNode.Lookup.TryGetValue(projKey, out var projNode))
                {
                    projNode = new FinancialNode
                    {
                        Id = projKey,
                        Name = projKey,
                        Type = "project"
                    };
                    acctNode.Lookup[projKey] = projNode;
                    acctNode.Children.Add(projNode);
                }

                AddAmount(projNode.MonthlyTotals, monthKey, amount);
                AddAmount(projNode.RevenueTotals, monthKey, revenue); // ✅

                // ---------------- EMPLOYEE ----------------
                if (!projNode.Lookup.TryGetValue(empKey, out var empNode))
                {
                    empNode = new FinancialNode
                    {
                        Id = empKey,
                        Name = empKey,
                        Type = "employee"
                    };
                    projNode.Lookup[empKey] = empNode;
                    projNode.Children.Add(empNode);
                }

                AddAmount(empNode.MonthlyTotals, monthKey, amount);
                AddAmount(empNode.RevenueTotals, monthKey, revenue); // ✅
            }

            var result = groups.Values.ToList();

            var revenueGroup = result.FirstOrDefault(x => x.Id == "40.01");

            if (revenueGroup == null)
            {
                revenueGroup = new FinancialNode
                {
                    Id = "40.01",
                    Name = "Revenue",
                    Type = "group"
                };

                result.Add(revenueGroup);
            }




            // exclude revenue itself
            var otherGroups = result.Where(x => x.Id != "40.01").ToList();

            // 🔥 PERIOD-WISE revenue aggregation
            revenueGroup.RevenueTotals = MergeTotals(
                otherGroups.Select(g => g.RevenueTotals)
            );

            // optional: same for cost/forecast
            revenueGroup.MonthlyTotals = MergeTotals(
                otherGroups.Select(g => g.MonthlyTotals)
            );


            foreach (var temp in result
            .Where(x => x.Id == "40.01")
            .FirstOrDefault()
            .RevenueTotals)
            {
                // temp.Key => "2026-01"

                var parts = temp.Key.Split('-');

                int year = int.Parse(parts[0]);
                int month = int.Parse(parts[1]);

                summaryLookup.TryGetValue((month, year, 1), out var revenue);

                if (new DateOnly(year, month, 1) < DateOnly.FromDateTime(DateTime.Parse(_context.PlConfigValues.FirstOrDefault(r => r.Name.Equals("closing_period")).Value)))
                {
                    decimal finalRevenue = revenue;

                    // update dictionary
                    result
                        .Where(x => x.Id == "40.01")
                        .First()
                        .RevenueTotals[temp.Key] = finalRevenue;
                }

                //if (new DateOnly(year, month, 1) < projPlan.ClosedPeriod.GetValueOrDefault())
                //{
                //    decimal finalRevenue = revenue;

                //    var adjustment = adj.FirstOrDefault(p =>
                //        p.EndDate.GetValueOrDefault().Year == year &&
                //        p.Period == month);

                //    if (adjustment != null)
                //    {
                //        finalRevenue += adjustment.RevAdj.GetValueOrDefault();
                //        finalRevenue += adjustment.RevAdj1.GetValueOrDefault();

                //        if (revenueFormula.ToUpper() == "CPFC")
                //        {
                //            if (projPlan.Type.Trim().ToUpper() == "A")
                //            {
                //                finalRevenue += adjustment.ActualFeeAmountOnCost;
                //            }
                //            else
                //            {
                //                finalRevenue += adjustment.TargetFeeAmountOnCost;
                //            }
                //        }

                //        if (revenueFormula.ToUpper() == "UNIT")
                //        {
                //            finalRevenue = adjustment.RevAmt.GetValueOrDefault();
                //        }
                //    }

                //    // update dictionary
                //    result
                //        .Where(x => x.Id == "40.01")
                //        .First()
                //        .RevenueTotals[temp.Key] = finalRevenue;
                //}
            }


            //foreach(var temp in result.Where(x=>x.Id == "40.01").FirstOrDefault().RevenueTotals)
            //{
            //    summaryLookup.TryGetValue(temp.Key, out var revenue);
            //    if (new DateOnly(temp.Year, temp.Month, 1) < projPlan.ClosedPeriod.GetValueOrDefault())
            //    {
            //        temp.Revenue = revenue;

            //        var adustment = adj.FirstOrDefault(p => p.EndDate.GetValueOrDefault().Year == temp.Year && p.Period == temp.Month);
            //        if (adustment != null)
            //        {
            //            temp.Revenue += adustment.RevAdj.GetValueOrDefault();
            //            temp.Revenue += adustment.RevAdj1.GetValueOrDefault();
            //            if (revenueFormula.ToUpper() == "CPFC")
            //            {
            //                if (projPlan.Type.Trim().ToUpper() == "A")
            //                {
            //                    temp.Revenue += adustment.ActualFeeAmountOnCost;
            //                }
            //                else
            //                {
            //                    temp.Revenue += adustment.TargetFeeAmountOnCost;

            //                }
            //            }
            //        }

            //        if (revenueFormula.ToUpper() == "UNIT")
            //        {
            //            temp.Revenue = adustment.RevAmt.GetValueOrDefault();
            //        }
            //        //temp.Cost = laborCost  ;
            //        //temp.OtherDifrectCost = otherDirectCost;
            //    }
            //    //summaryLookup.TryGetValue((temp.Month, temp.Year, 1), out var revenue);
            //}

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all forecasts");
            return StatusCode(500, "An error occurred while fetching forecasts.");
        }
    }


    [HttpGet("GetSummaryV1WorkingV1")]
    public async Task<IActionResult> GetSummaryV1WorkingV1()
    {
        _logger.LogInformation("GetAllForecasts called at {Time}", DateTime.UtcNow);
        try
        {
            var accountLookup = await _context.Accounts
                .Where(a => a.AcctId != null)
                .ToDictionaryAsync(
                    a => a.AcctId,
                    a => a.AcctName
                );

            var actualMonthlySummary = await _context.PSRFinalData
                    .Where(p => p.SubTotTypeNo == 1)
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

            //var actualMonthlySummary = await _context.PSRFinalData
            //        .Where(p => p.SubTotTypeNo == 1)
            //        .GroupBy(p => new { p.PdNo, p.FyCd, p.SubTotTypeNo, p.ProjId })
            //        .Select(g => new MonthlySummary
            //        {
            //            Month = g.Key.PdNo,
            //            Year = Convert.ToInt16(g.Key.FyCd),
            //            Cost = g.Sum(x => x.PtdIncurAmt),
            //            subTotalType = g.Key.SubTotTypeNo
            //            //Cost = (g.Key.SubTotTypeNo == 2 || g.Key.SubTotTypeNo == 3) ? g.Sum(x => x.PtdIncurAmt) : 0m

            //        })
            //        .ToListAsync();

            var summaryLookup = actualMonthlySummary
                .ToDictionary(
                    x => (x.Month, x.Year, x.subTotalType),
                    x => x.Cost);

            var plids = await _context.PlProjectPlans
                .Where(p => p.FinalVersion == true && (p.PlType == "EAC" || p.PlType == "NBBUD"))
                .Select(p => p.PlId)
                .ToListAsync();


            var actualMonthlyadjSummary1 = _context.ProjRevWrkPds.Where(p => plids.Contains(p.Pl_Id)).ToList();

            foreach (var item in actualMonthlyadjSummary1)
            {
                item.Fy_Cd = item.EndDate.GetValueOrDefault().Year;
            }
            //var actualMonthlyadjSummary = _context.ProjRevWrkPds
            var actualMonthlyadjSummary = actualMonthlyadjSummary1
                .Where(p => plids.Contains(p.Pl_Id))
                .AsEnumerable()
                .GroupBy(p => new { p.Fy_Cd, p.Period })
                .Select(g => new
                {
                    g.Key.Fy_Cd,
                    g.Key.Period,
                    AdjAmt = g.Sum(x => x.RevAdj ?? 0)
                })
                .ToList();

            //var actualMonthlyadjSummary = await _context.ProjRevWrkPds
            //    .Where(p => plids.Contains(p.Pl_Id))
            //    .AsEnumerable() // because Fy_Cd is unmapped
            //    .GroupBy(p => new { p.Fy_Cd, p.Period })
            //    .Select(g => new
            //    {
            //        g.Key.Fy_Cd,
            //        g.Key.Period,
            //        AdjAmt = g.Sum(x => x.RevAdj)
            //    })
            //    .ToListAsync();

            //var actualMonthlyadjSummary = await _context.ProjRevWrkPds.Where(p => plids.Contains(p.Pl_Id))
            //    .GroupBy(p => new { p.Fy_Cd, p.Period })
            //    .ToListAsync();
            //.GroupBy(p => new { p.Fy_Cd, p.Period })

            var forecasts = await _context.PlForecasts.Where(p => plids.Contains(p.PlId)).ToListAsync();
            var groups = new Dictionary<string, FinancialNode>();

            foreach (var f in forecasts)
            {
                if (string.IsNullOrEmpty(f.AcctId)) continue;

                var groupKey = GetGroup(f.AcctId);
                var acctKey = f.AcctId;
                var projKey = f.ProjId ?? "NO-PROJ";
                var empKey = f.EmplId ?? "NO-EMP";

                var monthKey = GetMonthKey(f.Year, f.Month);

                var amount = f.Cost;
                var revenue = f.Revenue; // ✅ NEW

                // ---------------- GROUP ----------------
                if (!groups.TryGetValue(groupKey, out var groupNode))
                {
                    groupNode = new FinancialNode
                    {
                        Id = groupKey,
                        Name = GetGroupName(groupKey),
                        Type = "group"
                    };
                    groups[groupKey] = groupNode;
                }

                AddAmount(groupNode.MonthlyTotals, monthKey, amount);
                AddAmount(groupNode.RevenueTotals, monthKey, revenue); // ✅

                // ---------------- ACCOUNT ----------------
                if (!groupNode.Lookup.TryGetValue(acctKey, out var acctNode))
                {
                    acctNode = new FinancialNode
                    {
                        Id = acctKey,
                        Name = accountLookup.ContainsKey(acctKey)
                                ? $"{acctKey} - {accountLookup[acctKey]}"
                                : acctKey,
                        // Name = acctKey,
                        Type = "account"
                    };
                    groupNode.Lookup[acctKey] = acctNode;
                    groupNode.Children.Add(acctNode);
                }

                AddAmount(acctNode.MonthlyTotals, monthKey, amount);
                AddAmount(acctNode.RevenueTotals, monthKey, revenue); // ✅

                // ---------------- PROJECT ----------------
                if (!acctNode.Lookup.TryGetValue(projKey, out var projNode))
                {
                    projNode = new FinancialNode
                    {
                        Id = projKey,
                        Name = projKey,
                        Type = "project"
                    };
                    acctNode.Lookup[projKey] = projNode;
                    acctNode.Children.Add(projNode);
                }

                AddAmount(projNode.MonthlyTotals, monthKey, amount);
                AddAmount(projNode.RevenueTotals, monthKey, revenue); // ✅

                // ---------------- EMPLOYEE ----------------
                if (!projNode.Lookup.TryGetValue(empKey, out var empNode))
                {
                    empNode = new FinancialNode
                    {
                        Id = empKey,
                        Name = empKey,
                        Type = "employee"
                    };
                    projNode.Lookup[empKey] = empNode;
                    projNode.Children.Add(empNode);
                }

                AddAmount(empNode.MonthlyTotals, monthKey, amount);
                AddAmount(empNode.RevenueTotals, monthKey, revenue); // ✅
            }

            var result = groups.Values.ToList();

            var revenueGroup = result.FirstOrDefault(x => x.Id == "40.01");

            if (revenueGroup == null)
            {
                revenueGroup = new FinancialNode
                {
                    Id = "40.01",
                    Name = "Revenue",
                    Type = "group"
                };

                result.Add(revenueGroup);
            }


            // exclude revenue itself
            var otherGroups = result.Where(x => x.Id != "40.01").ToList();

            // 🔥 PERIOD-WISE revenue aggregation
            revenueGroup.RevenueTotals = MergeTotals(
                otherGroups.Select(g => g.RevenueTotals)
            );

            // optional: same for cost/forecast
            revenueGroup.MonthlyTotals = MergeTotals(
                otherGroups.Select(g => g.MonthlyTotals)
            );


            foreach (var temp in result
            .Where(x => x.Id == "40.01")
            .FirstOrDefault()
            .RevenueTotals)
            {
                // temp.Key => "2026-01"

                var parts = temp.Key.Split('-');

                int year = int.Parse(parts[0]);
                int month = int.Parse(parts[1]);

                summaryLookup.TryGetValue((month, year, 1), out var revenue);

                //var adj = actualMonthlyadjSummary.FirstOrDefault(p => p.Year == year && p.Month == month).Cost;

                //revenue += adj;

                if (new DateOnly(year, month, 1) < DateOnly.FromDateTime(DateTime.Parse(_context.PlConfigValues.FirstOrDefault(r => r.Name.Equals("closing_period")).Value)))
                {
                    decimal finalRevenue = revenue;

                    // update dictionary
                    result
                        .Where(x => x.Id == "40.01")
                        .First()
                        .RevenueTotals[temp.Key] = finalRevenue;
                }
            }

            revenueGroup = result.FirstOrDefault(x => x.Id == "40.02");

            if (revenueGroup == null)
            {
                revenueGroup = new FinancialNode
                {
                    Id = "40.02",
                    Name = "Revenue Adjustment",
                    Type = "group"
                };

                result.Add(revenueGroup);
            }


            // 🔥 PERIOD-WISE revenue aggregation
            revenueGroup.RevenueTotals = MergeTotals(
                otherGroups.Select(g => g.RevenueTotals)
            );

            foreach (var temp in result
            .Where(x => x.Id == "40.02")
            .FirstOrDefault()
            .RevenueTotals)
            {
                // temp.Key => "2026-01"

                var parts = temp.Key.Split('-');

                int year = int.Parse(parts[0]);
                int month = int.Parse(parts[1]);

                //summaryLookup.TryGetValue((month, year, 1), out var revenue);

                //var adj = actualMonthlyadjSummary.FirstOrDefault(p => p.Fy_Cd == year && p.Period == month).AdjAmt;
                var item = actualMonthlyadjSummary.FirstOrDefault(p => p.Fy_Cd == year && p.Period == month);

                decimal adj = item?.AdjAmt ?? 0;

                result
                        .Where(x => x.Id == "40.02")
                        .First()
                        .RevenueTotals[temp.Key] = adj;
                //actualMonthlyadjSummary.Where(p => p.Fy_Cd == year && p.Period == month)
                //    .ToList()
                //    .ForEach(p =>
                //    {
                //        summaryLookup.TryGetValue((month, year, 1), out var revenue);
                //        decimal adj = p.AdjAmt;
                //        revenue += adj;
                //        //if (new DateOnly(year, month, 1) < DateOnly.FromDateTime(DateTime.Parse(_context.PlConfigValues.FirstOrDefault(r => r.Name.Equals("closing_period")).Value)))
                //        {
                //            decimal finalRevenue = revenue;
                //            // update dictionary
                //            result
                //                .Where(x => x.Id == "40.02")
                //                .First()
                //                .RevenueTotals[temp.Key] = finalRevenue;
                //        }
                //    });
                //revenue += adj;

                //if (new DateOnly(year, month, 1) < DateOnly.FromDateTime(DateTime.Parse(_context.PlConfigValues.FirstOrDefault(r => r.Name.Equals("closing_period")).Value)))
                //{
                //    decimal finalRevenue = adj;

                //    // update dictionary
                //    result
                //        .Where(x => x.Id == "40.01")
                //        .First()
                //        .RevenueTotals[temp.Key] = finalRevenue;
                //}
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all forecasts");
            return StatusCode(500, "An error occurred while fetching forecasts.");
        }
    }


    [HttpGet("GetSummaryV1WorkingV5")]
    public async Task<IActionResult> GetSummaryV1WorkingV5()
    {
        _logger.LogInformation("GetAllForecasts called at {Time}", DateTime.UtcNow);
        try
        {
            var accountLookup = await _context.Accounts
                .Where(a => a.AcctId != null)
                .ToDictionaryAsync(
                    a => a.AcctId,
                    a => a.AcctName
                );

            var actualMonthlySummary = await _context.PSRFinalData
                    .Where(p => p.SubTotTypeNo == 1)
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

            var actualMonthlySummary1 = await _context.PSRFinalData
                    .Where(p => p.SubTotTypeNo == 1)
                    .GroupBy(p => new { p.PdNo, p.FyCd, p.SubTotTypeNo, p.ProjId })
                    .Select(g => new MonthlySummary
                    {
                        Proj_Id = g.Key.ProjId,
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

            var plids = await _context.PlProjectPlans
                .Where(p => p.FinalVersion == true && (p.PlType == "EAC" || p.PlType == "NBBUD"))
                .Select(p => p.PlId)
                .ToListAsync();


            var actualMonthlyadjSummary1 = _context.ProjRevWrkPds.Where(p => plids.Contains(p.Pl_Id)).ToList();

            foreach (var item in actualMonthlyadjSummary1)
            {
                item.Fy_Cd = item.EndDate.GetValueOrDefault().Year;
            }
            //var actualMonthlyadjSummary = _context.ProjRevWrkPds
            var actualMonthlyadjSummary = actualMonthlyadjSummary1
                .Where(p => plids.Contains(p.Pl_Id))
                .AsEnumerable()
                .GroupBy(p => new { p.Fy_Cd, p.Period })
                .Select(g => new
                {
                    g.Key.Fy_Cd,
                    g.Key.Period,
                    AdjAmt = g.Sum(x => x.RevAdj ?? 0)
                })
                .ToList();

            var forecasts = await _context.PlForecasts.Where(p => plids.Contains(p.PlId)).ToListAsync();
            var groups = new Dictionary<string, FinancialNode>();

            foreach (var f in forecasts)
            {
                if (string.IsNullOrEmpty(f.AcctId)) continue;

                var groupKey = GetGroup(f.AcctId);
                var acctKey = f.AcctId;
                var projKey = f.ProjId ?? "NO-PROJ";
                var empKey = f.EmplId ?? "NO-EMP";

                var monthKey = GetMonthKey(f.Year, f.Month);

                var amount = f.Cost;
                var revenue = f.Revenue; // ✅ NEW

                // ---------------- GROUP ----------------
                if (!groups.TryGetValue(groupKey, out var groupNode))
                {
                    groupNode = new FinancialNode
                    {
                        Id = groupKey,
                        Name = GetGroupName(groupKey),
                        Type = "group"
                    };
                    groups[groupKey] = groupNode;
                }

                AddAmount(groupNode.MonthlyTotals, monthKey, amount);
                AddAmount(groupNode.RevenueTotals, monthKey, revenue); // ✅

                // ---------------- ACCOUNT ----------------
                if (!groupNode.Lookup.TryGetValue(acctKey, out var acctNode))
                {
                    acctNode = new FinancialNode
                    {
                        Id = acctKey,
                        Name = accountLookup.ContainsKey(acctKey)
                                ? $"{acctKey} - {accountLookup[acctKey]}"
                                : acctKey,
                        // Name = acctKey,
                        Type = "account"
                    };
                    groupNode.Lookup[acctKey] = acctNode;
                    groupNode.Children.Add(acctNode);
                }

                AddAmount(acctNode.MonthlyTotals, monthKey, amount);
                AddAmount(acctNode.RevenueTotals, monthKey, revenue); // ✅

                // ---------------- PROJECT ----------------
                if (!acctNode.Lookup.TryGetValue(projKey, out var projNode))
                {
                    projNode = new FinancialNode
                    {
                        Id = projKey,
                        Name = projKey,
                        Type = "project"
                    };
                    acctNode.Lookup[projKey] = projNode;
                    acctNode.Children.Add(projNode);
                }

                AddAmount(projNode.MonthlyTotals, monthKey, amount);
                AddAmount(projNode.RevenueTotals, monthKey, revenue); // ✅

                // ---------------- EMPLOYEE ----------------
                if (!projNode.Lookup.TryGetValue(empKey, out var empNode))
                {
                    empNode = new FinancialNode
                    {
                        Id = empKey,
                        Name = empKey,
                        Type = "employee"
                    };
                    projNode.Lookup[empKey] = empNode;
                    projNode.Children.Add(empNode);
                }

                AddAmount(empNode.MonthlyTotals, monthKey, amount);
                AddAmount(empNode.RevenueTotals, monthKey, revenue); // ✅
            }

            var result = groups.Values.ToList();

            var revenueGroup = result.FirstOrDefault(x => x.Id == "40.01");

            if (revenueGroup == null)
            {
                revenueGroup = new FinancialNode
                {
                    Id = "40.01",
                    Name = "Revenue",
                    Type = "group"
                };

                result.Add(revenueGroup);
            }


            // exclude revenue itself
            var otherGroups = result.Where(x => x.Id != "40.01").ToList();

            // 🔥 PERIOD-WISE revenue aggregation
            revenueGroup.RevenueTotals = MergeTotals(
                otherGroups.Select(g => g.RevenueTotals)
            );

            // optional: same for cost/forecast
            revenueGroup.MonthlyTotals = MergeTotals(
                otherGroups.Select(g => g.MonthlyTotals)
            );

            ///////////////////////////////////////////////////////////////////////////////////
            ///

            //----------------------------------------------------------
            // ADD PROJECT WISE REVENUE
            //----------------------------------------------------------

            var closingPeriod =
                DateOnly.FromDateTime(
                    DateTime.Parse(
                        _context.PlConfigValues
                            .First(r => r.Name == "closing_period")
                            .Value
                    ));

            var allProjectIds = forecasts
                .Select(x => x.ProjId)
                .Union(actualMonthlySummary1.Select(x => x.Proj_Id))
                .Distinct()
                .ToList();

            foreach (var projId in allProjectIds)
            {
                var safeProjId = projId ?? "NO-PROJ";

                //------------------------------------------------------
                // CREATE PROJECT NODE
                //------------------------------------------------------

                var projNode = new FinancialNode
                {
                    Id = safeProjId,
                    Name = safeProjId,
                    Type = "project"
                };

                //------------------------------------------------------
                // FORECAST REVENUE
                //------------------------------------------------------

                var forecastProjectData = forecasts
                    .Where(x => x.ProjId == projId)
                    .GroupBy(x => new
                    {
                        x.Year,
                        x.Month
                    })
                    .Select(g => new
                    {
                        g.Key.Year,
                        g.Key.Month,
                        Revenue = g.Sum(x => x.Revenue)
                    })
                    .ToList();

                foreach (var item in forecastProjectData)
                {
                    var monthKey =
                        GetMonthKey(
                            item.Year,
                            item.Month);

                    AddAmount(
                        projNode.RevenueTotals,
                        monthKey,
                        item.Revenue);
                }

                //------------------------------------------------------
                // ACTUAL REVENUE
                // OVERRIDE CLOSED PERIODS
                //------------------------------------------------------

                var actualProjectData = actualMonthlySummary1
                    .Where(x => x.Proj_Id == projId)
                    .ToList();

                foreach (var item in actualProjectData)
                {
                    var monthKey =
                        GetMonthKey(
                            item.Year,
                            item.Month);

                    var currentPeriod =
                        new DateOnly(
                            item.Year,
                            item.Month,
                            1);

                    //--------------------------------------------------
                    // REPLACE FORECAST WITH ACTUAL
                    //--------------------------------------------------

                    if (currentPeriod < closingPeriod)
                    {
                        projNode.RevenueTotals[monthKey] =
                            item.Cost;
                    }
                }

                //------------------------------------------------------
                // ADD PROJECT TO REVENUE GROUP
                //------------------------------------------------------

                revenueGroup.Children.Add(projNode);

                revenueGroup.Lookup[safeProjId] =
                    projNode;
            }



            //----------------------------------------------------------
            // ADD PROJECT WISE ACTUAL REVENUE
            //----------------------------------------------------------

            //var revenueProjects = actualMonthlySummary1
            //    .GroupBy(x => x.Proj_Id)
            //    .ToList();

            //foreach (var projGroup in revenueProjects)
            //{
            //    var projId = projGroup.Key ?? "NO-PROJ";

            //    //------------------------------------------------------
            //    // CREATE PROJECT NODE
            //    //------------------------------------------------------

            //    var projNode = new FinancialNode
            //    {
            //        Id = projId,
            //        Name = projId,
            //        Type = "project"
            //    };

            //    //------------------------------------------------------
            //    // ADD MONTHLY REVENUE
            //    //------------------------------------------------------

            //    foreach (var item in projGroup)
            //    {
            //        var monthKey =
            //            GetMonthKey(
            //                item.Year,
            //                item.Month);

            //        if (new DateOnly(item.Year, item.Month, 1) < DateOnly.FromDateTime(DateTime.Parse(_context.PlConfigValues.FirstOrDefault(r => r.Name.Equals("closing_period")).Value)))
            //        {
            //            AddAmount(
            //                projNode.RevenueTotals,
            //                monthKey,
            //                item.Cost);
            //        }
            //        else
            //        {

            //            AddAmount(
            //                projNode.RevenueTotals,
            //                monthKey,
            //                item.Cost);
            //        }
            //    }

            //    //------------------------------------------------------
            //    // ADD PROJECT TO REVENUE GROUP
            //    //------------------------------------------------------

            //    revenueGroup.Children.Add(projNode);

            //    revenueGroup.Lookup[projId] = projNode;
            //}


            /////////////////////////////////////////////////////////////////////////////////////


            foreach (var temp in result
            .Where(x => x.Id == "40.01")
            .FirstOrDefault()
            .RevenueTotals)
            {
                // temp.Key => "2026-01"

                var parts = temp.Key.Split('-');

                int year = int.Parse(parts[0]);
                int month = int.Parse(parts[1]);

                summaryLookup.TryGetValue((month, year, 1), out var revenue);



                //var adj = actualMonthlyadjSummary.FirstOrDefault(p => p.Year == year && p.Month == month).Cost;

                //revenue += adj;

                if (new DateOnly(year, month, 1) < DateOnly.FromDateTime(DateTime.Parse(_context.PlConfigValues.FirstOrDefault(r => r.Name.Equals("closing_period")).Value)))
                {
                    decimal finalRevenue = revenue;

                    // update dictionary
                    result
                        .Where(x => x.Id == "40.01")
                        .First()
                        .RevenueTotals[temp.Key] = finalRevenue;
                }
            }

            //Removed Adjustment
            revenueGroup = result.FirstOrDefault(x => x.Id == "40.02");

            if (revenueGroup == null)
            {
                revenueGroup = new FinancialNode
                {
                    Id = "40.02",
                    Name = "Revenue Adjustment",
                    Type = "group"
                };

                result.Add(revenueGroup);
            }


            // 🔥 PERIOD-WISE revenue aggregation
            revenueGroup.RevenueTotals = MergeTotals(
                otherGroups.Select(g => g.RevenueTotals)
            );

            foreach (var temp in result
            .Where(x => x.Id == "40.02")
            .FirstOrDefault()
            .RevenueTotals)
            {
                // temp.Key => "2026-01"

                var parts = temp.Key.Split('-');

                int year = int.Parse(parts[0]);
                int month = int.Parse(parts[1]);

                //summaryLookup.TryGetValue((month, year, 1), out var revenue);

                //var adj = actualMonthlyadjSummary.FirstOrDefault(p => p.Fy_Cd == year && p.Period == month).AdjAmt;
                var item = actualMonthlyadjSummary.FirstOrDefault(p => p.Fy_Cd == year && p.Period == month);

                decimal adj = item?.AdjAmt ?? 0;

                result
                        .Where(x => x.Id == "40.02")
                        .First()
                        .RevenueTotals[temp.Key] = adj;

            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all forecasts");
            return StatusCode(500, "An error occurred while fetching forecasts.");
        }
    }

    public class CostData
    {
        public string AcctId { get; set; }
        public string ProjId { get; set; }
        public string EmplId { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Cost { get; set; }
        public decimal Revenue { get; set; }
        public bool IsActual { get; set; }
    }
    [HttpGet("GetSummaryV1")]
    public async Task<IActionResult> GetSummaryV1()
    {
        _logger.LogInformation("GetAllForecasts called at {Time}", DateTime.UtcNow);
        try
        {

            var closingPeriod =
                DateOnly.FromDateTime(
                    DateTime.Parse(
                        _context.PlConfigValues
                            .First(r => r.Name == "closing_period")
                            .Value
                    ));

            var acctgrpSetup = await _context.Charts_Of_Accounts
                .Where(a => a.AccountId != null && a.AccountType != null).ToListAsync();

            var laborAccounts = acctgrpSetup
                .Where(a => a.AccountType == "LABOR" && a.BudgetSheet.ToUpper() == "STAFF HOURS")
                .Select(a => a.AccountId)
                .ToHashSet();

            //var nonlaborAccounts = acctgrpSetup
            //    .Where(a => a.AccountFunctionDescription == "NON-LABOR")
            //    .Select(a => a.AccountId)
            //    .ToHashSet();

            var lab_hours = _context.LabHours.Where(p => p.PdNo <= closingPeriod.Month && Convert.ToInt32(p.FyCd) == closingPeriod.Year && laborAccounts.Contains(p.AcctId)).ToList();
            var Odc = _context.GlPostDetails.Where(p => p.PdNo <= closingPeriod.Month && Convert.ToInt32(p.FyCd) == closingPeriod.Year &&
                    p.S_JNL_CD != "LD" &&
                    p.S_JNL_CD != "TS").ToList();

            var laborActualLookup = lab_hours.Where(p => p.ActHrs != null )
                .GroupBy(x => new
                {
                    x.AcctId,
                    x.ProjId,
                    x.EmplId,
                    x.FyCd,
                    x.PdNo
                })
                .ToDictionary(
                    g => (
                        g.Key.AcctId,
                        g.Key.ProjId,
                        g.Key.EmplId,
                        Year: Convert.ToInt32(g.Key.FyCd),
                        Month: g.Key.PdNo
                    ),
                    g => g.Sum(x => x.ActAmt)
                );

            var odcActualLookup = Odc
                .GroupBy(x => new
                {
                    x.AcctId,
                    x.ProjId,
                    x.Id,
                    x.FyCd,
                    x.PdNo
                })
                .ToDictionary(
                    g => (
                        g.Key.AcctId,
                        g.Key.ProjId,
                        g.Key.Id,
                        Year: Convert.ToInt32(g.Key.FyCd),
                        Month: g.Key.PdNo
                    ),
                    g => g.Sum(x => x.Amt1)
                );
            var processedActuals =
    new HashSet<(string AcctId, string ProjId, string emplId, int Year, int Month)>();

            //            var processedActuals =
            //new HashSet<(string AcctId, int Year, int Month)>();

            var accountLookup = await _context.Accounts
                .Where(a => a.AcctId != null)
                .ToDictionaryAsync(
                    a => a.AcctId,
                    a => a.AcctName
                );

            var projids = _config
          .GetSection("Projects")
          .Get<string[]>();

            var actualMonthlySummary = await _context.PSRFinalData
                .Where(p =>
                    p.SubTotTypeNo == 1 && (p.RateType == "A" || p.RateType == "N"))
                //projids.Any(id => p.ProjId.StartsWith(id)))
                .GroupBy(p => new { p.PdNo, p.FyCd, p.SubTotTypeNo })
                .Select(g => new MonthlySummary
                {
                    Month = g.Key.PdNo,
                    Year = Convert.ToInt16(g.Key.FyCd),
                    Cost = g.Sum(x => x.PtdIncurAmt),
                    subTotalType = g.Key.SubTotTypeNo
                })
                .ToListAsync();

            //        var actualMonthlySummary = await _context.PSRFinalData
            //.Where(p =>
            //    p.SubTotTypeNo == 1 && (p.RateType == "T" || p.RateType == "N") &&
            //    projids.Any(id => p.ProjId.StartsWith(id)))
            //.GroupBy(p => new { p.PdNo, p.FyCd, p.SubTotTypeNo })
            //.Select(g => new MonthlySummary
            //{
            //    Month = g.Key.PdNo,
            //    Year = Convert.ToInt16(g.Key.FyCd),
            //    Cost = g.Sum(x => x.PtdIncurAmt),
            //    subTotalType = g.Key.SubTotTypeNo
            //})
            //.ToListAsync();

            var actualMonthlySummary1 = _context.PSRFinalData
                .Where(p => p.SubTotTypeNo == 1 && (p.RateType == "A" || p.RateType == "N"))
                //projids.Any(id => p.ProjId.StartsWith(id)))
                .AsEnumerable()
                .GroupBy(p => new
                {
                    p.PdNo,
                    p.FyCd,
                    p.SubTotTypeNo,
                    ProjIdLevel2 = string.Join(".",
                        p.ProjId.Split('.').Take(2))
                })
                .Select(g => new MonthlySummary
                {
                    Proj_Id = g.Key.ProjIdLevel2,
                    Month = g.Key.PdNo,
                    Year = Convert.ToInt16(g.Key.FyCd),
                    Cost = g.Sum(x => x.PtdIncurAmt),
                    subTotalType = g.Key.SubTotTypeNo
                })
                .ToList();

            var summaryLookup = actualMonthlySummary
                .ToDictionary(
                    x => (x.Month, x.Year, x.subTotalType),
                    x => x.Cost);

            var plids = await _context.PlProjectPlans
                .Where(p => p.FinalVersion == true && (p.PlType == "EAC" || p.PlType == "NBBUD"))
                .Select(p => p.PlId)
                .ToListAsync();


            var actualMonthlyadjSummary1 = _context.ProjRevWrkPds.Where(p => plids.Contains(p.Pl_Id) && p.EndDate.Value.Year == 2026).ToList();

            foreach (var item in actualMonthlyadjSummary1)
            {
                item.Fy_Cd = item.EndDate.GetValueOrDefault().Year;
            }
            //var actualMonthlyadjSummary = _context.ProjRevWrkPds
            var actualMonthlyadjSummary = actualMonthlyadjSummary1
                .Where(p => plids.Contains(p.Pl_Id))
                .AsEnumerable()
                .GroupBy(p => new { p.Fy_Cd, p.Period })
                .Select(g => new
                {
                    g.Key.Fy_Cd,
                    g.Key.Period,
                    AdjAmt = g.Sum(x => x.RevAdj ?? 0)
                })
                .ToList();

            var forecasts = await _context.PlForecasts.Where(p => plids.Contains(p.PlId) && p.Year == 2026).ToListAsync();
            var groups = new Dictionary<string, FinancialNode>();

            var actualData = lab_hours
                .Select(x => new CostData
                {
                    AcctId = x.AcctId,
                    ProjId = x.ProjId,
                    EmplId = x.EmplId,
                    Year = Convert.ToInt32(x.FyCd),
                    Month = x.PdNo,
                    Cost = x.ActAmt ?? 0,
                    IsActual = true
                })
                .ToList();

            actualData.AddRange(
                Odc.Select(x => new CostData
                {
                    AcctId = x.AcctId,
                    ProjId = x.ProjId,
                    EmplId = x.Id,
                    Year = Convert.ToInt32(x.FyCd),
                    Month = x.PdNo.Value,
                    Cost = x.Amt1 ?? 0,
                    IsActual = true
                }));

            var forecastData = forecasts
                .Where(f =>
                    new DateOnly(f.Year, f.Month, 1) > closingPeriod)
                .Select(f => new CostData
                {
                    AcctId = f.AcctId,
                    ProjId = f.ProjId,
                    EmplId = f.EmplId,
                    Year = f.Year,
                    Month = f.Month,
                    Cost = f.Cost,
                    Revenue = f.Revenue,
                    IsActual = false
                })
                .ToList();

            var costData = actualData
                .Concat(forecastData)
                .ToList();

            foreach (var f in costData)
            {

                try
                {
                    if (string.IsNullOrEmpty(f.AcctId)) continue;

                    if (f.Month == 1 && f.AcctId == "646-001-135")
                    {

                    }

                    var groupKey = GetGroup(f.AcctId);
                    var acctKey = f.AcctId;
                    var projKey = f.ProjId ?? "NO-PROJ";
                    var empKey = f.EmplId ?? "NO-EMP";

                    var monthKey = GetMonthKey(f.Year, f.Month);

                    //var amount = f.Cost;
                    //var revenue = f.Revenue; // ✅ NEW

                    decimal amount;
                    var revenue = f.Revenue;

                    var periodDate =
                        new DateOnly(
                            f.Year,
                            f.Month,
                            1);

                    if (periodDate <= closingPeriod)
                    {
                        var key =
                        (
                            f.AcctId,
                            f.ProjId,
                            f.EmplId,
                            f.Year,
                            f.Month
                        );
                        if(f.AcctId == "646-001-135" && f.Month == 1)
                        {

                        }
                        if (processedActuals.Add(key))
                        {
                            laborActualLookup.TryGetValue(
                                key,
                                out var laborActual);

                            odcActualLookup.TryGetValue(
                                key,
                                out var odcActual);
                            if (laborActual.GetValueOrDefault() == odcActual.GetValueOrDefault())
                                amount = laborActual.GetValueOrDefault();
                            else
                                amount = laborActual.GetValueOrDefault() + odcActual.GetValueOrDefault();
                        }
                        else
                        {
                            amount = 0;
                        }
                    }
                    else
                    {
                        amount = f.Cost;
                    }

                    // ---------------- GROUP ----------------
                    if (!groups.TryGetValue(groupKey, out var groupNode))
                    {
                        groupNode = new FinancialNode
                        {
                            Id = groupKey,
                            Name = GetGroupName(groupKey),
                            Type = "group"
                        };
                        groups[groupKey] = groupNode;
                    }

                    AddAmount(groupNode.MonthlyTotals, monthKey, amount);
                    AddAmount(groupNode.RevenueTotals, monthKey, revenue); // ✅

                    // ---------------- ACCOUNT ----------------
                    if (!groupNode.Lookup.TryGetValue(acctKey, out var acctNode))
                    {
                        acctNode = new FinancialNode
                        {
                            Id = acctKey,
                            Name = accountLookup.ContainsKey(acctKey)
                                    ? $"{acctKey} - {accountLookup[acctKey]}"
                                    : acctKey,
                            // Name = acctKey,
                            Type = "account"
                        };
                        groupNode.Lookup[acctKey] = acctNode;
                        groupNode.Children.Add(acctNode);
                    }

                    AddAmount(acctNode.MonthlyTotals, monthKey, amount);
                    AddAmount(acctNode.RevenueTotals, monthKey, revenue); // ✅

                    // ---------------- PROJECT ----------------
                    if (!acctNode.Lookup.TryGetValue(projKey, out var projNode))
                    {
                        projNode = new FinancialNode
                        {
                            Id = projKey,
                            Name = projKey,
                            Type = "project"
                        };
                        acctNode.Lookup[projKey] = projNode;
                        acctNode.Children.Add(projNode);
                    }

                    AddAmount(projNode.MonthlyTotals, monthKey, amount);
                    AddAmount(projNode.RevenueTotals, monthKey, revenue); // ✅

                    // ---------------- EMPLOYEE ----------------
                    if (!projNode.Lookup.TryGetValue(empKey, out var empNode))
                    {
                        empNode = new FinancialNode
                        {
                            Id = empKey,
                            Name = empKey,
                            Type = "employee"
                        };
                        projNode.Lookup[empKey] = empNode;
                        projNode.Children.Add(empNode);
                    }

                    AddAmount(empNode.MonthlyTotals, monthKey, amount);
                    AddAmount(empNode.RevenueTotals, monthKey, revenue); // ✅

                }
                catch (Exception ex)
                {

                }
            }

            var result = groups.Values.ToList();

            var revenueGroup = result.FirstOrDefault(x => x.Name == "Revenue");

            if (revenueGroup == null)
            {
                revenueGroup = new FinancialNode
                {
                    Id = "40.01",
                    Name = "Revenue",
                    Type = "group"
                };

                result.Add(revenueGroup);
            }


            // exclude revenue itself
            var otherGroups = result.Where(x => x.Name != "Revenue").ToList();

            // 🔥 PERIOD-WISE revenue aggregation
            revenueGroup.RevenueTotals = MergeTotals(
                otherGroups.Select(g => g.RevenueTotals)
            );

            // optional: same for cost/forecast
            revenueGroup.MonthlyTotals = MergeTotals(
                otherGroups.Select(g => g.MonthlyTotals)
            );

            ///////////////////////////////////////////////////////////////////////////////////
            ///

            //----------------------------------------------------------
            // ADD PROJECT WISE REVENUE
            //----------------------------------------------------------



            var allProjectIds = forecasts
                .Select(x => x.ProjId)
                .Union(actualMonthlySummary1.Select(x => x.Proj_Id))
                .Distinct()
                .ToList();

            foreach (var projId in allProjectIds)
            {
                var safeProjId = projId ?? "NO-PROJ";

                //------------------------------------------------------
                // CREATE PROJECT NODE
                //------------------------------------------------------

                var projNode = new FinancialNode
                {
                    Id = safeProjId,
                    Name = safeProjId,
                    Type = "project"
                };

                //------------------------------------------------------
                // FORECAST REVENUE
                //------------------------------------------------------

                var forecastProjectData = forecasts
                    .Where(x => x.ProjId == projId)
                    .GroupBy(x => new
                    {
                        x.Year,
                        x.Month
                    })
                    .Select(g => new
                    {
                        g.Key.Year,
                        g.Key.Month,
                        Revenue = g.Sum(x => x.Revenue)
                    })
                    .ToList();

                foreach (var item in forecastProjectData)
                {
                    var monthKey =
                        GetMonthKey(
                            item.Year,
                            item.Month);

                    AddAmount(
                        projNode.RevenueTotals,
                        monthKey,
                        item.Revenue);
                }

                //------------------------------------------------------
                // ACTUAL REVENUE
                // OVERRIDE CLOSED PERIODS
                //------------------------------------------------------

                var actualProjectData = actualMonthlySummary1
                    .Where(x => x.Proj_Id == projId)
                    .ToList();

                foreach (var item in actualProjectData)
                {
                    var monthKey =
                        GetMonthKey(
                            item.Year,
                            item.Month);

                    var currentPeriod =
                        new DateOnly(
                            item.Year,
                            item.Month,
                            1);

                    //--------------------------------------------------
                    // REPLACE FORECAST WITH ACTUAL
                    //--------------------------------------------------

                    if (currentPeriod < closingPeriod)
                    {
                        projNode.RevenueTotals[monthKey] =
                            item.Cost;
                    }
                }

                //------------------------------------------------------
                // ADD PROJECT TO REVENUE GROUP
                //------------------------------------------------------

                revenueGroup.Children.Add(projNode);

                revenueGroup.Lookup[safeProjId] =
                    projNode;
            }
            revenueGroup.Children = revenueGroup.Children
                .Where(p => (p.RevenueTotals?.Values.Sum() ?? 0) != 0)
                .ToList();

            revenueGroup.Lookup = revenueGroup.Children
                .ToDictionary(x => x.Id, x => x);
            /////////////////////////////////////////////////////////////////////////////////////


            foreach (var temp in result
            .Where(x => x.Name == "Revenue")
            .FirstOrDefault()
            .RevenueTotals)
            {
                // temp.Key => "2026-01"

                var parts = temp.Key.Split('-');

                int year = int.Parse(parts[0]);
                int month = int.Parse(parts[1]);

                summaryLookup.TryGetValue((month, year, 1), out var revenue);



                //var adj = actualMonthlyadjSummary.FirstOrDefault(p => p.Year == year && p.Month == month).Cost;

                //revenue += adj;

                if (new DateOnly(year, month, 1) < DateOnly.FromDateTime(DateTime.Parse(_context.PlConfigValues.FirstOrDefault(r => r.Name.Equals("closing_period")).Value)))
                {
                    decimal finalRevenue = revenue;

                    // update dictionary
                    result
                        .Where(x => x.Name == "Revenue")
                        .First()
                        .RevenueTotals[temp.Key] = finalRevenue;
                }
            }

            //Removed Adjustment
            revenueGroup = result.FirstOrDefault(x => x.Id == "40.02");

            if (revenueGroup == null)
            {
                revenueGroup = new FinancialNode
                {
                    Id = "40.02",
                    Name = "Revenue Adjustment",
                    Type = "group"
                };

                result.Add(revenueGroup);
            }


            // 🔥 PERIOD-WISE revenue aggregation
            revenueGroup.RevenueTotals = MergeTotals(
                otherGroups.Select(g => g.RevenueTotals)
            );

            foreach (var temp in result
            .Where(x => x.Id == "40.02")
            .FirstOrDefault()
            .RevenueTotals)
            {
                // temp.Key => "2026-01"

                var parts = temp.Key.Split('-');

                int year = int.Parse(parts[0]);
                int month = int.Parse(parts[1]);

                //summaryLookup.TryGetValue((month, year, 1), out var revenue);

                //var adj = actualMonthlyadjSummary.FirstOrDefault(p => p.Fy_Cd == year && p.Period == month).AdjAmt;
                var item = actualMonthlyadjSummary.FirstOrDefault(p => p.Fy_Cd == year && p.Period == month);

                decimal adj = item?.AdjAmt ?? 0;

                result
                        .Where(x => x.Id == "40.02")
                        .First()
                        .RevenueTotals[temp.Key] = adj;

            }
            foreach (var group in result)
            {
                foreach (var account in group.Children)
                {
                    account.Children = account.Children
                        .Where(project =>
                            (project.MonthlyTotals?.Values.Sum() ?? 0) != 0 ||
                            (project.RevenueTotals?.Values.Sum() ?? 0) != 0)
                        .ToList();
                }
            }


            //result = result.Where(p => p.MonthlyTotals.Sum(x => x.Value) > 0 && p.RevenueTotals.Sum(x => x.Value) > 0).ToList();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all forecasts");
            return StatusCode(500, "An error occurred while fetching forecasts.");
        }
    }


    [HttpGet("GetSummaryByUser")]
    public async Task<IActionResult> GetSummaryByUser(int UserId)
    {
        _logger.LogInformation("GetSummaryV1 called at {Time}", DateTime.UtcNow);

        try
        {

            //_context.LabHours.Where(x => x.UserId == UserId)
            //    .Select(x => x.ProjId)
            //    .Distinct()
            //    .ToList();

            //----------------------------------------------------------
            // USER MAPPED PROJECTS
            //----------------------------------------------------------

            //var mappedProjectsQuery =
            //    _context.UserProjectMappings
            //        .Where(x => x.UserName == userName)
            //        .Select(x => x.ProjId);

            var mappedProjectsQuery = _context.UserProjectMaps
                    .Where(u => u.UserId == UserId)
                    .Select(u => u.ProjId);

            //----------------------------------------------------------
            // ACCOUNT LOOKUP
            //----------------------------------------------------------

            var accountLookup = await _context.Accounts
                .Where(a => a.AcctId != null)
                .ToDictionaryAsync(
                    a => a.AcctId,
                    a => a.AcctName
                );

            //----------------------------------------------------------
            // PLAN IDS
            //----------------------------------------------------------

            var plids = await _context.PlProjectPlans
                .Where(p =>
                    p.FinalVersion == true &&
                    (p.PlType == "EAC" || p.PlType == "NBBUD"))
                .Select(p => p.PlId)
                .ToListAsync();

            //----------------------------------------------------------
            // ACTUAL MONTHLY SUMMARY
            //----------------------------------------------------------

            var actualMonthlySummary = await _context.PSRFinalData
                .Where(p =>
                    p.SubTotTypeNo == 1 && p.RateType == "A" &&
                    mappedProjectsQuery.Contains(p.ProjId))
                .GroupBy(p => new
                {
                    p.PdNo,
                    p.FyCd,
                    p.SubTotTypeNo
                })
                .Select(g => new MonthlySummary
                {
                    Month = g.Key.PdNo,
                    Year = Convert.ToInt16(g.Key.FyCd),
                    Cost = g.Sum(x => x.PtdIncurAmt),
                    subTotalType = g.Key.SubTotTypeNo
                })
                .ToListAsync();

            //----------------------------------------------------------
            // PROJECT WISE ACTUAL SUMMARY
            //----------------------------------------------------------

            var actualMonthlySummary1 = await _context.PSRFinalData
                .Where(p =>
                    p.SubTotTypeNo == 1 && p.RateType == "A" &&
                    mappedProjectsQuery.Contains(p.ProjId))
                .GroupBy(p => new
                {
                    p.PdNo,
                    p.FyCd,
                    p.SubTotTypeNo,
                    p.ProjId
                })
                .Select(g => new MonthlySummary
                {
                    Proj_Id = g.Key.ProjId,
                    Month = g.Key.PdNo,
                    Year = Convert.ToInt16(g.Key.FyCd),
                    Cost = g.Sum(x => x.PtdIncurAmt),
                    subTotalType = g.Key.SubTotTypeNo
                })
                .ToListAsync();

            //----------------------------------------------------------
            // SUMMARY LOOKUP
            //----------------------------------------------------------

            var summaryLookup = actualMonthlySummary
                .ToDictionary(
                    x => (x.Month, x.Year, x.subTotalType),
                    x => x.Cost);

            //----------------------------------------------------------
            // REVENUE ADJUSTMENTS
            //----------------------------------------------------------

            var actualMonthlyadjSummary1 = await _context.ProjRevWrkPds
                .Where(p =>
                    plids.Contains(p.Pl_Id) &&
                    mappedProjectsQuery.Contains(p.ProjId))
                .ToListAsync();

            foreach (var item in actualMonthlyadjSummary1)
            {
                item.Fy_Cd =
                    item.EndDate.GetValueOrDefault().Year;
            }

            var actualMonthlyadjSummary = actualMonthlyadjSummary1
                .GroupBy(p => new
                {
                    p.Fy_Cd,
                    p.Period
                })
                .Select(g => new
                {
                    g.Key.Fy_Cd,
                    g.Key.Period,
                    AdjAmt = g.Sum(x => x.RevAdj ?? 0)
                })
                .ToList();

            //----------------------------------------------------------
            // FORECASTS
            //----------------------------------------------------------

            var forecasts = await _context.PlForecasts
                .Where(p =>
                    plids.Contains(p.PlId) &&
                    mappedProjectsQuery.Contains(p.ProjId))
                .ToListAsync();

            //----------------------------------------------------------
            // FINANCIAL TREE
            //----------------------------------------------------------

            var groups = new Dictionary<string, FinancialNode>();

            foreach (var f in forecasts)
            {
                if (string.IsNullOrEmpty(f.AcctId))
                    continue;

                var groupKey = GetGroup(f.AcctId);
                var acctKey = f.AcctId;
                var projKey = f.ProjId ?? "NO-PROJ";
                var empKey = f.EmplId ?? "NO-EMP";

                var monthKey = GetMonthKey(f.Year, f.Month);

                var amount = f.Cost;
                var revenue = f.Revenue;

                //------------------------------------------------------
                // GROUP
                //------------------------------------------------------

                if (!groups.TryGetValue(groupKey, out var groupNode))
                {
                    groupNode = new FinancialNode
                    {
                        Id = groupKey,
                        Name = GetGroupName(groupKey),
                        Type = "group"
                    };

                    groups[groupKey] = groupNode;
                }

                AddAmount(groupNode.MonthlyTotals, monthKey, amount);
                AddAmount(groupNode.RevenueTotals, monthKey, revenue);

                //------------------------------------------------------
                // ACCOUNT
                //------------------------------------------------------

                if (!groupNode.Lookup.TryGetValue(acctKey, out var acctNode))
                {
                    acctNode = new FinancialNode
                    {
                        Id = acctKey,
                        Name = accountLookup.ContainsKey(acctKey)
                            ? $"{acctKey} - {accountLookup[acctKey]}"
                            : acctKey,
                        Type = "account"
                    };

                    groupNode.Lookup[acctKey] = acctNode;
                    groupNode.Children.Add(acctNode);
                }

                AddAmount(acctNode.MonthlyTotals, monthKey, amount);
                AddAmount(acctNode.RevenueTotals, monthKey, revenue);

                //------------------------------------------------------
                // PROJECT
                //------------------------------------------------------

                if (!acctNode.Lookup.TryGetValue(projKey, out var projNode))
                {
                    projNode = new FinancialNode
                    {
                        Id = projKey,
                        Name = projKey,
                        Type = "project"
                    };

                    acctNode.Lookup[projKey] = projNode;
                    acctNode.Children.Add(projNode);
                }

                AddAmount(projNode.MonthlyTotals, monthKey, amount);
                AddAmount(projNode.RevenueTotals, monthKey, revenue);

                //------------------------------------------------------
                // EMPLOYEE
                //------------------------------------------------------

                if (!projNode.Lookup.TryGetValue(empKey, out var empNode))
                {
                    empNode = new FinancialNode
                    {
                        Id = empKey,
                        Name = empKey,
                        Type = "employee"
                    };

                    projNode.Lookup[empKey] = empNode;
                    projNode.Children.Add(empNode);
                }

                AddAmount(empNode.MonthlyTotals, monthKey, amount);
                AddAmount(empNode.RevenueTotals, monthKey, revenue);
            }

            //----------------------------------------------------------
            // RESULT
            //----------------------------------------------------------

            var result = groups.Values.ToList();

            //----------------------------------------------------------
            // REVENUE GROUP
            //----------------------------------------------------------

            var revenueGroup = result.FirstOrDefault(x => x.Id == "40.01");

            if (revenueGroup == null)
            {
                revenueGroup = new FinancialNode
                {
                    Id = "40.01",
                    Name = "Revenue",
                    Type = "group"
                };

                result.Add(revenueGroup);
            }

            var otherGroups =
                result.Where(x => x.Id != "40.01").ToList();

            revenueGroup.RevenueTotals = MergeTotals(
                otherGroups.Select(g => g.RevenueTotals));

            revenueGroup.MonthlyTotals = MergeTotals(
                otherGroups.Select(g => g.MonthlyTotals));

            //----------------------------------------------------------
            // CLOSING PERIOD
            //----------------------------------------------------------

            var closingPeriod =
                DateOnly.FromDateTime(
                    DateTime.Parse(
                        await _context.PlConfigValues
                            .Where(r => r.Name == "closing_period")
                            .Select(r => r.Value)
                            .FirstAsync()));

            //----------------------------------------------------------
            // ALL PROJECTS
            //----------------------------------------------------------

            var allProjectIds = forecasts
                .Select(x => x.ProjId)
                .Union(actualMonthlySummary1.Select(x => x.Proj_Id))
                .Where(x => x != null)
                .Distinct()
                .ToList();

            //----------------------------------------------------------
            // PROJECT REVENUE
            //----------------------------------------------------------

            foreach (var projId in allProjectIds)
            {
                var safeProjId = projId ?? "NO-PROJ";

                var projNode = new FinancialNode
                {
                    Id = safeProjId,
                    Name = safeProjId,
                    Type = "project"
                };

                //------------------------------------------------------
                // FORECAST REVENUE
                //------------------------------------------------------

                var forecastProjectData = forecasts
                    .Where(x => x.ProjId == projId)
                    .GroupBy(x => new
                    {
                        x.Year,
                        x.Month
                    })
                    .Select(g => new
                    {
                        g.Key.Year,
                        g.Key.Month,
                        Revenue = g.Sum(x => x.Revenue)
                    })
                    .ToList();

                foreach (var item in forecastProjectData)
                {
                    var monthKey =
                        GetMonthKey(item.Year, item.Month);

                    AddAmount(
                        projNode.RevenueTotals,
                        monthKey,
                        item.Revenue);
                }

                //------------------------------------------------------
                // ACTUAL REVENUE
                //------------------------------------------------------

                var actualProjectData = actualMonthlySummary1
                    .Where(x => x.Proj_Id == projId)
                    .ToList();

                foreach (var item in actualProjectData)
                {
                    var monthKey =
                        GetMonthKey(item.Year, item.Month);

                    var currentPeriod =
                        new DateOnly(item.Year, item.Month, 1);

                    if (currentPeriod < closingPeriod)
                    {
                        projNode.RevenueTotals[monthKey] =
                            item.Cost;
                    }
                }

                revenueGroup.Children.Add(projNode);

                revenueGroup.Lookup[safeProjId] =
                    projNode;
            }

            //----------------------------------------------------------
            // OVERRIDE CLOSED PERIOD REVENUE
            //----------------------------------------------------------

            foreach (var temp in revenueGroup.RevenueTotals.ToList())
            {
                var parts = temp.Key.Split('-');

                int year = int.Parse(parts[0]);
                int month = int.Parse(parts[1]);

                summaryLookup.TryGetValue(
                    (month, year, 1),
                    out var revenue);

                if (new DateOnly(year, month, 1) < closingPeriod)
                {
                    revenueGroup.RevenueTotals[temp.Key] =
                        revenue;
                }
            }

            //----------------------------------------------------------
            // REVENUE ADJUSTMENT GROUP
            //----------------------------------------------------------

            var adjustmentGroup =
                result.FirstOrDefault(x => x.Id == "40.02");

            if (adjustmentGroup == null)
            {
                adjustmentGroup = new FinancialNode
                {
                    Id = "40.02",
                    Name = "Revenue Adjustment",
                    Type = "group"
                };

                result.Add(adjustmentGroup);
            }

            adjustmentGroup.RevenueTotals = MergeTotals(
                otherGroups.Select(g => g.RevenueTotals));

            foreach (var temp in adjustmentGroup.RevenueTotals.ToList())
            {
                var parts = temp.Key.Split('-');

                int year = int.Parse(parts[0]);
                int month = int.Parse(parts[1]);

                var item = actualMonthlyadjSummary
                    .FirstOrDefault(p =>
                        p.Fy_Cd == year &&
                        p.Period == month);

                decimal adj = item?.AdjAmt ?? 0;

                adjustmentGroup.RevenueTotals[temp.Key] = adj;
            }

            //----------------------------------------------------------
            // RETURN
            //----------------------------------------------------------

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get summary");

            return StatusCode(
                500,
                "An error occurred while fetching summary.");
        }
    }

    public static Dictionary<string, decimal> MergeTotals(
    IEnumerable<Dictionary<string, decimal>> totals)
    {
        return totals
            .Where(t => t != null)
            .SelectMany(t => t)
            .GroupBy(x => x.Key) // 🔥 period-wise grouping
            .ToDictionary(
                g => g.Key,
                g => g.Sum(x => x.Value)
            );
    }
    public static void AddAmount(Dictionary<string, decimal> dict, string key, decimal value)
    {
        if (dict.TryGetValue(key, out var existing))
            dict[key] = existing + value;
        else
            dict[key] = value;
    }
    //public static void AddAmount(FinancialNode node, string monthKey, decimal amount)
    //{
    //    if (node.MonthlyTotals.TryGetValue(monthKey, out var existing))
    //        node.MonthlyTotals[monthKey] = existing + amount;
    //    else
    //        node.MonthlyTotals[monthKey] = amount;
    //}
    //[HttpGet("GetSummary")]
    //public async Task<IActionResult> GetSummary()
    //{
    //    _logger.LogInformation("GetAllForecasts called at {Time}", DateTime.UtcNow);
    //    try
    //    {
    //        var forecasts = await _pl_ForecastService.GetAllAsync();

    //        var result = forecasts
    //                .Where(x => !string.IsNullOrEmpty(x.AcctId))
    //                .GroupBy(x => GetGroup(x.AcctId))
    //                .Select(group => new FinancialNode
    //                {
    //                    Id = group.Key,
    //                    Name = GetGroupName(group.Key),
    //                    Type = "group",

    //                    Children = group
    //                        .GroupBy(x => x.AcctId)
    //                        .Select(acct => new FinancialNode
    //                        {
    //                            Id = acct.Key,
    //                            Name = acct.Key,
    //                            Type = "account",

    //                            Children = acct
    //                                .Where(x => !string.IsNullOrEmpty(x.ProjId))
    //                                .GroupBy(x => x.ProjId)
    //                                .Select(proj => new FinancialNode
    //                                {
    //                                    Id = proj.Key,
    //                                    Name = proj.Key,
    //                                    Type = "project",

    //                                    Children = proj
    //                                        .Where(x => !string.IsNullOrEmpty(x.EmplId))
    //                                        .GroupBy(x => x.EmplId)
    //                                        .Select(emp => new FinancialNode
    //                                        {
    //                                            Id = emp.Key,
    //                                            Name = emp.Key,
    //                                            Type = "employee",

    //                                            // Employee-level totals
    //                                            MonthlyTotals = ToMonthlyTotals(emp),

    //                                            // Optional: detailed year/month structure
    //                                            Data = emp
    //                                                .GroupBy(x => x.Year)
    //                                                .ToDictionary(
    //                                                    y => y.Key,
    //                                                    y => y.GroupBy(m => GetMonthName(m.Month))
    //                                                          .ToDictionary(
    //                                                              m => m.Key,
    //                                                              m => m.Sum(v => v.Forecastedamt ?? 0)
    //                                                          )
    //                                                )
    //                                        })
    //                                        .ToList(),

    //                                    // Project rollup = sum of employees
    //                                    MonthlyTotals = MergeTotals(
    //                                        proj.Where(x => !string.IsNullOrEmpty(x.EmplId))
    //                                            .GroupBy(x => x.EmplId)
    //                                            .Select(emp => ToMonthlyTotals(emp))
    //                                    )
    //                                })
    //                                .ToList(),

    //                            // Account rollup = sum of projects
    //                            MonthlyTotals = MergeTotals(
    //                                acct.Where(x => !string.IsNullOrEmpty(x.ProjId))
    //                                    .GroupBy(x => x.ProjId)
    //                                    .Select(proj => ToMonthlyTotals(proj))
    //                            )
    //                        })
    //                        .ToList(),

    //                    // Group rollup = sum of accounts
    //                    MonthlyTotals = MergeTotals(
    //                        group.GroupBy(x => x.AcctId)
    //                             .Select(acct => ToMonthlyTotals(acct))
    //                    )
    //                })
    //                .ToList();

    //        return Ok(result);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Failed to get all forecasts");
    //        return StatusCode(500, "An error occurred while fetching forecasts.");
    //    }
    //}

    public static string GetMonthKey(int year, int month)
    {
        return $"{year}-{month:00}"; // faster than culture conversion
    }
    //public static string GetGroup(string acctId)
    //{
    //    acctId = acctId.Trim();
    //    if (acctId.StartsWith("40.01")) return "40.01";
    //    if (acctId.StartsWith("40.02")) return "40.02";
    //    if (acctId.StartsWith("50.01")) return "50.01";
    //    //if (acctId.StartsWith("50.2")) return "50.2";
    //    if (acctId.StartsWith("50.2") ||
    //   acctId.StartsWith("50.32.02"))
    //        return "50.2";
    //    if (acctId.StartsWith("60")) return "60";
    //    if (acctId.StartsWith("62")) return "62";
    //    if (acctId.StartsWith("70")) return "70";
    //    if (acctId.StartsWith("78")) return "78";
    //    if (acctId.StartsWith("80")) return "80";
    //    if (acctId.StartsWith("82")) return "82";
    //    if (acctId.StartsWith("84")) return "84";
    //    if (acctId.StartsWith("90")) return "90";
    //    if (acctId.StartsWith("95")) return "95";
    //    //if (acctId.StartsWith("50.32.02")) return "50.32.02";

    //    return acctId.Length >= 5 ? acctId.Substring(0, 5) : acctId;
    //}

    [NonAction]
    public string GetGroup(string acctId)
    {
        acctId = acctId.Trim();

        if (acctId.StartsWith("50.2") ||
            acctId.StartsWith("50.32.02"))
            return "50.2";

        var groups = _config
            .GetSection("AccountGroups")
            .Get<string[]>();

        foreach (var group in groups)
        {
            if (acctId.StartsWith(group))
                return group;
        }

        return acctId.Length >= 5
            ? acctId.Substring(0, 5)
            : acctId;
    }
    [NonAction]
    public string GetGroupName(string group)
    {

        var groups = _config
            .GetSection("AccountGroups")
            .Get<string[]>();
        var groupNames = _config
            .GetSection("AccountNames")
            .Get<string[]>();

        int index = Array.IndexOf(groups, group);

        if (index >= 0 && index < groupNames.Length)
        {
            return groupNames[index];
        }

        return group;

        //return group switch
        //{
        //    "40.01" => "Revenue",
        //    "40.02" => "Revenue Adjustments",
        //    "50.01" => "Direct Labor",
        //    "50.2" => "Direct Non Labor",
        //    "60" => "Fringe Expenses",
        //    "62" => "Facility Expenses",
        //    "70" => "Overhead Expenses",
        //    "78" => "Material Handling",
        //    "80" => "General and Administrative",
        //    "82" => "B&P: Costs",
        //    "84" => "IR&D",
        //    "90" => "Unallowable Expenses",
        //    "50.32.02" => "Unallowable Expenses",
        //    "95" => "Non Operating Expenses",

        //    _ => "Other"
        //};

        return group switch
        {
            "40.01" => "Revenue",
            "40.02" => "Revenue Adjustments",

            "50.01" => "Direct Labor",

            "50.2" or "50.32.02" => "Direct Non Labor",

            "60" => "Fringe Expenses",
            "62" => "Facility Expenses",
            "70" => "Overhead Expenses",
            "78" => "Material Handling",
            "80" => "General and Administrative",
            "82" => "B&P: Costs",
            "84" => "IR&D",
            "90" => "Unallowable Expenses",

            "95" => "Non Operating Expenses",

            _ => "Other"
        };
    }
    public static Dictionary<string, decimal> ToMonthlyTotals(IEnumerable<PlForecast> items)
    {
        return items
            .GroupBy(x => $"{x.Year}-{GetMonthName(x.Month)}")
            .ToDictionary(
                g => g.Key,
                g => g.Sum(x => x.Forecastedamt ?? 0)
            );
    }
    //public static Dictionary<string, decimal> MergeTotals(IEnumerable<Dictionary<string, decimal>> totals)
    //{
    //    return totals
    //        .Where(t => t != null)
    //        .SelectMany(t => t)
    //        .GroupBy(x => x.Key)
    //        .ToDictionary(g => g.Key, g => g.Sum(x => x.Value));
    //}

    public static string GetMonthName(int month)
    {
        return CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(month);
    }

    [HttpGet("GetProjectFinancialSummary/{projId}")]
    public async Task<IActionResult> GetProjectFinancialSummary(string projId)
    {
        try
        {
            // -------------------------------------------------
            // Closing Period
            // Example Value : 31-12-2025
            // -------------------------------------------------
            var closingPeriodValue = await _context.PlConfigValues
                .Where(x => x.Name.ToLower() == "closing_period")
                .Select(x => x.Value)
                .FirstOrDefaultAsync();

            var closingDate = DateOnly.FromDateTime(
                DateTime.Parse(closingPeriodValue));

            // -------------------------------------------------
            // Final Budget Version
            // -------------------------------------------------
            var budgetPlId = await _context.PlProjectPlans
                .Where(p =>
                    p.ProjId == projId &&
                    p.PlType == "BUD" &&
                    p.FinalVersion == true)
                .Select(p => p.PlId)
                .FirstOrDefaultAsync();

            // -------------------------------------------------
            // Final EAC Version
            // -------------------------------------------------
            var eacPlId = await _context.PlProjectPlans
                .Where(p =>
                    p.ProjId == projId &&
                    p.PlType == "EAC" &&
                    p.FinalVersion == true)
                .Select(p => p.PlId)
                .FirstOrDefaultAsync();

            // -------------------------------------------------
            // Budget
            // -------------------------------------------------
            decimal budgetCost = 0;
            decimal budgetRevenue = 0;

            if (budgetPlId != 0)
            {
                budgetCost = await _context.PlForecasts
                    .Where(x => x.PlId == budgetPlId)
                    .SumAsync(x => (decimal?)x.Cost) ?? 0;

                budgetRevenue = await _context.PlForecasts
                    .Where(x => x.PlId == budgetPlId)
                    .SumAsync(x => (decimal?)x.Revenue) ?? 0;
            }

            // -------------------------------------------------
            // Forecast (Full EAC)
            // -------------------------------------------------
            decimal forecastCost = 0;
            decimal forecastRevenue = 0;

            if (eacPlId != 0)
            {
                forecastCost = await _context.PlForecasts
                    .Where(x => x.PlId == eacPlId)
                    .SumAsync(x => (decimal?)x.Cost) ?? 0;

                forecastRevenue = await _context.PlForecasts
                    .Where(x => x.PlId == eacPlId)
                    .SumAsync(x => (decimal?)x.Revenue) ?? 0;
            }

            // -------------------------------------------------
            // Actuals from EAC till Closed Period
            // -------------------------------------------------
            decimal actualCost = 0;
            decimal actualRevenue = 0;

            if (eacPlId != 0)
            {
                var actuals = await _context.PlForecasts
                    .Where(x =>
                        x.PlId == eacPlId &&
                        new DateOnly(x.Year, x.Month, 1) <= closingDate)
                    .GroupBy(x => x.ProjId)
                    .Select(g => new
                    {
                        Cost = g.Sum(x => x.Cost),
                        Revenue = g.Sum(x => x.Revenue)
                    })
                    .FirstOrDefaultAsync();

                actualCost = actuals?.Cost ?? 0;
                actualRevenue = actuals?.Revenue ?? 0;
            }

            // -------------------------------------------------
            // ETC = Remaining Forecast after Actuals
            // -------------------------------------------------
            decimal etcCost = forecastCost - actualCost;
            decimal etcRevenue = forecastRevenue - actualRevenue;

            // -------------------------------------------------
            // Response
            // -------------------------------------------------
            var result = new
            {
                ProjId = projId,

                Budget = new
                {
                    Cost = budgetCost,
                    Revenue = budgetRevenue
                },

                Forecast = new
                {
                    Cost = forecastCost,
                    Revenue = forecastRevenue
                },

                Actuals = new
                {
                    Cost = actualCost,
                    Revenue = actualRevenue
                },

                ETC = new
                {
                    Cost = etcCost,
                    Revenue = etcRevenue
                },

                ClosingPeriod = closingDate
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while generating project summary");
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("ProjectFinancialSummaryV1/{projId}")]
    public async Task<IActionResult> ProjectFinancialSummaryV1(string projId)
    {
        try
        {
            // -----------------------------------------
            // CLOSED PERIOD
            // -----------------------------------------
            var closingPeriodValue = await _context.PlConfigValues
                .Where(x => x.Name.ToLower() == "closing_period")
                .Select(x => x.Value)
                .FirstOrDefaultAsync();

            DateOnly closingPeriod =
                DateOnly.FromDateTime(DateTime.Parse(closingPeriodValue));

            int closedYear = closingPeriod.Year;
            int closedMonth = closingPeriod.Month;

            int currentYear = DateTime.Now.Year;
            int priorYear = currentYear - 1;

            // -----------------------------------------
            // FINAL BUDGET VERSION
            // -----------------------------------------
            var budgetPlId = await _context.PlProjectPlans
                .Where(x =>
                    x.ProjId == projId &&
                    x.FinalVersion == true &&
                    x.PlType == "BUD")
                .Select(x => x.PlId)
                .FirstOrDefaultAsync();

            // -----------------------------------------
            // FINAL EAC VERSION
            // -----------------------------------------
            var eacPlId = await _context.PlProjectPlans
                .Where(x =>
                    x.ProjId == projId &&
                    x.FinalVersion == true &&
                    x.PlType == "EAC")
                .Select(x => x.PlId)
                .FirstOrDefaultAsync();

            // -----------------------------------------
            // BUDGET
            // -----------------------------------------
            decimal budget = 0;

            if (budgetPlId != null)
            {
                budget = await _context.PlForecasts
                    .Where(x => x.PlId == budgetPlId)
                    .SumAsync(x => (decimal?)x.Cost) ?? 0;
            }

            // -----------------------------------------
            // FORECAST (FULL EAC)
            // -----------------------------------------
            decimal forecast = 0;

            if (eacPlId != null)
            {
                forecast = await _context.PlForecasts
                    .Where(x => x.PlId == eacPlId)
                    .SumAsync(x => (decimal?)x.Cost) ?? 0;
            }

            // -----------------------------------------
            // ACTUALS
            // Actuals from EAC till closed period
            // -----------------------------------------
            decimal actuals = 0;

            if (eacPlId != null)
            {
                actuals = await _context.PlForecasts
                    .Where(x =>
                        x.PlId == eacPlId &&
                        (
                            x.Year < closedYear ||
                            (x.Year == closedYear && x.Month <= closedMonth)
                        ))
                    .SumAsync(x => (decimal?)x.Cost) ?? 0;
            }

            // -----------------------------------------
            // ETC
            // Remaining forecast after closed period
            // -----------------------------------------
            decimal etc = 0;

            if (eacPlId != null)
            {
                etc = await _context.PlForecasts
                    .Where(x =>
                        x.PlId == eacPlId &&
                        (
                            x.Year > closedYear ||
                            (x.Year == closedYear && x.Month > closedMonth)
                        ))
                    .SumAsync(x => (decimal?)x.Cost) ?? 0;
            }

            // -----------------------------------------
            // PRIOR YEAR ACTUALS
            // -----------------------------------------
            decimal priorYearActuals = await _context.PSRFinalData
                .Where(x =>
                    x.ProjId == projId && x.RateType == "A" &&
                    x.FyCd == priorYear.ToString())
                .SumAsync(x => (decimal?)x.PtdIncurAmt) ?? 0;

            // -----------------------------------------
            // VARIANCES
            // -----------------------------------------
            decimal varianceVsBudget = forecast - budget;

            decimal varianceVsPriorYear =
                forecast - priorYearActuals;

            // -----------------------------------------
            // PERCENTAGES
            // -----------------------------------------
            decimal percentVsBudget = 0;

            if (budget != 0)
            {
                percentVsBudget =
                    ((forecast - budget) / budget) * 100;
            }

            decimal percentVsPriorYear = 0;

            if (priorYearActuals != 0)
            {
                percentVsPriorYear =
                    ((forecast - priorYearActuals)
                        / priorYearActuals) * 100;
            }

            // -----------------------------------------
            // RESPONSE
            // -----------------------------------------
            var result = new ProjectFinancialSummary
            {
                ProjId = projId,

                Budget = budget,

                Forecast = forecast,

                Actuals = actuals,

                ETC = etc,

                PriorYearActuals = priorYearActuals,

                VarianceVsBudget = varianceVsBudget,

                VarianceVsPriorYear = varianceVsPriorYear,

                PercentVsBudget =
                    Math.Round(percentVsBudget, 2),

                PercentVsPriorYear =
                    Math.Round(percentVsPriorYear, 2)
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error while calculating project summary");

            return StatusCode(500,
                "Error while calculating project summary");
        }
    }

    [HttpGet("FinancialCards/{projId}")]
    public async Task<IActionResult> FinancialCards(string projId)
    {
        try
        {
            // ------------------------------------------------
            // CLOSED PERIOD
            // ------------------------------------------------
            var closingPeriodValue = await _context.PlConfigValues
                .Where(x => x.Name.ToLower() == "closing_period")
                .Select(x => x.Value)
                .FirstOrDefaultAsync();

            DateOnly closingPeriod =
                DateOnly.FromDateTime(DateTime.Parse(closingPeriodValue));

            int closedYear = closingPeriod.Year;
            int closedMonth = closingPeriod.Month;

            int priorYear = closedYear - 1;

            // ------------------------------------------------
            // FINAL BUDGET PLAN
            // ------------------------------------------------
            var budgetPlId = await _context.PlProjectPlans
                .Where(x =>
                    x.ProjId == projId &&
                    x.FinalVersion == true &&
                    x.PlType == "BUD")
                .Select(x => x.PlId)
                .FirstOrDefaultAsync();

            // ------------------------------------------------
            // FINAL EAC PLAN
            // ------------------------------------------------
            var eacPlId = await _context.PlProjectPlans
                .Where(x =>
                    x.ProjId == projId &&
                    x.FinalVersion == true &&
                    x.PlType == "EAC")
                .Select(x => x.PlId)
                .FirstOrDefaultAsync();

            // ------------------------------------------------
            // BUDGET
            // ------------------------------------------------
            decimal budget = 0;

            if (budgetPlId != null)
            {
                budget = await _context.PlForecasts
                    .Where(x => x.PlId == budgetPlId)
                    .SumAsync(x => (decimal?)x.Cost) ?? 0;
            }

            // ------------------------------------------------
            // CY FORECAST
            // ------------------------------------------------
            decimal forecast = 0;

            if (eacPlId != null)
            {
                forecast = await _context.PlForecasts
                    .Where(x => x.PlId == eacPlId)
                    .SumAsync(x => (decimal?)x.Cost) ?? 0;
            }

            // ------------------------------------------------
            // YTD ACTUALS
            // ------------------------------------------------
            decimal ytdActuals = 0;

            if (eacPlId != null)
            {
                ytdActuals = await _context.PlForecasts
                    .Where(x =>
                        x.PlId == eacPlId &&
                        (
                            x.Year < closedYear ||
                            (x.Year == closedYear &&
                             x.Month <= closedMonth)
                        ))
                    .SumAsync(x => (decimal?)x.Cost) ?? 0;
            }

            // ------------------------------------------------
            // ETC
            // ------------------------------------------------
            decimal etc = 0;

            if (eacPlId != null)
            {
                etc = await _context.PlForecasts
                    .Where(x =>
                        x.PlId == eacPlId &&
                        (
                            x.Year > closedYear ||
                            (x.Year == closedYear &&
                             x.Month > closedMonth)
                        ))
                    .SumAsync(x => (decimal?)x.Cost) ?? 0;
            }

            // ------------------------------------------------
            // PRIOR YEAR ACTUALS
            // ------------------------------------------------
            decimal priorYearActuals = await _context.PSRFinalData
                .Where(x =>
                    x.ProjId == projId && x.RateType == "A" &&
                    x.FyCd == priorYear.ToString())
                .SumAsync(x => (decimal?)x.PtdIncurAmt) ?? 0;

            // ------------------------------------------------
            // HELPER METHODS
            // ------------------------------------------------
            decimal CalcPercent(decimal current, decimal compare)
            {
                if (compare == 0) return 0;

                return Math.Round(
                    ((current - compare) / compare) * 100,
                    2);
            }

            string Trend(decimal current, decimal compare)
            {
                return current >= compare ? "up" : "down";
            }

            decimal Diff(decimal current, decimal compare)
            {
                return Math.Round(Math.Abs(current - compare), 2);
            }

            // ------------------------------------------------
            // RESPONSE
            // ------------------------------------------------
            var response = new List<FinancialCardResponse>
        {
            // ------------------------------------------------
            // BUDGET
            // ------------------------------------------------
            new FinancialCardResponse
            {
                Title = "Budget",
                MainValue = Math.Round(budget, 2),

                LeftComparison = new ComparisonData
                {
                    Label = "Vs. Prior Year",

                    Value = Diff(budget, priorYearActuals),

                    Percentage =
                        CalcPercent(budget, priorYearActuals),

                    Trend =
                        Trend(budget, priorYearActuals)
                }
            },

            // ------------------------------------------------
            // CY FORECAST
            // ------------------------------------------------
            new FinancialCardResponse
            {
                Title = "CY Forecast",
                MainValue = Math.Round(forecast, 2),

                RightComparison = new ComparisonData
                {
                    Label = "Vs. Budget",

                    Value = Diff(forecast, budget),

                    Percentage =
                        CalcPercent(forecast, budget),

                    Trend =
                        Trend(forecast, budget)
                }
            },

            // ------------------------------------------------
            // YTD ACTUALS
            // ------------------------------------------------
            new FinancialCardResponse
            {
                Title = "YTD Actuals",
                MainValue = Math.Round(ytdActuals, 2),

                LeftComparison = new ComparisonData
                {
                    Label = "Vs. Prior Year",

                    Value = Diff(ytdActuals, priorYearActuals),

                    Percentage =
                        CalcPercent(
                            ytdActuals,
                            priorYearActuals),

                    Trend =
                        Trend(
                            ytdActuals,
                            priorYearActuals)
                },

                RightComparison = new ComparisonData
                {
                    Label = "Vs. Budget",

                    Value = Diff(ytdActuals, budget),

                    Percentage =
                        CalcPercent(
                            ytdActuals,
                            budget),

                    Trend =
                        Trend(
                            ytdActuals,
                            budget)
                }
            },

            // ------------------------------------------------
            // ETC
            // ------------------------------------------------
            new FinancialCardResponse
            {
                Title = "ETC",
                MainValue = Math.Round(etc, 2),

                LeftComparison = new ComparisonData
                {
                    Label = "Vs. Prior Year",

                    Value = Diff(etc, priorYearActuals),

                    Percentage =
                        CalcPercent(
                            etc,
                            priorYearActuals),

                    Trend =
                        Trend(
                            etc,
                            priorYearActuals)
                },

                RightComparison = new ComparisonData
                {
                    Label = "Vs. Budget",

                    Value = Diff(etc, budget),

                    Percentage =
                        CalcPercent(
                            etc,
                            budget),

                    Trend =
                        Trend(
                            etc,
                            budget)
                }
            }
        };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error while generating financial cards");

            return StatusCode(500,
                "Error while generating financial cards");
        }
    }

    [HttpGet("FinancialCards")]
    public async Task<IActionResult> FinancialCards()
    {
        try
        {
            // ------------------------------------------------
            // CLOSED PERIOD
            // ------------------------------------------------
            var closingPeriodValue = await _context.PlConfigValues
                .Where(x => x.Name.ToLower() == "closing_period")
                .Select(x => x.Value)
                .FirstOrDefaultAsync();

            DateOnly closingPeriod =
                DateOnly.FromDateTime(DateTime.Parse(closingPeriodValue));

            int closedYear = closingPeriod.Year;
            int closedMonth = closingPeriod.Month;

            decimal priorYear = closedYear - 1;

            // ------------------------------------------------
            // FINAL BUDGET PLAN IDS
            // ------------------------------------------------
            var budgetPlans = await _context.PlProjectPlans
                .Where(x =>
                    x.FinalVersion == true &&
                    x.PlType == "BUD")
                .Select(x => new
                {
                    x.ProjId,
                    x.PlId
                })
                .ToListAsync();

            // ------------------------------------------------
            // FINAL EAC PLAN IDS
            // ------------------------------------------------
            var eacPlans = await _context.PlProjectPlans
                .Where(x =>
                    x.FinalVersion == true &&
                    x.PlType == "EAC")
                .Select(x => new
                {
                    x.ProjId,
                    x.PlId
                })
                .ToListAsync();

            var budgetPlIds = budgetPlans
                .Select(x => x.PlId)
                .ToList();

            var eacPlIds = eacPlans
                .Select(x => x.PlId)
                .ToList();

            // ------------------------------------------------
            // BUDGET TOTALS
            // ------------------------------------------------
            var budgetTotals = await _context.PlForecasts
                .Where(x => budgetPlIds.Contains(x.PlId))
                .GroupBy(x => x.PlId)
                .Select(g => new
                {
                    PlId = g.Key,
                    Amount = g.Sum(x => x.Cost)
                })
                .ToDictionaryAsync(x => x.PlId, x => x.Amount);

            // ------------------------------------------------
            // FORECAST TOTALS
            // ------------------------------------------------
            var forecastTotals = await _context.PlForecasts
                .Where(x => eacPlIds.Contains(x.PlId))
                .GroupBy(x => x.PlId)
                .Select(g => new
                {
                    PlId = g.Key,
                    Amount = g.Sum(x => x.Cost)
                })
                .ToDictionaryAsync(x => x.PlId, x => x.Amount);

            // ------------------------------------------------
            // YTD ACTUALS
            // ------------------------------------------------
            var ytdTotals = await _context.PlForecasts
                .Where(x =>
                    eacPlIds.Contains(x.PlId) &&
                    (
                        x.Year < closedYear ||
                        (x.Year == closedYear &&
                         x.Month <= closedMonth)
                    ))
                .GroupBy(x => x.PlId)
                .Select(g => new
                {
                    PlId = g.Key,
                    Amount = g.Sum(x => x.Cost)
                })
                .ToDictionaryAsync(x => x.PlId, x => x.Amount);

            // ------------------------------------------------
            // ETC TOTALS
            // ------------------------------------------------
            var etcTotals = await _context.PlForecasts
                .Where(x =>
                    eacPlIds.Contains(x.PlId) &&
                    (
                        x.Year > closedYear ||
                        (x.Year == closedYear &&
                         x.Month > closedMonth)
                    ))
                .GroupBy(x => x.PlId)
                .Select(g => new
                {
                    PlId = g.Key,
                    Amount = g.Sum(x => x.Cost)
                })
                .ToDictionaryAsync(x => x.PlId, x => x.Amount);

            // ------------------------------------------------
            // PRIOR YEAR ACTUALS
            // ------------------------------------------------
            //var priorYearTotals = await _context.PSRFinalData
            //    .Where(x => x.FyCd == priorYear.ToString())
            //    .GroupBy(x => x.ProjId)
            //    .Select(g => new
            //    {
            //        ProjId = g.Key,
            //        Amount = g.Sum(x => x.PtdIncurAmt)
            //    })
            //    .ToDictionaryAsync(x => x.ProjId, x => x.Amount);

            var priorYearTotals = await _context.PSRFinalData
                .Where(x => x.FyCd == priorYear.ToString() && x.RateType == "A"
                         && !string.IsNullOrWhiteSpace(x.ProjId))
                .GroupBy(x => x.ProjId)
                .Select(g => new
                {
                    ProjId = g.Key,
                    Amount = g.Sum(x => x.PtdIncurAmt)
                })
                .ToDictionaryAsync(x => x.ProjId!, x => x.Amount);

            // ------------------------------------------------
            // HELPERS
            // ------------------------------------------------
            decimal CalcPercent(decimal current, decimal compare)
            {
                if (compare == 0)
                    return 0;

                return Math.Round(
                    ((current - compare) / compare) * 100,
                    2);
            }

            string Trend(decimal current, decimal compare)
            {
                return current >= compare
                    ? "up"
                    : "down";
            }

            decimal Diff(decimal current, decimal compare)
            {
                return Math.Round(
                    Math.Abs(current - compare),
                    2);
            }

            // ------------------------------------------------
            // ALL PROJECTS
            // ------------------------------------------------
            var allProjects = eacPlans
                .Select(x => x.ProjId)
                .Distinct()
                .ToList();

            var result = new List<object>();

            foreach (var projId in allProjects)
            {
                var budgetPlan =
                    budgetPlans.FirstOrDefault(x => x.ProjId == projId);

                var eacPlan =
                    eacPlans.FirstOrDefault(x => x.ProjId == projId);

                decimal budget =
                    budgetPlan != null &&
                    budgetPlan.PlId.HasValue &&
                    budgetTotals.ContainsKey(budgetPlan.PlId.Value)
                        ? budgetTotals[budgetPlan.PlId.Value]
                        : 0;

                decimal forecast =
                    eacPlan != null &&
                    eacPlan.PlId.HasValue &&
                    forecastTotals.ContainsKey(eacPlan.PlId.Value)
                        ? forecastTotals[eacPlan.PlId.Value]
                        : 0;

                decimal ytd =
                    eacPlan != null &&
                    eacPlan.PlId.HasValue &&
                    ytdTotals.ContainsKey(eacPlan.PlId.Value)
                        ? ytdTotals[eacPlan.PlId.Value]
                        : 0;

                decimal etc =
                    eacPlan != null &&
                    eacPlan.PlId.HasValue &&
                    etcTotals.ContainsKey(eacPlan.PlId.Value)
                        ? etcTotals[eacPlan.PlId.Value]
                        : 0;

                priorYear =
                    priorYearTotals.ContainsKey(projId)
                        ? priorYearTotals[projId]
                        : 0;

                result.Add(new
                {
                    ProjId = projId,

                    Cards = new List<FinancialCardResponse>
                {
                    // ----------------------------------------
                    // BUDGET
                    // ----------------------------------------
                    new FinancialCardResponse
                    {
                        Title = "Budget",
                        MainValue = Math.Round(budget, 2),

                        LeftComparison = new ComparisonData
                        {
                            Label = "Vs. Prior Year",

                            Value = Diff(budget, priorYear),

                            Percentage =
                                CalcPercent(
                                    budget,
                                    priorYear),

                            Trend =
                                Trend(
                                    budget,
                                    priorYear)
                        }
                    },

                    // ----------------------------------------
                    // FORECAST
                    // ----------------------------------------
                    new FinancialCardResponse
                    {
                        Title = "CY Forecast",
                        MainValue = Math.Round(forecast, 2),

                        RightComparison = new ComparisonData
                        {
                            Label = "Vs. Budget",

                            Value = Diff(forecast, budget),

                            Percentage =
                                CalcPercent(
                                    forecast,
                                    budget),

                            Trend =
                                Trend(
                                    forecast,
                                    budget)
                        }
                    },

                    // ----------------------------------------
                    // YTD
                    // ----------------------------------------
                    new FinancialCardResponse
                    {
                        Title = "YTD Actuals",
                        MainValue = Math.Round(ytd, 2),

                        LeftComparison = new ComparisonData
                        {
                            Label = "Vs. Prior Year",

                            Value = Diff(ytd, priorYear),

                            Percentage =
                                CalcPercent(
                                    ytd,
                                    priorYear),

                            Trend =
                                Trend(
                                    ytd,
                                    priorYear)
                        },

                        RightComparison = new ComparisonData
                        {
                            Label = "Vs. Budget",

                            Value = Diff(ytd, budget),

                            Percentage =
                                CalcPercent(
                                    ytd,
                                    budget),

                            Trend =
                                Trend(
                                    ytd,
                                    budget)
                        }
                    },

                    // ----------------------------------------
                    // ETC
                    // ----------------------------------------
                    new FinancialCardResponse
                    {
                        Title = "ETC",
                        MainValue = Math.Round(etc, 2),

                        LeftComparison = new ComparisonData
                        {
                            Label = "Vs. Prior Year",

                            Value = Diff(etc, priorYear),

                            Percentage =
                                CalcPercent(
                                    etc,
                                    priorYear),

                            Trend =
                                Trend(
                                    etc,
                                    priorYear)
                        },

                        RightComparison = new ComparisonData
                        {
                            Label = "Vs. Budget",

                            Value = Diff(etc, budget),

                            Percentage =
                                CalcPercent(
                                    etc,
                                    budget),

                            Trend =
                                Trend(
                                    etc,
                                    budget)
                        }
                    }
                }
                });
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error while generating financial cards");

            return StatusCode(
                500,
                "Error while generating financial cards");
        }
    }

    [HttpGet("FinancialCardsSummary")]
    public async Task<IActionResult> FinancialCardsSummary()
    {
        try
        {
            // ------------------------------------------------
            // CLOSED PERIOD
            // ------------------------------------------------
            var closingPeriodValue = await _context.PlConfigValues
                .Where(x => x.Name.ToLower() == "closing_period")
                .Select(x => x.Value)
                .FirstOrDefaultAsync();

            DateOnly closingPeriod =
                DateOnly.FromDateTime(DateTime.Parse(closingPeriodValue));

            int closedYear = closingPeriod.Year;
            int closedMonth = closingPeriod.Month;

            decimal priorYear = closedYear - 1;

            // ------------------------------------------------
            // FINAL BUDGET PLAN IDS
            // ------------------------------------------------
            var budgetPlIds = await _context.PlProjectPlans
                .Where(x =>
                    x.FinalVersion == true &&
                    x.PlType == "BUD")
                .Select(x => x.PlId)
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .ToListAsync();

            // ------------------------------------------------
            // FINAL EAC PLAN IDS
            // ------------------------------------------------
            var eacPlIds = await _context.PlProjectPlans
                .Where(x =>
                    x.FinalVersion == true &&
                    x.PlType == "EAC")
                .Select(x => x.PlId)
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .ToListAsync();

            // ------------------------------------------------
            // TOTAL BUDGET
            // ------------------------------------------------
            decimal budget = await _context.PlForecasts
                .Where(x => budgetPlIds.Contains(x.PlId))
                .SumAsync(x => (decimal?)x.Cost) ?? 0;

            // ------------------------------------------------
            // TOTAL FORECAST
            // ------------------------------------------------
            decimal forecast = await _context.PlForecasts
                .Where(x => eacPlIds.Contains(x.PlId))
                .SumAsync(x => (decimal?)x.Cost) ?? 0;

            // ------------------------------------------------
            // TOTAL YTD
            // ------------------------------------------------
            decimal ytd = await _context.PlForecasts
                .Where(x =>
                    eacPlIds.Contains(x.PlId) &&
                    (
                        x.Year < closedYear ||
                        (x.Year == closedYear &&
                         x.Month <= closedMonth)
                    ))
                .SumAsync(x => (decimal?)x.Cost) ?? 0;

            // ------------------------------------------------
            // TOTAL ETC
            // ------------------------------------------------
            decimal etc = await _context.PlForecasts
                .Where(x =>
                    eacPlIds.Contains(x.PlId) &&
                    (
                        x.Year > closedYear ||
                        (x.Year == closedYear &&
                         x.Month > closedMonth)
                    ))
                .SumAsync(x => (decimal?)x.Cost) ?? 0;

            // ------------------------------------------------
            // PRIOR YEAR TOTAL
            // ------------------------------------------------
            decimal priorYearTotal = await _context.PSRFinalData
                .Where(x =>
                    x.FyCd == priorYear.ToString() && x.RateType == "A" &&
                    !string.IsNullOrWhiteSpace(x.ProjId))
                .SumAsync(x => (decimal?)x.PtdIncurAmt) ?? 0;

            // ------------------------------------------------
            // HELPERS
            // ------------------------------------------------
            decimal CalcPercent(decimal current, decimal compare)
            {
                if (compare == 0)
                    return 0;

                return Math.Round(
                    ((current - compare) / compare) * 100,
                    2);
            }

            string Trend(decimal current, decimal compare)
            {
                return current >= compare
                    ? "up"
                    : "down";
            }

            decimal Diff(decimal current, decimal compare)
            {
                return Math.Round(
                    Math.Abs(current - compare),
                    2);
            }

            // ------------------------------------------------
            // RESPONSE
            // ------------------------------------------------
            var result = new List<FinancialCardResponse>
        {
            // ----------------------------------------
            // BUDGET
            // ----------------------------------------
            new FinancialCardResponse
            {
                Title = "Budget",
                MainValue = Math.Round(budget, 2),

                LeftComparison = new ComparisonData
                {
                    Label = "Vs. Prior Year",

                    Value = Diff(budget, priorYearTotal),

                    Percentage =
                        CalcPercent(
                            budget,
                            priorYearTotal),

                    Trend =
                        Trend(
                            budget,
                            priorYearTotal)
                }
            },

            // ----------------------------------------
            // FORECAST
            // ----------------------------------------
            new FinancialCardResponse
            {
                Title = "CY Forecast",
                MainValue = Math.Round(forecast, 2),

                RightComparison = new ComparisonData
                {
                    Label = "Vs. Budget",

                    Value = Diff(forecast, budget),

                    Percentage =
                        CalcPercent(
                            forecast,
                            budget),

                    Trend =
                        Trend(
                            forecast,
                            budget)
                }
            },

            // ----------------------------------------
            // YTD
            // ----------------------------------------
            new FinancialCardResponse
            {
                Title = "YTD Actuals",
                MainValue = Math.Round(ytd, 2),

                LeftComparison = new ComparisonData
                {
                    Label = "Vs. Prior Year",

                    Value = Diff(ytd, priorYearTotal),

                    Percentage =
                        CalcPercent(
                            ytd,
                            priorYearTotal),

                    Trend =
                        Trend(
                            ytd,
                            priorYearTotal)
                },

                RightComparison = new ComparisonData
                {
                    Label = "Vs. Budget",

                    Value = Diff(ytd, budget),

                    Percentage =
                        CalcPercent(
                            ytd,
                            budget),

                    Trend =
                        Trend(
                            ytd,
                            budget)
                }
            },

            // ----------------------------------------
            // ETC
            // ----------------------------------------
            new FinancialCardResponse
            {
                Title = "ETC",
                MainValue = Math.Round(etc, 2),

                LeftComparison = new ComparisonData
                {
                    Label = "Vs. Prior Year",

                    Value = Diff(etc, priorYearTotal),

                    Percentage =
                        CalcPercent(
                            etc,
                            priorYearTotal),

                    Trend =
                        Trend(
                            etc,
                            priorYearTotal)
                },

                RightComparison = new ComparisonData
                {
                    Label = "Vs. Budget",

                    Value = Diff(etc, budget),

                    Percentage =
                        CalcPercent(
                            etc,
                            budget),

                    Trend =
                        Trend(
                            etc,
                            budget)
                }
            }
        };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error while generating financial cards summary");

            return StatusCode(
                500,
                "Error while generating financial cards summary");
        }
    }

    [HttpGet("FinancialCardsSummaryWithGraphData")]
    public async Task<IActionResult> FinancialCardsSummaryWithGraphData()
    {
        try
        {
            // ------------------------------------------------
            // CLOSED PERIOD
            // ------------------------------------------------
            var closingPeriodValue = await _context.PlConfigValues
                .Where(x => x.Name.ToLower() == "closing_period")
                .Select(x => x.Value)
                .FirstOrDefaultAsync();

            DateOnly closingPeriod =
                DateOnly.FromDateTime(DateTime.Parse(closingPeriodValue));

            int closedYear = closingPeriod.Year;
            int closedMonth = closingPeriod.Month;

            decimal priorYear = closedYear - 1;

            string[] monthNames =
            {
            "Jan","Feb","Mar","Apr","May","Jun",
            "Jul","Aug","Sep","Oct","Nov","Dec"
        };

            // ------------------------------------------------
            // FINAL BUDGET PLAN IDS
            // ------------------------------------------------
            var budgetPlIds = await _context.PlProjectPlans
                .Where(x =>
                    x.FinalVersion == true &&
                    x.PlType == "BUD")
                .Select(x => x.PlId)
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .ToListAsync();

            // ------------------------------------------------
            // FINAL EAC PLAN IDS
            // ------------------------------------------------
            var eacPlIds = await _context.PlProjectPlans
                .Where(x =>
                    x.FinalVersion == true &&
                    x.PlType == "EAC")
                .Select(x => x.PlId)
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .ToListAsync();

            // ------------------------------------------------
            // TOTAL BUDGET
            // ------------------------------------------------
            decimal budget = await _context.PlForecasts
                .Where(x => budgetPlIds.Contains(x.PlId))
                .SumAsync(x => (decimal?)x.Cost) ?? 0;

            // ------------------------------------------------
            // TOTAL FORECAST
            // ------------------------------------------------
            decimal forecast = await _context.PlForecasts
                .Where(x => eacPlIds.Contains(x.PlId))
                .SumAsync(x => (decimal?)x.Cost) ?? 0;

            // ------------------------------------------------
            // TOTAL YTD
            // ------------------------------------------------
            decimal ytd = await _context.PlForecasts
                .Where(x =>
                    eacPlIds.Contains(x.PlId) &&
                    (
                        x.Year < closedYear ||
                        (x.Year == closedYear &&
                         x.Month <= closedMonth)
                    ))
                .SumAsync(x => (decimal?)x.Cost) ?? 0;

            // ------------------------------------------------
            // TOTAL ETC
            // ------------------------------------------------
            decimal etc = await _context.PlForecasts
                .Where(x =>
                    eacPlIds.Contains(x.PlId) &&
                    (
                        x.Year > closedYear ||
                        (x.Year == closedYear &&
                         x.Month > closedMonth)
                    ))
                .SumAsync(x => (decimal?)x.Cost) ?? 0;

            // ------------------------------------------------
            // PRIOR YEAR TOTAL
            // ------------------------------------------------
            decimal priorYearTotal = await _context.PSRFinalData
                .Where(x =>
                    x.FyCd == priorYear.ToString() && x.RateType == "A" &&
                    !string.IsNullOrWhiteSpace(x.ProjId))
                .SumAsync(x => (decimal?)x.PtdIncurAmt) ?? 0;

            // ------------------------------------------------
            // PRIOR YEAR MONTHLY
            // ------------------------------------------------
            var pyMonthly = await _context.PSRFinalData
                .Where(x =>
                    x.FyCd == priorYear.ToString() && x.RateType == "A" &&
                    x.PdNo != null)
                .GroupBy(x => x.PdNo)
                .Select(g => new
                {
                    Month = g.Key,
                    Amount = g.Sum(x => x.PtdIncurAmt)
                })
                .ToDictionaryAsync(
                    x => Convert.ToInt32(x.Month),
                    x => x.Amount);

            // ------------------------------------------------
            // CY MONTHLY
            // ------------------------------------------------
            var cyMonthly = await _context.PlForecasts
                .Where(x =>
                    eacPlIds.Contains(x.PlId) &&
                    x.Year == closedYear)
                .GroupBy(x => x.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Amount = g.Sum(x => x.Cost)
                })
                .ToDictionaryAsync(
                    x => x.Month,
                    x => x.Amount);

            // ------------------------------------------------
            // BUDGET MONTHLY
            // ------------------------------------------------
            var budgetMonthly = await _context.PlForecasts
                .Where(x =>
                    budgetPlIds.Contains(x.PlId) &&
                    x.Year == closedYear)
                .GroupBy(x => x.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Amount = g.Sum(x => x.Cost)
                })
                .ToDictionaryAsync(
                    x => x.Month,
                    x => x.Amount);

            // ------------------------------------------------
            // GRAPH DATA
            // ------------------------------------------------
            var graphData = Enumerable.Range(1, 12)
                .Select(month => new
                {
                    name = monthNames[month - 1],

                    py = Math.Round(
                        pyMonthly.ContainsKey(month)
                            ? pyMonthly[month]
                            : 0,
                        2),

                    cy = Math.Round(
                        cyMonthly.ContainsKey(month)
                            ? cyMonthly[month]
                            : 0,
                        2),

                    budget = Math.Round(
                        budgetMonthly.ContainsKey(month)
                            ? budgetMonthly[month]
                            : 0,
                        2)
                })
                .ToList();

            // ------------------------------------------------
            // HELPERS
            // ------------------------------------------------
            decimal CalcPercent(decimal current, decimal compare)
            {
                if (compare == 0)
                    return 0;

                return Math.Round(
                    ((current - compare) / compare) * 100,
                    2);
            }

            string Trend(decimal current, decimal compare)
            {
                return current >= compare
                    ? "up"
                    : "down";
            }

            decimal Diff(decimal current, decimal compare)
            {
                return Math.Round(
                    Math.Abs(current - compare),
                    2);
            }

            // ------------------------------------------------
            // CARDS
            // ------------------------------------------------
            var cards = new List<FinancialCardResponse>
        {
            // ----------------------------------------
            // BUDGET
            // ----------------------------------------
            new FinancialCardResponse
            {
                Title = "Budget",
                MainValue = Math.Round(budget, 2),

                LeftComparison = new ComparisonData
                {
                    Label = "Vs. Prior Year",

                    Value = Diff(budget, priorYearTotal),

                    Percentage =
                        CalcPercent(
                            budget,
                            priorYearTotal),

                    Trend =
                        Trend(
                            budget,
                            priorYearTotal)
                }
            },

            // ----------------------------------------
            // FORECAST
            // ----------------------------------------
            new FinancialCardResponse
            {
                Title = "CY Forecast",
                MainValue = Math.Round(forecast, 2),

                RightComparison = new ComparisonData
                {
                    Label = "Vs. Budget",

                    Value = Diff(forecast, budget),

                    Percentage =
                        CalcPercent(
                            forecast,
                            budget),

                    Trend =
                        Trend(
                            forecast,
                            budget)
                }
            },

            // ----------------------------------------
            // YTD
            // ----------------------------------------
            new FinancialCardResponse
            {
                Title = "YTD Actuals",
                MainValue = Math.Round(ytd, 2),

                LeftComparison = new ComparisonData
                {
                    Label = "Vs. Prior Year",

                    Value = Diff(ytd, priorYearTotal),

                    Percentage =
                        CalcPercent(
                            ytd,
                            priorYearTotal),

                    Trend =
                        Trend(
                            ytd,
                            priorYearTotal)
                },

                RightComparison = new ComparisonData
                {
                    Label = "Vs. Budget",

                    Value = Diff(ytd, budget),

                    Percentage =
                        CalcPercent(
                            ytd,
                            budget),

                    Trend =
                        Trend(
                            ytd,
                            budget)
                }
            },

            // ----------------------------------------
            // ETC
            // ----------------------------------------
            new FinancialCardResponse
            {
                Title = "ETC",
                MainValue = Math.Round(etc, 2),

                LeftComparison = new ComparisonData
                {
                    Label = "Vs. Prior Year",

                    Value = Diff(etc, priorYearTotal),

                    Percentage =
                        CalcPercent(
                            etc,
                            priorYearTotal),

                    Trend =
                        Trend(
                            etc,
                            priorYearTotal)
                },

                RightComparison = new ComparisonData
                {
                    Label = "Vs. Budget",

                    Value = Diff(etc, budget),

                    Percentage =
                        CalcPercent(
                            etc,
                            budget),

                    Trend =
                        Trend(
                            etc,
                            budget)
                }
            }
        };

            // ------------------------------------------------
            // FINAL RESPONSE
            // ------------------------------------------------
            return Ok(new
            {
                Cards = cards,
                GraphData = graphData
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error while generating financial cards summary");

            return StatusCode(
                500,
                "Error while generating financial cards summary");
        }
    }

    [HttpGet("FinancialCardsWithGraphData/{projId}")]
    public async Task<IActionResult> FinancialCardsWithGraphData(string projId)
    {
        try
        {
            // ------------------------------------------------
            // CLOSED PERIOD
            // ------------------------------------------------
            var closingPeriodValue = await _context.PlConfigValues
                .Where(x => x.Name.ToLower() == "closing_period")
                .Select(x => x.Value)
                .FirstOrDefaultAsync();

            DateOnly closingPeriod =
                DateOnly.FromDateTime(DateTime.Parse(closingPeriodValue));

            int closedYear = closingPeriod.Year;
            int closedMonth = closingPeriod.Month;

            int priorYear = closedYear - 1;

            string[] monthNames =
            {
            "Jan","Feb","Mar","Apr","May","Jun",
            "Jul","Aug","Sep","Oct","Nov","Dec"
        };

            // ------------------------------------------------
            // FINAL BUDGET PLAN
            // ------------------------------------------------
            var budgetPlId = await _context.PlProjectPlans
                .Where(x =>
                    x.ProjId == projId &&
                    x.FinalVersion == true &&
                    x.PlType == "BUD")
                .Select(x => x.PlId)
                .FirstOrDefaultAsync();

            // ------------------------------------------------
            // FINAL EAC PLAN
            // ------------------------------------------------
            var eacPlId = await _context.PlProjectPlans
                .Where(x =>
                    x.ProjId == projId &&
                    x.FinalVersion == true &&
                    x.PlType == "EAC")
                .Select(x => x.PlId)
                .FirstOrDefaultAsync();

            // ------------------------------------------------
            // BUDGET
            // ------------------------------------------------
            decimal budget = 0;

            if (budgetPlId != null)
            {
                budget = await _context.PlForecasts
                    .Where(x => x.PlId == budgetPlId)
                    .SumAsync(x => (decimal?)x.Cost) ?? 0;
            }

            // ------------------------------------------------
            // CY FORECAST
            // ------------------------------------------------
            decimal forecast = 0;

            if (eacPlId != null)
            {
                forecast = await _context.PlForecasts
                    .Where(x => x.PlId == eacPlId)
                    .SumAsync(x => (decimal?)x.Cost) ?? 0;
            }

            // ------------------------------------------------
            // YTD ACTUALS
            // ------------------------------------------------
            decimal ytdActuals = 0;

            if (eacPlId != null)
            {
                ytdActuals = await _context.PlForecasts
                    .Where(x =>
                        x.PlId == eacPlId &&
                        (
                            x.Year < closedYear ||
                            (x.Year == closedYear &&
                             x.Month <= closedMonth)
                        ))
                    .SumAsync(x => (decimal?)x.Cost) ?? 0;
            }

            // ------------------------------------------------
            // ETC
            // ------------------------------------------------
            decimal etc = 0;

            if (eacPlId != null)
            {
                etc = await _context.PlForecasts
                    .Where(x =>
                        x.PlId == eacPlId &&
                        (
                            x.Year > closedYear ||
                            (x.Year == closedYear &&
                             x.Month > closedMonth)
                        ))
                    .SumAsync(x => (decimal?)x.Cost) ?? 0;
            }

            // ------------------------------------------------
            // PRIOR YEAR ACTUALS
            // ------------------------------------------------
            decimal priorYearActuals = await _context.PSRFinalData
                .Where(x =>
                    x.ProjId == projId && x.RateType == "A" &&
                    x.FyCd == priorYear.ToString())
                .SumAsync(x => (decimal?)x.PtdIncurAmt) ?? 0;

            // ------------------------------------------------
            // PRIOR YEAR MONTHLY
            // ------------------------------------------------
            var pyMonthly = await _context.PSRFinalData
                .Where(x =>
                    x.ProjId == projId && x.RateType == "A" &&
                    x.FyCd == priorYear.ToString() &&
                    x.PdNo != null)
                .GroupBy(x => x.PdNo)
                .Select(g => new
                {
                    Month = g.Key,
                    Amount = g.Sum(x => x.PtdIncurAmt)
                })
                .ToDictionaryAsync(
                    x => Convert.ToInt32(x.Month),
                    x => x.Amount);

            // ------------------------------------------------
            // CY MONTHLY
            // ------------------------------------------------
            var cyMonthly = new Dictionary<int, decimal>();

            if (eacPlId != null)
            {
                cyMonthly = await _context.PlForecasts
                    .Where(x =>
                        x.PlId == eacPlId &&
                        x.Year == closedYear)
                    .GroupBy(x => x.Month)
                    .Select(g => new
                    {
                        Month = g.Key,
                        Amount = g.Sum(x => x.Cost)
                    })
                    .ToDictionaryAsync(
                        x => x.Month,
                        x => x.Amount);
            }

            // ------------------------------------------------
            // BUDGET MONTHLY
            // ------------------------------------------------
            var budgetMonthly = new Dictionary<int, decimal>();

            if (budgetPlId != null)
            {
                budgetMonthly = await _context.PlForecasts
                    .Where(x =>
                        x.PlId == budgetPlId &&
                        x.Year == closedYear)
                    .GroupBy(x => x.Month)
                    .Select(g => new
                    {
                        Month = g.Key,
                        Amount = g.Sum(x => x.Cost)
                    })
                    .ToDictionaryAsync(
                        x => x.Month,
                        x => x.Amount);
            }

            // ------------------------------------------------
            // GRAPH DATA
            // ------------------------------------------------
            var graphData = Enumerable.Range(1, 12)
                .Select(month => new
                {
                    name = monthNames[month - 1],

                    py = Math.Round(
                        pyMonthly.ContainsKey(month)
                            ? pyMonthly[month]
                            : 0,
                        2),

                    cy = Math.Round(
                        cyMonthly.ContainsKey(month)
                            ? cyMonthly[month]
                            : 0,
                        2),

                    budget = Math.Round(
                        budgetMonthly.ContainsKey(month)
                            ? budgetMonthly[month]
                            : 0,
                        2)
                })
                .ToList();

            // ------------------------------------------------
            // HELPER METHODS
            // ------------------------------------------------
            decimal CalcPercent(decimal current, decimal compare)
            {
                if (compare == 0)
                    return 0;

                return Math.Round(
                    ((current - compare) / compare) * 100,
                    2);
            }

            string Trend(decimal current, decimal compare)
            {
                return current >= compare
                    ? "up"
                    : "down";
            }

            decimal Diff(decimal current, decimal compare)
            {
                return Math.Round(
                    Math.Abs(current - compare),
                    2);
            }

            // ------------------------------------------------
            // RESPONSE
            // ------------------------------------------------
            var cards = new List<FinancialCardResponse>
        {
            // ------------------------------------------------
            // BUDGET
            // ------------------------------------------------
            new FinancialCardResponse
            {
                Title = "Budget",
                MainValue = Math.Round(budget, 2),

                LeftComparison = new ComparisonData
                {
                    Label = "Vs. Prior Year",

                    Value = Diff(budget, priorYearActuals),

                    Percentage =
                        CalcPercent(
                            budget,
                            priorYearActuals),

                    Trend =
                        Trend(
                            budget,
                            priorYearActuals)
                }
            },

            // ------------------------------------------------
            // CY FORECAST
            // ------------------------------------------------
            new FinancialCardResponse
            {
                Title = "CY Forecast",
                MainValue = Math.Round(forecast, 2),

                RightComparison = new ComparisonData
                {
                    Label = "Vs. Budget",

                    Value = Diff(forecast, budget),

                    Percentage =
                        CalcPercent(
                            forecast,
                            budget),

                    Trend =
                        Trend(
                            forecast,
                            budget)
                }
            },

            // ------------------------------------------------
            // YTD ACTUALS
            // ------------------------------------------------
            new FinancialCardResponse
            {
                Title = "YTD Actuals",
                MainValue = Math.Round(ytdActuals, 2),

                LeftComparison = new ComparisonData
                {
                    Label = "Vs. Prior Year",

                    Value = Diff(
                        ytdActuals,
                        priorYearActuals),

                    Percentage =
                        CalcPercent(
                            ytdActuals,
                            priorYearActuals),

                    Trend =
                        Trend(
                            ytdActuals,
                            priorYearActuals)
                },

                RightComparison = new ComparisonData
                {
                    Label = "Vs. Budget",

                    Value = Diff(
                        ytdActuals,
                        budget),

                    Percentage =
                        CalcPercent(
                            ytdActuals,
                            budget),

                    Trend =
                        Trend(
                            ytdActuals,
                            budget)
                }
            },

            // ------------------------------------------------
            // ETC
            // ------------------------------------------------
            new FinancialCardResponse
            {
                Title = "ETC",
                MainValue = Math.Round(etc, 2),

                LeftComparison = new ComparisonData
                {
                    Label = "Vs. Prior Year",

                    Value = Diff(
                        etc,
                        priorYearActuals),

                    Percentage =
                        CalcPercent(
                            etc,
                            priorYearActuals),

                    Trend =
                        Trend(
                            etc,
                            priorYearActuals)
                },

                RightComparison = new ComparisonData
                {
                    Label = "Vs. Budget",

                    Value = Diff(
                        etc,
                        budget),

                    Percentage =
                        CalcPercent(
                            etc,
                            budget),

                    Trend =
                        Trend(
                            etc,
                            budget)
                }
            }
        };

            // ------------------------------------------------
            // FINAL RESPONSE
            // ------------------------------------------------
            return Ok(new
            {
                Cards = cards,
                GraphData = graphData
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error while generating financial cards");

            return StatusCode(
                500,
                "Error while generating financial cards");
        }
    }

    //----------------------------------------------------------
    // GET:
    // api/utilization/monthly-employee-hours?year=2026
    //----------------------------------------------------------
    [HttpGet("monthly-employee-hours")]
    public async Task<IActionResult> GetMonthlyEmployeeHours(
        int year)
    {
        DateOnly startDate =
    new DateOnly(year, 1, 1);

        DateOnly endDate =
            new DateOnly(year, 12, 31);
        ScheduleHelper scheduleHelper = new ScheduleHelper();

        var schedule = scheduleHelper.GetWorkingDaysForDuration(startDate, endDate, _orgService).Select(x => x.WorkingHours);

        //------------------------------------------------------
        // GET FINAL VERSION PLAN IDs
        //------------------------------------------------------

        var finalPlanIds = await _context.PlProjectPlans
            .AsNoTracking()
            .Where(x =>
                x.FinalVersion == true)
            .Select(x => x.PlId)
            .ToListAsync();

        //------------------------------------------------------
        // GET FORECAST DATA
        //------------------------------------------------------

        var forecastData = await _context.PlForecasts.Include(x => x.Emple)
            .AsNoTracking()
            .Where(x =>
                x.Year == year &&
                finalPlanIds.Contains(x.PlId))
            .ToListAsync();

        //------------------------------------------------------
        // GROUP + FORMAT RESPONSE
        //------------------------------------------------------

        var result = forecastData
            .GroupBy(x => new
            {
                x.EmplId
            })
            .Select(g => new
            {
                id = g.Key.EmplId,

                //name = g
                //    .Select(x => x.Emple.LastName + " " + x.Emple.FirstName)
                //    .FirstOrDefault(),

                name = g
                        .Select(x =>
                            x.Emple != null
                                ? $"{x.Emple.LastName ?? ""} {x.Emple.FirstName ?? ""}".Trim()
                                : ""
                        )
                        .FirstOrDefault() ?? "",

                //status = g
                //    .Select(x => x.Emple.Status)
                //    .FirstOrDefault(),

                standardHours = schedule,

                hours = Enumerable
                    .Range(1, 12)
                    .Select(month =>
                        Math.Round(
                            g.Where(x =>
                                    x.Month == month)
                             .Sum(x =>
                                 x.Actualhours),
                            2))
                    .ToArray()
            })
            .OrderBy(x => x.id)
            .ToList();

        //------------------------------------------------------
        // RETURN RESPONSE
        //------------------------------------------------------

        return Ok(result);
    }
}
