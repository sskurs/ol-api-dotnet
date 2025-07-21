using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ol_api_dotnet.Models
{
    public class Achievement
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
        public int Progress { get; set; } = 0; // percent
        public bool Completed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 