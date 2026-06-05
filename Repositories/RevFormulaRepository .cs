using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;

namespace PlanningAPI.Repositories
{
    public interface IRevFormulaRepository
    {
        Task<IEnumerable<RevFormula>> GetAllAsync();
        Task<RevFormula> GetByIdAsync(string formulaCd);
        Task<RevFormula> AddAsync(RevFormula formula);
        Task<bool> UpdateAsync(RevFormula formula);
        Task<bool> DeleteAsync(string formulaCd);
    }

    public class RevFormulaRepository : IRevFormulaRepository
    {
        private readonly MydatabaseContext _context;
        private readonly ILogger<RevFormulaRepository> _logger;

        public RevFormulaRepository(MydatabaseContext context, ILogger<RevFormulaRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<RevFormula>> GetAllAsync()
        {
            try
            {
                return await _context.RevFormulas.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all RevFormulas");
                throw;
            }
        }

        public async Task<RevFormula> GetByIdAsync(string formulaCd)
        {
            try
            {
                return await _context.RevFormulas.FindAsync(formulaCd);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving RevFormula with Code: {Code}", formulaCd);
                throw;
            }
        }

        public async Task<RevFormula> AddAsync(RevFormula formula)
        {
            try
            {
                _context.RevFormulas.Add(formula);
                await _context.SaveChangesAsync();
                return formula;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding RevFormula");
                throw;
            }
        }

        public async Task<bool> UpdateAsync(RevFormula formula)
        {
            try
            {
                _context.Entry(formula).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating RevFormula with Code: {Code}", formula.FormulaCd);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(string formulaCd)
        {
            try
            {
                var entity = await _context.RevFormulas.FindAsync(formulaCd);
                if (entity == null) return false;

                _context.RevFormulas.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting RevFormula with Code: {Code}", formulaCd);
                return false;
            }
        }
    }

}
