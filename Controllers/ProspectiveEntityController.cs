using Microsoft.AspNetCore.Mvc;
using PlanningAPI.Repositories;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProspectiveEntityController : ControllerBase
    {
        private readonly IProspectiveEntityRepository _repository;

        public ProspectiveEntityController(IProspectiveEntityRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _repository.GetAllAsync());

        [HttpGet("type/{type}")]
        public async Task<IActionResult> GetByType(string type)
        {
            var result = await _repository.GetByTypeAsync(type);
            if (!result.Any())
                return NotFound($"No prospective entities found for type '{type}'.");

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _repository.GetByIdAsync(id);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProspectiveEntity entity)
        {
            await _repository.AddAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = entity.ProspectiveId }, entity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProspectiveEntity entity)
        {
            if (id != entity.ProspectiveId) return BadRequest();

            if (!await _repository.ExistsAsync(id)) return NotFound();

            await _repository.UpdateAsync(entity);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _repository.ExistsAsync(id)) return NotFound();

            await _repository.DeleteAsync(id);
            return NoContent();
        }
    }

}
