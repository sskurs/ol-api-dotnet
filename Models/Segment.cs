using System;
using System.ComponentModel.DataAnnotations;

namespace ol_api_dotnet.Models
{
    public class Segment
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string Criteria { get; set; } = "[]"; // JSON string
        
        [Range(0, int.MaxValue)]
        public int MemberCount { get; set; } = 0;
        
        [Required]
        [RegularExpression("^(active|inactive|draft)$")]
        public string Status { get; set; } = "active"; // active/inactive/draft
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        
        [StringLength(7)]
        [RegularExpression("^#[0-9A-Fa-f]{6}$")]
        public string? Color { get; set; }
    }
} 