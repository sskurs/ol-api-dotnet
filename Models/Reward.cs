using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ol_api_dotnet.Models
{
    public class Reward
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
        public string? Description { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Availability { get; set; } = 0; // percent
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 