using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ol_api_dotnet.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "CustomEvents");

            migrationBuilder.DropColumn(
                name: "EventName",
                table: "CustomEvents");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Transactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MerchantId",
                table: "Transactions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MerchantId1",
                table: "Transactions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<JsonDocument>(
                name: "Data",
                table: "CustomEvents",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "CustomEvents",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_MerchantId1",
                table: "Transactions",
                column: "MerchantId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Merchants_MerchantId1",
                table: "Transactions",
                column: "MerchantId1",
                principalTable: "Merchants",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Merchants_MerchantId1",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_MerchantId1",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "MerchantId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "MerchantId1",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Data",
                table: "CustomEvents");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "CustomEvents");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CustomEvents",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EventName",
                table: "CustomEvents",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
