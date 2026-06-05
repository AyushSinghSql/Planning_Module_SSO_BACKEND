namespace WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;
using WebApi.Helpers;
using WebApi.Models;
using WebApi.Models.Users;
using WebApi.Repositories;
using WebApi.Services;

[ApiController]
[Route("[controller]")]
public class DirectCostController : ControllerBase
{
    private IDirectCostService _directCostService;
    private readonly MydatabaseContext _context;
    private readonly ILogger<DirectCostController> _logger;
    private readonly IConfiguration _config;

    public DirectCostController(ILogger<DirectCostController> logger, IDirectCostService directCostService, MydatabaseContext context, IConfiguration config)
    {
        _logger = logger;
        _directCostService = directCostService;
        _context = context;
        _config = config;
    }

    [HttpGet("GetAllDirectCostsByPlanId/{PlanId}")]
    public async Task<IActionResult> GetAllDirectCostsByPlanId(int PlanId)
    {
        _logger.LogInformation("GetAllDirectCosts called at {Time}", DateTime.UtcNow);

        try
        {
            var directCosts = await _directCostService.GetAllDirectCostsByPlanId(PlanId);
            return Ok(directCosts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all directcosts");
            return StatusCode(500, new { Message = "Failed to fetch directcosts.", Error = ex.Message });
        }
    }

    [HttpGet("GetAllDirectCosts")]
    public async Task<IActionResult> GetAllDirectCosts()
    {
        _logger.LogInformation("GetAllDirectCosts called at {Time}", DateTime.UtcNow);

        try
        {
            var directCosts = await _directCostService.GetAllDirectCosts();
            return Ok(directCosts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all directcosts");
            return StatusCode(500, new { Message = "Failed to fetch directcosts.", Error = ex.Message });
        }
    }

    [HttpPut("UpdateDirectCost")]
    public async Task<IActionResult> UpdateDirectCost([FromBody] PlDct plDct,int plid, int TemplateId)
    {
        _logger.LogInformation("UpdateDirectCost called for EmpID: {DctId}", plDct?.DctId);

        try
        {
            var result = await _directCostService.UpdateDirectCostAsync(plDct);
            PlForecastRepository plForecastRepository = new PlForecastRepository(_context, _config);
            try
            {
                await plForecastRepository.CalculateRevenueCost(plid, TemplateId, "");
            }
            catch (Exception ex)
            {
                //logger.LogError(ex, "Error Calculation Revenue for {PlanId}", result.PlId);
            }
            return result ? Ok("success") : BadRequest("Update failed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Direct cost with ID: {DctId}", plDct?.DctId);
            return StatusCode(500, new { Message = "Failed to update Direct cost.", Error = ex.Message });
        }
    }

    [HttpPost("AddNewDirectCost")]
    public async Task<IActionResult> AddNewDirectCost([FromBody] PlDct plDct)
    {
        _logger.LogInformation("AddNewDirectCost called for Direct cost: {DctId}", plDct?.DctId);

        try
        {
            var entry = await _directCostService.AddNewDirectCostAsync(plDct, "");

            if (entry != null)
            {

            }
            return base.Ok((object)entry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding new Direct cost");
            return StatusCode(500, new { Message = "Failed to add new Direct cost.", Error = ex.Message });
        }
    }

    [HttpPost("AddNewDirectCosts")]
    public async Task<IActionResult> AddNewDirectCosts([FromBody] List<PlDct> plDct, int plid, int templateid)
    {
        //_logger.LogInformation("AddNewDirectCost called for Direct cost: {DctId}", plDct?.DctId);

        try
        {
            await _directCostService.AddNewDirectCostsAsync(plDct, "");

            PlForecastRepository plForecastRepository = new PlForecastRepository(_context, _config);
            try
            {
                await plForecastRepository.CalculateRevenueCost(plid, templateid, "");
            }
            catch (Exception ex)
            {
                //logger.LogError(ex, "Error Calculation Revenue for {PlanId}", result.PlId);
            }

            return base.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding new Direct cost");
            return StatusCode(500, new { Message = "Failed to add new Direct cost.", Error = ex.Message });
        }
    }

    [HttpDelete("DeleteDirectCost/{id}")]
    public async Task<IActionResult> DeleteDirectCost(int id)
    {
        _logger.LogInformation("DeleteDirectCost called for DctId: {DctId}", id);

        try
        {
            var result = await _directCostService.DeleteDirectCostAsync(id);
            if (!result)
                return NotFound(new { Message = $"Direct cost with Id {id} not found." });

            return Ok(new { Message = $"Direct cost with Id {id} deleted successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Direct cost with ID: {DctId}", id);
            return StatusCode(500, new { Message = "Failed to delete Direct cost.", Error = ex.Message });
        }
    }

}