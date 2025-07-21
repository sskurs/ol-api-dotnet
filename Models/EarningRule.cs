using System.ComponentModel.DataAnnotations;

namespace ol_api_dotnet.Models
{
    public class EarningRule
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string RuleJson { get; set; } = string.Empty; // To store RulesEngine workflow
        public bool IsActive { get; set; } = true;
    }
} 