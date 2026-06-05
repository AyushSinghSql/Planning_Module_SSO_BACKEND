using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlanningAPI.Models;
using PlanningAPI.Repositories;
using System;
using System.Threading.Tasks;

[ApiController]
[Route("[controller]")]
public class ProjEmplRtController : ControllerBase
{
    private readonly IProjEmplRtRepository _repository;
    private readonly ILogger<ProjEmplRtController> _logger;
    private readonly MydatabaseContext _context;

    public ProjEmplRtController(IProjEmplRtRepository repository, ILogger<ProjEmplRtController> logger, MydatabaseContext context)
    {
        _repository = repository;
        _logger = logger;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var result = await _repository.GetAllAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ProjEmplRt records.");
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var result = await _repository.GetByIdAsync(id);
            if (result == null)
                return NotFound();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve ProjEmplRt with ID {Id}", id);
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpPatch("bulk-billingrate")]
    public async Task<IActionResult> BulkUpdateBillingRateFast([FromBody] List<ProjEmplRt> rates)
    {
        foreach (var rate in rates)
        {
            var entity = new ProjEmplRt { ProjEmplRtKey = rate.ProjEmplRtKey, BillRtAmt = rate.BillRtAmt };
            _context.ProjEmplRts.Attach(entity);
            _context.Entry(entity).Property(r => r.BillRtAmt).IsModified = true;
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> Create(ProjEmplRt entity)
    {
        var created = await _repository.AddAsync(entity);
        return CreatedAtAction(nameof(GetById), new { id = created.ProjEmplRtKey }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ProjEmplRt entity)
    {
        if (id != entity.ProjEmplRtKey)
            return BadRequest("ID mismatch.");

        try
        {
            var updated = await _repository.UpdateAsync(entity);
            if (!updated)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update ProjEmplRt with ID {Id}", id);
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _repository.DeleteAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete ProjEmplRt with ID {Id}", id);
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpDelete("bulk-delete")]
    public async Task<IActionResult> DeleteMultiple([FromBody] List<int> ids)
    {

        if (ids == null || ids.Count == 0)
            return BadRequest("No IDs provided.");
        try
        {
            var entities = await _context.ProjEmplRts
                    .Where(x => ids.Contains(x.ProjEmplRtKey))
                    .ToListAsync();

            if (!entities.Any())
                return BadRequest("No IDs Found.");

            _context.ProjEmplRts.RemoveRange(entities);
            await _context.SaveChangesAsync();
            return NoContent();

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete ProjVendRt records with IDs {@Ids}", ids);
            return StatusCode(500, "Internal server error.");
        }
    }
}
