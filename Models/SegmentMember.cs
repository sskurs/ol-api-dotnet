using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ol_api_dotnet.Models
{
    public class SegmentMember
    {
        [Key, Column(Order = 0)]
        public Guid SegmentId { get; set; }
        public Segment Segment { get; set; } = null!;

        [Key, Column(Order = 1)]
        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
} 