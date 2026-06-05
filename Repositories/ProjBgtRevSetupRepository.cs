using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using PlanningAPI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApi.Repositories;


public interface IProjBgtRevSetupRepository
{
    Task<IEnumerable<ProjBgtRevSetup>> GetAllAsync();
    Task<ProjBgtRevSetup> GetByIdAsync(int id);
    Task<ProjBgtRevSetup> AddAsync(ProjBgtRevSetup entity);
    Task<bool> UpdateAsync(ProjBgtRevSetup entity);
    Task<bool> DeleteAsync(int id);
    Task<ProjBgtRevSetup> GetByProjIdAsync(string projid, int Version, string BgtType);
    Task<ProjBgtRevSetup> GetByProjIdAsync(int pl_id);
    Task<ProjBgtRevSetup> UpsertAsync(ProjBgtRevSetup entity);
}
public class ProjBgtRevSetupRepository : IProjBgtRevSetupRepository
{
    private readonly MydatabaseContext _context;
    private readonly ILogger<ProjBgtRevSetupRepository> _logger;
    private readonly IConfiguration _config;

    public ProjBgtRevSetupRepository(MydatabaseContext context, ILogger<ProjBgtRevSetupRepository> logger, IConfiguration config)
    {
        _context = context;
        _logger = logger;
        _config = config;
    }


    public async Task<ProjBgtRevSetup> UpsertAsync(ProjBgtRevSetup entity)
    {
        try
        {
            var existing = await _context.ProjBgtRevSetups.FindAsync(entity.Id);
            if (existing == null)
            {
                _context.ProjBgtRevSetups.Add(entity);
            }
            else
            {
                _context.Entry(existing).CurrentValues.SetValues(entity);
            }

            await _context.SaveChangesAsync();
            PlForecastRepository plForecastRepository = new PlForecastRepository(_context, _config);
            try
            {
                var existing_plan = _context.PlProjectPlans.FirstOrDefault(p => p.PlId == entity.PlId);
                if (existing_plan != null)
                    await plForecastRepository.CalculateRevenueCost(existing_plan.PlId.GetValueOrDefault(), existing_plan.TemplateId.GetValueOrDefault(), existing_plan.PlType);
            }
            catch (Exception ex)
            {
                //logger.LogError(ex, "Error Calculation Revenue for {PlanId}", result.PlId);
            }
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpsertAsync for ProjBgtRevSetup");
            throw;
        }
    }

    public async Task<IEnumerable<ProjBgtRevSetup>> GetAllAsync()
    {
        try
        {
            return await _context.ProjBgtRevSetups.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving all ProjBgtRevSetup records.");
            throw;
        }
    }

    public async Task<ProjBgtRevSetup> GetByIdAsync(int id)
    {
        try
        {
            return await _context.ProjBgtRevSetups.FindAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving ProjBgtRevSetup with ID {Id}.", id);
            throw;
        }
    }

    public async Task<ProjBgtRevSetup> AddAsync(ProjBgtRevSetup entity)
    {
        try
        {
            _context.ProjBgtRevSetups.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while adding a new ProjBgtRevSetup.");
            throw;
        }
    }

    public async Task<bool> UpdateAsync(ProjBgtRevSetup entity)
    {
        try
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency conflict while updating ProjBgtRevSetup with ID {Id}.", entity.Id);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating ProjBgtRevSetup with ID {Id}.", entity.Id);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var entity = await _context.ProjBgtRevSetups.FindAsync(id);
            if (entity == null)
            {
                _logger.LogWarning("Delete failed. ProjBgtRevSetup with ID {Id} not found.", id);
                return false;
            }

            _context.ProjBgtRevSetups.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting ProjBgtRevSetup with ID {Id}.", id);
            return false;
        }
    }

    public async Task<ProjBgtRevSetup> GetByProjIdAsync(string projid, int Version, string BgtType)
    {
        try
        {
            return await _context.ProjBgtRevSetups.FirstOrDefaultAsync(p => p.ProjId == projid && p.VersionNo == Version && p.BgtType == BgtType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving ProjBgtRevSetup with ProjectID - {projid}, BudgetType - {BgtType} and Version - {Version} .", projid, BgtType, Version);
            throw;
        }
    }

    public async Task<ProjBgtRevSetup> GetByProjIdAsync(int pl_id)
    {
        try
        {
            var result = await _context.ProjBgtRevSetups.FirstOrDefaultAsync(p => p.PlId == pl_id);

            var NBBud = _context.NewBusinessBudgets.FirstOrDefault(p => p.BusinessBudgetId == result.ProjId);
            result.BgtType = NBBud.NBType;
            return result;
        }
        catch (Exception ex)
        {
            //_logger.LogError(ex, "An error occurred while retrieving ProjBgtRevSetup with ProjectID - {projid}, BudgetType - {BgtType} and Version - {Version} .", projid, BgtType, Version);
            throw ex;
        }
    }
}
