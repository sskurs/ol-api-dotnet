using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ol_api_dotnet.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ol_api_dotnet.Data;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.AspNetCore.Authorization;

namespace ol_api_dotnet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _db.Users.FindAsync(int.Parse(userId));
            if (user == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                id = user.Id,
                email = user.Email,
                name = $"{user.FirstName} {user.LastName}",
                role = user.Role
            });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            if (await _db.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest(new { message = "Email already registered" });

            Guid? merchantId = null;
            if (dto.MerchantId.HasValue)
            {
                // Try to parse int to Guid if possible, or expect frontend to send Guid
                merchantId = dto.MerchantId.Value;
            }

            var activationToken = Guid.NewGuid().ToString();
            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Phone = dto.Phone,
                Role = "user", // Default role
                MerchantId = merchantId,
                IsActive = false,
                ActivationToken = activationToken
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Send activation email
            await SendActivationEmail(user);

            return Ok(new { message = "User registered successfully. Please check your email to activate your account." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email || u.FirstName == dto.Email || u.LastName == dto.Email);
            bool valid = false;
            if (user != null)
            {
                try
                {
                    valid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
                }
                catch
                {
                    // Ignore, will try plain text below
                }
                if (!valid)
                {
                    valid = dto.Password == user.PasswordHash;
                }
            }
            if (user == null || !valid)
                return Unauthorized(new { message = "Invalid credentials" });

            var token = GenerateJwtToken(user);

            return Ok(new
            {
                token,
                user = new
                {
                    id = user.Id.ToString(),
                    email = user.Email,
                    name = $"{user.FirstName} {user.LastName}",
                    role = user.Role
                }
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UserUpdateDto dto)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();
            if (!string.IsNullOrEmpty(dto.FirstName)) user.FirstName = dto.FirstName;
            if (!string.IsNullOrEmpty(dto.LastName)) user.LastName = dto.LastName;
            if (!string.IsNullOrEmpty(dto.Email)) user.Email = dto.Email;
            if (!string.IsNullOrEmpty(dto.Password)) user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            if (!string.IsNullOrEmpty(dto.Phone)) user.Phone = dto.Phone;
            if (dto.MerchantId.HasValue)
            {
                user.MerchantId = dto.MerchantId.Value;
            }
            await _db.SaveChangesAsync();
            return Ok(user);
        }

        [HttpPut("members/{id}")]
        public async Task<IActionResult> UpdateMember(int id, [FromBody] UserUpdateDto dto)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();
            if (!string.IsNullOrEmpty(dto.FirstName)) user.FirstName = dto.FirstName;
            if (!string.IsNullOrEmpty(dto.LastName)) user.LastName = dto.LastName;
            if (!string.IsNullOrEmpty(dto.Email)) user.Email = dto.Email;
            if (!string.IsNullOrEmpty(dto.Password)) user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            if (!string.IsNullOrEmpty(dto.Phone)) user.Phone = dto.Phone;
            if (dto.MerchantId.HasValue)
            {
                user.MerchantId = dto.MerchantId.Value;
            }
            await _db.SaveChangesAsync();
            return Ok(user);
        }

        [HttpGet("activate")]
        [AllowAnonymous]
        public async Task<IActionResult> Activate([FromQuery] string token)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.ActivationToken == token);
            if (user == null) return BadRequest(new { message = "Invalid or expired activation token." });
            if (user.IsActive) return BadRequest(new { message = "Account already activated." });
            user.IsActive = true;
            user.ActivationToken = null;
            await _db.SaveChangesAsync();
            return Ok(new { message = "Account activated successfully. You can now log in." });
        }

        private string GenerateJwtToken(User user)
        {
            var jwt = _config.GetSection("Jwt");
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpireMinutes"] ?? "60"));
            var token = new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task SendActivationEmail(User user)
        {
            var smtp = _config.GetSection("Smtp");
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("OpenLoyalty", smtp["From"] ?? "noreply@openloyalty.local"));
            message.To.Add(new MailboxAddress(user.FirstName + " " + user.LastName, user.Email));
            message.Subject = "Activate your OpenLoyalty Account";
            var activationLink = $"https://your-frontend-url.com/(auth)/activate?token={Uri.EscapeDataString(user.ActivationToken ?? "")}";
            message.Body = new TextPart("html")
            {
                Text = $@"
  <div style='font-family: Arial, sans-serif; background: #f7fafc; padding: 32px;'>
    <div style='max-width: 480px; margin: 0 auto; background: #fff; border-radius: 8px; box-shadow: 0 2px 8px #0001; padding: 32px;'>
      <div style='text-align: center; margin-bottom: 24px;'>
        <img src='https://your-frontend-url.com/logo.png' alt='OpenLoyalty' style='height: 48px; margin-bottom: 8px;' />
        <h2 style='color: #2563eb; margin: 0;'>Welcome to OpenLoyalty!</h2>
      </div>
      <p style='font-size: 16px; color: #222;'>Hi <b>{user.FirstName}</b>,</p>
      <p style='font-size: 16px; color: #222;'>Thank you for registering. Please activate your account by clicking the button below:</p>
      <div style='text-align: center; margin: 32px 0;'>
        <a href='{activationLink}' style='display: inline-block; background: #2563eb; color: #fff; font-weight: bold; padding: 14px 32px; border-radius: 6px; text-decoration: none; font-size: 18px;'>Activate Account</a>
      </div>
      <p style='font-size: 15px; color: #555;'>If you did not register, you can ignore this email.</p>
      <p style='font-size: 15px; color: #555;'>Thank you,<br/>The OpenLoyalty Team</p>
    </div>
  </div>"
            };
            using var client = new SmtpClient();
            await client.ConnectAsync(smtp["Host"] ?? "mailhog", int.Parse(smtp["Port"] ?? "1025"), bool.Parse(smtp["EnableSsl"] ?? "false"));
            if (!string.IsNullOrEmpty(smtp["User"]))
            {
                await client.AuthenticateAsync(smtp["User"], smtp["Pass"]);
            }
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }

    public class RegisterDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public Guid? MerchantId { get; set; } // Associated partner/merchant
    }
    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class UserUpdateDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Phone { get; set; }
        public Guid? MerchantId { get; set; }
    }
} 