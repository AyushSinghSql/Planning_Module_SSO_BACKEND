using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

public interface IHolidayCalendarRepository
{
    Task<IEnumerable<Holidaycalender>> GetAllAsync();
    Task<Holidaycalender?> GetByIdAsync(int id);
    Task<Holidaycalender> AddAsync(Holidaycalender holiday);
    Task<bool> UpdateAsync(Holidaycalender holiday);
    Task<bool> DeleteAsync(int id);
}

public class HolidayCalendarRepository : IHolidayCalendarRepository
{
    private readonly MydatabaseContext _context;

    public HolidayCalendarRepository(MydatabaseContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Holidaycalender>> GetAllAsync()
    {
        return await _context.Holidaycalenders.ToListAsync();
    }

    public async Task<Holidaycalender?> GetByIdAsync(int id)
    {
        return await _context.Holidaycalenders.FindAsync(id);
    }

    public async Task<Holidaycalender> AddAsync(Holidaycalender holiday)
    {
        holiday.Date = holiday.Date.ToUniversalTime();
        var entry = await _context.Holidaycalenders.AddAsync(holiday);
        await _context.SaveChangesAsync();
        return entry.Entity;
    }

    public async Task<bool> UpdateAsync(Holidaycalender holiday)
    {
        var existing = await _context.Holidaycalenders.FindAsync(holiday.Id);
        if (existing == null) return false;

        existing.Date = holiday.Date.ToUniversalTime();
        existing.Type = holiday.Type;
        existing.Name = holiday.Name;
        existing.Ispublicholiday = holiday.Ispublicholiday;
        existing.State = holiday.State;
        existing.Remarks = holiday.Remarks;
        existing.Year = holiday.Year;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var holiday = await _context.Holidaycalenders.FindAsync(id);
        if (holiday == null) return false;

        _context.Holidaycalenders.Remove(holiday);
        await _context.SaveChangesAsync();
        return true;
    }
}
