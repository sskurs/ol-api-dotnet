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
    public class CampaignController : ControllerBase
    {
        private readonly AppDbContext _db;
        public CampaignController(AppDbContext db)
        {
            _db = db;
        }

        // GET: api/campaign?page=1&perPage=10
        [HttpGet]
        public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int perPage = 10)
        {
            var query = _db.Campaigns.AsQueryable();
            var total = await query.CountAsync();
            var campaigns = await query.Skip((page - 1) * perPage).Take(perPage).ToListAsync();
            return Ok(new { campaigns, total, page, perPage, totalPages = (int)Math.Ceiling(total / (double)perPage) });
        }

        // GET: api/campaign/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var campaign = await _db.Campaigns.FindAsync(id);
            if (campaign == null) return NotFound();
            return Ok(campaign);
        }

        // POST: api/campaign
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CampaignCreateDto dto)
        {
            var campaign = new Campaign
            {
                CampaignId = Guid.NewGuid(),
                Reward = dto.Reward,
                Name = dto.Name,
                ShortDescription = dto.ShortDescription,
                ConditionsDescription = dto.ConditionsDescription,
                UsageInstruction = dto.UsageInstruction,
                Active = dto.Active,
                CostInPoints = dto.CostInPoints,
                Levels = dto.Levels ?? "[]",
                Segments = dto.Segments ?? "[]",
                Unlimited = dto.Unlimited,
                SingleCoupon = dto.SingleCoupon,
                UsageLimit = dto.UsageLimit,
                LimitPerUser = dto.LimitPerUser,
                Coupons = dto.Coupons ?? "[]",
                CampaignActivityAllTimeActive = dto.CampaignActivityAllTimeActive,
                CampaignActivityActiveFrom = dto.CampaignActivityActiveFrom,
                CampaignActivityActiveTo = dto.CampaignActivityActiveTo,
                CampaignVisibilityAllTimeVisible = dto.CampaignVisibilityAllTimeVisible,
                CampaignVisibilityVisibleFrom = dto.CampaignVisibilityVisibleFrom,
                CampaignVisibilityVisibleTo = dto.CampaignVisibilityVisibleTo,
                CampaignPhotoPath = dto.CampaignPhotoPath,
                CampaignPhotoOriginalName = dto.CampaignPhotoOriginalName,
                CampaignPhotoMime = dto.CampaignPhotoMime
            };
            _db.Campaigns.Add(campaign);
            await _db.SaveChangesAsync();
            return Ok(campaign);
        }

        // PUT: api/campaign/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(Guid id, [FromBody] CampaignEditDto dto)
        {
            var campaign = await _db.Campaigns.FindAsync(id);
            if (campaign == null) return NotFound();
            if (!string.IsNullOrEmpty(dto.Reward)) campaign.Reward = dto.Reward;
            if (!string.IsNullOrEmpty(dto.Name)) campaign.Name = dto.Name;
            if (dto.ShortDescription != null) campaign.ShortDescription = dto.ShortDescription;
            if (dto.ConditionsDescription != null) campaign.ConditionsDescription = dto.ConditionsDescription;
            if (dto.UsageInstruction != null) campaign.UsageInstruction = dto.UsageInstruction;
            if (dto.Active.HasValue) campaign.Active = dto.Active.Value;
            if (dto.CostInPoints.HasValue) campaign.CostInPoints = dto.CostInPoints.Value;
            if (dto.Levels != null) campaign.Levels = dto.Levels;
            if (dto.Segments != null) campaign.Segments = dto.Segments;
            if (dto.Unlimited.HasValue) campaign.Unlimited = dto.Unlimited.Value;
            if (dto.SingleCoupon.HasValue) campaign.SingleCoupon = dto.SingleCoupon.Value;
            if (dto.UsageLimit.HasValue) campaign.UsageLimit = dto.UsageLimit;
            if (dto.LimitPerUser.HasValue) campaign.LimitPerUser = dto.LimitPerUser;
            if (dto.Coupons != null) campaign.Coupons = dto.Coupons;
            if (dto.CampaignActivityAllTimeActive.HasValue) campaign.CampaignActivityAllTimeActive = dto.CampaignActivityAllTimeActive;
            if (dto.CampaignActivityActiveFrom.HasValue) campaign.CampaignActivityActiveFrom = dto.CampaignActivityActiveFrom;
            if (dto.CampaignActivityActiveTo.HasValue) campaign.CampaignActivityActiveTo = dto.CampaignActivityActiveTo;
            if (dto.CampaignVisibilityAllTimeVisible.HasValue) campaign.CampaignVisibilityAllTimeVisible = dto.CampaignVisibilityAllTimeVisible;
            if (dto.CampaignVisibilityVisibleFrom.HasValue) campaign.CampaignVisibilityVisibleFrom = dto.CampaignVisibilityVisibleFrom;
            if (dto.CampaignVisibilityVisibleTo.HasValue) campaign.CampaignVisibilityVisibleTo = dto.CampaignVisibilityVisibleTo;
            if (dto.CampaignPhotoPath != null) campaign.CampaignPhotoPath = dto.CampaignPhotoPath;
            if (dto.CampaignPhotoOriginalName != null) campaign.CampaignPhotoOriginalName = dto.CampaignPhotoOriginalName;
            if (dto.CampaignPhotoMime != null) campaign.CampaignPhotoMime = dto.CampaignPhotoMime;
            await _db.SaveChangesAsync();
            return Ok(campaign);
        }

        // POST: api/campaign/demo
        [HttpPost("demo")]
        [AllowAnonymous]
        public async Task<IActionResult> GenerateDemoData()
        {
            if (await _db.Campaigns.AnyAsync())
                return BadRequest(new { message = "Demo data already exists." });
            var demoCampaigns = new List<Campaign>
            {
                new Campaign
                {
                    CampaignId = Guid.NewGuid(),
                    Reward = "10% Discount",
                    Name = "Summer Sale",
                    ShortDescription = "Get 10% off on all items!",
                    ConditionsDescription = "Valid for all users.",
                    UsageInstruction = "Use at checkout.",
                    Active = true,
                    CostInPoints = 100,
                    Levels = "[\"Gold\",\"Silver\"]",
                    Segments = "[\"All\"]",
                    Unlimited = true,
                    SingleCoupon = true,
                    UsageLimit = 1000,
                    LimitPerUser = 1,
                    Coupons = "[\"SUMMER10\"]",
                    CampaignActivityAllTimeActive = true,
                    CampaignActivityActiveFrom = DateTime.UtcNow.AddDays(-10),
                    CampaignActivityActiveTo = DateTime.UtcNow.AddDays(20),
                    CampaignVisibilityAllTimeVisible = true,
                    CampaignVisibilityVisibleFrom = DateTime.UtcNow.AddDays(-10),
                    CampaignVisibilityVisibleTo = DateTime.UtcNow.AddDays(20),
                    CampaignPhotoPath = null,
                    CampaignPhotoOriginalName = null,
                    CampaignPhotoMime = null
                },
                new Campaign
                {
                    CampaignId = Guid.NewGuid(),
                    Reward = "Free Shipping",
                    Name = "VIP Free Shipping",
                    ShortDescription = "VIPs get free shipping for a month!",
                    ConditionsDescription = "Only for VIP level.",
                    UsageInstruction = "Auto-applied at checkout.",
                    Active = true,
                    CostInPoints = 200,
                    Levels = "[\"VIP\"]",
                    Segments = "[\"VIPs\"]",
                    Unlimited = false,
                    SingleCoupon = false,
                    UsageLimit = 100,
                    LimitPerUser = 2,
                    Coupons = "[\"FREESHIPVIP\"]",
                    CampaignActivityAllTimeActive = false,
                    CampaignActivityActiveFrom = DateTime.UtcNow.AddDays(-5),
                    CampaignActivityActiveTo = DateTime.UtcNow.AddDays(25),
                    CampaignVisibilityAllTimeVisible = false,
                    CampaignVisibilityVisibleFrom = DateTime.UtcNow.AddDays(-5),
                    CampaignVisibilityVisibleTo = DateTime.UtcNow.AddDays(25),
                    CampaignPhotoPath = null,
                    CampaignPhotoOriginalName = null,
                    CampaignPhotoMime = null
                }
            };
            _db.Campaigns.AddRange(demoCampaigns);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Demo campaigns created.", count = demoCampaigns.Count });
        }
    }

    public class CampaignCreateDto
    {
        public string Reward { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string? ConditionsDescription { get; set; }
        public string? UsageInstruction { get; set; }
        public bool Active { get; set; } = false;
        public decimal CostInPoints { get; set; }
        public string? Levels { get; set; }
        public string? Segments { get; set; }
        public bool Unlimited { get; set; } = true;
        public bool SingleCoupon { get; set; } = true;
        public int? UsageLimit { get; set; }
        public int? LimitPerUser { get; set; }
        public string? Coupons { get; set; }
        public bool? CampaignActivityAllTimeActive { get; set; }
        public DateTime? CampaignActivityActiveFrom { get; set; }
        public DateTime? CampaignActivityActiveTo { get; set; }
        public bool? CampaignVisibilityAllTimeVisible { get; set; }
        public DateTime? CampaignVisibilityVisibleFrom { get; set; }
        public DateTime? CampaignVisibilityVisibleTo { get; set; }
        public string? CampaignPhotoPath { get; set; }
        public string? CampaignPhotoOriginalName { get; set; }
        public string? CampaignPhotoMime { get; set; }
    }
    public class CampaignEditDto
    {
        public string? Reward { get; set; }
        public string? Name { get; set; }
        public string? ShortDescription { get; set; }
        public string? ConditionsDescription { get; set; }
        public string? UsageInstruction { get; set; }
        public bool? Active { get; set; }
        public decimal? CostInPoints { get; set; }
        public string? Levels { get; set; }
        public string? Segments { get; set; }
        public bool? Unlimited { get; set; }
        public bool? SingleCoupon { get; set; }
        public int? UsageLimit { get; set; }
        public int? LimitPerUser { get; set; }
        public string? Coupons { get; set; }
        public bool? CampaignActivityAllTimeActive { get; set; }
        public DateTime? CampaignActivityActiveFrom { get; set; }
        public DateTime? CampaignActivityActiveTo { get; set; }
        public bool? CampaignVisibilityAllTimeVisible { get; set; }
        public DateTime? CampaignVisibilityVisibleFrom { get; set; }
        public DateTime? CampaignVisibilityVisibleTo { get; set; }
        public string? CampaignPhotoPath { get; set; }
        public string? CampaignPhotoOriginalName { get; set; }
        public string? CampaignPhotoMime { get; set; }
    }
} 