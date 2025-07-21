using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ol_api_dotnet.Models
{
    public class Admin : User
    {
        public bool External { get; set; } = false;
        public string? ApiKey { get; set; }

        public Admin()
        {
            Role = "admin";
        }
    }
} 