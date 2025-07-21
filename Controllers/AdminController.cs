using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ol_api_dotnet.Models;
using ol_api_dotnet.Data;

namespace ol_api_dotnet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _db;
        public AdminController(AppDbContext db)
        {
            _db = db;
        }

        // GET: api/admin?page=1&perPage=10
        [HttpGet]
        public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int perPage = 10)
        {
            var query = _db.Admins.AsQueryable();
            var total = await query.CountAsync();
            var admins = await query.Skip((page - 1) * perPage).Take(perPage).ToListAsync();
            return Ok(new { admins, total, page, perPage, totalPages = (int)Math.Ceiling(total / (double)perPage) });
        }

        // GET: api/admin/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var admin = await _db.Admins.FindAsync(id);
            if (admin == null) return NotFound();
            return Ok(admin);
        }

        // POST: api/admin
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AdminCreateDto dto)
        {
            if (await _db.Admins.AnyAsync(a => a.Email == dto.Email))
                return BadRequest(new { message = "Email already exists" });
            var admin = new Admin
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Phone = dto.Phone,
                External = dto.External,
                ApiKey = dto.ApiKey
            };
            _db.Admins.Add(admin);
            await _db.SaveChangesAsync();
            return Ok(admin);
        }

        // PUT: api/admin/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, [FromBody] AdminEditDto dto)
        {
            var admin = await _db.Admins.FindAsync(id);
            if (admin == null) return NotFound();
            if (!string.IsNullOrEmpty(dto.FirstName)) admin.FirstName = dto.FirstName;
            if (!string.IsNullOrEmpty(dto.LastName)) admin.LastName = dto.LastName;
            if (!string.IsNullOrEmpty(dto.Email)) admin.Email = dto.Email;
            if (!string.IsNullOrEmpty(dto.Password)) admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            if (!string.IsNullOrEmpty(dto.Phone)) admin.Phone = dto.Phone;
            if (dto.External.HasValue) admin.External = dto.External.Value;
            if (!string.IsNullOrEmpty(dto.ApiKey)) admin.ApiKey = dto.ApiKey;
            await _db.SaveChangesAsync();
            return Ok(admin);
        }

        // GET: api/admin/analytics
        [HttpGet("analytics")]
        public IActionResult GetAnalytics()
        {
            var totalMembers = _db.Users.Count();
            var pointsCirculating = _db.Points.Sum(p => (int?)p.Balance) ?? 0;
            var averagePointsPerMember = totalMembers > 0 ? pointsCirculating / totalMembers : 0;
            var totalTransactions = _db.Transactions.Count();
            var totalSpent = _db.Transactions.Where(t => t.Type == "purchase").Sum(t => (decimal?)t.Amount) ?? 0;
            var systemRevenue = totalSpent; // For now, treat as same

            // Tier distribution
            var tierCounts = _db.Tiers
                .GroupBy(t => t.Level.ToLower())
                .Select(g => new { Level = g.Key, Count = g.Count() })
                .ToDictionary(x => x.Level, x => x.Count);
            var tierDistribution = new {
                bronze = tierCounts.ContainsKey("bronze") ? tierCounts["bronze"] : 0,
                silver = tierCounts.ContainsKey("silver") ? tierCounts["silver"] : 0,
                gold = tierCounts.ContainsKey("gold") ? tierCounts["gold"] : 0,
                platinum = tierCounts.ContainsKey("platinum") ? tierCounts["platinum"] : 0
            };

            // Top members by points, with tier and totalSpent
            var topMembers = _db.Points
                .OrderByDescending(p => p.Balance)
                .Take(5)
                .Select(p => new {
                    id = p.UserId.ToString(),
                    name = p.User.FirstName + " " + p.User.LastName,
                    points = p.Balance,
                    tier = _db.Tiers.Where(t => t.UserId == p.UserId).OrderByDescending(t => t.AssignedAt).Select(t => t.Level).FirstOrDefault() ?? "bronze",
                    totalSpent = _db.Transactions.Where(t => t.UserId == p.UserId && t.Type == "purchase").Sum(t => (decimal?)t.Amount) ?? 0
                }).ToList();

            // Recent activity (last 5 transactions), with name, lastActivity, points
            var recentActivity = _db.Transactions
                .OrderByDescending(t => t.Date)
                .Take(5)
                .Select(t => new {
                    id = t.Id.ToString(),
                    name = t.User.FirstName + " " + t.User.LastName,
                    lastActivity = t.Date,
                    points = _db.Points.Where(p => p.UserId == t.UserId).Select(p => p.Balance).FirstOrDefault()
                }).ToList();

            // Monthly growth (last 6 months), with month, members, revenue, points
            var now = DateTime.UtcNow;
            var monthlyGrowth = Enumerable.Range(0, 6)
                .Select(i =>
                {
                    var month = now.AddMonths(-i);
                    var members = _db.Users.Count(u => u.CreatedAt.Year == month.Year && u.CreatedAt.Month == month.Month);
                    var revenue = _db.Transactions.Where(t => t.Date.Year == month.Year && t.Date.Month == month.Month && t.Type == "purchase").Sum(t => (decimal?)t.Amount) ?? 0;
                    // Points by month is not tracked, so just use total points
                    var points = _db.Points.Sum(p => (int?)p.Balance) ?? 0;
                    return new {
                        month = month.ToString("yyyy-MM"),
                        members,
                        revenue,
                        points
                    };
                })
                .Reverse()
                .ToArray();

            // Ensure all properties are present and not undefined
            var analytics = new {
                totalMembers = totalMembers,
                activePartners = 0, // Placeholder
                pointsCirculating = pointsCirculating,
                systemRevenue = systemRevenue,
                averagePointsPerMember = averagePointsPerMember,
                totalSpent = totalSpent,
                totalTransactions = totalTransactions,
                tierDistribution = tierDistribution,
                topMembers = topMembers,
                recentActivity = recentActivity,
                monthlyGrowth = monthlyGrowth
            };
            return Ok(analytics);
        }

        // GET: api/admin/customers-stats
        [HttpGet("customers-stats")]
        public IActionResult GetCustomersStats()
        {
            var total = _db.Users.Count();
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var activeUserIds = _db.Transactions
                .Where(t => t.Date >= thirtyDaysAgo)
                .Select(t => t.UserId)
                .Distinct()
                .ToList();
            var active = activeUserIds.Count;
            var inactive = total - active;
            var stats = new {
                total,
                active,
                inactive
            };
            return Ok(stats);
        }

        // GET: api/admin/analytics/customers
        [HttpGet("analytics/customers")]
        public IActionResult GetAnalyticsCustomers()
        {
            var total = _db.Users.Count();
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            var activeUserIds = _db.Transactions
                .Where(t => t.Date >= thirtyDaysAgo)
                .Select(t => t.UserId)
                .Distinct()
                .ToList();
            var active = activeUserIds.Count;
            var inactive = total - active;

            // Tier distribution
            var tierCounts = _db.Tiers
                .GroupBy(t => t.Level.ToLower())
                .Select(g => new { Level = g.Key, Count = g.Count() })
                .ToDictionary(x => x.Level, x => x.Count);
            var tierDistribution = new {
                bronze = tierCounts.ContainsKey("bronze") ? tierCounts["bronze"] : 0,
                silver = tierCounts.ContainsKey("silver") ? tierCounts["silver"] : 0,
                gold = tierCounts.ContainsKey("gold") ? tierCounts["gold"] : 0,
                platinum = tierCounts.ContainsKey("platinum") ? tierCounts["platinum"] : 0
            };

            var averagePointsPerMember = total > 0 ? (_db.Points.Sum(p => (int?)p.Balance) ?? 0) / total : 0;
            var totalPoints = _db.Points.Sum(p => (int?)p.Balance) ?? 0;
            var totalSpent = _db.Transactions.Where(t => t.Type == "purchase").Sum(t => (decimal?)t.Amount) ?? 0;
            var totalTransactions = _db.Transactions.Count();

            // No suspended logic yet
            var suspended = 0;

            var stats = new {
                total = total,
                active = active,
                inactive = inactive,
                suspended = suspended,
                averagePointsPerMember = averagePointsPerMember,
                totalPoints = totalPoints,
                totalSpent = totalSpent,
                totalTransactions = totalTransactions,
                tierDistribution = tierDistribution
            };
            return Ok(stats);
        }

        // GET: api/admin/transfers
        [HttpGet("transfers")]
        public async Task<IActionResult> GetTransfers([FromQuery] int page = 1, [FromQuery] int perPage = 10, [FromQuery] string? search = null)
        {
            var query = _db.Transfers
                .Include(t => t.FromUser)
                .Include(t => t.ToUser)
                .Include(t => t.Admin)
                .AsQueryable();

            // Apply search filter if provided
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(t => 
                    t.FromUser.FirstName.Contains(search) || 
                    t.FromUser.LastName.Contains(search) ||
                    t.ToUser.FirstName.Contains(search) || 
                    t.ToUser.LastName.Contains(search) ||
                    t.Reason.Contains(search) ||
                    t.Notes.Contains(search)
                );
            }

            var total = await query.CountAsync();
            var transfers = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * perPage)
                .Take(perPage)
                .Select(t => new
                {
                    id = t.Id.ToString(),
                    fromMemberId = t.FromUserId.ToString(),
                    fromMemberName = t.FromUser.FirstName + " " + t.FromUser.LastName,
                    toMemberId = t.ToUserId.ToString(),
                    toMemberName = t.ToUser.FirstName + " " + t.ToUser.LastName,
                    points = t.Points,
                    status = t.Status,
                    createdAt = t.CreatedAt,
                    completedAt = t.CompletedAt,
                    reason = t.Reason,
                    notes = t.Notes
                })
                .ToListAsync();

            // Calculate transfer statistics
            var totalTransfers = _db.Transfers.Count();
            var totalPointsTransferred = _db.Transfers.Where(t => t.Status == "completed").Sum(t => (int?)t.Points) ?? 0;
            var completedTransfers = _db.Transfers.Count(t => t.Status == "completed");
            var pendingTransfers = _db.Transfers.Count(t => t.Status == "pending");
            var failedTransfers = _db.Transfers.Count(t => t.Status == "failed");
            var averageTransferAmount = completedTransfers > 0 ? totalPointsTransferred / completedTransfers : 0;

            var stats = new
            {
                totalTransfers,
                totalPointsTransferred,
                completedTransfers,
                pendingTransfers,
                failedTransfers,
                averageTransferAmount
            };

            return Ok(new
            {
                transfers,
                total,
                page,
                perPage,
                totalPages = (int)Math.Ceiling(total / (double)perPage),
                stats
            });
        }

        // POST: api/admin/transfers
        [HttpPost("transfers")]
        public async Task<IActionResult> CreateTransfer([FromBody] TransferCreateDto dto)
        {
            // Validate users exist
            var fromUser = await _db.Users.FindAsync(dto.FromUserId);
            var toUser = await _db.Users.FindAsync(dto.ToUserId);
            
            if (fromUser == null || toUser == null)
                return BadRequest(new { message = "One or both users not found" });

            if (dto.FromUserId == dto.ToUserId)
                return BadRequest(new { message = "Cannot transfer points to same user" });

            if (dto.Points <= 0)
                return BadRequest(new { message = "Points must be positive" });

            // Check if from user has enough points
            var fromUserPoints = await _db.Points.FirstOrDefaultAsync(p => p.UserId == dto.FromUserId);
            if (fromUserPoints == null || fromUserPoints.Balance < dto.Points)
                return BadRequest(new { message = "Insufficient points for transfer" });

            // Create transfer
            var transfer = new Transfer
            {
                FromUserId = dto.FromUserId,
                ToUserId = dto.ToUserId,
                Points = dto.Points,
                Status = "pending",
                Reason = dto.Reason,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _db.Transfers.Add(transfer);
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Transfer created successfully",
                transfer = new
                {
                    id = transfer.Id.ToString(),
                    fromMemberId = transfer.FromUserId.ToString(),
                    fromMemberName = fromUser.FirstName + " " + fromUser.LastName,
                    toMemberId = transfer.ToUserId.ToString(),
                    toMemberName = toUser.FirstName + " " + toUser.LastName,
                    points = transfer.Points,
                    status = transfer.Status,
                    createdAt = transfer.CreatedAt,
                    reason = transfer.Reason,
                    notes = transfer.Notes
                }
            });
        }
    }

    public class TransferCreateDto
    {
        public int FromUserId { get; set; }
        public int ToUserId { get; set; }
        public int Points { get; set; }
        public string? Reason { get; set; }
        public string? Notes { get; set; }
    }

    public class AdminCreateDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public bool External { get; set; } = false;
        public string? ApiKey { get; set; }
    }
    public class AdminEditDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Phone { get; set; }
        public bool? External { get; set; }
        public string? ApiKey { get; set; }
    }
} 