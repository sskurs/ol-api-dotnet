using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ol_api_dotnet.Models
{
    public class Tier
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
        [Required]
        public string Level { get; set; } = "bronze"; // bronze, silver, gold, platinum
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
} 