using System.ComponentModel.DataAnnotations;

namespace ol_api_dotnet.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        [Required]
        public string Role { get; set; } = "user";
        public int? RoleId { get; set; }
        public Role? RoleEntity { get; set; }
        public Guid? MerchantId { get; set; } // Associated partner/merchant
        public ICollection<UserPartner> UserPartners { get; set; } = new List<UserPartner>();
        public string Status { get; set; } = "active";
        public string? Phone { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = false;
        public string? ActivationToken { get; set; }
    }
} 