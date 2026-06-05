
using AutoMapper;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;
using WebApi.DTO;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models.Users;
using WebApi.Repositories;

public interface INewBusinessService
{
    Task<NewBusinessBudget> CreateNewBusinessAsync(NewBusinessBudget newBusinessBudget);
    Task<bool> DeleteNewBusinessAsync(string id);
    Task<NewBusinessBudget> UpdateNewBusinessAsync(NewBusinessBudget updatedBudget);
    Task<List<NewBusinessBudget>> GetAllNewBusinessAsync();
    Task<List<NewBusinessBudget>> GetAllNewBusinessByID(string newBusinessId);

}

public class NewBusinessService : INewBusinessService
{
    private readonly MydatabaseContext _context;
    public NewBusinessService(MydatabaseContext context)
    {
        _context = context;
    }

    public async Task<NewBusinessBudget> CreateNewBusinessAsync(NewBusinessBudget newBusinessBudget)
    {
        var entry = await _context.NewBusinessBudgets.AddAsync(newBusinessBudget);
        await _context.SaveChangesAsync();
        return entry.Entity;
    }
    public async Task<NewBusinessBudget> UpdateNewBusinessAsync(NewBusinessBudget updatedBudget)
    {
        var existing = await _context.NewBusinessBudgets.FirstOrDefaultAsync(p => p.BusinessBudgetId == updatedBudget.BusinessBudgetId);

        if (existing == null)
        {
            throw new KeyNotFoundException($"NewBusinessBudget with ID {updatedBudget.BusinessBudgetId} not found.");
        }

        _context.Entry(existing).CurrentValues.SetValues(updatedBudget);
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteNewBusinessAsync(string newBusinessId)
    {
        var existing = await _context.NewBusinessBudgets.FirstOrDefaultAsync(p => p.BusinessBudgetId == newBusinessId);
        if (existing == null)
        {
            return false;
        }

        _context.NewBusinessBudgets.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<List<NewBusinessBudget>> GetAllNewBusinessAsync()
    {
        return await _context.NewBusinessBudgets
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<NewBusinessBudget>> GetAllNonTransferedNewBusinessAsync()
    {

        //var nbbudgets = await _context.PlProjectPlans.Where(p=>p.PlType.ToUpper() == "NBBUD" && p.FinalVersion == true).Select(q=>q.ProjId).ToListAsync();
        //return await _context.NewBusinessBudgets
        //    .Where(p => nbbudgets.Contains(p.BusinessBudgetId) && (p.Status == null || p.Status.ToUpper() != "TRANSFERRED"))
        //    .OrderByDescending(n => n.CreatedAt)
        //    .ToListAsync();
        return await _context.NewBusinessBudgets
                .Where(nb =>
                    _context.PlProjectPlans.Any(pp =>
                        pp.ProjId == nb.BusinessBudgetId &&
                        pp.PlType == "NBBUD" &&
                        pp.FinalVersion.Value))
                .Where(nb => nb.Status == null || nb.Status != "TRANSFERRED")
                .OrderByDescending(nb => nb.CreatedAt)
                .ToListAsync();


    }

    public Task<List<NewBusinessBudget>> GetAllNewBusinessByID(string newBusinessId)
    {
        return _context.NewBusinessBudgets.Where(p=>p.BusinessBudgetId.ToUpper().StartsWith(newBusinessId.ToUpper()))
    .OrderByDescending(n => n.CreatedAt)
    .ToListAsync();
    }
}