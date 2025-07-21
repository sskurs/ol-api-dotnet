using System.ComponentModel.DataAnnotations;

namespace ol_api_dotnet.Models
{
    public class TransactionDto
    {
        [Required]
        public int UserId { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }
        
        [Required]
        public string Type { get; set; } = string.Empty;
        
        public DateTime? Date { get; set; }
    }
} 