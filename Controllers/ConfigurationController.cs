using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using PlanningAPI.Helpers;
using PlanningAPI.Models;
using WebApi.Controllers;
using WebApi.Services;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {


        private readonly MydatabaseContext _context;
        private readonly ILogger<ProjectController> _logger;
        private readonly ConfigurationHelper _configurationHelper;
        private readonly HolidayCalendarRepository _holidayCalendarRepository;



        public ConfigurationController(ILogger<ProjectController> logger, MydatabaseContext context)
        {
            _context = context;
            _logger = logger;
            _configurationHelper = new ConfigurationHelper(_context);
        }

        [HttpGet("GetAllConfigValuesByProject/{projectId}")]
        public async Task<IActionResult> GetAllConfigValuesByProject(string projectId)
        {
            try
            {
                var configValues = await _configurationHelper.GetAllConfigValuesByProject(projectId);
                return Ok(configValues);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching config values for ProjectId: {ProjectId}", projectId);
                return StatusCode(500, "Internal server error while retrieving config values.");
            }
        }

        [HttpGet("GetConfigValueByName/{name}")]
        public async Task<IActionResult> GetConfigValueByName(string name)
        {
            try
            {
                var configValue = await _configurationHelper.GetConfigValueByName(name);
                if (configValue == null)
                    return NotFound($"Config value with name '{name}' not found.");

                return Ok(configValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting config value by name: {Name}", name);
                return StatusCode(500, "Internal server error while reading config value.");
            }
        }

        [HttpPost("AddConfigValue")]
        public async Task<IActionResult> AddConfigValue([FromBody] PlConfigValue plConfigValue)
        {
            try
            {
                var result = await _configurationHelper.AddConfigValue(plConfigValue);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding config value.");
                return StatusCode(500, "Internal server error while adding config value.");
            }
        }

        [HttpPut("UpdateConfigValue")]
        public async Task<IActionResult> UpdateConfigValue([FromBody] PlConfigValue plConfigValue)
        {
            try
            {
                var result = await _configurationHelper.UpdateConfigValue(plConfigValue);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating config value.");
                return StatusCode(500, "Internal server error while updating config value.");
            }
        }
        [HttpPut("BulkUpsertConfigsAsync")]
        public async Task<IActionResult> BulkUpsertConfigsAsync(List<PlConfigValue> plConfigValue)
        {
            try
            {
                await _configurationHelper.BulkUpsertAsync(plConfigValue);
                return Ok("Success");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating config value.");
                return StatusCode(500, "Internal server error while updating config value.");
            }
        }
        [HttpPut("UpdateConfigValues")]
        public async Task<IActionResult> UpdateConfigValues([FromBody] List<PlConfigValue> plConfigValues)
        {
            if (plConfigValues == null || !plConfigValues.Any())
                return BadRequest("No records provided");
            try
            {
                var result = await _configurationHelper.UpdateConfigValuesAsync(plConfigValues);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating config value.");
                return StatusCode(500, "Internal server error while updating config value.");
            }
        }

        [HttpDelete("DeleteConfigValueByName/{name}")]
        public async Task<IActionResult> DeleteConfigValueByName(string name)
        {
            try
            {
                var success = await _configurationHelper.DeleteConfigValueByName(name);
                if (!success)
                    return NotFound($"Config value with name '{name}' not found.");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting config value by name: {Name}", name);
                return StatusCode(500, "Internal server error while deleting config value.");
            }
        }

        [HttpDelete("DeleteConfigValueById/{id}")]
        public async Task<IActionResult> DeleteConfigValueById(int id)
        {
            try
            {
                var success = await _configurationHelper.DeleteConfigValueById(id);
                if (!success)
                    return NotFound($"Config value with ID '{id}' not found.");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting config value by ID: {Id}", id);
                return StatusCode(500, "Internal server error while deleting config value.");
            }
        }

        [HttpGet("GetAllHolidayCalenderAsync")]
        public async Task<IEnumerable<Holidaycalender>> GetAllHolidayCalenderAsync()
        {
            return await _configurationHelper.GetAllHolidayCalenderAsync();
        }

        [HttpGet("GetHolidayCalenderByIdAsync/{id}")]
        public async Task<Holidaycalender?> GetHolidayCalenderByIdAsync(int id)
        {
            return await _configurationHelper.GetHolidayCalenderByIdAsync(id);
        }

        [HttpPost("AddHolidayCalenderAsync")]
        public async Task<Holidaycalender> AddHolidayCalenderAsync([FromBody] Holidaycalender holiday)
        {
            try
            {
                return await _configurationHelper.AddHolidayCalenderAsync(holiday);
            }
            catch (DbUpdateException dbEx) // EF Core database update errors
            {
                if (dbEx.InnerException is PostgresException innerPgEx)
                {
                    Console.WriteLine($"Postgres Error: {innerPgEx.SqlState} - {innerPgEx.MessageText}");
                    Console.WriteLine($"Detail: {innerPgEx.Detail}");
                    if (innerPgEx.SqlState == "23505")
                        throw new Exception("Holiday already set for date - " + holiday.Date.ToString());
                }
                else
                {
                    Console.WriteLine($"DbUpdateException: {dbEx.Message}");
                }
                throw;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpPut("UpdateHolidayCalenderAsync")]
        public async Task<bool> UpdateHolidayCalenderAsync([FromBody] Holidaycalender holiday)
        {

            try
            {
                return await _configurationHelper.UpdateHolidayCalenderAsync(holiday);
            }
            catch (DbUpdateException dbEx) // EF Core database update errors
            {
                if (dbEx.InnerException is PostgresException innerPgEx)
                {
                    Console.WriteLine($"Postgres Error: {innerPgEx.SqlState} - {innerPgEx.MessageText}");
                    Console.WriteLine($"Detail: {innerPgEx.Detail}");
                    if (innerPgEx.SqlState == "23505")
                        throw new Exception("Holiday already set for date - " + holiday.Date.ToString());
                }
                else
                {
                    Console.WriteLine($"DbUpdateException: {dbEx.Message}");
                }
                throw;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [HttpDelete("DeleteHolidayCalenderAsync/{id}")]
        public async Task<bool> DeleteHolidayCalenderAsync(int id)
        {
            return await _configurationHelper.DeleteHolidayCalenderAsync(id);

        }

        [HttpDelete("bulk-delete-config")]
        public async Task<IActionResult> BulkDeleteConfigValues([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
                return BadRequest("No IDs provided for deletion.");

            var configsToDelete = await _context.PlConfigValues
                                                .Where(c => ids.Contains(c.Id))
                                                .ToListAsync();

            if (!configsToDelete.Any())
                return NotFound("No matching records found to delete.");

            _context.PlConfigValues.RemoveRange(configsToDelete);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("bulk-upsert")]
        public async Task<IActionResult> BulkUpsertConfigValues([FromBody] List<PlConfigValue> configValues)
        {
            if (configValues == null || !configValues.Any())
                return BadRequest("No records provided");

            // Extract all (name, projId) pairs
            var keys = configValues.Select(c => new { c.Name, c.ProjId }).ToList();

            // Load existing records from DB that match these keys
            var existingConfigs = await _context.PlConfigValues
                .Where(c => keys.Any(k => k.Name == c.Name && k.ProjId == c.ProjId))
                .ToListAsync();

            foreach (var config in configValues)
            {
                // Try to find matching record
                var existing = existingConfigs.FirstOrDefault(c => c.Name == config.Name && c.ProjId == config.ProjId);

                if (existing != null)
                {
                    // Update existing record
                    existing.Value = config.Value;
                    existing.CreatedAt = config.CreatedAt ?? existing.CreatedAt; // optional: keep existing timestamp if not provided
                }
                else
                {
                    // Insert new record
                    config.CreatedAt = config.CreatedAt ?? DateTime.UtcNow;
                    _context.PlConfigValues.Add(config);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                // Handle unique constraint conflicts or other DB errors
                return Conflict($"Database update failed: {ex.Message}");
            }

            return NoContent();
        }

        [HttpPost("bulk-upsert-sql")]
        public async Task<IActionResult> BulkUpsertConfigValuesSql([FromBody] List<PlConfigValue> configValues)
        {
            if (configValues == null || !configValues.Any())
                return BadRequest("No records provided");

            // Build parameterized VALUES list
            var parameters = new List<Npgsql.NpgsqlParameter>();
            var valueRows = new List<string>();
            int index = 0;

            foreach (var config in configValues)
            {
                var nameParam = new Npgsql.NpgsqlParameter($"@p{index}_name", config.Name);
                var valueParam = new Npgsql.NpgsqlParameter($"@p{index}_value", (object?)config.Value ?? DBNull.Value);
                var createdAtParam = new Npgsql.NpgsqlParameter($"@p{index}_created_at", (object?)config.CreatedAt ?? DateTime.UtcNow);
                var projIdParam = new Npgsql.NpgsqlParameter($"@p{index}_proj_id", config.ProjId);

                parameters.AddRange(new[] { nameParam, valueParam, createdAtParam, projIdParam });

                valueRows.Add($"({nameParam.ParameterName}, {valueParam.ParameterName}, {createdAtParam.ParameterName}, {projIdParam.ParameterName})");

                index++;
            }

            var sql = $@"
        INSERT INTO public.pl_config_values (name, value, created_at, proj_id)
        VALUES {string.Join(", ", valueRows)}
        ON CONFLICT (name, proj_id) 
        DO UPDATE SET
            value = EXCLUDED.value,
            created_at = EXCLUDED.created_at;
    ";

            try
            {
                await _context.Database.ExecuteSqlRawAsync(sql, parameters.ToArray());
            }
            catch (Exception ex)
            {
                return Conflict($"Bulk upsert failed: {ex.Message}");
            }

            return NoContent();
        }

        private bool ConfigValueExists(int id)
        {
            return _context.PlConfigValues.Any(e => e.Id == id);
        }

    }
}
