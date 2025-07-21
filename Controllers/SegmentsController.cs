using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ol_api_dotnet.Data;
using ol_api_dotnet.Models;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ol_api_dotnet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class SegmentsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public SegmentsController(AppDbContext db)
        {
            _db = db;
        }

        // Helper to count users matching segment criteria
        private async Task<int> CountMatchingUsers(string criteriaJson)
        {
            if (string.IsNullOrWhiteSpace(criteriaJson)) return 0;
            var criteriaList = JsonSerializer.Deserialize<List<CriteriaItem>>(criteriaJson);
            if (criteriaList == null || criteriaList.Count == 0) return 0;

            var users = await _db.Users.ToListAsync();
            var transactions = await _db.Transactions.ToListAsync();
            int count = 0;
            foreach (var user in users)
            {
                decimal totalSpent = transactions.Where(t => t.UserId == user.Id && t.Type == "purchase").Sum(t => t.Amount);
                DateTime? lastPurchase = transactions.Where(t => t.UserId == user.Id && t.Type == "purchase").OrderByDescending(t => t.Date).Select(t => (DateTime?)t.Date).FirstOrDefault();
                DateTime joinDate = user.CreatedAt;

                bool matches = true;
                foreach (var crit in criteriaList)
                {
                    bool thisMatch = true;
                    switch (crit.Field)
                    {
                        case "totalSpent":
                            thisMatch = CompareDecimal(totalSpent, crit.Operator, crit.Value);
                            break;
                        case "joinDate":
                            thisMatch = CompareDate(joinDate, crit.Operator, crit.Value);
                            break;
                        case "lastPurchase":
                            thisMatch = lastPurchase.HasValue && CompareDate(lastPurchase.Value, crit.Operator, crit.Value);
                            break;
                        default:
                            thisMatch = false;
                            break;
                    }
                    // Debug: log user and criteria match result (commented out)
                    // System.Diagnostics.Debug.WriteLine($"User {user.Id}: {crit.Field} {crit.Operator} {crit.Value} => {thisMatch}");
                    if (!thisMatch)
                    {
                        matches = false;
                        break;
                    }
                }
                if (matches) count++;
            }
            return count;
        }

        private async Task<List<int>> GetMatchingUserIds(string criteriaJson)
        {
            if (string.IsNullOrWhiteSpace(criteriaJson)) return new List<int>();
            var criteriaList = JsonSerializer.Deserialize<List<CriteriaItem>>(criteriaJson);
            if (criteriaList == null || criteriaList.Count == 0) return new List<int>();

            var users = await _db.Users.ToListAsync();
            var transactions = await _db.Transactions.ToListAsync();
            var matchingUserIds = new List<int>();
            foreach (var user in users)
            {
                decimal totalSpent = transactions.Where(t => t.UserId == user.Id && t.Type == "purchase").Sum(t => t.Amount);
                DateTime? lastPurchase = transactions.Where(t => t.UserId == user.Id && t.Type == "purchase").OrderByDescending(t => t.Date).Select(t => (DateTime?)t.Date).FirstOrDefault();
                DateTime joinDate = user.CreatedAt;

                bool matches = true;
                foreach (var crit in criteriaList)
                {
                    bool thisMatch = true;
                    switch (crit.Field)
                    {
                        case "totalSpent":
                            thisMatch = CompareDecimal(totalSpent, crit.Operator, crit.Value);
                            break;
                        case "joinDate":
                            thisMatch = CompareDate(joinDate, crit.Operator, crit.Value);
                            break;
                        case "lastPurchase":
                            thisMatch = lastPurchase.HasValue && CompareDate(lastPurchase.Value, crit.Operator, crit.Value);
                            break;
                        default:
                            thisMatch = false;
                            break;
                    }
                    if (!thisMatch)
                    {
                        matches = false;
                        break;
                    }
                }
                if (matches) matchingUserIds.Add(user.Id);
            }
            return matchingUserIds;
        }

        private bool CompareDecimal(decimal actual, string op, string value)
        {
            if (!decimal.TryParse(value.Replace("$", "").Replace(",", ""), out var val)) return false;
            return op switch
            {
                ">=" => actual >= val,
                "<=" => actual <= val,
                ">" => actual > val,
                "<" => actual < val,
                "==" => actual == val,
                _ => false
            };
        }

        private bool CompareDate(DateTime actual, string op, string value)
        {
            DateTime compareDate;
            if (value.Contains("days ago"))
            {
                var days = int.Parse(value.Split(' ')[0]);
                compareDate = DateTime.UtcNow.AddDays(-days);
            }
            else if (value == "current month")
            {
                compareDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                return actual.Month == compareDate.Month && actual.Year == compareDate.Year;
            }
            else if (!DateTime.TryParse(value, out compareDate))
            {
                return false;
            }
            return op switch
            {
                ">=" => actual >= compareDate,
                "<=" => actual <= compareDate,
                ">" => actual > compareDate,
                "<" => actual < compareDate,
                "==" => actual.Date == compareDate.Date,
                _ => false
            };
        }

        private class CriteriaItem
        {
            public string Field { get; set; } = string.Empty;
            public string Operator { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
            public string? LogicalOperator { get; set; }
        }

        // GET: api/segments
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> List([FromQuery] string? search = null, [FromQuery] string? status = null, [FromQuery] int page = 1, [FromQuery] int perPage = 20)
        {
            try
            {
                var query = _db.Segments.AsQueryable();
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(s => s.Name.ToLower().Contains(search.ToLower()) || s.Description.ToLower().Contains(search.ToLower()));
                }
                if (!string.IsNullOrEmpty(status) && status != "all")
                {
                    query = query.Where(s => s.Status == status);
                }
                var total = await query.CountAsync();
                var segments = await query
                    .OrderByDescending(s => s.CreatedAt)
                    .Skip((page - 1) * perPage)
                    .Take(perPage)
                    .ToListAsync();
                // Calculate real member count for each segment
                foreach (var segment in segments)
                {
                    segment.MemberCount = await CountMatchingUsers(segment.Criteria);
                }
                return Ok(new { segments, total, page, perPage, totalPages = (int)Math.Ceiling(total / (double)perPage) });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving segments", error = ex.Message });
            }
        }

        // GET: api/segments/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var segment = await _db.Segments.FindAsync(id);
                if (segment == null) return NotFound(new { message = "Segment not found" });
                // Calculate real member count
                segment.MemberCount = await CountMatchingUsers(segment.Criteria);
                return Ok(segment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the segment", error = ex.Message });
            }
        }

        // POST: api/segments
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SegmentDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Invalid data provided", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
                }

                // Check if segment with same name already exists
                var existingSegment = await _db.Segments.FirstOrDefaultAsync(s => s.Name.ToLower() == dto.Name.ToLower());
                if (existingSegment != null)
                {
                    return BadRequest(new { message = "A segment with this name already exists" });
                }

                // Validate Criteria JSON
                if (!string.IsNullOrEmpty(dto.Criteria))
                {
                    try
                    {
                        JsonDocument.Parse(dto.Criteria);
                    }
                    catch (JsonException)
                    {
                        return BadRequest(new { message = "Criteria must be valid JSON" });
                    }
                }

                var segment = new Segment
                {
                    Id = Guid.NewGuid(),
                    Name = dto.Name.Trim(),
                    Description = dto.Description?.Trim() ?? string.Empty,
                    Criteria = dto.Criteria ?? "[]",
                    MemberCount = 0, // will be set below
                    Status = dto.Status.ToLower(),
                    CreatedAt = DateTime.UtcNow,
                    LastUpdated = DateTime.UtcNow,
                    Color = dto.Color?.Trim()
                };

                _db.Segments.Add(segment);
                await _db.SaveChangesAsync();

                // Associate members
                var matchingUserIds = await GetMatchingUserIds(segment.Criteria);
                var segmentMembers = matchingUserIds.Select(userId => new SegmentMember { SegmentId = segment.Id, UserId = userId }).ToList();
                _db.SegmentMembers.AddRange(segmentMembers);
                segment.MemberCount = segmentMembers.Count;
                await _db.SaveChangesAsync();
                
                return CreatedAtAction(nameof(Get), new { id = segment.Id }, segment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the segment", error = ex.Message });
            }
        }

        // PUT: api/segments/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SegmentUpdateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { message = "Invalid data provided", errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });
                }

                var segment = await _db.Segments.FindAsync(id);
                if (segment == null) 
                {
                    return NotFound(new { message = "Segment not found" });
                }

                // Check if segment with same name already exists (excluding current segment)
                var existingSegment = await _db.Segments.FirstOrDefaultAsync(s => s.Name.ToLower() == dto.Name.ToLower() && s.Id != id);
                if (existingSegment != null)
                {
                    return BadRequest(new { message = "A segment with this name already exists" });
                }

                // Validate Criteria JSON
                if (!string.IsNullOrEmpty(dto.Criteria))
                {
                    try
                    {
                        JsonDocument.Parse(dto.Criteria);
                    }
                    catch (JsonException)
                    {
                        return BadRequest(new { message = "Criteria must be valid JSON" });
                    }
                }

                // Update segment properties
                segment.Name = dto.Name.Trim();
                segment.Description = dto.Description?.Trim() ?? string.Empty;
                segment.Criteria = dto.Criteria ?? "[]";
                segment.MemberCount = 0; // will be set below
                segment.Status = dto.Status.ToLower();
                segment.LastUpdated = DateTime.UtcNow;
                segment.Color = dto.Color?.Trim();

                // Update members
                var matchingUserIds = await GetMatchingUserIds(segment.Criteria);
                var oldMembers = _db.SegmentMembers.Where(sm => sm.SegmentId == segment.Id);
                _db.SegmentMembers.RemoveRange(oldMembers);
                var segmentMembers = matchingUserIds.Select(userId => new SegmentMember { SegmentId = segment.Id, UserId = userId }).ToList();
                _db.SegmentMembers.AddRange(segmentMembers);
                segment.MemberCount = segmentMembers.Count;
                await _db.SaveChangesAsync();
                return Ok(segment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the segment", error = ex.Message });
            }
        }

        // DELETE: api/segments/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var segment = await _db.Segments.FindAsync(id);
                if (segment == null) 
                {
                    return NotFound(new { message = "Segment not found" });
                }
                
                _db.Segments.Remove(segment);
                await _db.SaveChangesAsync();
                return Ok(new { message = "Segment deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the segment", error = ex.Message });
            }
        }

        // POST: api/segments/demo
        [HttpPost("demo")]
        [AllowAnonymous]
        public async Task<IActionResult> GenerateDemoData()
        {
            try
            {
                if (await _db.Segments.AnyAsync())
                    return BadRequest(new { message = "Demo data already exists." });
                    
                var demoSegments = new List<Segment>
                {
                    new Segment
                    {
                        Id = Guid.NewGuid(),
                        Name = "High Value Customers",
                        Description = "Customers with high lifetime value and frequent purchases",
                        Criteria = "[{\"id\":\"1\",\"field\":\"totalSpent\",\"operator\":\">=\",\"value\":\"1000\"},{\"id\":\"2\",\"field\":\"transactionCount\",\"operator\":\">=\",\"value\":\"10\",\"logicalOperator\":\"AND\"}]",
                        MemberCount = 245,
                        Status = "active",
                        CreatedAt = DateTime.UtcNow.AddDays(-30),
                        LastUpdated = DateTime.UtcNow.AddDays(-25),
                        Color = "#10B981"
                    },
                    new Segment
                    {
                        Id = Guid.NewGuid(),
                        Name = "New Members",
                        Description = "Recently joined members within the last 30 days",
                        Criteria = "[{\"id\":\"1\",\"field\":\"joinDate\",\"operator\":\">=\",\"value\":\"30 days ago\"}]",
                        MemberCount = 89,
                        Status = "active",
                        CreatedAt = DateTime.UtcNow.AddDays(-35),
                        LastUpdated = DateTime.UtcNow.AddDays(-28),
                        Color = "#3B82F6"
                    },
                    new Segment
                    {
                        Id = Guid.NewGuid(),
                        Name = "Inactive Members",
                        Description = "Members who haven't made a purchase in the last 90 days",
                        Criteria = "[{\"id\":\"1\",\"field\":\"lastPurchase\",\"operator\":\"<=\",\"value\":\"90 days ago\"}]",
                        MemberCount = 156,
                        Status = "active",
                        CreatedAt = DateTime.UtcNow.AddDays(-40),
                        LastUpdated = DateTime.UtcNow.AddDays(-35),
                        Color = "#EF4444"
                    },
                    new Segment
                    {
                        Id = Guid.NewGuid(),
                        Name = "Birthday Club",
                        Description = "Members with birthdays in the current month",
                        Criteria = "[{\"id\":\"1\",\"field\":\"birthMonth\",\"operator\":\"=\",\"value\":\"current month\"}]",
                        MemberCount = 34,
                        Status = "active",
                        CreatedAt = DateTime.UtcNow.AddDays(-50),
                        LastUpdated = DateTime.UtcNow.AddDays(-40),
                        Color = "#F59E0B"
                    },
                    new Segment
                    {
                        Id = Guid.NewGuid(),
                        Name = "VIP Platinum",
                        Description = "Platinum tier members with high engagement",
                        Criteria = "[{\"id\":\"1\",\"field\":\"tier\",\"operator\":\"=\",\"value\":\"platinum\"},{\"id\":\"2\",\"field\":\"pointsEarned\",\"operator\":\">=\",\"value\":\"5000\",\"logicalOperator\":\"AND\"}]",
                        MemberCount = 67,
                        Status = "draft",
                        CreatedAt = DateTime.UtcNow.AddDays(-20),
                        LastUpdated = DateTime.UtcNow.AddDays(-20),
                        Color = "#8B5CF6"
                    }
                };
                
                _db.Segments.AddRange(demoSegments);
                await _db.SaveChangesAsync();
                return Ok(new { message = "Demo segments created successfully.", count = demoSegments.Count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while generating demo data", error = ex.Message });
            }
        }

        [HttpGet("{id}/members")]
        public async Task<IActionResult> GetSegmentMembers(Guid id)
        {
            var members = await _db.SegmentMembers
                .Where(sm => sm.SegmentId == id)
                .Select(sm => sm.User)
                .ToListAsync();
            return Ok(members);
        }
    }
} 