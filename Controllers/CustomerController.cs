using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ol_api_dotnet.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System;

namespace ol_api_dotnet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CustomerController : ControllerBase
    {
        private readonly AppDbContext _db;

        public CustomerController(AppDbContext db)
        {
            _db = db;
        }

        // GET: api/customer/me
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            // Log all claims
            Console.WriteLine("[DEBUG] Claims: " + string.Join(", ", User.Claims.Select(c => c.Type + "=" + c.Value)));
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub" || c.Type == "userId");
            Console.WriteLine($"[DEBUG] userIdClaim: {userIdClaim?.Type}={userIdClaim?.Value}");
            if (userIdClaim == null)
            {
                Console.WriteLine("[DEBUG] No userId claim found");
                return Unauthorized(new { message = "User ID not found in token" });
            }
            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                Console.WriteLine($"[DEBUG] Invalid userId claim value: {userIdClaim.Value}");
                return Unauthorized(new { message = "Invalid user ID in token" });
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            Console.WriteLine($"[DEBUG] User lookup for userId {userId}: {(user == null ? "NOT FOUND" : "FOUND")}");
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            // Return a minimal customer profile
            Console.WriteLine($"[DEBUG] Returning profile for userId: {userId}");
            return Ok(new
            {
                customerId = user.Id,
                firstName = user.FirstName,
                lastName = user.LastName,
                email = user.Email,
                tier = user.Role, // or user.Level if available
                createdAt = user.CreatedAt,
            });
        }

        // GET: api/customer/all
        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var users = await _db.Users
                .Select(u => new { u.Id, Name = u.FirstName + " " + u.LastName, u.Email })
                .ToListAsync();
            return Ok(users);
        }
    }
} 