using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace ol_api_dotnet.Models
{
    public class CustomEvent
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [ForeignKey("UserId")]
        public User? User { get; set; }
        
        [Required]
        public string Type { get; set; } = "";
        
        public JsonDocument? Data { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public int? PointsAwarded { get; set; }
        
        public string? Metadata { get; set; } // JSON string for additional data
    }
} 