using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;
using PlanningAPI.Repositories;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectPlcRatesController : ControllerBase
    {
        private readonly IProjectPlcRateRepository _repository;
        private readonly MydatabaseContext _context;


        public ProjectPlcRatesController(IProjectPlcRateRepository repository, MydatabaseContext context)
        {
            _repository = repository;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectPlcRate>>> GetAll()
        {
            var rates = await _repository.GetAllAsync();
            return Ok(rates);
        }

        [HttpGet("{PlcCode}")]
        public async Task<ActionResult<ProjectPlcRate>> Get(string PlcCode)
        {
            var rate = await _repository.GetByPlcCode(PlcCode);
            if (rate == null)
                return NotFound();
            return Ok(rate);
        }

        [HttpGet("GetById/{id}")]
        public async Task<ActionResult<ProjectPlcRate>> GetById(int id)
        {
            var rate = await _repository.GetByIdAsync(id);
            if (rate == null)
                return NotFound();
            return Ok(rate);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] ProjectPlcRate rate)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _repository.AddAsync(rate);
            return CreatedAtAction(nameof(GetById), new { id = rate.Id }, rate);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] ProjectPlcRate rate)
        {
            if (id != rate.Id)
                return BadRequest("ID mismatch.");

            if (!await _repository.ExistsAsync(id))
                return NotFound();

            await _repository.UpdateAsync(rate);
            return NoContent();
        }

        [HttpPatch("bulk-billingrate")]
        public async Task<IActionResult> BulkUpdateBillingRateFast([FromBody] List<ProjectPlcRate> rates)
        {
            foreach (var rate in rates)
            {
                var entity = new ProjectPlcRate { Id = rate.Id, BillingRate = rate.BillingRate };
                _context.ProjectPlcRates.Attach(entity);
                _context.Entry(entity).Property(r => r.BillingRate).IsModified = true;
            }
            await _context.SaveChangesAsync();
            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            if (!await _repository.ExistsAsync(id))
                return NotFound();

            await _repository.DeleteAsync(id);
            return NoContent();
        }

        [HttpDelete("bulk-delete")]
        public async Task<IActionResult> DeleteMultiple([FromBody] List<int> ids)
        {

            if (ids == null || ids.Count == 0)
                return BadRequest("No IDs provided.");
            try
            {
                var entities = await _context.ProjectPlcRates
                        .Where(x => ids.Contains(x.Id))
                        .ToListAsync();

                if (!entities.Any())
                    return BadRequest("No IDs Found.");

                _context.ProjectPlcRates.RemoveRange(entities);
                await _context.SaveChangesAsync();
                return NoContent();

            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Failed to delete ProjVendRt records with IDs {@Ids}", ids);
                return StatusCode(500, "Internal server error.");
            }
        }
    }

}
