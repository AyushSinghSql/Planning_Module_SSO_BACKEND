using System;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;
using WebApi.DTO;

namespace PlanningAPI.Repositories
{
    public interface IProjRevWrkPdRepository
    {
        Task<IEnumerable<ProjRevWrkPd>> GetAllAsync();
        Task<ProjRevWrkPd> GetByIdAsync(int id);
        Task<ProjRevWrkPd> AddAsync(ProjRevWrkPd entity);
        Task<bool> UpdateAsync(ProjRevWrkPd entity);
        Task<bool> DeleteAsync(int id);
        Task<ProjRevWrkPd> UpsertAsync(ProjRevWrkPd entity);
        Task<IEnumerable<ProjRevWrkPd>> GetByFilterAsync(string projId = null, int? versionNo = null, string bgtType = null);
        Task<IEnumerable<ProjRevWrkPd>> GetByFilterAsync(string projId = null, int? versionNo = null, string bgtType = null, int? pl_id = null);
        void UpdateActualRevenue(string projId = null, int? versionNo = null, string bgtType = null);
        Task AddRevenueForNBAsync(List<NB_Revenue> entity, int pl_id, string proj_id);
    }
    public class ProjRevWrkPdRepository : IProjRevWrkPdRepository
    {
        private readonly MydatabaseContext _context;
        private readonly ILogger<ProjRevWrkPdRepository> _logger;

