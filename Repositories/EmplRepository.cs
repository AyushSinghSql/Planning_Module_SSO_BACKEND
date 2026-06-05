using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using PlanningAPI.Models;
using WebApi.Helpers;

public interface IEmplRepository
{
    Task<List<PlEmployeee>> GetAllEployees();
    //Task<List<PlEmployeee>> GetAllEployeesByProject(string projId);

    Task<PlEmployeee> AddNewEmployeeAsync(PlEmployeee newPlEmployeee);
    Task AddNewEmployeesAsync(List<PlEmployeee> newPlEmployee);
    Task<bool> UpdateEmployeeAsync(PlEmployeee updatedEmployee);
    Task<PlEmployeee> GetEployeeByID(string emplId);
    Task<bool> DeleteEmployeeAsync(int employeeId);   // <-- new
}

public class EmplRepository : IEmplRepository
{
    private readonly MydatabaseContext _context;
    private readonly IConfiguration _config;

    public EmplRepository(MydatabaseContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public Task<PlEmployeee> GetEployeeByID(string emplId)
    {
        var employee = _context.PlEmployeees
                               .Where(p => p.EmplId.Equals(emplId))
                               .FirstOrDefault();
        return Task.FromResult(employee);
    }

    public Task<List<PlEmployeee>> GetAllEployees()
    {
        var employees = _context.PlEmployeees.ToList();
        return Task.FromResult(employees);
    }

    public async Task<PlEmployeee> AddNewEmployeeAsync(PlEmployeee newPlEmployeee)
    {
        try
        {
            var random = new Random();
            int number = random.Next(1, 100000); // 1 to 99999

            if (newPlEmployeee.Type.ToUpper() == "PLC")
            {
                newPlEmployeee.FirstName = newPlEmployeee.Type + number.ToString("D5");
                newPlEmployeee.EmplId = newPlEmployeee.Type + number.ToString("D5");
            }
            else
            {
                newPlEmployeee.FirstName = "TBD_" + number.ToString("D5");
                newPlEmployeee.EmplId = "TBD_" + number.ToString("D5");
            }

            var entry = await _context.PlEmployeees.AddAsync(newPlEmployeee);
            await _context.SaveChangesAsync();
            return entry.Entity;
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate") == true)
        {
            throw new InvalidOperationException("Employee combination already exists.", ex);
        }
    }
    public async Task AddNewEmployeesAsync(List<PlEmployeee> newPlEmployeee)
    {
        try
        {
            Helper helper = new Helper(_context, _config);
            var random = new Random();
            var projId = "xxxxx";
            var escallation_percent =
            _context.PlConfigValues
                .Where(r => r.Name.ToLower() == "escallation_percent"
                         && (r.ProjId == projId || r.ProjId == "xxxxx"))
                .OrderByDescending(r => r.ProjId == projId) // prefer project-specific
                .Select(r => r.Value)
                .FirstOrDefault() ?? "3";

            foreach (var empl in newPlEmployeee)
            {

                int number = random.Next(1, 100000); // 1 to 99999

                if (empl.Type.ToUpper() == "PLC")
                {
                    empl.EmplId = empl.EmplId + "_" + number.ToString("D5");
                }
                if (empl.Type.ToUpper() == "OTHER")
                {
                    empl.EmplId = empl.EmplId + "_" + number.ToString("D5");
                }

                foreach(var forecast in empl.PlForecasts)
                {
                    forecast.EmplId = empl.EmplId;
                }
            }

            var forecasts = newPlEmployeee
    .SelectMany(x => x.PlForecasts ?? new List<PlForecast>())
    .ToList();

            var planType = await _context.PlProjectPlans
                .Where(x => x.PlId == forecasts.First().PlId)
                .Select(x => x.PlType)
                .FirstOrDefaultAsync();

            await helper.ValidateEmployeeHoursAsync(forecasts, planType);

            await _context.PlEmployeees.AddRangeAsync(newPlEmployeee);
            await _context.SaveChangesAsync();
            //return entry.Entity;
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("duplicate") == true)
        {
            throw new InvalidOperationException("Employee combination already exists.", ex);
        }
    }
    public async Task<bool> UpdateEmployeeAsync(PlEmployeee updatedEmployee)
    {
        try
        {
            var existing = await _context.PlEmployeees.FindAsync(updatedEmployee.Id);
            if (existing == null)
                return false;

            // Update employee
            existing.AccId = updatedEmployee.AccId;
            existing.FirstName = updatedEmployee.FirstName;
            existing.LastName = updatedEmployee.LastName;
            existing.PlcGlcCode = updatedEmployee.PlcGlcCode;
            existing.OrgId = updatedEmployee.OrgId;
            existing.PerHourRate = updatedEmployee.PerHourRate;
            existing.IsRev = updatedEmployee.IsRev;
            existing.IsBrd = updatedEmployee.IsBrd;
            existing.Esc_Percent = updatedEmployee.Esc_Percent;
            existing.EffectiveDate = updatedEmployee.EffectiveDate;

            var forecasts = await _context.PlForecasts
               .Where(p => p.empleId == updatedEmployee.Id)
               .ToListAsync();

            foreach (var forecast in forecasts)
            {
                forecast.EmplId = updatedEmployee.EmplId;
                forecast.AcctId = updatedEmployee.AccId;
                forecast.Plc = updatedEmployee.PlcGlcCode;
                forecast.OrgId = updatedEmployee.OrgId;
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }


    public async Task<bool> DeleteEmployeeAsync(int employeeId)   // <-- new method
    {
        try
        {
            var employee = await _context.PlEmployeees.FindAsync(employeeId);
            if (employee == null)
                return false;

            _context.PlEmployeees.Remove(employee);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
