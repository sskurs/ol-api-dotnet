using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ol_api_dotnet.Models
{
    public class PurchaseEvent
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public int TransactionId { get; set; }
        
        [Required]
        public decimal Amount { get; set; }
        
        [Required]
        public string EventType { get; set; } = "purchase"; // "purchase", "refund", etc.
        
        public int? PointsAwarded { get; set; }
        
        public string? EarningRuleApplied { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        [ForeignKey("UserId")]
        [JsonIgnore]
        public User User { get; set; } = null!;
        
        [ForeignKey("TransactionId")]
        [JsonIgnore]
        public Transaction Transaction { get; set; } = null!;
    }
} 