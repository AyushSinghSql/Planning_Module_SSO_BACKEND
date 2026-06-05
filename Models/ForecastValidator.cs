using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Protocol;
using RTools_NTS.Util;
using System.Diagnostics;
using WebApi.Controllers;

namespace PlanningAPI.Models
{
    public class ForecastValidator
    {
        private readonly MydatabaseContext _context;
        private readonly ILogger<ForecastController> _logger;

        public ForecastValidator(MydatabaseContext context, ILogger<ForecastController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ValidateForecastsAsync(List<PlForecast> forecasts)
        {
            if (forecasts == null || forecasts.Count == 0)
                return;
            var planid = forecasts.First().PlId;
            var stopwatch = Stopwatch.StartNew();

            var EmployeeHours = await (
                from f in _context.PlForecasts
                join pp in _context.PlProjectPlans on f.PlId equals pp.PlId
                where forecasts.Select(p => p.EmplId).Contains(f.EmplId) && f.Year == 2025
                      && (pp.FinalVersion == true || pp.PlId == planid) && f.Emple.Type.ToUpper() == "EMPLOYEE"
                select new
                {
                    f.EmplId,
                    f.Month,
                    f.Year,
                    Hours =
                        pp.PlId == planid && pp.PlType == "EAC" ? f.Actualhours :
                        pp.PlId == planid && pp.PlType == "BUD" ? f.Forecastedhours :
                        pp.PlId != planid && pp.PlType == "EAC" && pp.FinalVersion == true ? f.Actualhours :
                        pp.PlId != planid && pp.PlType == "BUD" && pp.FinalVersion == true &&
                        !_context.PlProjectPlans.Any(pp2 => pp2.ProjId == pp.ProjId && pp2.PlType == "EAC" && pp2.FinalVersion == true)
                            ? f.Forecastedhours : 0
                }).ToListAsync();

            var TotalEMployeeHours = EmployeeHours
                .GroupBy(f => new { f.EmplId, f.Month, f.Year })
                .Select(g => new PlForecast
                {
                    EmplId = g.Key.EmplId,
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    Forecastedhours = g.Sum(x => x.Hours),
                })
                .ToList();


            var years = forecasts.Select(f => f.Year).Distinct().ToList();
            var months = forecasts.Select(f => f.Month).Distinct().ToList();
            var plIds = forecasts.Select(f => f.PlId).Distinct().ToList();
            var emplIds = forecasts.Select(f => f.EmplId).Distinct().ToList();

            // ✅ Preload data efficiently
            var schedules = await _context.Schedules
                .Where(s => years.Contains(s.Year) && months.Contains(s.MonthNo))
                .ToDictionaryAsync(s => (s.Year, s.MonthNo), s => s.WorkingHours);

            var planTypes = await _context.PlProjectPlans
                .Where(p => plIds.Contains(p.PlId.GetValueOrDefault()))
                .ToDictionaryAsync(p => p.PlId, p => p.PlType);

            // ✅ Load existing warnings WITHOUT tracking (important!)
            var existingWarnings = await _context.PLWarnings
                .AsNoTracking() // 👈 prevents tracking conflict
                .Where(w => plIds.Contains(w.PlId)
                         && emplIds.Contains(w.EmplId)
                         && years.Contains(w.Year)
                         && months.Contains(w.Month))
                .ToListAsync();

            var newWarnings = new List<PlWarning>();
            var warningsToDelete = new List<PlWarning>();

            foreach (var forecast in forecasts)
            {
                var key = (forecast.Year, forecast.Month);
                if (!schedules.TryGetValue(key, out var schedHours))
                {
                    _logger.LogInformation("No schedule defined for {Year}/{Month}", forecast.Year, forecast.Month);
                    continue;
                }

                planTypes.TryGetValue(forecast.PlId, out var planType);
                var hoursToCheck = planType == "EAC" ? forecast.Actualhours : forecast.Forecastedhours;
                var monthName = new DateTime(forecast.Year, forecast.Month, 1).ToString("MMMM");

                // ⚠️ Check 1: Project-level hours exceed schedule
                if (hoursToCheck > schedHours)
                {
                    var warningText = $"Assigned hours ({hoursToCheck}) exceed the standard limit ({schedHours}) for {monthName} {forecast.Year}.";
                    UpsertWarning(existingWarnings, newWarnings, forecast, warningText, false);
                }
                else
                {
                    MarkWarningForDeletion(existingWarnings, warningsToDelete, forecast, false);
                }

                // ⚠️ Check 2: Total monthly hours across multiple projects
                //var totalEmployeeHours = await (
                //    from f in _context.PlForecasts
                //    join pp in _context.PlProjectPlans on f.PlId equals pp.PlId
                //    where f.EmplId == forecast.EmplId
                //          && f.Year == forecast.Year
                //          && f.Month == forecast.Month
                //          && (pp.FinalVersion == true || pp.PlId == forecast.PlId)
                //    select new
                //    {
                //        Hours =
                //            pp.PlId == forecast.PlId && pp.PlType == "EAC" ? f.Actualhours :
                //            pp.PlId == forecast.PlId && pp.PlType == "BUD" ? f.Forecastedhours :
                //            pp.PlId != forecast.PlId && pp.PlType == "EAC" && pp.FinalVersion == true ? f.Actualhours :
                //            pp.PlId != forecast.PlId && pp.PlType == "BUD" && pp.FinalVersion == true &&
                //            !_context.PlProjectPlans.Any(pp2 => pp2.ProjId == pp.ProjId && pp2.PlType == "EAC" && pp2.FinalVersion == true)
                //                ? f.Forecastedhours : 0
                //    })
                //    .SumAsync(x => x.Hours);

                var totalEmployeeHours = TotalEMployeeHours
                    .Where(teh => teh.EmplId == forecast.EmplId && teh.Year == forecast.Year && teh.Month == forecast.Month)
                    .Sum(teh => teh.Forecastedhours);
                if (totalEmployeeHours > schedHours)
                {
                    var warningText = $"Assigned total hours ({totalEmployeeHours}) exceed the limit ({schedHours}) for {monthName} {forecast.Year} across multiple projects for employee {forecast.EmplId}.";
                    UpsertWarning(existingWarnings, newWarnings, forecast, warningText, true);
                }
                else
                {
                    MarkWarningForDeletion(existingWarnings, warningsToDelete, forecast, true);
                }
            }

            // ✅ Safe apply of changes — prevent EF tracking conflicts
            foreach (var entry in _context.ChangeTracker.Entries<PlWarning>().ToList())
                entry.State = EntityState.Detached;

            //using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (warningsToDelete.Any())
                    _context.BulkDeleteAsync(warningsToDelete);
                    //_context.PLWarnings.RemoveRange(warningsToDelete);

                if (newWarnings.Any())
                {
                    // Deduplicate new warnings in-memory (avoid tracking duplicates)
                    newWarnings = newWarnings
                        .GroupBy(w => new { w.PlId, w.ProjId, w.EmplId, w.Year, w.Month, w.MultipleProjects })
                        .Select(g => g.First())
                        .ToList();

                    // Load existing keys (no tracking)
                    var existingKeys = await _context.PLWarnings
                        .AsNoTracking()
                        .Select(w => new
                        {
                            w.PlId,
                            w.ProjId,
                            w.EmplId,
                            w.Year,
                            w.Month,
                            w.MultipleProjects
                        })
                        .ToListAsync();

                    // Separate what to add vs update
                    var newToAdd = newWarnings
                        .Where(nw => !existingKeys.Any(ek =>
                            ek.PlId == nw.PlId &&
                            ek.ProjId == nw.ProjId &&
                            ek.EmplId == nw.EmplId &&
                            ek.Year == nw.Year &&
                            ek.Month == nw.Month &&
                            ek.MultipleProjects == nw.MultipleProjects))
                        .ToList();

                    var toUpdate = newWarnings
                        .Where(nw => existingKeys.Any(ek =>
                            ek.PlId == nw.PlId &&
                            ek.ProjId == nw.ProjId &&
                            ek.EmplId == nw.EmplId &&
                            ek.Year == nw.Year &&
                            ek.Month == nw.Month &&
                            ek.MultipleProjects == nw.MultipleProjects))
                        .ToList();

                    // Detach any already-tracked duplicates before adding
                    foreach (var warn in newToAdd)
                    {
                        var local = _context.PLWarnings.Local.FirstOrDefault(l =>
                            l.PlId == warn.PlId &&
                            l.ProjId == warn.ProjId &&
                            l.EmplId == warn.EmplId &&
                            l.Year == warn.Year &&
                            l.Month == warn.Month &&
                            l.MultipleProjects == warn.MultipleProjects);

                        if (local != null)
                            _context.Entry(local).State = EntityState.Detached;
                    }

                    // ✅ Add only truly new
                    if (newToAdd.Any())
                        await _context.BulkInsertAsync(newToAdd);
                        //await _context.PLWarnings.AddRangeAsync(newToAdd);

                    // ✅ Attach updates safely
                    foreach (var warn in toUpdate)
                    {
                        var tracked = _context.PLWarnings.Local.FirstOrDefault(l =>
                            l.PlId == warn.PlId &&
                            l.ProjId == warn.ProjId &&
                            l.EmplId == warn.EmplId &&
                            l.Year == warn.Year &&
                            l.Month == warn.Month &&
                            l.MultipleProjects == warn.MultipleProjects);

                        if (tracked == null)
                            _context.PLWarnings.Attach(warn);

                        _context.Entry(warn).State = EntityState.Modified;
                    }
                }


                await _context.SaveChangesAsync();

                //await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                //await transaction.RollbackAsync();
                _logger.LogError(ex, "Error while saving forecast warnings");
            }

            stopwatch.Stop();
            _logger.LogInformation(
                "Forecast validation completed in {ElapsedMs} ms. Added: {Added}, Deleted: {Deleted}",
                stopwatch.ElapsedMilliseconds, newWarnings.Count, warningsToDelete.Count
            );
        }


