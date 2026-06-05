using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlanningAPI.Models;
using PlanningAPI.Repositories;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProjRevWrkPdController : ControllerBase
    {
        private readonly IProjRevWrkPdRepository _repository;
        private readonly ILogger<ProjRevWrkPdController> _logger;

        public ProjRevWrkPdController(IProjRevWrkPdRepository repository, ILogger<ProjRevWrkPdController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var items = await _repository.GetAllAsync();
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAll");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var item = await _repository.GetByIdAsync(id);
                if (item == null) return NotFound();
                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetById");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProjRevWrkPd entity)
        {
            try
            {
                var created = await _repository.AddAsync(entity);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Create");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProjRevWrkPd entity)
        {
            if (id != entity.Id)
                return BadRequest("ID mismatch");

            try
            {
                var success = await _repository.UpdateAsync(entity);
                if (!success) return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Update");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _repository.DeleteAsync(id);
                if (!success) return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Delete");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("filter")]
        public async Task<IActionResult> GetByFilter(
        [FromQuery] string projId,
        [FromQuery] int? versionNo,
        [FromQuery] string bgtType,
        [FromQuery] int? pl_id)
        {
            try
            {
                var results = await _repository.GetByFilterAsync(projId, versionNo, bgtType, pl_id);


                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByFilter endpoint (ProjId, VersionNo, BgtType)");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("filterdepr")]
        public async Task<IActionResult> GetByFilterdepr(
        [FromQuery] string projId,
        [FromQuery] int? versionNo,
        [FromQuery] string bgtType)
        {
            try
            {
                var results = await _repository.GetByFilterAsync(projId, versionNo, bgtType);


                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByFilter endpoint (ProjId, VersionNo, BgtType)");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpPost("upsert")]
        public async Task<IActionResult> Upsert([FromBody] ProjRevWrkPd entity)
        {
            try
            {
                var result = await _repository.UpsertAsync(entity);

                if (entity.Id == 0)
                    return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Upsert endpoint");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("AddRevenueForNB")]
        public async Task<IActionResult> AddRevenueForNB([FromBody] List<NB_Revenue> entity, int pl_id, string proj_id)
        {
            try
            {
                await _repository.AddRevenueForNBAsync(entity, pl_id, proj_id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddRevenueForNB endpoint");
                return StatusCode(500, "Internal server error");
            }
        }
    }

}
