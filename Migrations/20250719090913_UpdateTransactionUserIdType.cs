using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ol_api_dotnet.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTransactionUserIdType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // No changes to Id column. If you need to change UserId type, add here.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No changes to Id column. If you need to revert UserId type, add here.
        }
    }
}
