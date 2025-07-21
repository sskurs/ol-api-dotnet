using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ol_api_dotnet.Models
{
    public class Campaign
    {
        [Key]
        public Guid CampaignId { get; set; }
        [Required]
        [MaxLength(255)]
        public string Reward { get; set; } = string.Empty;
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string? ConditionsDescription { get; set; }
        public string? UsageInstruction { get; set; }
        public bool Active { get; set; } = false;
        [Column(TypeName = "numeric(14,2)")]
        public decimal CostInPoints { get; set; }
        [Required]
        public string Levels { get; set; } = "[]"; // JSON
        [Required]
        public string Segments { get; set; } = "[]"; // JSON
        public bool Unlimited { get; set; } = true;
        public bool SingleCoupon { get; set; } = true;
        public int? UsageLimit { get; set; }
        public int? LimitPerUser { get; set; }
        [Required]
        public string Coupons { get; set; } = "[]"; // JSON
        public bool? CampaignActivityAllTimeActive { get; set; } = false;
        public DateTime? CampaignActivityActiveFrom { get; set; }
        public DateTime? CampaignActivityActiveTo { get; set; }
        public bool? CampaignVisibilityAllTimeVisible { get; set; } = false;
        public DateTime? CampaignVisibilityVisibleFrom { get; set; }
        public DateTime? CampaignVisibilityVisibleTo { get; set; }
        public string? CampaignPhotoPath { get; set; }
        public string? CampaignPhotoOriginalName { get; set; }
        public string? CampaignPhotoMime { get; set; }
    }
} 