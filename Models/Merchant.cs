using System;
using System.ComponentModel.DataAnnotations;

namespace ol_api_dotnet.Models
{
    public class Merchant
    {
        [Key]
        public Guid Id { get; set; }
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
        public string Status { get; set; } = "active";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
} 