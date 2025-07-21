using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ol_api_dotnet.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } = "";
        public DateTime Date { get; set; }
        public string? Description { get; set; }
        public int? MerchantId { get; set; }
        
        public User? User { get; set; }
        public Merchant? Merchant { get; set; }
    }
} 