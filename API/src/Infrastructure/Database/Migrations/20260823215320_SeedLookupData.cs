using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace FinancialPlanner.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class SeedLookupData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Currencies",
                columns: new[] { "Id", "Name", "UsdExchangeRate" },
                values: new object[,]
                {
                    { 1, "UAH", 0.0224m },
                    { 2, "USD", 1.0000m },
                    { 3, "EUR", 1.1685m }
                });

            migrationBuilder.InsertData(
                table: "IntervalUnits",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1, "Day" },
                    { 2, "Week" },
                    { 3, "Month" },
                    { 4, "Year" }
                });

            migrationBuilder.InsertData(
                table: "TransactionTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Expense" },
                    { 2, "Income" },
                    { 3, "Transfer" },
                    { 4, "Adjustment" }
                });

            migrationBuilder.InsertData(
                table: "Frequencies",
                columns: new[] { "Id", "IntervalUnitId", "IntervalValue", "Name", "UserId" },
                values: new object[,]
                {
                    { 1, 2, 1, "Week", null },
                    { 2, 2, 2, "Two Weeks", null },
                    { 3, 1, 1, "Day", null },
                    { 4, 3, 1, "Month", null },
                    { 5, 4, 1, "Year", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Frequencies",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Frequencies",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Frequencies",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Frequencies",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Frequencies",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "TransactionTypes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "TransactionTypes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "TransactionTypes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "TransactionTypes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "IntervalUnits",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "IntervalUnits",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "IntervalUnits",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "IntervalUnits",
                keyColumn: "id",
                keyValue: 4);
        }
    }
}
