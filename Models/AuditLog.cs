using System.ComponentModel.DataAnnotations;

namespace ol_api_dotnet.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty; // e.g. User, Role
        public int? EntityId { get; set; }
        public string? PerformedBy { get; set; } // username or id
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? Details { get; set; }
    }
} 