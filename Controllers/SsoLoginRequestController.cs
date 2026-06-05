using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using PlanningAPI.Models;

namespace PlanningAPI.Controllers
{
    [Authorize]
    [Route("/api/[controller]")]
    [ApiController]
    public class SsoLoginRequestController : ControllerBase 
    {
        private readonly MydatabaseContext _context;

        public SsoLoginRequestController(MydatabaseContext context)
        {
            _context = context;
        }

        [HttpPost("ssoLogin")]
        public async Task<IActionResult> SsoLogin()
        {
            var email = User.FindFirst("preferred_username")?.Value
                     ?? User.FindFirst(ClaimTypes.Upn)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized("Unable to retrieve email from token.");
            }

            var user = await _context.Users
                .Include(u => u.UserRole)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

            if (user == null)
            {
                return Forbid();
            }

            return Ok(new
            {
                userId = user.UserId,
                username = user.Username,
                fullName = user.FullName,
                role = user.UserRole?.RoleName,
                email = user.Email
            });
        }
    }
}