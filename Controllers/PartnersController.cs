using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ol_api_dotnet.Data;
using ol_api_dotnet.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using MailKit.Net.Smtp;
using MimeKit;
using System.IO;

[ApiController]
[Route("api/admin/partners")]
[Authorize]
public class PartnersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<PartnersController> _logger;
    private readonly IConfiguration _config;
    
    public PartnersController(AppDbContext db, ILogger<PartnersController> logger, IConfiguration config) 
    { 
        _db = db; 
        _logger = logger; 
        _config = config;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(int page = 1, int limit = 10, string search = "")
    {
        _logger.LogInformation("GET /api/admin/partners?page={Page}&limit={Limit}&search={Search}", page, limit, search);
        
        var query = _db.Merchants.AsQueryable();
        
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(m => m.Name.Contains(search) || m.Email.Contains(search));
        }
        
        var total = await query.CountAsync();
        var partners = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(m => new
            {
                m.Id,
                m.Name,
                m.Email,
                m.Phone,
                m.Address,
                m.State,
                m.City,
                m.Zipcode,
                m.Category,
                m.TaxId,
                m.Description,
                m.Website,
                m.CommisionRate,
                m.Status,
                m.CreatedAt,
                m.UpdatedAt,
                memberCount = _db.Users.Count(u => u.MerchantId == m.Id)
            })
            .ToListAsync();
            
        return Ok(new { partners, total, page, totalPages = (int)Math.Ceiling(total / (double)limit) });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        _logger.LogInformation("GET /api/admin/partners/{Id}", id);
        
        var partner = await _db.Merchants
            .Where(m => m.Id == id)
            .Select(m => new
            {
                m.Id,
                m.Name,
                m.Email,
                m.Phone,
                m.Address,
                m.State,
                m.City,
                m.Zipcode,
                m.Category,
                m.TaxId,
                m.Description,
                m.Website,
                m.CommisionRate,
                m.Status,
                m.CreatedAt,
                m.UpdatedAt
            })
            .FirstOrDefaultAsync();
            
        if (partner == null) 
        {
            _logger.LogWarning("Partner not found: {Id}", id);
            return NotFound();
        }
        
        return Ok(partner);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PartnerCreateRequest request)
    {
        _logger.LogInformation("POST /api/admin/partners - Creating partner: {@Request}", request);
        
        var partner = new Merchant
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            State = request.State,
            City = request.City,
            Zipcode = request.Zipcode,
            Category = request.Category,
            TaxId = request.TaxId,
            Description = request.Description,
            Website = request.Website,
            CommisionRate = request.CommisionRate,
            Status = request.Status ?? "pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        _db.Merchants.Add(partner);
        await _db.SaveChangesAsync();

        // Require activation for partner user
        var activationToken = Guid.NewGuid().ToString();
        var partnerUser = new User
        {
            FirstName = request.Name,
            LastName = "Partner",
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = "seller",
            MerchantId = partner.Id,
            Status = "active",
            IsActive = false, // Require activation
            ActivationToken = activationToken,
            CreatedAt = DateTime.UtcNow
        };
        _db.Users.Add(partnerUser);
        await _db.SaveChangesAsync();

        try
        {
            // Send activation email (reuse AuthController logic)
            var smtp = _config.GetSection("Smtp");
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("OpenLoyalty", smtp["From"] ?? "noreply@openloyalty.local"));
            message.To.Add(new MailboxAddress(partnerUser.FirstName + " " + partnerUser.LastName, partnerUser.Email));
            message.Subject = "Activate your OpenLoyalty Partner Account";
            var activationLink = $"https://localhost:3000/(auth)/activate?token={Uri.EscapeDataString(partnerUser.ActivationToken ?? "")}";
            message.Body = new TextPart("html")
            {
                Text = $@"
  <div style='font-family: Arial, sans-serif; background: #f7fafc; padding: 32px;'>
    <div style='max-width: 480px; margin: 0 auto; background: #fff; border-radius: 8px; box-shadow: 0 2px 8px #0001; padding: 32px;'>
      <div style='text-align: center; margin-bottom: 24px;'>
        <img src='https://localhost:3000/logo.png' alt='OpenLoyalty' style='height: 48px; margin-bottom: 8px;' />
        <h2 style='color: #2563eb; margin: 0;'>Welcome to OpenLoyalty!</h2>
      </div>
      <p style='font-size: 16px; color: #222;'>Hi <b>{partnerUser.FirstName}</b>,</p>
      <p style='font-size: 16px; color: #222;'>Thank you for registering as a partner. Please activate your account by clicking the button below:</p>
      <div style='text-align: center; margin: 32px 0;'>
        <a href='{activationLink}' style='display: inline-block; background: #2563eb; color: #fff; font-weight: bold; padding: 14px 32px; border-radius: 6px; text-decoration: none; font-size: 18px;'>Activate Account</a>
      </div>
      <p style='font-size: 15px; color: #555;'>If you did not register, you can ignore this email.</p>
      <p style='font-size: 15px; color: #555;'>Thank you,<br/>The OpenLoyalty Team</p>
    </div>
  </div>"
            };
            using (var client = new MailKit.Net.Smtp.SmtpClient())
            {
                await client.ConnectAsync(smtp["Host"] ?? "mailhog", int.Parse(smtp["Port"] ?? "1025"), bool.Parse(smtp["EnableSsl"] ?? "false"));
                if (!string.IsNullOrEmpty(smtp["User"]))
                {
                    await client.AuthenticateAsync(smtp["User"], smtp["Pass"]);
                }
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send activation email to partner user.");
            // Optionally: add a warning to the response
        }
        
        _logger.LogInformation("Partner created: {Id}", partner.Id);
        return Ok(new
        {
            partner.Id,
            partner.Name,
            partner.Email,
            partner.Phone,
            partner.Address,
            partner.State,
            partner.City,
            partner.Zipcode,
            partner.Category,
            partner.TaxId,
            partner.Description,
            partner.Website,
            partner.CommisionRate,
            partner.Status,
            partner.CreatedAt,
            partner.UpdatedAt,
            user = new {
                partnerUser.Id,
                partnerUser.FirstName,
                partnerUser.LastName,
                partnerUser.Email,
                partnerUser.Role,
                partnerUser.MerchantId,
                partnerUser.Status,
                partnerUser.IsActive,
                partnerUser.CreatedAt
            }
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] PartnerUpdateRequest request)
    {
        _logger.LogInformation("PUT /api/admin/partners/{Id} - Updating partner", id);
        
        var partner = await _db.Merchants.FindAsync(id);
        if (partner == null) 
        {
            _logger.LogWarning("Partner not found for update: {Id}", id);
            return NotFound();
        }
        
        // Update only provided fields
        if (!string.IsNullOrEmpty(request.Name))
            partner.Name = request.Name;
        if (!string.IsNullOrEmpty(request.Email))
            partner.Email = request.Email;
        if (!string.IsNullOrEmpty(request.Phone))
            partner.Phone = request.Phone;
        if (!string.IsNullOrEmpty(request.Address))
            partner.Address = request.Address;
        if (!string.IsNullOrEmpty(request.State))
            partner.State = request.State;
        if (!string.IsNullOrEmpty(request.City))
            partner.City = request.City;
        if (!string.IsNullOrEmpty(request.Zipcode))
            partner.Zipcode = request.Zipcode;
        if (!string.IsNullOrEmpty(request.Category))
            partner.Category = request.Category;
        if (!string.IsNullOrEmpty(request.TaxId))
            partner.TaxId = request.TaxId;
        if (!string.IsNullOrEmpty(request.Description))
            partner.Description = request.Description;
        if (!string.IsNullOrEmpty(request.Website))
            partner.Website = request.Website;
        if (request.CommisionRate.HasValue)
            partner.CommisionRate = request.CommisionRate.Value;
        if (!string.IsNullOrEmpty(request.Status))
            partner.Status = request.Status;
            
        partner.UpdatedAt = DateTime.UtcNow;
        
        await _db.SaveChangesAsync();
        
        _logger.LogInformation("Partner updated: {Id}", partner.Id);
        return Ok(new
        {
            partner.Id,
            partner.Name,
            partner.Email,
            partner.Phone,
            partner.Address,
            partner.State,
            partner.City,
            partner.Zipcode,
            partner.Category,
            partner.TaxId,
            partner.Description,
            partner.Website,
            partner.CommisionRate,
            partner.Status,
            partner.CreatedAt,
            partner.UpdatedAt
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogInformation("DELETE /api/admin/partners/{Id} - Deleting partner", id);
        
        var partner = await _db.Merchants.FindAsync(id);
        if (partner == null) 
        {
            _logger.LogWarning("Partner not found for delete: {Id}", id);
            return NotFound();
        }
        
        _db.Merchants.Remove(partner);
        await _db.SaveChangesAsync();
        
        _logger.LogInformation("Partner deleted: {Id}", id);
        return Ok(new { message = "Partner deleted successfully" });
    }

    [HttpGet("{partnerId}/members")]
    public async Task<IActionResult> GetPartnerMembers(Guid partnerId)
    {
        var members = await _db.UserPartners
            .Where(up => up.PartnerId == partnerId)
            .Select(up => new {
                up.User.Id,
                up.User.FirstName,
                up.User.LastName,
                up.User.Email
            })
            .ToListAsync();
        return Ok(members);
    }

    [HttpPost("{partnerId}/members")]
    public async Task<IActionResult> AssociateMembers(Guid partnerId, [FromBody] AssociateMembersDto dto)
    {
        var partner = await _db.Merchants.FindAsync(partnerId);
        if (partner == null) return NotFound();
        foreach (var memberId in dto.MemberIds.Distinct())
        {
            var exists = await _db.UserPartners.AnyAsync(up => up.UserId == memberId && up.PartnerId == partnerId);
            if (!exists)
            {
                _db.UserPartners.Add(new UserPartner { UserId = memberId, PartnerId = partnerId });
            }
        }
        await _db.SaveChangesAsync();
        return Ok(new { message = "Members associated with partner." });
    }

    [HttpDelete("{partnerId}/members/{memberId}")]
    public async Task<IActionResult> DisassociateMember(Guid partnerId, int memberId)
    {
        var association = await _db.UserPartners.FirstOrDefaultAsync(up => up.UserId == memberId && up.PartnerId == partnerId);
        if (association == null) return NotFound();
        _db.UserPartners.Remove(association);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Member disassociated from partner." });
    }

    public class AssociateMembersDto
    {
        public List<int> MemberIds { get; set; } = new List<int>();
    }
}

// Request/Response DTOs
public class PartnerCreateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Zipcode { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public decimal CommisionRate { get; set; }
    public string? Status { get; set; }
}

public class PartnerUpdateRequest
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
    public string? Zipcode { get; set; }
    public string? Category { get; set; }
    public string? TaxId { get; set; }
    public string? Description { get; set; }
    public string? Website { get; set; }
    public decimal? CommisionRate { get; set; }
    public string? Status { get; set; }
} 