using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ol_api_dotnet.Data;
using ol_api_dotnet.Models;

namespace ol_api_dotnet.Controllers
{
    [ApiController]
    [Route("api/admin/roles")]
    [Authorize]
    public class RoleController : ControllerBase
    {
        private readonly AppDbContext _db;
        public RoleController(AppDbContext db)
        {
            _db = db;
        }

        // GET: api/admin/roles
        [HttpGet]
        public async Task<IActionResult> List()
        {
            var roles = await _db.Roles.ToListAsync();
            return Ok(roles);
        }

        // GET: api/admin/roles/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var role = await _db.Roles.FindAsync(id);
            if (role == null) return NotFound();
            return Ok(role);
        }

        // POST: api/admin/roles
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Role role)
        {
            if (await _db.Roles.AnyAsync(r => r.Name == role.Name))
                return BadRequest(new { message = "Role already exists" });
            _db.Roles.Add(role);
            await _db.SaveChangesAsync();
            return Ok(role);
        }

        // PUT: api/admin/roles/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Role update)
        {
            var role = await _db.Roles.FindAsync(id);
            if (role == null) return NotFound();
            if (!string.IsNullOrEmpty(update.Name)) role.Name = update.Name;
            await _db.SaveChangesAsync();
            return Ok(role);
        }

        // DELETE: api/admin/roles/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var role = await _db.Roles.FindAsync(id);
            if (role == null) return NotFound();
            _db.Roles.Remove(role);
            await _db.SaveChangesAsync();
            return Ok(new { message = $"Role {id} deleted" });
        }
    }
} 