using System.ComponentModel.DataAnnotations;

namespace ol_api_dotnet.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
    }
} 