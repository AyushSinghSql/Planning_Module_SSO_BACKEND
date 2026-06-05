namespace WebApi.Repositories;

using Dapper;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using PlanningAPI.Models;
using WebApi.DTO;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models;

public interface IDirectCostRepository
{
    Task<List<PlDct>> GetAllDirectCosts();
    //Task<List<PlDct>> GetAllDirectCostsByProject(string projId);

    Task<PlDct> AddNewDirectCostAsync(PlDct plDct, string projId);
    Task AddNewDirectCostsAsync(List<PlDct> plDct, string projId);
    Task<bool> UpdateDirectCostAsync(PlDct updatedEmployee);
    Task<PlDct> GetDirectCostByID(int plDctId);
    Task<List<PlDct>> GetAllDirectCostsByPlanId(int planId);
    Task<bool> DeleteDirectCostAsync(int id);
}

public class DirectCostRepository : IDirectCostRepository
{

    private readonly MydatabaseContext _context;

    public DirectCostRepository(MydatabaseContext context)
    {
        _context = context;
    }
    public async Task<PlDct> AddNewDirectCostAsync(PlDct newPlDct, string projId)
    {
        try
        {
            List<PlForecast> plForecasts = new List<PlForecast>();
            var random = new Random();
            int number = random.Next(1, 100000); // 1 to 99999

            if (newPlDct.Type.ToUpper() == "PLC")
            {
                newPlDct.Category = newPlDct.Type + number.ToString("D5");
                newPlDct.Id = newPlDct.Type + number.ToString("D5");
            }
            else
            {
                newPlDct.Category = "TBD_" + number.ToString("D5");
                newPlDct.Id = "TBD_" + number.ToString("D5");
            }

            var entry = await _context.PlDcts.AddAsync(newPlDct);
            await _context.SaveChangesAsync();
            return entry.Entity;
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate") == true)
        {
            throw new InvalidOperationException("Direct Cost combination already exists.", ex);
        }
    }
    public async Task AddNewDirectCostsAsync(List<PlDct> newPlDct, string projId)
    {
        try
        {
            List<PlForecast> plForecasts = new List<PlForecast>();
            var random = new Random();

            foreach (var dct in newPlDct)
            {
                int number = random.Next(1, 100000); // 1 to 99999

                if (dct.Type.ToUpper() == "PLC")
                {
                    //dct.Category = "PLC_" + number.ToString("D5");
                    dct.Id = dct.Id + "_" + number.ToString("D5");
                }
                if (dct.Type.ToUpper() == "OTHER")
                {
                    //dct.Category = "TBD_" + number.ToString("D5");
                    dct.Id = dct.Id + "_" + number.ToString("D5");
                }
                foreach (var forecast in dct.PlForecasts)
                {
                    forecast.EmplId = dct.Id;
                }
            }

            await _context.PlDcts.AddRangeAsync(newPlDct);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate") == true)
        {
            throw new InvalidOperationException("Direct Cost combination already exists.", ex);
        }
    }
    public Task<List<PlDct>> GetAllDirectCosts()
    {
        var directCosts = _context.PlDcts.Include(p=>p.PlForecasts).ToList();
        return Task.FromResult(directCosts);
    }

    public Task<List<PlDct>> GetAllDirectCostsByPlanId(int planId)
    {
        var directCosts = _context.PlDcts.Where(p=>p.PlId == planId).Include(p => p.PlForecasts).ToList();
        return Task.FromResult(directCosts);
    }

    public  Task<PlDct> GetDirectCostByID(int plDctId)
    {
        return Task.FromResult(_context.PlDcts.FirstOrDefault(p=>p.DctId == plDctId));
    }

    public async Task<bool> UpdateDirectCostAsync(PlDct updatedplDct)
    {
        try
        {

            bool isDuplicate = await _context.PlDcts.AnyAsync(d =>
                            d.AcctId == updatedplDct.AcctId &&
                            d.OrgId == updatedplDct.OrgId &&
                            d.Id == updatedplDct.Id &&
                            d.PlId == updatedplDct.PlId &&
                            d.DctId != updatedplDct.DctId); // Exclude current record by ID

            var existing = await _context.PlDcts.FindAsync(updatedplDct.DctId);
            if (existing == null)
                return false;

            existing.AcctId = updatedplDct.AcctId;
            existing.PlcGlc = updatedplDct.PlcGlc;
            existing.OrgId = updatedplDct.OrgId;
            existing.IsBrd = updatedplDct.IsBrd;
            existing.IsRev = updatedplDct.IsRev;
            existing.AmountType = updatedplDct.AmountType;
            existing.Category = updatedplDct.Category;
            existing.LastModifiedBy = updatedplDct.LastModifiedBy;
            existing.Notes = updatedplDct.Notes;
            //existing.LastModifiedDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            var forecasts = await _context.PlForecasts
                .Where(p => p.DctId == updatedplDct.DctId)
                .ToListAsync();

            foreach (var forecast in forecasts)
            {
                forecast.AcctId = updatedplDct.AcctId;
                forecast.Plc = updatedplDct.PlcGlc;
                forecast.OrgId = updatedplDct.OrgId;
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate") == true)
        {
            throw new InvalidOperationException("Direct cost combination already exists.", ex);
        }
    }

    public async Task<bool> DeleteDirectCostAsync(int id)
    {
        var entity = await _context.PlDcts.FindAsync(id);
        if (entity == null)
            return false;

        _context.PlDcts.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }
}
