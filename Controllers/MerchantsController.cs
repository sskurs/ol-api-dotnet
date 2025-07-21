using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ol_api_dotnet.Data;
using ol_api_dotnet.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api/admin/merchants")]
[Authorize]
public class MerchantsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<MerchantsController> _logger;
    
    public MerchantsController(AppDbContext db, ILogger<MerchantsController> logger) 
    { 
        _db = db; 
        _logger = logger; 
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(int page = 1, int limit = 10, string search = "")
    {
        _logger.LogInformation("GET /api/admin/merchants?page={Page}&limit={Limit}&search={Search}", page, limit, search);
        
        var query = _db.Merchants.AsQueryable();
        
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(m => m.Name.Contains(search) || m.Email.Contains(search));
        }
        
        var total = await query.CountAsync();
        var merchants = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();
            
        return Ok(new { merchants, total, page, totalPages = (int)Math.Ceiling(total / (double)limit) });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        _logger.LogInformation("GET /api/admin/merchants/{Id}", id);
        
        var merchant = await _db.Merchants.FindAsync(id);
        if (merchant == null) 
        {
            _logger.LogWarning("Merchant not found: {Id}", id);
            return NotFound();
        }
        
        return Ok(merchant);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Merchant merchant)
    {
        _logger.LogInformation("POST /api/admin/merchants - Creating merchant: {@Merchant}", merchant);
        
        merchant.Id = Guid.NewGuid();
        merchant.CreatedAt = DateTime.UtcNow;
        merchant.UpdatedAt = DateTime.UtcNow;
        
        _db.Merchants.Add(merchant);
        await _db.SaveChangesAsync();
        
        _logger.LogInformation("Merchant created: {Id}", merchant.Id);
        return Ok(merchant);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Merchant data)
    {
        _logger.LogInformation("PUT /api/admin/merchants/{Id} - Updating merchant", id);
        
        var merchant = await _db.Merchants.FindAsync(id);
        if (merchant == null) 
        {
            _logger.LogWarning("Merchant not found for update: {Id}", id);
            return NotFound();
        }
        
        // Only update fields that are provided (not null or empty)
        if (!string.IsNullOrEmpty(data.Name))
            merchant.Name = data.Name;
        if (!string.IsNullOrEmpty(data.Email))
            merchant.Email = data.Email;
        if (!string.IsNullOrEmpty(data.Phone))
            merchant.Phone = data.Phone;
        if (!string.IsNullOrEmpty(data.Address))
            merchant.Address = data.Address;
        if (!string.IsNullOrEmpty(data.State))
            merchant.State = data.State;
        if (!string.IsNullOrEmpty(data.City))
            merchant.City = data.City;
        if (!string.IsNullOrEmpty(data.Zipcode))
            merchant.Zipcode = data.Zipcode;
        if (!string.IsNullOrEmpty(data.Category))
            merchant.Category = data.Category;
        if (!string.IsNullOrEmpty(data.TaxId))
            merchant.TaxId = data.TaxId;
        if (!string.IsNullOrEmpty(data.Description))
            merchant.Description = data.Description;
        if (!string.IsNullOrEmpty(data.Website))
            merchant.Website = data.Website;
        if (data.CommisionRate > 0)
            merchant.CommisionRate = data.CommisionRate;
        if (!string.IsNullOrEmpty(data.Status))
            merchant.Status = data.Status;
            
        merchant.UpdatedAt = DateTime.UtcNow;
        
        await _db.SaveChangesAsync();
        
        _logger.LogInformation("Merchant updated: {Id}", merchant.Id);
        return Ok(merchant);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogInformation("DELETE /api/admin/merchants/{Id} - Deleting merchant", id);
        
        var merchant = await _db.Merchants.FindAsync(id);
        if (merchant == null) 
        {
            _logger.LogWarning("Merchant not found for delete: {Id}", id);
            return NotFound();
        }
        
        _db.Merchants.Remove(merchant);
        await _db.SaveChangesAsync();
        
        _logger.LogInformation("Merchant deleted: {Id}", id);
        return Ok(new { message = "Merchant deleted successfully" });
    }
} 