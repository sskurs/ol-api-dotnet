using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ol_api_dotnet.Migrations
{
    /// <inheritdoc />
    public partial class SeedPurchaseAmountRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "EarningRules",
                columns: new[] { "Id", "IsActive", "Name", "RuleJson" },
                values: new object[] { 1, true, "Purchase Amount Greater Than 5000", "\r\n        {\r\n            \"WorkflowName\": \"PurchaseRule\",\r\n            \"Rules\": [\r\n                {\r\n                    \"RuleName\": \"PurchaseAmountGreaterThan5000\",\r\n                    \"ErrorMessage\": \"Purchase amount is not greater than 5000.\",\r\n                    \"ErrorType\": \"Error\",\r\n                    \"RuleExpressionType\": \"LambdaExpression\",\r\n                    \"Expression\": \"input.PurchaseAmount > 5000\",\r\n                    \"Actions\": {\r\n                        \"OnSuccess\": {\r\n                            \"Name\": \"Evaluate\",\r\n                            \"Context\": {\r\n                                \"Expression\": \"100\"\r\n                            }\r\n                        }\r\n                    }\r\n                }\r\n            ]\r\n        }" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "EarningRules",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
