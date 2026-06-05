using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;
using System;

namespace PlanningAPI.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    public class AnalgsRtController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public AnalgsRtController(MydatabaseContext context)
        {
            _context = context;
        }

        // GET: api/AnalgsRt
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _context.AnalgsRts
                .OrderByDescending(x => x.ClsPd)
                .ToListAsync();

            return Ok(result);
        }

        // GET: api/AnalgsRt/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var record = await _context.AnalgsRts.FindAsync(id);

            if (record == null)
                return NotFound();

            return Ok(record);
        }

        // POST: api/AnalgsRt
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AnalgsRt model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var exists = await _context.AnalgsRts
                .AnyAsync(x => x.AnalgId == model.AnalgId);

            if (exists)
                return Conflict($"AnalgId {model.AnalgId} already exists.");

            model.TimeStamp = DateTime.UtcNow;

            _context.AnalgsRts.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = model.AnalgId }, model);
        }

        // PUT: api/AnalgsRt/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AnalgsRt model)
        {
            if (id != model.AnalgId)
                return BadRequest("AnalgId mismatch.");

            var record = await _context.AnalgsRts.FindAsync(id);

            if (record == null)
                return NotFound();

            record.TotRev = model.TotRev;
            record.LabOnste = model.LabOnste;
            record.LabOnsteNonBill = model.LabOnsteNonBill;
            record.NonLabTrvl = model.NonLabTrvl;
            record.SubLab = model.SubLab;
            record.SubNonLab = model.SubNonLab;
            record.ClsPd = model.ClsPd;
            record.OvrwrteRt = model.OvrwrteRt;
            record.FyCd = model.FyCd;
            record.ActualAmt = model.ActualAmt;
            record.ModifiedBy = model.ModifiedBy;
            record.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/AnalgsRt/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var record = await _context.AnalgsRts.FindAsync(id);

            if (record == null)
                return NotFound();

            _context.AnalgsRts.Remove(record);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

}
