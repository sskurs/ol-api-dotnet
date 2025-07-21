using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ol_api_dotnet.Data;
using ol_api_dotnet.Models;
using System.ComponentModel.DataAnnotations;

namespace ol_api_dotnet.Controllers
{
    [ApiController]
    [Route("api/admin/members")]
    [Authorize]
    public class MemberController : ControllerBase
    {
        private readonly AppDbContext _db;
        public MemberController(AppDbContext db)
        {
            _db = db;
        }

        // GET: api/admin/members?page=1&limit=10&search=&status=&roleId=
        [HttpGet]
        public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int limit = 10, [FromQuery] string search = "", [FromQuery] string status = "", [FromQuery] int? roleId = null)
        {
            var query = _db.Users.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u => u.FirstName.Contains(search) || u.LastName.Contains(search) || u.Email.Contains(search));
            }
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(u => u.Status == status);
            }
            if (roleId.HasValue)
            {
                query = query.Where(u => u.RoleId == roleId);
            }
            var total = await query.CountAsync();
            var members = await query.OrderBy(u => u.Id).Skip((page - 1) * limit).Take(limit).ToListAsync();
            // Attach points balance to each member
            var memberIds = members.Select(m => m.Id).ToList();
            var pointsDict = await _db.Points.Where(p => memberIds.Contains(p.UserId)).ToDictionaryAsync(p => p.UserId, p => p.Balance);
            var membersWithPoints = members.Select(m => new {
                m.Id,
                m.FirstName,
                m.LastName,
                m.Email,
                m.Phone,
                m.Status,
                m.Role,
                m.RoleId,
                m.CreatedAt,
                Points = pointsDict.ContainsKey(m.Id) ? pointsDict[m.Id] : 0
            });
            return Ok(new { members = membersWithPoints, total, page, totalPages = (int)Math.Ceiling(total / (double)limit) });
        }

        // GET: api/admin/members/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var member = await _db.Users.FindAsync(id);
            if (member == null) return NotFound();
            var points = await _db.Points.FirstOrDefaultAsync(p => p.UserId == id);
            var result = new {
                member.Id,
                member.FirstName,
                member.LastName,
                member.Email,
                member.Phone,
                member.Status,
                member.Role,
                member.RoleId,
                member.CreatedAt,
                Points = points?.Balance ?? 0
            };
            return Ok(result);
        }

        // POST: api/admin/members
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] User user)
        {
            if (await _db.Users.AnyAsync(u => u.Email == user.Email))
                return BadRequest(new { message = "Email already exists" });
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            user.CreatedAt = DateTime.UtcNow;
            user.Status = string.IsNullOrEmpty(user.Status) ? "active" : user.Status;
            if (user.RoleId.HasValue)
            {
                var role = await _db.Roles.FindAsync(user.RoleId.Value);
                if (role == null) return BadRequest(new { message = "Invalid RoleId" });
                user.Role = role.Name;
            }
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return Ok(user);
        }

        // PUT: api/admin/members/{id}
        [HttpPost("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] User update)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();
            if (!string.IsNullOrEmpty(update.FirstName)) user.FirstName = update.FirstName;
            if (!string.IsNullOrEmpty(update.LastName)) user.LastName = update.LastName;
            if (!string.IsNullOrEmpty(update.Email)) user.Email = update.Email;
            if (!string.IsNullOrEmpty(update.PasswordHash)) user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(update.PasswordHash);
            if (!string.IsNullOrEmpty(update.Phone)) user.Phone = update.Phone;
            if (!string.IsNullOrEmpty(update.Status)) user.Status = update.Status;
            if (update.RoleId.HasValue)
            {
                var role = await _db.Roles.FindAsync(update.RoleId.Value);
                if (role == null) return BadRequest(new { message = "Invalid RoleId" });
                user.RoleId = update.RoleId;
                user.Role = role.Name;
            }
            await _db.SaveChangesAsync();
            return Ok(user);
        }

        // POST: api/admin/members/{id}/suspend
        [HttpPost("{id}/suspend")]
        public async Task<IActionResult> Suspend(int id, [FromBody] SuspendDto dto)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();
            user.Status = "suspended";
            await _db.SaveChangesAsync();
            return Ok(new { message = $"User {id} suspended for reason: {dto.Reason}" });
        }

        // POST: api/admin/members/{id}/status
        [HttpPost("{id}/status")]
        public async Task<IActionResult> SetStatus(int id, [FromBody] StatusDto dto)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();
            user.Status = dto.Status;
            await _db.SaveChangesAsync();
            return Ok(new { message = $"User {id} status set to {dto.Status}" });
        }

        // DELETE: api/admin/members/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();
            return Ok(new { message = $"User {id} deleted" });
        }

        // POST: api/admin/members/{id}/points
        [HttpPost("{id}/points")]
        public async Task<IActionResult> AddPoints(int id, [FromBody] PointsDto dto)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();
            var points = await _db.Points.FirstOrDefaultAsync(p => p.UserId == id);
            if (points == null)
            {
                points = new Points { UserId = id, Balance = 0 };
                _db.Points.Add(points);
            }
            points.Balance += dto.Points;
            // Add to history
            var history = new PointsHistory
            {
                UserId = id,
                Change = dto.Points,
                BalanceAfter = points.Balance,
                Reason = dto.Reason ?? "",
                Timestamp = DateTime.UtcNow
            };
            _db.PointsHistories.Add(history);
            await _db.SaveChangesAsync();
            return Ok(new { message = $"Added {dto.Points} points to user {id}", points = points.Balance });
        }

        // POST: api/admin/members/transfer
        [HttpPost("transfer")]
        public async Task<IActionResult> TransferPoints([FromBody] TransferPointsDto dto)
        {
            if (dto.FromUserId == dto.ToUserId)
                return BadRequest(new { message = "Cannot transfer points to the same user." });
            if (dto.Points <= 0)
                return BadRequest(new { message = "Points must be greater than zero." });

            var sender = await _db.Users.FindAsync(dto.FromUserId);
            var recipient = await _db.Users.FindAsync(dto.ToUserId);
            if (sender == null || recipient == null)
                return NotFound(new { message = "Sender or recipient not found." });

            var senderPoints = await _db.Points.FirstOrDefaultAsync(p => p.UserId == dto.FromUserId);
            if (senderPoints == null || senderPoints.Balance < dto.Points)
                return BadRequest(new { message = "Insufficient points in sender account." });

            var recipientPoints = await _db.Points.FirstOrDefaultAsync(p => p.UserId == dto.ToUserId);
            if (recipientPoints == null)
            {
                recipientPoints = new Points { UserId = dto.ToUserId, Balance = 0 };
                _db.Points.Add(recipientPoints);
            }

            // Deduct from sender
            senderPoints.Balance -= dto.Points;
            // Add to recipient
            recipientPoints.Balance += dto.Points;

            // Add to history for sender
            var senderHistory = new PointsHistory
            {
                UserId = dto.FromUserId,
                Change = -dto.Points,
                BalanceAfter = senderPoints.Balance,
                Reason = $"Transfer to user {dto.ToUserId}: {dto.Reason ?? ""}",
                Timestamp = DateTime.UtcNow
            };
            _db.PointsHistories.Add(senderHistory);

            // Add to history for recipient
            var recipientHistory = new PointsHistory
            {
                UserId = dto.ToUserId,
                Change = dto.Points,
                BalanceAfter = recipientPoints.Balance,
                Reason = $"Transfer from user {dto.FromUserId}: {dto.Reason ?? ""}",
                Timestamp = DateTime.UtcNow
            };
            _db.PointsHistories.Add(recipientHistory);

            await _db.SaveChangesAsync();
            return Ok(new { message = $"Transferred {dto.Points} points from user {dto.FromUserId} to user {dto.ToUserId}.", newSenderBalance = senderPoints.Balance, newRecipientBalance = recipientPoints.Balance });
        }

        [HttpGet("{id}/points/history")]
        public async Task<IActionResult> GetPointsHistory(int id)
        {
            var history = await _db.PointsHistories
                .Where(h => h.UserId == id)
                .OrderByDescending(h => h.Timestamp)
                .ToListAsync();
            return Ok(history);
        }

        // GET: api/admin/members/{id}/rewards
        [HttpGet("{id}/rewards")]
        public async Task<IActionResult> GetRewards(int id)
        {
            var rewards = await _db.Rewards.Where(r => r.UserId == id).ToListAsync();
            return Ok(rewards);
        }

        // GET: api/admin/members/{id}/redeemed-rewards
        [HttpGet("{id}/redeemed-rewards")]
        public async Task<IActionResult> GetRedeemedRewards(int id)
        {
            var redeemed = await _db.RedeemedRewards.Where(r => r.UserId == id).ToListAsync();
            return Ok(redeemed);
        }

        // GET: api/admin/members/{id}/achievements
        [HttpGet("{id}/achievements")]
        public async Task<IActionResult> GetAchievements(int id)
        {
            var achievements = await _db.Achievements.Where(a => a.UserId == id).ToListAsync();
            return Ok(achievements);
        }

        // --- DTOs ---
        public class RewardDto
        {
            [Required, MaxLength(255)] public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
            [Range(0, 100)] public int Availability { get; set; } = 0;
        }
        public class RedeemedRewardDto
        {
            [Required, MaxLength(255)] public string Name { get; set; } = string.Empty;
            public DateTime RedeemedDate { get; set; } = DateTime.UtcNow;
            public string? TransactionId { get; set; }
            [Required] public string Status { get; set; } = "Redeemed";
        }
        public class AchievementDto
        {
            [Required, MaxLength(255)] public string Name { get; set; } = string.Empty;
            [Range(0, 100)] public int Progress { get; set; } = 0;
            public bool Completed { get; set; } = false;
        }

        // --- REWARDS ---
        [HttpPost("{id}/rewards")]
        public async Task<IActionResult> CreateReward(int id, [FromBody] RewardDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var entity = new Reward {
                UserId = id,
                Name = dto.Name,
                Description = dto.Description,
                FromDate = dto.FromDate,
                ToDate = dto.ToDate,
                Availability = dto.Availability
            };
            _db.Rewards.Add(entity);
            await _db.SaveChangesAsync();
            return Ok(entity);
        }

        [HttpPut("{id}/rewards/{rewardId}")]
        public async Task<IActionResult> UpdateReward(int id, int rewardId, [FromBody] RewardDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var reward = await _db.Rewards.FirstOrDefaultAsync(r => r.Id == rewardId && r.UserId == id);
            if (reward == null) return NotFound();
            reward.Name = dto.Name;
            reward.Description = dto.Description;
            reward.FromDate = dto.FromDate;
            reward.ToDate = dto.ToDate;
            reward.Availability = dto.Availability;
            await _db.SaveChangesAsync();
            return Ok(reward);
        }

        [HttpDelete("{id}/rewards/{rewardId}")]
        public async Task<IActionResult> DeleteReward(int id, int rewardId)
        {
            var reward = await _db.Rewards.FirstOrDefaultAsync(r => r.Id == rewardId && r.UserId == id);
            if (reward == null) return NotFound();
            _db.Rewards.Remove(reward);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Reward deleted" });
        }

        // --- REDEEMED REWARDS ---
        [HttpPost("{id}/redeemed-rewards")]
        public async Task<IActionResult> CreateRedeemedReward(int id, [FromBody] RedeemedRewardDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var entity = new RedeemedReward {
                UserId = id,
                Name = dto.Name,
                RedeemedDate = dto.RedeemedDate,
                TransactionId = dto.TransactionId,
                Status = dto.Status
            };
            _db.RedeemedRewards.Add(entity);
            await _db.SaveChangesAsync();
            return Ok(entity);
        }

        [HttpPut("{id}/redeemed-rewards/{redeemedId}")]
        public async Task<IActionResult> UpdateRedeemedReward(int id, int redeemedId, [FromBody] RedeemedRewardDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var reward = await _db.RedeemedRewards.FirstOrDefaultAsync(r => r.Id == redeemedId && r.UserId == id);
            if (reward == null) return NotFound();
            reward.Name = dto.Name;
            reward.RedeemedDate = dto.RedeemedDate;
            reward.TransactionId = dto.TransactionId;
            reward.Status = dto.Status;
            await _db.SaveChangesAsync();
            return Ok(reward);
        }

        [HttpDelete("{id}/redeemed-rewards/{redeemedId}")]
        public async Task<IActionResult> DeleteRedeemedReward(int id, int redeemedId)
        {
            var reward = await _db.RedeemedRewards.FirstOrDefaultAsync(r => r.Id == redeemedId && r.UserId == id);
            if (reward == null) return NotFound();
            _db.RedeemedRewards.Remove(reward);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Redeemed reward deleted" });
        }

        // --- ACHIEVEMENTS ---
        [HttpPost("{id}/achievements")]
        public async Task<IActionResult> CreateAchievement(int id, [FromBody] AchievementDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var entity = new Achievement {
                UserId = id,
                Name = dto.Name,
                Progress = dto.Progress,
                Completed = dto.Completed
            };
            _db.Achievements.Add(entity);
            await _db.SaveChangesAsync();
            return Ok(entity);
        }

        [HttpPut("{id}/achievements/{achievementId}")]
        public async Task<IActionResult> UpdateAchievement(int id, int achievementId, [FromBody] AchievementDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var achievement = await _db.Achievements.FirstOrDefaultAsync(a => a.Id == achievementId && a.UserId == id);
            if (achievement == null) return NotFound();
            achievement.Name = dto.Name;
            achievement.Progress = dto.Progress;
            achievement.Completed = dto.Completed;
            await _db.SaveChangesAsync();
            return Ok(achievement);
        }

        [HttpDelete("{id}/achievements/{achievementId}")]
        public async Task<IActionResult> DeleteAchievement(int id, int achievementId)
        {
            var achievement = await _db.Achievements.FirstOrDefaultAsync(a => a.Id == achievementId && a.UserId == id);
            if (achievement == null) return NotFound();
            _db.Achievements.Remove(achievement);
            await _db.SaveChangesAsync();
            return Ok(new { message = "Achievement deleted" });
        }

        // GET: api/admin/members/{id}/partners
        [HttpGet("{id}/partners")]
        public async Task<IActionResult> GetUserPartners(int id)
        {
            var user = await _db.Users.Include(u => u.UserPartners).ThenInclude(up => up.Partner).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();
            var partners = user.UserPartners.Select(up => new { up.Partner.Id, up.Partner.Name }).ToList();
            return Ok(partners);
        }

        // POST: api/admin/members/{id}/partners
        [HttpPost("{id}/partners")]
        public async Task<IActionResult> SetUserPartners(int id, [FromBody] SetUserPartnersDto dto)
        {
            var user = await _db.Users.Include(u => u.UserPartners).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();
            // Remove existing associations
            _db.UserPartners.RemoveRange(user.UserPartners);
            // Add new associations
            foreach (var partnerId in dto.PartnerIds.Distinct())
            {
                var partner = await _db.Set<Partner>().FindAsync(partnerId);
                if (partner != null)
                {
                    _db.UserPartners.Add(new UserPartner { UserId = id, PartnerId = partnerId });
                }
            }
            await _db.SaveChangesAsync();
            return Ok(new { message = "User partners updated." });
        }

        public class SuspendDto
        {
            public string Reason { get; set; } = string.Empty;
        }

        public class StatusDto
        {
            public string Status { get; set; } = string.Empty;
        }

        public class PointsDto
        {
            public int Points { get; set; }
            public string? Reason { get; set; }
        }

        public class TransferPointsDto
        {
            public int FromUserId { get; set; }
            public int ToUserId { get; set; }
            public int Points { get; set; }
            public string? Reason { get; set; }
        }

        public class SetUserPartnersDto
        {
            public List<Guid> PartnerIds { get; set; } = new List<Guid>();
        }
    }
} 