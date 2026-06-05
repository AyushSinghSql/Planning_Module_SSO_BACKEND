using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ParametricRateController.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NbiPrmtrcRtController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public NbiPrmtrcRtController(MydatabaseContext context)
        {
            _context = context;
        }

        // GET: api/NbiPrmtrcRt
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NbiPrmtrcRt>>> GetAll()
        {
            return await _context.NbiPrmtrcRts.ToListAsync();
        }

        // GET: api/NbiPrmtrcRt/5
        [HttpGet("{id}")]
        public async Task<ActionResult<NbiPrmtrcRt>> GetById(int id)
        {
            var item = await _context.NbiPrmtrcRts.FindAsync(id);

            if (item == null)
                return NotFound();

            return item;
        }

        // POST: api/NbiPrmtrcRt
        [HttpPost]
        public async Task<ActionResult<NbiPrmtrcRt>> Create(NbiPrmtrcRt item)
        {
            _context.NbiPrmtrcRts.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = item.ParaId }, item);
        }

        // PUT: api/NbiPrmtrcRt/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, NbiPrmtrcRt item)
        {
            var entity = await _context.NbiPrmtrcRts.FindAsync(id);

            if (entity == null)
                return NotFound();

            entity.ContractType = item.ContractType;
            entity.DriverGrp = item.DriverGrp;
            entity.ProjId = item.ProjId;
            entity.LabOnste = item.LabOnste;
            entity.LabOffste = item.LabOffste;
            entity.NonLab = item.NonLab;
            entity.SubLab = item.SubLab;
            entity.SubNonLab = item.SubNonLab;
            entity.ClsPd = item.ClsPd;
            entity.FyCd = item.FyCd;
            entity.ModifiedBy = item.ModifiedBy;
            entity.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/NbiPrmtrcRt/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.NbiPrmtrcRts.FindAsync(id);
            if (item == null)
                return NotFound();

            _context.NbiPrmtrcRts.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ItemExists(int id)
        {
            return _context.NbiPrmtrcRts.Any(e => e.ParaId == id);
        }
    }
}