using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ol_api_dotnet.Data;
using ol_api_dotnet.Models;
using System.Linq;
using System.Threading.Tasks;

namespace ol_api_dotnet.Controllers
{
    [ApiController]
    [Route("api/loyalty")]
    public class LoyaltyController : ControllerBase
    {
        private readonly AppDbContext _db;
        public LoyaltyController(AppDbContext db) { _db = db; }

        // GET: api/loyalty/user?userId=123
        [HttpGet("user")]
        public async Task<IActionResult> GetUserLoyalty([FromQuery] int userId)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user == null) return NotFound(new { message = "User not found" });

            var points = await _db.Points.Where(p => p.UserId == userId).Select(p => p.Balance).FirstOrDefaultAsync();
            var tier = await _db.Tiers.Where(t => t.UserId == userId).Select(t => t.Level).FirstOrDefaultAsync();
            var rewards = await _db.Rewards.ToListAsync(); // You can filter by user if needed
            var transactions = await _db.Transactions.Where(t => t.UserId == userId).OrderByDescending(t => t.Date).ToListAsync();

            return Ok(new
            {
                points = points,
                tier = tier ?? "Bronze",
                rewards = rewards,
                transactions = transactions
            });
        }
    }
} 