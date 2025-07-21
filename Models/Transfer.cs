using System.ComponentModel.DataAnnotations;

namespace ol_api_dotnet.Models;

public class Transfer
{
    public int Id { get; set; }
    
    [Required]
    public int FromUserId { get; set; }
    public User FromUser { get; set; } = null!;
    
    [Required]
    public int ToUserId { get; set; }
    public User ToUser { get; set; } = null!;
    
    [Required]
    public int Points { get; set; }
    
    [Required]
    public string Status { get; set; } = "pending"; // pending, completed, failed, cancelled
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? CompletedAt { get; set; }
    
    public string? Reason { get; set; }
    
    public string? Notes { get; set; }
    
    public int? AdminId { get; set; }
    public Admin? Admin { get; set; }
} 