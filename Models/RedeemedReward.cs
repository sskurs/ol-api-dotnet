using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ol_api_dotnet.Models
{
    public class RedeemedReward
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        public DateTime RedeemedDate { get; set; } = DateTime.UtcNow;
        public string? TransactionId { get; set; }
        public string Status { get; set; } = "Redeemed";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 