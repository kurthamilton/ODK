using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class PaymentsActualAmountsAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ActualAmount",
                table: "Payments",
                type: "money",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ActualFeeAmount",
                table: "Payments",
                type: "money",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ActualNetAmount",
                table: "Payments",
                type: "money",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ActualTransferAmount",
                table: "Payments",
                type: "money",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SettlementCurrencyCode",
                table: "Payments",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualAmount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ActualFeeAmount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ActualNetAmount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ActualTransferAmount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "SettlementCurrencyCode",
                table: "Payments");
        }
    }
}
