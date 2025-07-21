using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ol_api_dotnet.Migrations
{
    /// <inheritdoc />
    public partial class UserPartnerGuidSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Partner_PartnerId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_PartnerId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PartnerId",
                table: "Users");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Partner",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateTable(
                name: "UserPartners",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    PartnerId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPartners", x => new { x.UserId, x.PartnerId });
                    table.ForeignKey(
                        name: "FK_UserPartners_Partner_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "Partner",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserPartners_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserPartners_PartnerId",
                table: "UserPartners",
                column: "PartnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserPartners");

            migrationBuilder.AddColumn<int>(
                name: "PartnerId",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Partner",
                type: "integer",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

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
    }
}
