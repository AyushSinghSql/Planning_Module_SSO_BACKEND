using System;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Repositories
{

    public interface IVendorEmployeeRepository
    {
        Task<IEnumerable<VendorEmployee>> GetAllAsync();
        Task<VendorEmployee?> GetByIdAsync(string vendEmplId, string vendId);
        Task AddAsync(VendorEmployee employee);
        Task UpdateAsync(VendorEmployee employee);
        Task DeleteAsync(string vendEmplId, string vendId);
    }
    public class VendorEmployeeRepository : IVendorEmployeeRepository
    {
        private readonly MydatabaseContext _context;

        public VendorEmployeeRepository(MydatabaseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VendorEmployee>> GetAllAsync()
        {
            return await _context.VendorEmployees.ToListAsync();
        }

        public async Task<VendorEmployee?> GetByIdAsync(string vendEmplId, string vendId)
        {
            return await _context.VendorEmployees
                .FindAsync(vendEmplId, vendId);
        }

        public async Task AddAsync(VendorEmployee employee)
        {
            _context.VendorEmployees.Add(employee);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(VendorEmployee employee)
        {
            _context.VendorEmployees.Update(employee);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(string vendEmplId, string vendId)
        {
            var emp = await GetByIdAsync(vendEmplId, vendId);
            if (emp != null)
            {
                _context.VendorEmployees.Remove(emp);
                await _context.SaveChangesAsync();
            }
        }
    }

}
