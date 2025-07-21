using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ol_api_dotnet.Models
{
    public class Points
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; } // Re-added UserId
        [ForeignKey("UserId")]
        [JsonIgnore]
        public User? User { get; set; }
        public int Balance { get; set; } = 0;
    }
} 