        private void UpsertWarning(List<PlWarning> existingWarnings, List<PlWarning> newWarnings, PlForecast forecast, string text, bool multiple)
        {
            var existing = existingWarnings.FirstOrDefault(w =>
                w.PlId == forecast.PlId &&
                w.ProjId == forecast.ProjId &&
                w.EmplId == forecast.EmplId &&
                w.Year == forecast.Year &&
                w.Month == forecast.Month &&
                w.MultipleProjects == multiple);

            if (existing != null)
            {
                existing.Warning = text;
                existing.LastUpdated = DateTime.UtcNow;
            }
            else
            {
                newWarnings.Add(new PlWarning
                {
                    PlId = forecast.PlId,
                    ProjId = forecast.ProjId,
                    EmplId = forecast.EmplId,
                    Year = forecast.Year,
                    Month = forecast.Month,
                    Warning = text,
                    MultipleProjects = multiple,
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow
                });
            }
        }

        private void MarkWarningForDeletion(List<PlWarning> existingWarnings, List<PlWarning> toDelete, PlForecast forecast, bool multiple)
        {
            var existing = existingWarnings.FirstOrDefault(w =>
                w.PlId == forecast.PlId &&
                w.ProjId == forecast.ProjId &&
                w.EmplId == forecast.EmplId &&
                w.Year == forecast.Year &&
                w.Month == forecast.Month &&
                w.MultipleProjects == multiple);

            if (existing != null)
                toDelete.Add(existing);
        }
    }
}
