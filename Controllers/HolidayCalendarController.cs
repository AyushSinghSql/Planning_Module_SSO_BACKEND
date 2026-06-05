using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using PlanningAPI.Models;

[ApiController]
[Route("[controller]")]
public class HolidayCalendarController : ControllerBase
{
    private readonly IHolidayCalendarRepository _repository;
    private readonly ILogger<HolidayCalendarController> _logger;

    public HolidayCalendarController(IHolidayCalendarRepository repository, ILogger<HolidayCalendarController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var holidays = await _repository.GetAllAsync();
        return Ok(holidays);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var holiday = await _repository.GetByIdAsync(id);
        if (holiday == null) return NotFound();
        return Ok(holiday);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Holidaycalender holiday)
    {
        var result = await _repository.AddAsync(holiday);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Holidaycalender holiday)
    {
        try
        {
            if (id != holiday.Id) return BadRequest("ID mismatch.");
            var updated = await _repository.UpdateAsync(holiday);
            return updated ? NoContent() : NotFound();
        }
        catch(Exception ex)
        {
            throw ex;
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _repository.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
