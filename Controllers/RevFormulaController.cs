using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlanningAPI.Models;
using PlanningAPI.Repositories;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RevFormulaController : ControllerBase
    {
        private readonly IRevFormulaRepository _repo;
        private readonly ILogger<RevFormulaController> _logger;

        public RevFormulaController(IRevFormulaRepository repo, ILogger<RevFormulaController> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var data = await _repo.GetAllAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all RevFormulas");
                return StatusCode(500);
            }
        }

        [HttpGet("{formulaCd}")]
        public async Task<IActionResult> Get(string formulaCd)
        {
            try
            {
                var item = await _repo.GetByIdAsync(formulaCd);
                return item == null ? NotFound() : Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get RevFormula with Code: {formulaCd}", formulaCd);
                return StatusCode(500);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(RevFormula model)
        {
            try
            {
                var created = await _repo.AddAsync(model);
                return CreatedAtAction(nameof(Get), new { formulaCd = created.FormulaCd }, created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create RevFormula");
                return StatusCode(500);
            }
        }

        [HttpPut("{formulaCd}")]
        public async Task<IActionResult> Update(string formulaCd, RevFormula model)
        {
            if (formulaCd != model.FormulaCd)
                return BadRequest();

            try
            {
                var success = await _repo.UpdateAsync(model);
                return success ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update RevFormula with Code: {formulaCd}", formulaCd);
                return StatusCode(500);
            }
        }

        [HttpDelete("{formulaCd}")]
        public async Task<IActionResult> Delete(string formulaCd)
        {
            try
            {
                var deleted = await _repo.DeleteAsync(formulaCd);
                return deleted ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete RevFormula with Code: {formulaCd}", formulaCd);
                return StatusCode(500);
            }
        }
    }

}
