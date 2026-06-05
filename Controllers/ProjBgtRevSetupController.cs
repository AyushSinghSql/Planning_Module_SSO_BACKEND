using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlanningAPI.Models;
using WebApi.Repositories;

namespace PlanningAPI.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class ProjBgtRevSetupController : ControllerBase
    {
        private readonly IProjBgtRevSetupRepository _repository;

        public ProjBgtRevSetupController(IProjBgtRevSetupRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjBgtRevSetup>>> GetAll()
        {
            var result = await _repository.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProjBgtRevSetup>> GetById(int id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpGet("GetByProjectId/{Projid}/{Version}/{BgtType}")]
        public async Task<ActionResult<ProjBgtRevSetup>> GetByProjectId(string Projid, int Version, string BgtType)
        {
            var item = await _repository.GetByProjIdAsync(Projid, Version, BgtType);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpGet("GetByBudgetId/{Pl_id}")]
        public async Task<ActionResult<ProjBgtRevSetup>> GetByBudgetId(int pl_id)
        {
            var item = await _repository.GetByProjIdAsync(pl_id);
            if (item == null) return NotFound();
            return Ok(item);
        }
        [HttpPost("upsert")]
        public async Task<IActionResult> Upsert([FromBody] ProjBgtRevSetup entity)
        {
            try
            {
                var result = await _repository.UpsertAsync(entity);

                // If it's a new record, return Created
                if (entity.Id == 0 || result.Id == entity.Id)
                {

                    return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
                }

                // Otherwise, just return 204 NoContent
                return NoContent();
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error in Upsert operation for ProjBgtRevSetup");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProjBgtRevSetup entity)
        {
            if (id != entity.Id) return BadRequest("ID mismatch.");
            var success = await _repository.UpdateAsync(entity);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _repository.DeleteAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
