namespace WebApi.Repositories;

using PlanningAPI.Models;
using Dapper;
using Microsoft.EntityFrameworkCore;
using WebApi.Entities;
using WebApi.Helpers;
using WebApi.Models;

public interface IConfigValuesRepository
{
    Task<List<PlConfigValue>> GetAllConfigValues();
    Task<PlConfigValue> GetConfigValueByName(string name);
    Task<PlConfigValue> AddConfigValue(PlConfigValue plConfigValue);
    Task<bool> UpdateConfigValue(PlConfigValue plConfigValue);
    Task<bool> DeleteConfigValueByName(string name);

}

public class ConfigValuesRepository : IConfigValuesRepository
{
    //private DataContext _context;
    private readonly MydatabaseContext _context;

    public ConfigValuesRepository(MydatabaseContext context)
    {
        _context = context;
    }



    public async Task<List<PlConfigValue>> GetAllConfigValues()
    {
        return await _context.PlConfigValues.ToListAsync();
    }


    public async Task<PlConfigValue> GetConfigValueByName(string name)
    {
        var config = await _context.PlConfigValues
            .FirstOrDefaultAsync(p => p.Name == name);

        if (config == null)
        {
            return null;
        }

        return config;
    }

    public async Task<PlConfigValue> AddConfigValue(PlConfigValue plConfigValue)
    {
        _context.PlConfigValues.Add(plConfigValue);
        await _context.SaveChangesAsync();
        return plConfigValue;
    }


    public async Task<bool> DeleteConfigValueByName(string name)
    {
        var config = await _context.PlConfigValues.FindAsync(name);
        if (config == null)
            return false;

        _context.PlConfigValues.Remove(config);
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<bool> UpdateConfigValue(PlConfigValue plConfigValue)
    {
        var existing = await _context.PlConfigValues.FindAsync(plConfigValue.Name);
        if (existing == null)
            return false;

        // Update fields
        existing.Value = plConfigValue.Value;
        existing.ProjId = plConfigValue.ProjId;
        _context.PlConfigValues.Update(existing);
        await _context.SaveChangesAsync();
        return true;
    }
}