using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlanningAPI.Models;
using PlanningAPI.Repositories;

namespace PlanningAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VendorEmployeeController : ControllerBase
    {
        private readonly IVendorEmployeeRepository _repository;

        public VendorEmployeeController(IVendorEmployeeRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VendorEmployee>>> GetAll()
        {
            var result = await _repository.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{vendEmplId}/{vendId}")]
        public async Task<ActionResult<VendorEmployee>> Get(string vendEmplId, string vendId)
        {
            var emp = await _repository.GetByIdAsync(vendEmplId, vendId);
            if (emp == null) return NotFound();
            return Ok(emp);
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] VendorEmployee employee)
        {
            await _repository.AddAsync(employee);
            return CreatedAtAction(nameof(Get), new { employee.VendEmplId, employee.VendId }, employee);
        }

        [HttpPut("{vendEmplId}/{vendId}")]
        public async Task<ActionResult> Update(string vendEmplId, string vendId, [FromBody] VendorEmployee employee)
        {
            if (vendEmplId != employee.VendEmplId || vendId != employee.VendId)
                return BadRequest("Mismatched keys");

            await _repository.UpdateAsync(employee);
            return NoContent();
        }

        [HttpDelete("{vendEmplId}/{vendId}")]
        public async Task<ActionResult> Delete(string vendEmplId, string vendId)
        {
            await _repository.DeleteAsync(vendEmplId, vendId);
            return NoContent();
        }
    }

}
