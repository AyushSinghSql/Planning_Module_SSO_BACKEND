using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;
using WebApi.Controllers;
using WebApi.Services;

namespace PlanningAPI.Controllers
{
    public class NewBusinessController : ControllerBase
    {

        private readonly MydatabaseContext _context;
        private readonly ILogger<ProjectController> _logger;
        NewBusinessService _service;
        private IProjPlanService _projPlanService;
        public NewBusinessController(ILogger<ProjectController> logger, MydatabaseContext context, IProjPlanService projPlanService)
        {
            _context = context;
            _logger = logger;
            _service = new NewBusinessService(_context);
            _projPlanService = projPlanService;
        }

        [HttpGet("GetAllNewBusiness")]
        public async Task<IActionResult> GetAllNewBusiness()
        {
            try
            {
                var users = await _service.GetAllNewBusinessAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                if (ex.InnerException is Npgsql.PostgresException pgEx)
                {
                    // Return or log the Detail from the Postgres exception
                    return BadRequest(new { error = pgEx.Detail });
                }
                _logger.LogError(ex, "Error fetching all New Business.");
                return StatusCode(500, "Internal server error while retrieving New Business.");
            }
        }

        [HttpGet("GetAllNonTransferedNewBusinessAsync")]
        public async Task<IActionResult> GetAllNonTransferedNewBusinessAsync()
        {
            try
            {
                var users = await _service.GetAllNonTransferedNewBusinessAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                if (ex.InnerException is Npgsql.PostgresException pgEx)
                {
                    // Return or log the Detail from the Postgres exception
                    return BadRequest(new { error = pgEx.Detail });
                }
                _logger.LogError(ex, "Error fetching all New Business.");
                return StatusCode(500, "Internal server error while retrieving New Business.");
            }
        }

        [HttpGet("GetAllNewBusinessByID/{newBusinessId}")]
        public async Task<IActionResult> GetAllNewBusinessByID(string newBusinessId)
        {
            try
            {
                var users = await _service.GetAllNewBusinessByID(newBusinessId);
                return Ok(users);
            }
            catch (Exception ex)
            {
                if (ex.InnerException is Npgsql.PostgresException pgEx)
                {
                    // Return or log the Detail from the Postgres exception
                    return BadRequest(new { error = pgEx.Detail });
                }
                _logger.LogError(ex, "Error fetching all New Business.");
                return StatusCode(500, "Internal server error while retrieving New Business.");
            }
        }


        [HttpPut("UpdateNewBusiness")]
        public async Task<IActionResult> UpdateNewBusiness([FromBody] NewBusinessBudget updatedBusiness)
        {
            if (updatedBusiness == null || updatedBusiness.BusinessBudgetId == null)
                return BadRequest("Invalid request data.");

            try
            {
                var result = await _service.UpdateNewBusinessAsync(updatedBusiness);
                return Ok(result);
            }
            catch (Exception ex)
            {
                if (ex.InnerException is Npgsql.PostgresException pgEx)
                {
                    // Return or log the Detail from the Postgres exception
                    return BadRequest(new { error = pgEx.Detail });
                }
                _logger.LogError(ex, $"Error updating business ID {updatedBusiness.BusinessBudgetId}.");
                return StatusCode(500, "Internal server error while updating business.");
            }
        }


        [HttpPost("AddNewBusiness")]
        public async Task<IActionResult> AddNewBusiness([FromBody] NewBusinessBudget newBusiness)
        {
            if (newBusiness == null)
                return BadRequest("New Business data is null.");

            try
            {

                newBusiness.StartDate = newBusiness.StartDate.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(newBusiness.StartDate, DateTimeKind.Utc)
                    : newBusiness.StartDate.ToUniversalTime();

                newBusiness.EndDate = newBusiness.EndDate.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(newBusiness.EndDate, DateTimeKind.Utc)
                    : newBusiness.EndDate.ToUniversalTime();
                var created = await _service.CreateNewBusinessAsync(newBusiness);
                return CreatedAtAction(nameof(GetAllNewBusiness), new { id = created.BusinessBudgetId }, created);
            }
            catch (Exception ex)
            {

                if (ex.InnerException is Npgsql.PostgresException pgEx)
                {
                    // Return or log the Detail from the Postgres exception
                    return BadRequest(new { error = pgEx.Detail });
                }

                // For other exceptions
                return BadRequest(new { error = ex.Message });
                _logger.LogError(ex, "Error adding new business.");
                return StatusCode(500, "Internal server error while adding new business.");
            }
        }

