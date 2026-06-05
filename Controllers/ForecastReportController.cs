using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql;
using PlanningAPI.Models;
using QuestPDF.Fluent;
using SQLitePCL;
using System.Collections.Generic;
using WebApi.DTO;
using WebApi.Helpers;
using static PlanningAPI.Models.ForecastReport;

namespace PlanningAPI.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    public class ForecastReportController : ControllerBase
    {
        private readonly IAiService _aiService;
        private readonly MydatabaseContext _context;

        public ForecastReportController(IAiService aiService, MydatabaseContext context)
        {
            _aiService = aiService;
            _context = context;
        }


        [HttpPost("CallForecastRolloverFlexibleAsync")]
        public async Task<IActionResult> CallForecastRolloverFlexibleAsync(
    string periodType,
    DateOnly baseDate,
    DateOnly targetStartDate,
    string? projId,
    int? plId,
    decimal increasePct = 0)
        {
            try
            {
                // ✅ Basic Validations
                if (string.IsNullOrWhiteSpace(periodType))
                    return BadRequest("periodType is required");

                var allowedPeriods = new[] { "DAILY", "MONTHLY", "YEARLY" };
                if (!allowedPeriods.Contains(periodType.ToUpper()))
                    return BadRequest("Invalid periodType. Allowed: DAILY, MONTHLY, YEARLY");

                if (increasePct < 0)
                    return BadRequest("increasePct cannot be negative");

                if (plId.HasValue && plId <= 0)
                    return BadRequest("plId must be greater than 0");

                // Convert DateOnly → DateTime (required for EF)
                //var baseDateTime = baseDate.ToDateTime(TimeOnly.MinValue);
                //var targetStartDateTime = targetStartDate.ToDateTime(TimeOnly.MinValue);

                bool dryRun = false;

                // ✅ Execute SP with proper DATE casting
                await _context.Database.ExecuteSqlInterpolatedAsync($@"
            CALL public.sp_forecast_rollover_flexible(
                {periodType},
                {baseDate.ToString("yyyy-MM-dd")}::date,
                {targetStartDate.ToString("yyyy-MM-dd")}::date,
                {projId},
                {plId},
                {increasePct},
                {dryRun}
            );
        ");

                return Ok(new
                {
                    message = "Stored procedure executed successfully"
                });
            }
            catch (Npgsql.NpgsqlException ex)
            {
                // ✅ PostgreSQL-specific errors
                return StatusCode(500, new
                {
                    message = "Database error while executing stored procedure",
                    detail = ex.Message,
                    sqlState = ex.SqlState
                });
            }
            catch (Exception ex)
            {
                // ✅ General errors
                return StatusCode(500, new
                {
                    message = "Unexpected error",
                    detail = ex.Message
                });
            }
        }
        [HttpGet("UpdateForecastFromYearAsync")]
        public async Task UpdateForecastFromYearAsync(int plId, int sourceYear, int targetYear)
        {
            // Step 1: Get source data
            var sourceData = await _context.PlForecasts
                .Where(x => x.PlId == plId && x.Year == sourceYear)
                .ToListAsync();

            if (!sourceData.Any())
                return;

            // Step 2: Get target data
            var targetData = await _context.PlForecasts
                .Where(x => x.PlId == plId && x.Year == targetYear)
                .ToListAsync();

            foreach (var src in sourceData)
            {
                var target = targetData.FirstOrDefault(x =>
                    x.EmplId == src.EmplId &&
                    x.Plc == src.Plc &&
                    x.empleId == src.empleId &&
                    x.Month == src.Month &&     // ✅ keep same month mapping
                    x.DctId == src.DctId);      // optional but recommended

                if (target != null)
                {
                    if (target.Forecastedhours == src.Forecastedhours &&
                        target.Forecastedamt == src.Forecastedamt)
                    {
                        // ✅ Skip if no changes
                        continue;
                    }
                    if (src.DctId == null && target.Forecastedhours == 0)
                    {
                        target.Forecastedhours = src.Forecastedhours;
                    }

                    if (src.empleId == null && target.Forecastedamt == 0)
                    {
                        target.Forecastedamt = src.Forecastedamt;
                    }
                    // ✅ Update only required fields



                    //target.Updatedat = DateTime.UtcNow;
                }
                else
                {
                    // ✅ Insert if missing
                    var clone = PlForecast.CloneWithoutId(src);

                    clone.Year = targetYear;
                    clone.PlId = plId;
                    //clone.Createdat = DateTime.UtcNow;

                    await _context.PlForecasts.AddAsync(clone);
                }
            }

            await _context.SaveChangesAsync();
        }

        [HttpGet("UpdateForecastFlexibleAsync")]
        public async Task UpdateForecastFlexibleAsync(
    int plId,
    int sourceYear,
    int targetYear,
    decimal percentage,          // e.g. 2 for 2%
    string periodType            // Monthly, Quarterly, HalfYearly, Yearly
)
        {
            var factor = 1 + (percentage / 100);

            var sourceData = await _context.PlForecasts
                .Where(x => x.PlId == plId && x.Year == sourceYear)
                .ToListAsync();

            if (!sourceData.Any())
                return;

            var targetData = await _context.PlForecasts
                .Where(x => x.PlId == plId && x.Year == targetYear)
                .ToListAsync();

            foreach (var src in sourceData)
            {
                var target = targetData.FirstOrDefault(x =>
                    x.EmplId == src.EmplId &&
                    x.Plc == src.Plc &&
                    x.empleId == src.empleId &&
                    IsMonthMatch(src.Month, x.Month, periodType) &&
                    x.DctId == src.DctId);

                if (target != null)
                {
                    // Skip if same
                    if (target.Forecastedhours == src.Forecastedhours &&
                        target.Forecastedamt == src.Forecastedamt)
                        continue;

                    // ✅ Apply conditions + percentage
                    if (src.DctId == null && target.Forecastedhours == 0)
                    {
                        target.Forecastedhours = src.Forecastedhours * factor;
                    }

                    if (src.empleId == null && target.Forecastedamt == 0)
                    {
                        target.Forecastedamt = src.Forecastedamt * factor;
                    }

                    target.Updatedat = DateTime.UtcNow;
                }
                else
                {
                    var clone = PlForecast.CloneWithoutId(src);

                    clone.Year = targetYear;
                    clone.PlId = plId;

                    // ✅ Apply percentage
                    clone.Forecastedhours = src.Forecastedhours * factor;
                    clone.Forecastedamt = src.Forecastedamt * factor;

                    await _context.PlForecasts.AddAsync(clone);
                }
            }

            await _context.SaveChangesAsync();
        }

        [NonAction]
        private bool IsMonthMatch(int sourceMonth, int targetMonth, string periodType)
        {
            switch (periodType.ToLower())
            {
                case "monthly":
                    return sourceMonth == targetMonth;

                case "quarterly":
                    return GetQuarter(sourceMonth) == GetQuarter(targetMonth);

                case "halfyearly":
                    return GetHalf(sourceMonth) == GetHalf(targetMonth);

                case "yearly":
                    return true; // all months match

                default:
                    return false;
            }
        }

        private int GetQuarter(int month) => (month - 1) / 3 + 1;

        private int GetHalf(int month) => (month - 1) / 6 + 1;



        [HttpPost("UpdateForecastWithPeriodShift")]
        public async Task UpdateForecastWithPeriodShift(
    int plId,
    int sourceYear,
    int targetYear,
    int sourcePeriod,     // month (1-12) OR quarter (1-4) OR half (1-2)
    int targetPeriod,
    decimal percentage,
    string periodType     // Monthly, Quarterly, HalfYearly
)
        {
            var plan = await _context.PlProjectPlans.FirstOrDefaultAsync(p => p.PlId == plId);

            if(plan == null)
                throw new Exception("Plan not found");


            var factor = 1 + (percentage / 100);

            var sourceData = await _context.PlForecasts
                .Where(x => x.PlId == plId && x.Year == sourceYear)
                .ToListAsync();

            var targetData = await _context.PlForecasts
                .Where(x => x.PlId == plId && x.Year == targetYear)
                .ToListAsync();

            foreach (var src in sourceData)
            {
                if (!IsSourceMatch(src.Month, sourcePeriod, periodType))
                    continue;

                var targetMonth = GetTargetMonth(src.Month, sourcePeriod, targetPeriod, periodType);

                var target = targetData.FirstOrDefault(x =>
                    x.EmplId == src.EmplId &&
                    x.Plc == src.Plc &&
                    x.empleId == src.empleId &&
                    x.Month == targetMonth &&
                    x.DctId == src.DctId);

                if (target != null)
                {
                    if (src.DctId == null && target.Forecastedhours == 0)
                    {

                        switch (plan.PlType?.ToUpper())
                        {
                            case "BUD":
                                target.Forecastedhours = src.Forecastedhours * factor;
                                break;

                            case "EAC":
                                target.Actualhours = src.Actualhours * factor;
                                break;
                        }
                        //target.Forecastedhours = src.Forecastedhours * factor;

                    }

                    if (src.empleId == null && target.Forecastedamt == 0)
                    {
                        switch (plan.PlType?.ToUpper())
                        {
                            case "BUD":
                                target.Forecastedamt = src.Forecastedamt * factor;
                                break;

                            case "EAC":
                                target.Actualamt = src.Actualamt * factor;
                                break;
                        }
                        //target.Forecastedamt = src.Forecastedamt * factor;
                    }

                    //target.Updatedat = DateTime.UtcNow;
                }
                else
                {
                    var clone = PlForecast.CloneWithoutId(src);

                    clone.Year = targetYear;
                    clone.Month = targetMonth;
                    clone.PlId = plId;

                    switch (plan.PlType?.ToUpper())
                    {
                        case "BUD":
                            clone.Forecastedhours = src.Forecastedhours * factor;
                            clone.Forecastedamt = src.Forecastedamt * factor;
                            break;

                        case "EAC":
                            clone.Actualamt = src.Actualamt * factor;
                            clone.Actualhours = src.Actualhours * factor;
                            break;
                    }
                    //clone.Forecastedhours = src.Forecastedhours * factor;
                    //clone.Forecastedamt = src.Forecastedamt * factor;

                    await _context.PlForecasts.AddAsync(clone);
                }
            }

            await _context.SaveChangesAsync();
        }

        private bool IsSourceMatch(int month, int sourcePeriod, string type)
        {
            return type.ToLower() switch
            {
                "monthly" => month == sourcePeriod,

                "quarterly" => GetQuarter(month) == sourcePeriod,

                "halfyearly" => GetHalf(month) == sourcePeriod,

                _ => false
            };
        }

        private int GetTargetMonth(int sourceMonth, int sourcePeriod, int targetPeriod, string type)
        {
            switch (type.ToLower())
            {
                case "monthly":
                    return targetPeriod; // direct month mapping

                case "quarterly":
                    int monthOffsetQ = sourceMonth % 3 == 0 ? 3 : sourceMonth % 3;
                    return (targetPeriod - 1) * 3 + monthOffsetQ;

                case "halfyearly":
                    int monthOffsetH = sourceMonth % 6 == 0 ? 6 : sourceMonth % 6;
                    return (targetPeriod - 1) * 6 + monthOffsetH;

                default:
                    return sourceMonth;
            }
        }


        [HttpPost("UpdateForecastByProject")]
        public async Task<IActionResult> UpdateForecastByProject([FromBody] ForecastShiftByProjectRequest request)
        {
            if (request == null)
                return BadRequest("Invalid request");

            // Step 1: Get PlId from Project + PlanType + Version
            var plan = await _context.PlProjectPlans
                .FirstOrDefaultAsync(x =>
                    x.ProjId == request.ProjectId &&
                    x.PlType == request.PlanType &&
                    x.Version == request.Version);

            if (plan == null)
                return NotFound("Project Plan not found");

            int plId = plan.PlId.Value;

            var factor = 1 + (request.Percentage / 100);

            // Step 2: Get source data
            var sourceData = await _context.PlForecasts
                .Where(x => x.PlId == plId && x.Year == request.SourceYear)
                .ToListAsync();

            if (!sourceData.Any())
                return NotFound("No source data found");

            // Step 3: Get target data
            var targetData = await _context.PlForecasts
                .Where(x => x.PlId == plId && x.Year == request.TargetYear)
                .ToListAsync();

            int updated = 0, inserted = 0;

            foreach (var src in sourceData)
            {
                if (!IsSourceMatch(src.Month, request.SourcePeriod, request.PeriodType))
                    continue;

                var targetMonth = GetTargetMonth(src.Month, request.SourcePeriod, request.TargetPeriod, request.PeriodType);

                var target = targetData.FirstOrDefault(x =>
                    x.EmplId == src.EmplId &&
                    x.Plc == src.Plc &&
                    x.empleId == src.empleId &&
                    x.Month == targetMonth &&
                    x.DctId == src.DctId);

                if (target != null)
                {
                    if (src.DctId == null && target.Forecastedhours == 0)
                        target.Forecastedhours = Math.Round(src.Forecastedhours * factor, 2);

                    if (src.empleId == null && target.Forecastedamt == 0)
                        target.Forecastedamt = Math.Round(src.Forecastedamt.GetValueOrDefault() * factor, 2);

                    //target.Updatedat = DateTime.UtcNow;
                    updated++;
                }
                else
                {
                    var clone = PlForecast.CloneWithoutId(src);

                    clone.PlId = plId;
                    clone.Year = request.TargetYear;
                    clone.Month = targetMonth;

                    clone.Forecastedhours = Math.Round(src.Forecastedhours * factor, 2);
                    clone.Forecastedamt = Math.Round(src.Forecastedamt.GetValueOrDefault() * factor, 2);

                    await _context.PlForecasts.AddAsync(clone);
                    inserted++;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Forecast updated successfully",
                ProjectId = request.ProjectId,
                PlanType = request.PlanType,
                Version = request.Version,
                Updated = updated,
                Inserted = inserted
            });
        }



        [HttpPost("generate")]
        public async Task<IActionResult> GenerateForecastReport([FromBody] PlanForecastSummary forecast)
        {
            if (forecast == null)
                return BadRequest("Forecast data is required.");

            // 1. Call AI service for insights
            var aiInsight = await _aiService.GetForecastInsightAsync(forecast);

            // 2. Generate PDF
            var report = new ForecastReport(forecast, aiInsight);
            var pdfBytes = report.GeneratePdf();

            // 3. Return PDF as downloadable file
            return File(pdfBytes, "application/pdf", $"{forecast.Proj_Id}_ForecastReport.pdf");
        }
        [HttpGet("GetPSRData")]
        public async Task<IActionResult> GetPSRData([FromQuery] string? proj_id)
        {
            var query = _context.PSRFinalData.AsQueryable();

            if (!string.IsNullOrEmpty(proj_id))
            {
                query = query.Where(x => x.ProjId == proj_id);
            }

            var list = await query.ToListAsync();
            return Ok(list);
        }


        [HttpGet("GetPSRHeaderData")]
        public async Task<IActionResult> GetPSRHeaderData([FromQuery] string? proj_id)
        {
            var query = _context.PsrHeader.AsQueryable();

            if (!string.IsNullOrEmpty(proj_id))
            {
                query = query.Where(x => x.ProjId == proj_id);
            }

            var list = await query.ToListAsync();
            return Ok(list);
        }

        [HttpGet("GetGLData")]
        public async Task<IActionResult> GetGLData()
        {
            var list = _context.PlFinancialTransactions.ToList();
            return Ok(list);

        }

        [HttpGet("GetViewData")]
        public async Task<IActionResult> GetViewData()
        {

            var result = await _context.ViewPsrData
                                .ToListAsync();
            return Ok(result);

        }
        [HttpGet("GetForecastView")]
        public async Task<IActionResult> GetForecastView()
        {


            var result = _context.ForecastView.ToList();
            return Ok(result);

        }
        [HttpGet("GetLabHSData")]
        public async Task<IActionResult> GetLabHSData([FromQuery] int? take = null)
        {
            var query = _context.LabHours.AsQueryable();

            if (take.HasValue && take.Value > 0)
            {
                query = query.Take(take.Value);
            }

            var result = await query.ToListAsync();
            return Ok(result);
        }

        [HttpGet("GetAccountGroupSetupData")]
        public async Task<IActionResult> GetAccountGroupSetupData([FromQuery] int? take = null)
        {
            var query = _context.AccountGroupSetup.AsQueryable();

            if (take.HasValue && take.Value > 0)
            {
                query = query.Take(take.Value);
            }

            var result = await query.ToListAsync();
            return Ok(result);
        }

        [HttpGet("GetFsData")]
        public async Task<IActionResult> GetFsData([FromQuery] int? take = null)
        {
            var query = _context.Fs.AsQueryable();

            if (take.HasValue && take.Value > 0)
            {
                query = query.Take(take.Value);
            }

            var result = await query.ToListAsync();
            return Ok(result);
        }

        [HttpGet("GetFsLnData")]
        public async Task<IActionResult> GetFsLnData([FromQuery] int? take = null)
        {
            var query = _context.FsLns.AsQueryable();

            if (take.HasValue && take.Value > 0)
            {
                query = query.Take(take.Value);
            }

            var result = await query.ToListAsync();
            return Ok(result);
        }

        [HttpGet("GetFsLnAcctData")]
        public async Task<IActionResult> GetFsLnAcctData([FromQuery] int? take = null)
        {
            var query = _context.FsLnAccts.AsQueryable();

            if (take.HasValue && take.Value > 0)
            {
                query = query.Take(take.Value);
            }

            var result = await query.ToListAsync();
            return Ok(result);
        }

        [HttpGet("GetGlPostDetail")]
        public async Task<IActionResult> GetGlPostDetail([FromQuery] int? take = null)
        {
            var query = _context.GlPostDetails.AsQueryable();

            if (take.HasValue && take.Value > 0)
            {
                query = query.Take(take.Value);
            }

            var result = await query.ToListAsync();
            return Ok(result);
        }

        [HttpGet("GetISData")]
        public async Task<IActionResult> GetISData([FromQuery] int? take = null)
        {
            var query = _context.View_Is_Report.AsQueryable();

            if (take.HasValue && take.Value > 0)
            {
                query = query.Take(take.Value);
            }

            var result = await query.ToListAsync();
            return Ok(result);
        }

        [HttpGet("GetAllEmployee")]
        public async Task<IActionResult> GetAllEmployee()
        {

            var sql = $@"SELECT empl.empl_id AS EmplId, 
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
            where public.empl_lab_info.end_dt = '2078-12-31'";

            var employeeDetails = _context.Empl_Master_Dto
                .FromSqlRaw(sql)
                .ToList();

            return Ok(employeeDetails);
        }
        [HttpGet("GetDbInfo")]
        public async Task<ActionResult<DbInfoViewModel>> GetDbInfo()
        {
            var dbInfo = new DbInfoViewModel();
            var model = _context.Model;

            var conn = _context.Database.GetDbConnection();
            await conn.OpenAsync();

            foreach (var entityType in model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (string.IsNullOrEmpty(tableName)) continue;

                var table = new TableInfoViewModel
                {
                    TableName = tableName
                };

                foreach (var property in entityType.GetProperties())
                {
                    table.Columns.Add(new ColumnInfoViewModel
                    {
                        ColumnName = property.GetColumnName(),
                        ColumnType = property.GetColumnType() ?? property.ClrType.Name,
                        IsNullable = property.IsColumnNullable(),
                        DefaultValue = property.GetDefaultValueSql()
                    });
                }

                try
                {
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = $"SELECT COUNT(*) FROM \"{tableName}\"";
                    var result = await cmd.ExecuteScalarAsync();
                    table.RowCount = Convert.ToInt32(result);
                }
                catch
                {
                    table.RowCount = 0; // ignore errors for unmapped/shadow tables
                }

                dbInfo.Tables.Add(table);
            }

            return Ok(dbInfo);
        }

        [HttpGet("GetDbInfoV1")]
        public async Task<ActionResult<DbInfoViewModel>> GetDbInfoV1()
        {
            var dbInfo = new DbInfoViewModel();
            //var model = _context.Model;
            var model = _context.GetService<IDesignTimeModel>().Model;

            var conn = _context.Database.GetDbConnection();
            await conn.OpenAsync();

            foreach (var entityType in model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                var schema = entityType.GetSchema();

                if (string.IsNullOrEmpty(tableName))
                    continue;

                var table = new TableInfoViewModel
                {
                    TableName = tableName,
                    Schema = schema
                };

                // =========================
                // 1️⃣ PRIMARY KEY
                // =========================
                var primaryKey = entityType.FindPrimaryKey();
                if (primaryKey != null)
                {
                    table.PrimaryKeys = primaryKey.Properties
                        .Select(p => p.GetColumnName())
                        .ToList();
                }

                // =========================
                // 2️⃣ COLUMNS
                // =========================
                foreach (var property in entityType.GetProperties())
                {
                    table.Columns.Add(new ColumnInfoViewModel
                    {
                        ColumnName = property.GetColumnName(),
                        Comment = property.GetComment() ?? string.Empty,
                        ColumnType = property.GetColumnType() ?? property.ClrType.Name,
                        IsNullable = property.IsColumnNullable(),
                        DefaultValue = property.GetDefaultValueSql(),
                        IsPrimaryKey = primaryKey?.Properties.Contains(property) ?? false
                    });
                }

                // =========================
                // 3️⃣ FOREIGN KEYS
                // =========================
                foreach (var fk in entityType.GetForeignKeys())
                {
                    table.ForeignKeys.Add(new ForeignKeyInfoViewModel
                    {
                        Name = fk.GetConstraintName() ?? "",
                        Columns = fk.Properties
                            .Select(p => p.GetColumnName())
                            .ToList(),
                        PrincipalTable = fk.PrincipalEntityType.GetTableName() ?? "",
                        PrincipalColumns = fk.PrincipalKey.Properties
                            .Select(p => p.GetColumnName())
                            .ToList()
                    });
                }

                // =========================
                // 4️⃣ INDEXES
                // =========================
                foreach (var index in entityType.GetIndexes())
                {
                    table.Indexes.Add(new IndexInfoViewModel
                    {
                        Name = index.GetDatabaseName() ?? "",
                        Columns = index.Properties
                            .Select(p => p.GetColumnName())
                            .ToList(),
                        IsUnique = index.IsUnique
                    });
                }

                // =========================
                // 5️⃣ ROW COUNT (Optional)
                // =========================
                try
                {
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = $"SELECT COUNT(*) FROM \"{tableName}\"";
                    var result = await cmd.ExecuteScalarAsync();
                    table.RowCount = Convert.ToInt32(result);
                }
                catch
                {
                    table.RowCount = 0;
                }

                dbInfo.Tables.Add(table);
            }

            return Ok(dbInfo);
        }

        [HttpPost("GetPerimetricValuesByProjIdsAsync")]
        public async Task<List<ParametricView>> GetPerimetricValuesByProjIdsAsync([FromBody] ProjIdsRequest request)
        {
            if (request.ProjIds == null || request.ProjIds.Count == 0)
                return new List<ParametricView>();

            // Filter using StartsWith for any of the projIds
            var query = _context.ParametricViews
                                .Where(p => request.ProjIds.Any(prefix => p.ProjId!.StartsWith(prefix)))
                                .AsQueryable();

            if (request.Take.HasValue)
                query = query.Take(request.Take.Value);

            return await query.ToListAsync();
        }

        [HttpGet("GetIncomeStatementReport")]
        public async Task<ActionResult<DbInfoViewModel>> GetIncomeStatementReport()
        {
            try
            {
                var data = await _context.IsRpt
                    .AsNoTracking()
                    .ToListAsync();
                return Ok(data);

            }
            catch(Exception ex)
            {
                return StatusCode(500, $"Report unavailable try after some time");
            }
        }

        [HttpGet("GetTrendReport")]
        public async Task<IActionResult> GetTrendReport(
        string? acctId = null,
        int? fyCd = null,
        int? take = null)
        {
            var query = _context.TrndBsRpts.AsQueryable();

            // Optional Account Filter
            if (!string.IsNullOrWhiteSpace(acctId))
            {
                query = query.Where(x => x.AcctId == acctId);
            }

            // Optional Fiscal Year Filter
            if (fyCd.HasValue)
            {
                query = query.Where(x => x.FyCd == fyCd.Value);
            }

            // Default Ordering
            query = query.OrderBy(x => x.AcctId);

            // Optional Take
            if (take.HasValue && take.Value > 0)
            {
                query = query.Take(take.Value);
            }

            var result = await query.ToListAsync();

            return Ok(result);
        }

        [HttpGet("GetForecastReport")]
        public async Task<IActionResult> GetForecastReport(
           [FromQuery] string? projId = null,
           [FromQuery] string? emplId = null,
           [FromQuery] string? fyCd = null,
           [FromQuery] int? take = null)
        {
            try
            {
                // Start with an open query against your view
                var query = _context.ForecastReportDatas.AsQueryable();

                // 1. Optional Project ID Filter
                if (!string.IsNullOrWhiteSpace(projId))
                {
                    query = query.Where(x => x.ProjId == projId);
                }

                // 2. Optional Employee ID Filter
                if (!string.IsNullOrWhiteSpace(emplId))
                {
                    query = query.Where(x => x.EmplId == emplId);
                }

                // 3. Optional Fiscal Year Code Filter
                if (!string.IsNullOrWhiteSpace(fyCd))
                {
                    query = query.Where(x => x.FyCd == fyCd);
                }

                // Default Ordering (e.g., ordering by Project ID then Period Number)
                query = query.OrderBy(x => x.ProjId).ThenBy(x => x.PdNo);

                // 4. Optional Record Limit (Pagination/Take)
                if (take.HasValue && take.Value > 0)
                {
                    query = query.Take(take.Value);
                }

                // Execute the built query against the database view asynchronously
                var result = await query.ToListAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpGet("GetVarianceReport")]
        public async Task<object> GetVarianceReport(
    int sourcePlId,
    int targetPlId,
    string projId)
        {
            var sourceData = await _context.PlForecasts
                .AsNoTracking()
                .Where(x =>
                    x.PlId == sourcePlId &&
                    x.ProjId == projId)
                .ToListAsync();

            var targetData = await _context.PlForecasts
                .AsNoTracking()
                .Where(x =>
                    x.PlId == targetPlId &&
                    x.ProjId == projId)
                .ToListAsync();

            var varianceReport = sourceData
                .GroupJoin(
                    targetData,
                    s => new
                    {
                        s.Year,
                        s.Month,
                        s.EmplId,
                        s.DctId,
                        s.AcctId
                    },
                    t => new
                    {
                        t.Year,
                        t.Month,
                        t.EmplId,
                        t.DctId,
                        t.AcctId
                    },
                    (s, targets) => new
                    {
                        Source = s,
                        Target = targets.FirstOrDefault()
                    })
                .Select(x => new
                {
                    x.Source.Year,
                    x.Source.Month,
                    x.Source.EmplId,
                    x.Source.DctId,
                    x.Source.AcctId,

                    SourceHours = x.Source.Forecastedhours,
                    TargetHours = x.Target?.Forecastedhours ?? 0,

                    HoursVariance =
                        x.Source.Forecastedhours -
                        (x.Target?.Forecastedhours ?? 0),

                    SourceCost = x.Source.ForecastedCost ?? 0,
                    TargetCost = x.Target?.ForecastedCost ?? 0,

                    CostVariance =
                        (x.Source.ForecastedCost ?? 0) -
                        (x.Target?.ForecastedCost ?? 0),

                    SourceRevenue = x.Source.Revenue,
                    TargetRevenue = x.Target?.Revenue ?? 0,

                    RevenueVariance =
                        x.Source.Revenue -
                        (x.Target?.Revenue ?? 0),

                    SourceBurden = x.Source.Burden,
                    TargetBurden = x.Target?.Burden ?? 0,

                    BurdenVariance =
                        x.Source.Burden -
                        (x.Target?.Burden ?? 0)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            return new
            {
                SourcePlan = sourcePlId,
                TargetPlan = targetPlId,
                ProjectId = projId,

                TotalSourceCost = varianceReport.Sum(x => x.SourceCost),
                TotalTargetCost = varianceReport.Sum(x => x.TargetCost),
                TotalCostVariance = varianceReport.Sum(x => x.CostVariance),

                TotalSourceRevenue = varianceReport.Sum(x => x.SourceRevenue),
                TotalTargetRevenue = varianceReport.Sum(x => x.TargetRevenue),
                TotalRevenueVariance = varianceReport.Sum(x => x.RevenueVariance),

                TotalSourceHours = varianceReport.Sum(x => x.SourceHours),
                TotalTargetHours = varianceReport.Sum(x => x.TargetHours),
                TotalHoursVariance = varianceReport.Sum(x => x.HoursVariance),

                Details = varianceReport
            };
        }

    }

}