        public ProjRevWrkPdRepository(MydatabaseContext context, ILogger<ProjRevWrkPdRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<IEnumerable<ProjRevWrkPd>> GetByFilterAsync(string projId = null, int? versionNo = null, string bgtType = null)
        {
            ProjectRevenueAdjustment projectRevenueAdjustment = new ProjectRevenueAdjustment();
            try
            {
                var query = _context.ProjRevWrkPds.AsQueryable();

                if (!string.IsNullOrEmpty(projId))
                    query = query.Where(p => p.ProjId == projId);

                if (versionNo.HasValue)
                    query = query.Where(p => p.VersionNo == versionNo);

                if (!string.IsNullOrEmpty(bgtType))
                    query = query.Where(p => p.BgtType == bgtType);

                var allPds = await query.ToListAsync();

                foreach (var pd in allPds)
                {
                    pd.Fy_Cd = pd.EndDate.GetValueOrDefault().Year;
                }

                if (allPds.Count() == 0)
                {
                    var project = _context.PlProjects.FirstOrDefault(p => p.ProjId == projId);

                    if (project != null)
                    {
                        //var psrData = _context.PSRFinalData.Where(p => p.ProjId == projId && p.SubTotTypeNo == 1 && p.RateType == "A").ToList();
                        var psrData = _context.PSRFinalData.Where(p => p.ProjId.StartsWith(projId) && p.SubTotTypeNo == 1 && p.RateType == "A").ToList();
                        ScheduleHelper helper = new ScheduleHelper();
                        var months = helper.GetMonthsBetween(project.ProjStartDt.GetValueOrDefault(), project.ProjEndDt.GetValueOrDefault());

                        var AdjustmentData = await _context.Set<ProjectRevenueAdjustment>()
                                    .AsNoTracking()
                                    .Where(x => x.ProjId.StartsWith(projId))
                                    .ToListAsync();

                        foreach (var (year, month) in months)
                        {
                            projectRevenueAdjustment = new ProjectRevenueAdjustment();
                            decimal revData = 0;
                            if (psrData.FirstOrDefault(p => p.FyCd == year.ToString() && p.PdNo == month) != null)
                                revData = psrData.FirstOrDefault(p => p.FyCd == year.ToString() && p.PdNo == month).PtdIncurAmt;

                            if (AdjustmentData.FirstOrDefault(p => p.FyCd == year.ToString() && p.PdNo == month) != null)
                                projectRevenueAdjustment = AdjustmentData.FirstOrDefault(p => p.FyCd == year.ToString() && p.PdNo == month);


                            var dateTime = new DateOnly(year, month, 1);
                            allPds.Add(new ProjRevWrkPd() { Period = month, Fy_Cd = year, EndDate = dateTime, RevDesc = projectRevenueAdjustment.RevAdjDesc ?? string.Empty, RevAdj = projectRevenueAdjustment.RevAdjAmt ?? 0, RevAmt = revData, ProjId = projId, VersionNo = versionNo, BgtType = bgtType });
                        }

                        _context.ProjRevWrkPds.AddRange(allPds);
                        await _context.SaveChangesAsync();
                        allPds = await query.ToListAsync();
                    }
                }
                else
                {
                    var AdjustmentData = await _context.Set<ProjectRevenueAdjustment>()
                                  .AsNoTracking()
                                  .Where(x => x.ProjId.StartsWith(projId))
                                  .ToListAsync();
                    foreach (var pd in allPds)
                    {
                        pd.Fy_Cd = pd.EndDate.GetValueOrDefault().Year;
                        if (AdjustmentData.FirstOrDefault(p => p.FyCd == pd.EndDate.GetValueOrDefault().Year.ToString() && p.PdNo == pd.EndDate.GetValueOrDefault().Month) != null)
                            projectRevenueAdjustment = AdjustmentData.FirstOrDefault(p => p.FyCd == pd.EndDate.GetValueOrDefault().Year.ToString() && p.PdNo == pd.EndDate.GetValueOrDefault().Month);
                        
                        if (projectRevenueAdjustment != null)
                        {
                            pd.RevAdj = projectRevenueAdjustment.RevAdjAmt ?? 0;
                            pd.RevDesc = projectRevenueAdjustment.RevAdjDesc ?? string.Empty;
                        }
                    }
                    await _context.SaveChangesAsync();
                }


                return allPds;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByFilterAsync (ProjId, VersionNo, BgtType)");
                throw;
            }
        }

        public async Task<IEnumerable<ProjRevWrkPd>> GetByFilterAsync(string projId = null, int? versionNo = null, string bgtType = null, int? pl_id = null)
        {
            ProjectRevenueAdjustment projectRevenueAdjustment = new ProjectRevenueAdjustment();
            try
            {
                var query = _context.ProjRevWrkPds.AsQueryable();

                if (!string.IsNullOrEmpty(projId))
                    query = query.Where(p => p.ProjId == projId);

                query = query.Where(p => p.Pl_Id == pl_id);

                var allPds = await query.ToListAsync();

                foreach (var pd in allPds)
                {
                    pd.Fy_Cd = pd.EndDate.GetValueOrDefault().Year;
                }

                if (allPds.Count() == 0)
                {
                    var project = _context.PlProjects.FirstOrDefault(p => p.ProjId == projId);
                    var projectPlan = _context.PlProjectPlans.FirstOrDefault(p => p.PlId == pl_id);


                    if (project != null)
                    {
                        var psrData = _context.PSRFinalData.Where(p => p.ProjId.StartsWith(projId) && p.SubTotTypeNo == 1 && p.RateType == "A").ToList();
                        ScheduleHelper helper = new ScheduleHelper();

                        //Commented to get start date and end date of Budget
                        //var months = helper.GetMonthsBetween(project.ProjStartDt.GetValueOrDefault(), project.ProjEndDt.GetValueOrDefault());
                        var months = helper.GetMonthsBetween(projectPlan.ProjStartDt.GetValueOrDefault(), projectPlan.ProjEndDt.GetValueOrDefault());


                        var AdjustmentData = await _context.Set<ProjectRevenueAdjustment>()
                                    .AsNoTracking()
                                    .Where(x => x.ProjId.StartsWith(projId))
                                    .ToListAsync();

                        foreach (var (year, month) in months)
                        {
                            projectRevenueAdjustment = new ProjectRevenueAdjustment();
                            decimal revData = 0;
                            if (psrData.FirstOrDefault(p => p.FyCd == year.ToString() && p.PdNo == month) != null)
                                revData = psrData.FirstOrDefault(p => p.FyCd == year.ToString() && p.PdNo == month).PtdIncurAmt;

                            if (AdjustmentData.FirstOrDefault(p => p.FyCd == year.ToString() && p.PdNo == month) != null)
                                projectRevenueAdjustment = AdjustmentData.FirstOrDefault(p => p.FyCd == year.ToString() && p.PdNo == month);


                            //var dateTime = new DateOnly(year, month, 1);
                            var dateTime = new DateOnly(year, month, DateTime.DaysInMonth(year, month));

                            allPds.Add(new ProjRevWrkPd() { Pl_Id = pl_id??0, Period = month, Fy_Cd = year, EndDate = dateTime, RevDesc = projectRevenueAdjustment.RevAdjDesc ?? string.Empty, RevAdj = projectRevenueAdjustment.RevAdjAmt ?? 0, RevAmt = revData, ProjId = projId, VersionNo = versionNo, BgtType = bgtType, RevAdj1 = 0 });
                        }


                        _context.ProjRevWrkPds.AddRange(allPds);
                        await _context.SaveChangesAsync();
                        allPds = await query.ToListAsync();
                    }
                }
                else
                {
                    var AdjustmentData = await _context.Set<ProjectRevenueAdjustment>()
                                  .AsNoTracking()
                                  .Where(x => x.ProjId.StartsWith(projId))
                                  .ToListAsync();
                    foreach (var pd in allPds)
                    {
                        pd.EndDate = new DateOnly(pd.EndDate.GetValueOrDefault().Year, pd.EndDate.GetValueOrDefault().Month, DateTime.DaysInMonth(pd.EndDate.GetValueOrDefault().Year, pd.EndDate.GetValueOrDefault().Month));

                        projectRevenueAdjustment = new ProjectRevenueAdjustment();
                        pd.Fy_Cd = pd.EndDate.GetValueOrDefault().Year;

                        ////Commented for Unannet
                        //if (AdjustmentData.FirstOrDefault(p => p.FyCd == pd.EndDate.GetValueOrDefault().Year.ToString() && p.PdNo == pd.EndDate.GetValueOrDefault().Month) != null)
                        //    projectRevenueAdjustment = AdjustmentData.FirstOrDefault(p => p.FyCd == pd.EndDate.GetValueOrDefault().Year.ToString() && p.PdNo == pd.EndDate.GetValueOrDefault().Month);

                        projectRevenueAdjustment = AdjustmentData
                            .Where(p =>
                                p.FyCd == pd.EndDate.GetValueOrDefault().Year.ToString() &&
                                p.PdNo == pd.EndDate.GetValueOrDefault().Month)
                            .GroupBy(p => new
                            {
                                p.FyCd,
                                p.PdNo
                            })
                            .Select(g => new ProjectRevenueAdjustment
                            {
                                FyCd = g.Key.FyCd,
                                PdNo = g.Key.PdNo,
                                RevAdjAmt = g.Sum(x => x.RevAdjAmt),
                                RevAwdAdjAmt = g.Sum(x => x.RevAwdAdjAmt)
                            })
                            .FirstOrDefault();

                        if (projectRevenueAdjustment != null)
                        {
                            pd.RevAdj = projectRevenueAdjustment.RevAdjAmt ?? 0;
                            pd.RevDesc = projectRevenueAdjustment.RevAdjDesc ?? string.Empty;
                        }
                    }
                    await _context.SaveChangesAsync();
                }


                return allPds;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByFilterAsync (ProjId, VersionNo, BgtType)");
                throw;
            }
        }

        public async void UpdateActualRevenue(string projId = null, int? versionNo = null, string bgtType = null)
        {
            try
            {
                var query = _context.ProjRevWrkPds.AsQueryable();

                if (!string.IsNullOrEmpty(projId))
                    query = query.Where(p => p.ProjId == projId);

                if (versionNo.HasValue)
                    query = query.Where(p => p.VersionNo == versionNo);

                if (!string.IsNullOrEmpty(bgtType))
                    query = query.Where(p => p.BgtType == bgtType);

                var allPds = await query.ToListAsync();

                foreach (var pd in allPds)
                {
                    pd.Fy_Cd = pd.EndDate.GetValueOrDefault().Year;
                }

                var project = _context.PlProjects.FirstOrDefault(p => p.ProjId == projId);

                if (project != null)
                {
                    var psrData = _context.PSRFinalData.Where(p => p.ProjId == projId && p.SubTotTypeNo == 1 && p.RateType == "A").ToList();
                    ScheduleHelper helper = new ScheduleHelper();
                    var months = helper.GetMonthsBetween(project.ProjStartDt.GetValueOrDefault(), project.ProjEndDt.GetValueOrDefault());

                    foreach (var (year, month) in months)
                    {
                        decimal revData = 0;
                        if (psrData.FirstOrDefault(p => p.FyCd == year.ToString() && p.PdNo == month) != null)
                            revData = psrData.FirstOrDefault(p => p.FyCd == year.ToString() && p.PdNo == month).PtdIncurAmt;
                        var dateTime = new DateOnly(year, month, 1);
                        allPds.Add(new ProjRevWrkPd() { Period = month, Fy_Cd = year, EndDate = dateTime, RevAdj = 0, RevAmt = revData, ProjId = projId, VersionNo = versionNo, BgtType = bgtType });
                    }

                    _context.ProjRevWrkPds.AddRange(allPds);
                    await _context.SaveChangesAsync();
                    allPds = await query.ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByFilterAsync (ProjId, VersionNo, BgtType)");
                throw;
            }
        }

        public async Task<IEnumerable<ProjRevWrkPd>> GetAllAsync()
        {
            try
            {
                var allPds = await _context.ProjRevWrkPds.ToListAsync();
                foreach (var pd in allPds)
                {
                    pd.Fy_Cd = pd.EndDate.GetValueOrDefault().Year;
                }
                return allPds;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all ProjRevWrkPds");
                throw;
            }
        }

        public async Task<ProjRevWrkPd> GetByIdAsync(int id)
        {
            try
            {
                return await _context.ProjRevWrkPds.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving ProjRevWrkPd by id: {id}");
                throw;
            }
        }

        public async Task<ProjRevWrkPd> AddAsync(ProjRevWrkPd entity)
        {
            try
            {
                entity.CreatedAt = DateTime.UtcNow;
                entity.TimeStamp = DateTime.UtcNow;
                _context.ProjRevWrkPds.Add(entity);
                await _context.SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding ProjRevWrkPd");
                throw;
            }
        }
        public async Task<ProjRevWrkPd> UpsertAsync(ProjRevWrkPd entity)
        {
            try
            {
                ProjRevWrkPd existing = null;

                if (entity.Id != 0)
                {
                    existing = await _context.ProjRevWrkPds.FindAsync(entity.Id);
                }

                if (existing == null)
                {
                    // New entry
                    entity.EndDate = entity.EndDate.GetValueOrDefault();
                    entity.CreatedAt = DateTime.UtcNow;
                    entity.TimeStamp = DateTime.UtcNow;
                    _context.ProjRevWrkPds.Add(entity);
                }
                else
                {
                    // Only update selected fields
                    existing.RevAmt = entity.RevAmt;
                    existing.RevAdj1 = entity.RevAdj1;
                    existing.RevDesc = entity.RevDesc;
                    existing.TimeStamp = DateTime.UtcNow;
                    existing.ModifiedBy = entity.ModifiedBy; // Optional if provided
                }

                await _context.SaveChangesAsync();
                return existing ?? entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during UpsertAsync for ProjRevWrkPd (RevAmt/RevAdj only)");
                throw;
            }
        }

        public async Task<bool> UpdateAsync(ProjRevWrkPd entity)
        {
            try
            {
                var existing = await _context.ProjRevWrkPds.FindAsync(entity.Id);
                if (existing == null) return false;

                _context.Entry(existing).CurrentValues.SetValues(entity);
                existing.TimeStamp = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating ProjRevWrkPd with id {entity.Id}");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var entity = await _context.ProjRevWrkPds.FindAsync(id);
                if (entity == null) return false;

                _context.ProjRevWrkPds.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting ProjRevWrkPd with id {id}");
                throw;
            }
        }

        public Task AddRevenueForNBAsync(List<NB_Revenue> entity, int pl_id, string proj_id)
        {
           foreach(var rev in entity)
            {
                var dateTime = new DateOnly(rev.Fy_Cd.GetValueOrDefault(), rev.Period.GetValueOrDefault(), DateTime.DaysInMonth(rev.Fy_Cd.GetValueOrDefault(), rev.Period.GetValueOrDefault()));
                _context.ProjRevWrkPds.Add(new ProjRevWrkPd() { Pl_Id = pl_id, Period = rev.Period.GetValueOrDefault(), Fy_Cd = rev.Fy_Cd.GetValueOrDefault(), EndDate = dateTime, RevDesc = "" ?? string.Empty, RevAdj = 0, RevAmt = rev.RevAmt, ProjId = proj_id, VersionNo = 0, BgtType = "" });

            }
            return _context.SaveChangesAsync();
        }
    }

}
