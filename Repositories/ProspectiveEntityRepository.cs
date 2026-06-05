using System;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Repositories
{

    public interface IProspectiveEntityRepository
    {
        Task<IEnumerable<ProspectiveEntity>> GetAllAsync();
        Task<ProspectiveEntity> GetByIdAsync(int id);
        Task AddAsync(ProspectiveEntity entity);
        Task UpdateAsync(ProspectiveEntity entity);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<ProspectiveEntity>> GetByTypeAsync(string type);
    }

    public class ProspectiveEntityRepository : IProspectiveEntityRepository
    {
        private readonly MydatabaseContext _context;

        public ProspectiveEntityRepository(MydatabaseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProspectiveEntity>> GetAllAsync()
        {
            return await _context.ProspectiveEntities
                .Include(p => p.EmployeeDetails)
                .Include(p => p.VendorDetails)
                .Include(p => p.VendorEmployeeDetails)
                .Include(p => p.PLCDetails)
                .Include(p => p.GenericStaffDetails)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProspectiveEntity>> GetByTypeAsync(string type)
        {
            return await _context.ProspectiveEntities
                .Where(p => p.Type.ToLower() == type.ToLower())
                .Include(p => p.EmployeeDetails)
                .Include(p => p.VendorDetails)
                .Include(p => p.VendorEmployeeDetails)
                .Include(p => p.PLCDetails)
                .Include(p => p.GenericStaffDetails)
                .ToListAsync();
        }


        public async Task<ProspectiveEntity> GetByIdAsync(int id)
        {
            return await _context.ProspectiveEntities
                .Include(p => p.EmployeeDetails)
                .Include(p => p.VendorDetails)
                .Include(p => p.VendorEmployeeDetails)
                .Include(p => p.PLCDetails)
                .Include(p => p.GenericStaffDetails)
                .FirstOrDefaultAsync(p => p.ProspectiveId == id);
        }

        public async Task AddAsync(ProspectiveEntity entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            _context.ProspectiveEntities.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ProspectiveEntity entity)
        {
            entity.UpdatedAt = DateTime.UtcNow;
            _context.ProspectiveEntities.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.ProspectiveEntities.FindAsync(id);
            if (entity != null)
            {
                _context.ProspectiveEntities.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.ProspectiveEntities.AnyAsync(e => e.ProspectiveId == id);
        }
    }

}
