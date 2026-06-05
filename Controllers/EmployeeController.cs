namespace WebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Protocol;
using PlanningAPI.Models;
using WebApi.Helpers;
using WebApi.Models;
using WebApi.Models.Users;
using WebApi.Repositories;
using WebApi.Services;

[ApiController]
[Route("[controller]")]
public class EmployeeController : ControllerBase
{
    private IEmplService _emplService;
    private readonly MydatabaseContext _context;
    private readonly ILogger<EmployeeController> _logger;
    private readonly IConfiguration _config;
    public EmployeeController(ILogger<EmployeeController> logger, IEmplService emplService, MydatabaseContext context, IConfiguration config)
    {
        _logger = logger;
        _emplService = emplService;
        _context = context;
        _config = config;
    }

    [HttpGet("GetAllEmployees")]
    public async Task<IActionResult> GetAllEmployees()
    {
        _logger.LogInformation("GetAllEmployees called at {Time}", DateTime.UtcNow);

        try
        {
            var employees = await _emplService.GetAllEmployees();
            return Ok(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all employees");
            return StatusCode(500, new { Message = "Failed to fetch employees.", Error = ex.Message });
        }
    }

    //[HttpGet("GetAllEmployeesFromMaster")]
    //public async Task<IActionResult> GetAllEmployeesFromMaster()
    //{
    //    _logger.LogInformation("GetAllEmployees called at {Time}", DateTime.UtcNow);

    //    try
    //    {
    //        //var sql = $@"SELECT empl_id as EmplId, orig_hire_dt, term_dt, last_name, first_name, last_first_name
    //        //            FROM public.empl";

    //        var sql = """
    //                SELECT 
    //                    empl_id AS "EmplId",
    //                    orig_hire_dt,
    //                    term_dt,
    //                    last_name,
    //                    first_name,
    //                    last_first_name
    //                FROM public.empl
    //            """;

    //        var employees = _context.Database
    //             .SqlQuery<EmployeeDto>(sql)
    //             .ToList();
    //        return Ok(employees);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error retrieving all employees");
    //        return StatusCode(500, new { Message = "Failed to fetch employees.", Error = ex.Message });
    //    }
    //}


    [HttpPut("UpdateEmployee")]
    public async Task<IActionResult> UpdateEmployee([FromBody] PlEmployeee plEmployee, int plid, int TemplateId)
    {
        _logger.LogInformation("UpdateEmployee called for EmpID: {EmpId}", plEmployee?.EmplId);

        try
        {
            var result = await _emplService.UpdateEmployeeAsync(plEmployee);
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
            _logger.LogError(ex, "Error updating employee with ID: {EmpId}", plEmployee?.EmplId);
            return StatusCode(500, new { Message = "Failed to update employee.", Error = ex.Message });
        }
    }

    [HttpPost("AddNewEmployee")]
    public async Task<IActionResult> AddNewEmployee([FromBody] PlEmployeee plEmployee)
    {
        _logger.LogInformation("AddNewEmployee called for EmpID: {EmpId}", plEmployee?.EmplId);

        try
        {
            var employee = await _emplService.AddNewEmployeeAsync(plEmployee);
            return Ok(employee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding new employee");
            return StatusCode(500, new { Message = "Failed to add new employee.", Error = ex.Message });
        }
    }

    [HttpPost("AddNewEmployees")]
    public async Task<IActionResult> AddNewEmployees([FromBody] List<PlEmployeee> plEmployee,int plid, int TemplateId)
    {
        _logger.LogInformation("AddNewEmployee called for EmpIDs: {EmpId}", string.Join(',',plEmployee?.Select(p=>p.EmplId).ToArray()));

        try
        {
            await _emplService.AddNewEmployeesAsync(plEmployee);
            PlForecastRepository plForecastRepository = new PlForecastRepository(_context, _config);
            try
            {
                await plForecastRepository.CalculateRevenueCost(plid, TemplateId, "");
            }
            catch (Exception ex)
            {
                //logger.LogError(ex, "Error Calculation Revenue for {PlanId}", result.PlId);
            }
            return Ok();
        }
        catch (Exception ex)
            {
            _logger.LogError(ex, "Error adding new employee");
            return StatusCode(500, new { Message = "Failed to add new employee.", Error = ex.Message });
        }
    }

    [HttpDelete("DeleteEmployee/{id}")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        _logger.LogInformation("DeleteEmployee called for EmployeeId: {EmployeeId}", id);

        try
        {
            var result = await _emplService.DeleteEmployeeAsync(id);
            if (!result)
                return NotFound(new { Message = $"Employee with Id {id} not found." });

            return Ok(new { Message = $"Employee with Id {id} deleted successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting employee with ID: {EmployeeId}", id);
            return StatusCode(500, new { Message = "Failed to delete employee.", Error = ex.Message });
        }
    }

}