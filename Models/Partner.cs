using ol_api_dotnet.Models;

public class UserPartner
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid PartnerId { get; set; }
    public Partner Partner { get; set; } = null!;
}

public class Partner
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    // Add other fields as needed
    public ICollection<UserPartner> UserPartners { get; set; } = new List<UserPartner>();
} 