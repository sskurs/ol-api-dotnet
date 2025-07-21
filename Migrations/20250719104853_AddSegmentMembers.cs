using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ol_api_dotnet.Migrations
{
    /// <inheritdoc />
    public partial class AddSegmentMembers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SegmentMembers",
                columns: table => new
                {
                    SegmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegmentMembers", x => new { x.SegmentId, x.UserId });
                    table.ForeignKey(
                        name: "FK_SegmentMembers_Segments_SegmentId",
                        column: x => x.SegmentId,
                        principalTable: "Segments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SegmentMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SegmentMembers_UserId",
                table: "SegmentMembers",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SegmentMembers");
        }
    }
}
