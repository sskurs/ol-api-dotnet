namespace ol_api_dotnet.Data;

using Microsoft.EntityFrameworkCore;
using ol_api_dotnet.Models;

public class AppDbContext : DbContext
{

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<User> Users { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Campaign> Campaigns { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Points> Points { get; set; }
    public DbSet<EarningRule> EarningRules { get; set; }
    public DbSet<Tier> Tiers { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<PointsHistory> PointsHistories { get; set; }
    public DbSet<Reward> Rewards { get; set; }
    public DbSet<RedeemedReward> RedeemedRewards { get; set; }
    public DbSet<Achievement> Achievements { get; set; }
    public DbSet<Merchant> Merchants { get; set; }
    public DbSet<Transfer> Transfers { get; set; }
    public DbSet<Segment> Segments { get; set; }
    public DbSet<SegmentMember> SegmentMembers { get; set; }
    public DbSet<CustomEvent> CustomEvents { get; set; }
    public DbSet<UserPartner> UserPartners { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>()
            .HasOne(u => u.RoleEntity)
            .WithMany()
            .HasForeignKey(u => u.RoleId);
        modelBuilder.Entity<SegmentMember>()
            .HasKey(sm => new { sm.SegmentId, sm.UserId });

        // Many-to-many User <-> Partner
        modelBuilder.Entity<UserPartner>()
            .HasKey(up => new { up.UserId, up.PartnerId });
        modelBuilder.Entity<UserPartner>()
            .HasOne(up => up.User)
            .WithMany(u => u.UserPartners)
            .HasForeignKey(up => up.UserId);
        modelBuilder.Entity<UserPartner>()
            .HasOne(up => up.Partner)
            .WithMany(p => p.UserPartners)
            .HasForeignKey(up => up.PartnerId);

        var ruleJson = @"
        {
            ""WorkflowName"": ""PurchaseRule"",
            ""Rules"": [
                {
                    ""RuleName"": ""PurchaseAmountGreaterThan5000"",
                    ""ErrorMessage"": ""Purchase amount is not greater than 5000."",
                    ""ErrorType"": ""Error"",
                    ""RuleExpressionType"": ""LambdaExpression"",
                    ""Expression"": ""input.PurchaseAmount > 5000"",
                    ""Actions"": {
                        ""OnSuccess"": {
                            ""Name"": ""Evaluate"",
                            ""Context"": {
                                ""Expression"": ""100""
                            }
                        }
                    }
                }
            ]
        }";

        modelBuilder.Entity<EarningRule>().HasData(
            new EarningRule
            {
                Id = 1,
                Name = "Purchase Amount Greater Than 5000",
                RuleJson = ruleJson,
                IsActive = true
            }
        );
    }
} 