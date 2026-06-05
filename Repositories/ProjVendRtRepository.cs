using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using PlanningAPI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IProjVendRtRepository
{
    Task<IEnumerable<ProjVendRt>> GetAllAsync();
    Task<ProjVendRt> GetByIdAsync(int id);
    Task<ProjVendRt> AddAsync(ProjVendRt entity);
    Task<bool> UpdateAsync(ProjVendRt entity);
    Task<bool> DeleteAsync(int id);
}
public class ProjVendRtRepository : IProjVendRtRepository
{
    private readonly MydatabaseContext _context;
    private readonly ILogger<ProjVendRtRepository> _logger;

    public ProjVendRtRepository(MydatabaseContext context, ILogger<ProjVendRtRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<ProjVendRt>> GetAllAsync()
    {
        try
        {
            var result = _context.ProjVendRts
                .Join(_context.VendorEmployees,
                      rt => rt.VendEmplId,              // 👈 join on VendEmplId instead of VendId
                      v => v.VendEmplId,                // 👈 match VendorEmployees.VendEmplId
                      (rt, v) => new { rt, v })
                .Join(_context.PlcCodes,
                      combined => combined.rt.BillLabCatCd,
                      plc => plc.LaborCategoryCode,
                      (combined, plc) => new ProjVendRt
                      {
                          ProjId = combined.rt.ProjId,
                          BillRtAmt = combined.rt.BillRtAmt,
                          vendEmplName = combined.v.VendEmplName,
                          StartDt = combined.rt.StartDt,
                          EndDt = combined.rt.EndDt,
                          VendId = combined.rt.VendId,
                          BillLabCatCd = combined.rt.BillLabCatCd,
                          BillDiscRt = combined.rt.BillDiscRt,
                          ProjVendRtKey = combined.rt.ProjVendRtKey,
                          Type = combined.rt.Type,
                          SBillRtTypeCd = combined.rt.SBillRtTypeCd,
                          CompanyId = combined.rt.CompanyId,
                          VendEmplId = combined.rt.VendEmplId,
                          PlcDescription = plc.Description
                      })
                .Distinct()
                .ToList();

            //var result = _context.ProjVendRts
            //    .Join(_context.VendorEmployees,
            //          rt => rt.VendId,
            //          v => v.VendId,
            //          (rt, v) => new { rt, v })
            //    .Join(_context.PlcCodes,
            //          combined => combined.rt.BillLabCatCd,
            //          plc => plc.LaborCategoryCode,
            //          (combined, plc) => new ProjVendRt
            //          {
            //              ProjId = combined.rt.ProjId,
            //              BillRtAmt = combined.rt.BillRtAmt,
            //              vendEmplName = combined.v.VendEmplName,
            //              StartDt = combined.rt.StartDt,
            //              EndDt = combined.rt.EndDt,
            //              VendId = combined.rt.VendId,
            //              BillLabCatCd = combined.rt.BillLabCatCd,
            //              BillDiscRt = combined.rt.BillDiscRt,
            //              ProjVendRtKey = combined.rt.ProjVendRtKey,
            //              Type = combined.rt.Type,
            //              SBillRtTypeCd = combined.rt.SBillRtTypeCd,
            //              CompanyId = combined.rt.CompanyId,
            //              VendEmplId = combined.rt.VendEmplId,
            //              PlcDescription = plc.Description // Add this property to your model
            //          }).Distinct()
            //    .ToList();

            return result;// await _context.ProjVendRts.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all ProjVendRt records.");
            throw;
        }
    }

    public async Task<ProjVendRt> GetByIdAsync(int id)
    {
        try
        {
            return await _context.ProjVendRts.FindAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ProjVendRt with ID {Id}", id);
            throw;
        }
    }

    public async Task<ProjVendRt> AddAsync(ProjVendRt entity)
    {
        try
        {
            _context.ProjVendRts.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23514")
        {
            throw new InvalidOperationException("Date validation failed: Start date must be before or equal to end date.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding ProjVendRt.");
            throw;
        }
    }

    public async Task<bool> UpdateAsync(ProjVendRt entity)
    {
        try
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23514")
        {
            throw new InvalidOperationException("Date validation failed: Start date must be before or equal to end date.", ex);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "Concurrency error updating ProjVendRt with ID {Id}", entity.ProjVendRtKey);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ProjVendRt with ID {Id}", entity.ProjVendRtKey);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var entity = await _context.ProjVendRts.FindAsync(id);
            if (entity == null)
            {
                _logger.LogWarning("ProjVendRt with ID {Id} not found for deletion.", id);
                return false;
            }

            _context.ProjVendRts.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting ProjVendRt with ID {Id}", id);
            return false;
        }
    }
}

