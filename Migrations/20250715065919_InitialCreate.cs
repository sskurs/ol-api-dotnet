using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ol_api_dotnet.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Campaigns",
                columns: table => new
                {
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reward = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ShortDescription = table.Column<string>(type: "text", nullable: true),
                    ConditionsDescription = table.Column<string>(type: "text", nullable: true),
                    UsageInstruction = table.Column<string>(type: "text", nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false),
                    CostInPoints = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    Levels = table.Column<string>(type: "text", nullable: false),
                    Segments = table.Column<string>(type: "text", nullable: false),
                    Unlimited = table.Column<bool>(type: "boolean", nullable: false),
                    SingleCoupon = table.Column<bool>(type: "boolean", nullable: false),
                    UsageLimit = table.Column<int>(type: "integer", nullable: true),
                    LimitPerUser = table.Column<int>(type: "integer", nullable: true),
                    Coupons = table.Column<string>(type: "text", nullable: false),
                    CampaignActivityAllTimeActive = table.Column<bool>(type: "boolean", nullable: true),
                    CampaignActivityActiveFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CampaignActivityActiveTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CampaignVisibilityAllTimeVisible = table.Column<bool>(type: "boolean", nullable: true),
                    CampaignVisibilityVisibleFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CampaignVisibilityVisibleTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CampaignPhotoPath = table.Column<string>(type: "text", nullable: true),
                    CampaignPhotoOriginalName = table.Column<string>(type: "text", nullable: true),
                    CampaignPhotoMime = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campaigns", x => x.CampaignId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Discriminator = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    External = table.Column<bool>(type: "boolean", nullable: true),
                    ApiKey = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Campaigns");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
