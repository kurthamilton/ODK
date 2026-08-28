using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class PaymentsActualAmountsSplitByParty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ActualTransferAmount",
                table: "Payments",
                newName: "ActualConnectedAccountAmount");

            migrationBuilder.AddColumn<decimal>(
                name: "ActualCommissionAmount",
                table: "Payments",
                type: "money",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualCommissionAmount",
                table: "Payments");

            migrationBuilder.RenameColumn(
                name: "ActualConnectedAccountAmount",
                table: "Payments",
                newName: "ActualTransferAmount");
        }
    }
}
