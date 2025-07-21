using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ol_api_dotnet.Migrations
{
    /// <inheritdoc />
    public partial class AddPartnerAssociationToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PartnerId",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Partner",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partner", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_PartnerId",
                table: "Users",
                column: "PartnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Partner_PartnerId",
                table: "Users",
                column: "PartnerId",
                principalTable: "Partner",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Partner_PartnerId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Partner");

            migrationBuilder.DropIndex(
                name: "IX_Users_PartnerId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PartnerId",
                table: "Users");
        }
    }
}
