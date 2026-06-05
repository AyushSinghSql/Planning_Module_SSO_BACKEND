using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PlanningAPI.Models;
using System;
using System.Threading.Tasks;

[ApiController]
[Route("[controller]")]
public class ProjVendRtController : ControllerBase
{
    private readonly IProjVendRtRepository _repository;
    private readonly ILogger<ProjVendRtController> _logger;
    private readonly MydatabaseContext _context;

    public ProjVendRtController(IProjVendRtRepository repository, ILogger<ProjVendRtController> logger, MydatabaseContext context)
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
            _logger.LogError(ex, "Failed to retrieve ProjVendRt records.");
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
            _logger.LogError(ex, "Failed to retrieve ProjVendRt with ID {Id}", id);
            return StatusCode(500, "Internal server error.");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(ProjVendRt entity)
    {
        var created = await _repository.AddAsync(entity);
        return CreatedAtAction(nameof(GetById), new { id = created.ProjVendRtKey }, created);
    }

    [HttpPatch("bulk-billingrate")]
    public async Task<IActionResult> BulkUpdateBillingRateFast([FromBody] List<ProjVendRt> rates)
    {
        foreach (var rate in rates)
        {
            var entity = new ProjVendRt
            {
                ProjVendRtKey = rate.ProjVendRtKey,
                BillRtAmt = rate.BillRtAmt
            };

            _context.ProjVendRts.Attach(entity);
            _context.Entry(entity).Property(e => e.BillRtAmt).IsModified = true;
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ProjVendRt entity)
    {
        if (id != entity.ProjVendRtKey)
            return BadRequest("ID mismatch.");

        var updated = await _repository.UpdateAsync(entity);
        if (!updated)
            return NotFound();

        return NoContent();
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
            _logger.LogError(ex, "Failed to delete ProjVendRt with ID {Id}", id);
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
            var entities = await _context.ProjVendRts
                    .Where(x => ids.Contains(x.ProjVendRtKey))
                    .ToListAsync();

            if (!entities.Any())
                return BadRequest("No IDs found.");

            _context.ProjVendRts.RemoveRange(entities);
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
