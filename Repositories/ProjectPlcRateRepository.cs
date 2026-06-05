using System;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Repositories
{
    public interface IProjectPlcRateRepository
    {
        Task<IEnumerable<ProjectPlcRate>> GetAllAsync();
        Task<IEnumerable<ProjectPlcRate>> GetByPlcCode(string PlcCode);
        Task<ProjectPlcRate> GetByIdAsync(int id);
        Task AddAsync(ProjectPlcRate entity);
        Task UpdateAsync(ProjectPlcRate entity);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }

    public class ProjectPlcRateRepository : IProjectPlcRateRepository
    {
        private readonly MydatabaseContext _context;
        private readonly ILogger<ProjectPlcRateRepository> _logger;
        public ProjectPlcRateRepository(MydatabaseContext context, ILogger<ProjectPlcRateRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<ProjectPlcRate>> GetAllAsync()
        {
            try
            {
                return await _context.ProjectPlcRates
                    //.Include(p => p.Project)
                    //.Include(p => p.PlcCode)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all ProjectPlcRates.");
                throw; // Re-throw the exception so it can be handled by upper layers (controller/global handler)
            }
        }

        public async Task<IEnumerable<ProjectPlcRate>> GetByPlcCode(string PlcCode)
        {
            try
            {
                return await _context.ProjectPlcRates.Where(p => p.LaborCategoryCode == PlcCode)
                    //.Include(p => p.Project)
                    //.Include(p => p.PlcCode)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all ProjectPlcRates.");
                throw; // Re-throw the exception so it can be handled by upper layers (controller/global handler)
            }
        }

        public async Task<ProjectPlcRate> GetByIdAsync(int id)
        {
            try
            {
                return await _context.ProjectPlcRates
                //.Include(p => p.Project)
                //.Include(p => p.PlcCode)
                .FirstOrDefaultAsync(p => p.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all ProjectPlcRates.");
                throw; // Re-throw the exception so it can be handled by upper layers (controller/global handler)
            }
        }

        public async Task AddAsync(ProjectPlcRate entity)
        {
            try
            {
                //entity.CreatedAt = DateTime.UtcNow;
                entity.EffectiveDate = entity.EffectiveDate.ToUniversalTime();
                entity.EndDate = entity.EndDate.GetValueOrDefault().ToUniversalTime();

                _context.ProjectPlcRates.Add(entity);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all ProjectPlcRates.");
                throw; // Re-throw the exception so it can be handled by upper layers (controller/global handler)
            }
        }

        public async Task UpdateAsync(ProjectPlcRate entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            entity.EffectiveDate = entity.EffectiveDate.ToUniversalTime();
            entity.EndDate = entity.EndDate.GetValueOrDefault().ToUniversalTime();
            _context.ProjectPlcRates.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.ProjectPlcRates.FindAsync(id);
            if (entity != null)
            {
                _context.ProjectPlcRates.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.ProjectPlcRates.AnyAsync(p => p.Id == id);
        }
    }

}