        [HttpDelete("DeleteNewBusiness/{id}")]
        public async Task<IActionResult> DeleteNewBusiness(string id)
        {
            try
            {
                await _service.DeleteNewBusinessAsync(id);

                var NBBudgets = _context.PlProjectPlans.Where(p => p.ProjId == id).ToList();

                foreach (var nbBudget in NBBudgets)
                {
                    var success = await _projPlanService.DeleteProjectPlanAsync(nbBudget.PlId.GetValueOrDefault());
                    if (!success)
                    {
                        _logger.LogWarning("Project plan with ID {ProjectPlanId} not found.", id);
                    }
                }
                return NoContent(); // 204 - successfully deleted
            }
            catch (Exception ex)
            {
                if (ex.InnerException is Npgsql.PostgresException pgEx)
                {
                    // Return or log the Detail from the Postgres exception
                    return BadRequest(new { error = pgEx.Detail });
                }
                _logger.LogError(ex, $"Error deleting business ID {id}.");
                return StatusCode(500, "Internal server error while deleting business.");
            }
        }


        [HttpPost("BulkDeleteNewBusiness")]
        public async Task<IActionResult> BulkDeleteNewBusiness([FromBody] List<string> ids)
        {
            if (ids == null || ids.Count == 0)
                return BadRequest("No IDs provided.");

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1️⃣ Load all related project plans in ONE query
                var allBudgets = await _context.PlProjectPlans
                    .AsNoTracking()
                    .Where(p => ids.Contains(p.ProjId))
                    .Select(p => new { p.ProjId, p.PlId })
                    .ToListAsync();

                // 2️⃣ Group budgets by business ID
                var budgetsByProj = allBudgets
                    .Where(b => b.PlId.HasValue)
                    .GroupBy(b => b.ProjId)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.PlId!.Value));

                // 3️⃣ Delete businesses + related project plans
                foreach (var id in ids)
                {
                    await _service.DeleteNewBusinessAsync(id);

                    if (budgetsByProj.TryGetValue(id, out var planIds))
                    {
                        foreach (var planId in planIds)
                        {
                            var success = await _projPlanService.DeleteProjectPlanAsync(planId);
                            if (!success)
                            {
                                _logger.LogWarning(
                                    "Project plan with ID {ProjectPlanId} not found.",
                                    planId
                                );
                            }
                        }
                    }
                }

                await transaction.CommitAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                if (ex.InnerException is Npgsql.PostgresException pgEx)
                {
                    return BadRequest(new { error = pgEx.Detail });
                }

                _logger.LogError(ex, "Error bulk deleting businesses {@Ids}", ids);
                return StatusCode(500, "Internal server error while bulk deleting businesses.");
            }
        }


        //[HttpPost("BulkDeleteNewBusiness")]
        //public async Task<IActionResult> BulkDeleteNewBusiness(List<string> ids)
        //{
        //    try
        //    {
        //        foreach (var id in ids)
        //        {
        //            await _service.DeleteNewBusinessAsync(id);

        //            var NBBudgets = _context.PlProjectPlans.Where(p => p.ProjId == id).ToList();

        //            foreach (var nbBudget in NBBudgets)
        //            {
        //                var success = await _projPlanService.DeleteProjectPlanAsync(nbBudget.PlId.GetValueOrDefault());
        //                if (!success)
        //                {
        //                    _logger.LogWarning("Project plan with ID {ProjectPlanId} not found.", id);
        //                }
        //            }
        //        }
        //        return NoContent(); // 204 - successfully deleted
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex.InnerException is Npgsql.PostgresException pgEx)
        //        {
        //            // Return or log the Detail from the Postgres exception
        //            return BadRequest(new { error = pgEx.Detail });
        //        }
        //        _logger.LogError(ex, $"Error deleting business ID {ids}.");
        //        return StatusCode(500, "Internal server error while deleting business.");
        //    }
        //}




    }
}
