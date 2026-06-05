using System.Threading.Tasks;
using PlanningAPI.Models;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using WebApi.Repositories;

namespace PlanningAPI.Repositories
{

    public interface IProjectFees_Repository
    {
        Task<ProjectFee> GetActiveFeeForAProject(string ProjId);
    }
    public class ProjectFees_Repository : IProjectFees_Repository
    {
        private readonly MydatabaseContext _context;
        public ProjectFees_Repository(MydatabaseContext context)
        {
            _context = context;
        }

        public async Task<ProjectFee> GetActiveFeeForAProject(string ProjId)
        {
            var today = DateTime.Now;
            return await _context.ProjectFees
                    .Where(f => f.ProjId == ProjId
                            && f.IsActive
                            && f.EffectiveDate <= today
                            && (f.EndDate == null || f.EndDate >= today))
                    .OrderByDescending(f => f.EffectiveDate) // Use most recent if multiple
                    .FirstOrDefaultAsync();
        }

    }
}
