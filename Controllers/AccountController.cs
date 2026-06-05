using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanningAPI.Helpers;
using PlanningAPI.Models;
using WebApi.Controllers;
using WebApi.Services;

namespace PlanningAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {

        private readonly MydatabaseContext _context;
        private readonly ILogger<AccountController> _logger;
        public AccountController(ILogger<AccountController> logger, MydatabaseContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpPost("CreateAccount")]
        public async Task<IActionResult> CreateAccount([FromBody] Account Account)
        {
            try
            {
                if (Account == null)
                    return BadRequest();

                await _context.Accounts.AddAsync(Account);
                await _context.SaveChangesAsync();

                return Ok(Account);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("account_pkey") == true)
                {
                    return Conflict(new
                    {
                        message = $"Account with ID '{Account.AcctId}' already exists."
                    });
                }

                return StatusCode(500, "An unexpected database error occurred.");
            }
        }
        [HttpDelete("DeleteAccount/{AcctId}")]
        public async Task<IActionResult> DeleteAccount(string AcctId)
        {
            var Account = await _context.Accounts.FindAsync(AcctId);

            if (Account == null)
                return NotFound();

            _context.Accounts.Remove(Account);
            await _context.SaveChangesAsync();

            return Ok("Account deleted successfully");
        }

        [HttpGet("GetAccount/{AcctId}")]
        public async Task<IActionResult> GetAccount(string AcctId)
        {
            var Account = await _context.Accounts
                .FirstOrDefaultAsync(p => p.AcctId == AcctId);

            if (Account == null)
                return NotFound();

            return Ok(Account);
        }

        [HttpPut("UpdateAccount/{acctId}")]
        public async Task<IActionResult> UpdateAccount(string acctId, [FromBody] Account updatedAccount)
        {
            if (acctId != updatedAccount.AcctId)
                return BadRequest("Account ID mismatch");

            var account = await _context.Accounts.FindAsync(acctId);

            if (account == null)
                return NotFound("Account not found");

            // Update fields
            account.AcctName = updatedAccount.AcctName;
            account.ActiveFlag = updatedAccount.ActiveFlag;
            account.L1AcctName = updatedAccount.L1AcctName;
            account.L2AcctName = updatedAccount.L2AcctName;
            account.L3AcctName = updatedAccount.L3AcctName;
            account.L4AcctName = updatedAccount.L4AcctName;
            account.L5AcctName = updatedAccount.L5AcctName;
            account.L6AcctName = updatedAccount.L6AcctName;
            account.L7AcctName = updatedAccount.L7AcctName;
            account.LvlNo = updatedAccount.LvlNo;
            account.SAcctTypeCd = updatedAccount.SAcctTypeCd;
            account.ModifiedBy = updatedAccount.ModifiedBy;

            account.Updatedat = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(account);
        }


        [HttpPost("AddOrgAccount")]
        public async Task<IActionResult> AddOrgAccount([FromBody] OrgAccount orgAccount)
        {
            if (orgAccount == null)
                return BadRequest();

            var exists = await _context.OrgAccounts
                .AnyAsync(x => x.OrgId == orgAccount.OrgId && x.AcctId == orgAccount.AcctId);

            if (exists)
                return Conflict("OrgAccount mapping already exists");

            orgAccount.TimeStamp = DateTime.UtcNow;

            await _context.OrgAccounts.AddAsync(orgAccount);
            await _context.SaveChangesAsync();

            return Ok(orgAccount);
        }

        [HttpDelete("DeleteOrgAccount")]
        public async Task<IActionResult> DeleteOrgAccount(string orgId, string acctId)
        {
            var orgAccount = await _context.OrgAccounts
                .FirstOrDefaultAsync(x => x.OrgId == orgId && x.AcctId == acctId);

            if (orgAccount == null)
                return NotFound("Mapping not found");

            _context.OrgAccounts.Remove(orgAccount);
            await _context.SaveChangesAsync();

            return Ok("OrgAccount deleted successfully");
        }

        [HttpGet("GetOrgAccounts/{OrgId}")]
        public async Task<IActionResult> GetOrgAccounts(string OrgId)
        {
            var Accounts = await _context.OrgAccounts
                .Where(p => p.OrgId == OrgId).Select(p => p.Account).ToListAsync();

            if (Accounts == null)
                return NotFound();

            return Ok(Accounts);
        }
        [HttpGet("GetAllAccounts")]
        public async Task<IActionResult> GetAllAccounts()
        {
            var Accounts = await _context.Accounts.ToListAsync();

            if (Accounts == null)
                return NotFound();

            return Ok(Accounts);
        }

        [HttpPost("BulkSyncOrgAccounts")]
        public async Task<IActionResult> BulkSyncOrgAccounts(string orgId, List<OrgAccount> accounts)
        {
            var existing = _context.OrgAccounts.Where(x => x.OrgId == orgId);
            _context.OrgAccounts.RemoveRange(existing);

            foreach (var acc in accounts)
                acc.TimeStamp = DateTime.UtcNow;

            await _context.OrgAccounts.AddRangeAsync(accounts);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
