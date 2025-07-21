using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ol_api_dotnet.Models
{
    public class PointsHistory
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        [JsonIgnore]
        public User? User { get; set; }
        public int Change { get; set; } // +100, -50, etc.
        public int BalanceAfter { get; set; }
        public string Reason { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
} 