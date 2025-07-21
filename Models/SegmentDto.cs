using System.ComponentModel.DataAnnotations;

namespace ol_api_dotnet.Models
{
    public class SegmentDto
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Criteria JSON cannot exceed 2000 characters")]
        public string Criteria { get; set; } = "[]";

        [Range(0, int.MaxValue, ErrorMessage = "Member count must be a positive number")]
        public int MemberCount { get; set; } = 0;

        [RegularExpression("^(active|inactive|draft)$", ErrorMessage = "Status must be 'active', 'inactive', or 'draft'")]
        public string Status { get; set; } = "active";

        [StringLength(7, ErrorMessage = "Color must be a valid hex color (e.g., #FF0000)")]
        [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex color (e.g., #FF0000)")]
        public string? Color { get; set; }
    }

    public class SegmentUpdateDto
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Criteria JSON cannot exceed 2000 characters")]
        public string Criteria { get; set; } = "[]";

        [Range(0, int.MaxValue, ErrorMessage = "Member count must be a positive number")]
        public int MemberCount { get; set; } = 0;

        [RegularExpression("^(active|inactive|draft)$", ErrorMessage = "Status must be 'active', 'inactive', or 'draft'")]
        public string Status { get; set; } = "active";

        [StringLength(7, ErrorMessage = "Color must be a valid hex color (e.g., #FF0000)")]
        [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex color (e.g., #FF0000)")]
        public string? Color { get; set; }
    }
} 