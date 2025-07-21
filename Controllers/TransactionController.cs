using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ol_api_dotnet.Data;
using ol_api_dotnet.Models;
using ol_api_dotnet.Services;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic; // Added for List

namespace ol_api_dotnet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly CustomEventService _customEventService;
        private readonly EarningRuleEngineService _earningRuleEngineService;

        public TransactionController(AppDbContext db, CustomEventService customEventService, EarningRuleEngineService earningRuleEngineService)
        {
            _db = db;
            _customEventService = customEventService;
            _earningRuleEngineService = earningRuleEngineService;
        }

        // POST: api/transaction/simulate
        [HttpPost("simulate")]
        public async Task<IActionResult> SimulatePurchase([FromBody] TransactionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var transaction = new Transaction
            {
                UserId = dto.UserId,
                Amount = dto.Amount,
                Type = dto.Type,
                Date = dto.Date ?? DateTime.UtcNow
            };

            var earnedPoints = await _earningRuleEngineService.EvaluateTransaction(transaction);

            return Ok(new { pointsEarned = earnedPoints });
        }

        // POST: api/transaction
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TransactionDto dto)
        {
            // Debug: Log the received data
            Console.WriteLine($"🔍 Received transaction request - UserId: {dto?.UserId}, Amount: {dto?.Amount}, Type: {dto?.Type}");

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
                Console.WriteLine($"❌ Model validation failed: {string.Join(", ", errors)}");
                return BadRequest(new { message = "Invalid data provided", errors = errors });
            }

            // Validate user exists
            var user = await _db.Users.FindAsync(dto.UserId);
            if (user == null)
                return BadRequest(new { message = "User not found" });

            Console.WriteLine($"✅ User validation passed for user: {user.Id}");

            // Create transaction
            var transaction = new Transaction
            {
                UserId = dto.UserId,
                Amount = dto.Amount,
                Type = dto.Type,
                Date = dto.Date ?? DateTime.UtcNow
            };
            _db.Transactions.Add(transaction);

            // Evaluate earning rules
            var earnedPoints = await _earningRuleEngineService.EvaluateTransaction(transaction);
            Console.WriteLine($"[TransactionController] Points earned from rules: {earnedPoints}");

            if (earnedPoints > 0)
            {
                // Update user's points
                var userPoints = await _db.Points.FirstOrDefaultAsync(p => p.UserId == dto.UserId);
                if (userPoints == null)
                {
                    userPoints = new Points { UserId = dto.UserId, Balance = 0 };
                    _db.Points.Add(userPoints);
                }

                userPoints.Balance += earnedPoints;

                // Create points earning transaction
                var pointsTransaction = new Transaction
                {
                    UserId = dto.UserId,
                    Amount = earnedPoints,
                    Type = "points_earned",
                    Date = DateTime.UtcNow,
                    Description = "Points earned from purchase"
                };

                _db.Transactions.Add(pointsTransaction);
            }

            await _db.SaveChangesAsync();
            Console.WriteLine($"✅ Transaction and points saved.");

            if (earnedPoints > 0)
            {
                var userPoints = await _db.Points.FirstOrDefaultAsync(p => p.UserId == dto.UserId);
                // Create custom event for points earned
                await _customEventService.CreateEventAsync(
                    dto.UserId,
                    "points_earned",
                    new Dictionary<string, object>
                    {
                        { "points", earnedPoints },
                        { "sourceTransactionId", transaction.Id },
                        { "newBalance", userPoints?.Balance ?? 0 }
                    }
                );
            }

            return Ok(new
            {
                transaction,
                pointsEarned = earnedPoints,
                currentBalance = (await _db.Points.FirstOrDefaultAsync(p => p.UserId == dto.UserId))?.Balance ?? 0
            });
        }

        // GET: api/transaction
        [HttpGet]
        public async Task<IActionResult> List([FromQuery] int userId, [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var query = _db.Transactions.Where(t => t.UserId == userId);
            var total = await query.CountAsync();
            var transactions = await query
                .OrderByDescending(t => t.Date)
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToListAsync();
            return Ok(new { transactions, total, page, totalPages = (int)Math.Ceiling(total / (double)limit) });
        }

        // GET: api/transaction/events
        [HttpGet("events")]
        public async Task<IActionResult> GetUserEvents([FromQuery] int userId, [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var events = await _customEventService.GetUserEventsAsync(userId, page, limit);
            var total = await _db.CustomEvents.Where(e => e.UserId == userId).CountAsync();
            
            return Ok(new 
            { 
                events, 
                total, 
                page, 
                totalPages = (int)Math.Ceiling(total / (double)limit) 
            });
        }

        // GET: api/transaction/test
        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { message = "TransactionController is working", eventService = _customEventService != null });
        }

        // POST: api/transaction/seed
        [HttpPost("seed")]
        public async Task<IActionResult> SeedTestData()
        {
            try
            {
                // Check if users already exist
                if (await _db.Users.AnyAsync())
                {
                    return Ok(new { message = "Database already seeded", userCount = await _db.Users.CountAsync() });
                }

                // Create test users
                var users = new List<User>
                {
                    new User { FirstName = "Alice", LastName = "Smith", Email = "alice@example.com", PasswordHash = "hash1", Role = "user" },
                    new User { FirstName = "Bob", LastName = "Jones", Email = "bob@example.com", PasswordHash = "hash2", Role = "user" },
                    new User { FirstName = "Carol", LastName = "Lee", Email = "carol@example.com", PasswordHash = "hash3", Role = "user" }
                };
                
                _db.Users.AddRange(users);
                await _db.SaveChangesAsync();

                // Get the created users
                var alice = await _db.Users.FirstAsync(u => u.Email == "alice@example.com");
                var bob = await _db.Users.FirstAsync(u => u.Email == "bob@example.com");
                var carol = await _db.Users.FirstAsync(u => u.Email == "carol@example.com");

                // Create initial points for users
                var points = new List<Points>
                {
                    new Points { UserId = alice.Id, Balance = 500 },
                    new Points { UserId = bob.Id, Balance = 300 },
                    new Points { UserId = carol.Id, Balance = 800 }
                };

                _db.Points.AddRange(points);
                await _db.SaveChangesAsync();

                return Ok(new 
                { 
                    message = "Test data seeded successfully", 
                    users = users.Select(u => new { u.Id, u.FirstName, u.LastName, u.Email }),
                    points = points.Select(p => new { p.UserId, p.Balance })
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to seed data", error = ex.Message });
            }
        }
    }
} 