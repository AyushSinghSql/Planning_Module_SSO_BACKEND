using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Models;
using System;

namespace PlanningAPI.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    public class ChartOfAccountsController : ControllerBase
    {
        private readonly MydatabaseContext _context;

        public ChartOfAccountsController(MydatabaseContext context)
        {
            _context = context;
        }

        // 🔹 GET: api/ChartOfAccounts
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var accounts = await _context.ChartOfAccounts
                .OrderBy(x => x.AccountId)
                .ToListAsync();

            return Ok(accounts);
        }
        [HttpGet("GetAllJWT")]
        [Authorize]
        public async Task<IActionResult> GetAllJWT()
        {
            var accounts = await _context.ChartOfAccounts
                .OrderBy(x => x.AccountId)
                .ToListAsync();

            return Ok(accounts);
        }
        // 🔹 GET: api/ChartOfAccounts/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var account = await _context.ChartOfAccounts.FindAsync(id);

            if (account == null)
                return NotFound();

            return Ok(account);
        }

        // 🔹 POST: api/ChartOfAccounts
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ChartOfAccount model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var exists = await _context.ChartOfAccounts
                .AnyAsync(x => x.AccountId == model.AccountId);

            if (exists)
                return Conflict($"AccountId '{model.AccountId}' already exists.");

            model.CreatedAt = DateTime.UtcNow;
            model.UpdatedAt = DateTime.UtcNow;

            _context.ChartOfAccounts.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = model.AccountId }, model);
        }

        // 🔹 PUT: api/ChartOfAccounts/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] ChartOfAccount model)
        {
            if (id != model.AccountId)
                return BadRequest("AccountId mismatch.");

            var account = await _context.ChartOfAccounts.FindAsync(id);

            if (account == null)
                return NotFound();

            account.AccountName = model.AccountName;
            account.CostType = model.CostType;
            account.AccountType = model.AccountType;
            account.BudgetSheet = model.BudgetSheet;
            account.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // 🔹 DELETE: api/ChartOfAccounts/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var account = await _context.ChartOfAccounts.FindAsync(id);

            if (account == null)
                return NotFound();

            _context.ChartOfAccounts.Remove(account);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

}
