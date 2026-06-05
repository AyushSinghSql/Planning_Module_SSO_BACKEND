using Microsoft.EntityFrameworkCore;
using Npgsql;
using PlanningAPI.Models;

namespace PlanningAPI.Repositories
{
    public interface IProjEmplRtRepository
    {
        Task<IEnumerable<ProjEmplRt>> GetAllAsync();
        Task<ProjEmplRt> GetByIdAsync(int id);
        Task<ProjEmplRt> AddAsync(ProjEmplRt entity);
        Task<bool> UpdateAsync(ProjEmplRt entity);
        Task<bool> DeleteAsync(int id);
    }
    public class ProjEmplRtRepository : IProjEmplRtRepository
    {
        private readonly MydatabaseContext _context;
        private readonly ILogger<ProjEmplRtRepository> _logger;

        public ProjEmplRtRepository(MydatabaseContext context, ILogger<ProjEmplRtRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<ProjEmplRt>> GetAllAsync()
        {
            try
            {
                var plcs = await _context.ProjEmplRts.Include(p => p.plc).ToListAsync();
                if (plcs.Count() == 0)
                {
                    return plcs;
                }
                foreach (var plc in plcs)
                {
                    plc.plcDescription = plc.plc.Description;
                    plc.plc = null;
                }

                var ids = plcs.Select(p => p.EmplId).ToList();
                var parameters = ids
                    .Select((id, i) => new NpgsqlParameter($"p{i}", id))
                    .ToArray();

                var placeholders = string.Join(",", parameters.Select(p => $"@{p.ParameterName}"));

                var sql = $@"
                        SELECT empl_id AS EmplId, last_first_name AS FirstName,
                               0 AS PerHourRate, '' AS Status, 0 AS Salary,
                               null AS EffectiveDate
                        FROM public.empl
                        WHERE empl_id IN ({placeholders})";

                var data = await _context.Database
                    .SqlQueryRaw<Empl_Master>(sql, parameters)
                    .ToListAsync();

                var nameLookup = data.ToDictionary(e => e.EmplId, e => e.FirstName);

                foreach (var plc in plcs)
                {
                    if (nameLookup.TryGetValue(plc.EmplId, out var firstName))
                    {
                        plc.EmplName = firstName;
                    }
                }


                return plcs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all ProjEmplRt records.");
                throw;
            }
        }

        public async Task<ProjEmplRt> GetByIdAsync(int id)
        {
            try
            {
                return await _context.ProjEmplRts.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ProjEmplRt by ID {Id}", id);
                throw;
            }
        }

        public async Task<ProjEmplRt> AddAsync(ProjEmplRt entity)
        {
            try
            {
                entity.StartDt = entity.StartDt.GetValueOrDefault().ToUniversalTime();
                entity.EndDt = entity.EndDt.GetValueOrDefault().ToUniversalTime();
                _context.ProjEmplRts.Add(entity);
                await _context.SaveChangesAsync();
                return entity;
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23514")
            {
                throw new InvalidOperationException("Date validation failed: Start date must be before or equal to end date.", ex);
            }
        }

        public async Task<bool> UpdateAsync(ProjEmplRt entity)
        {
            try
            {
                entity.StartDt = entity.StartDt.GetValueOrDefault().ToUniversalTime();
                entity.EndDt = entity.EndDt.GetValueOrDefault().ToUniversalTime();
                _context.Entry(entity).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23514")
            {
                throw new InvalidOperationException("Date validation failed: Start date must be before or equal to end date.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ProjEmplRt with ID {Id}", entity.ProjEmplRtKey);
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var entity = await _context.ProjEmplRts.FindAsync(id);
                if (entity == null)
                {
                    _logger.LogWarning("ProjEmplRt with ID {Id} not found for deletion.", id);
                    return false;
                }

                _context.ProjEmplRts.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting ProjEmplRt with ID {Id}", id);
                return false;
            }
        }
    }
}
