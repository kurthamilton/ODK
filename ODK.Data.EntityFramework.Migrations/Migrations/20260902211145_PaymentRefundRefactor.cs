using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class PaymentRefundRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActualCommissionAmount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ActualConnectedAccountAmount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ExternalTransferId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ReconciliationFailedUtc",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ReconciliationFailureReason",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ReconciliationIgnoredUtc",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "TransferWithheldAmount",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "TransferredUtc",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "DeclinedReason",
                table: "PaymentRefunds");

            migrationBuilder.DropColumn(
                name: "ExternalReversalId",
                table: "PaymentRefunds");

            migrationBuilder.DropColumn(
                name: "ResolvedByMemberId",
                table: "PaymentRefunds");

            migrationBuilder.DropColumn(
                name: "ResolvedUtc",
                table: "PaymentRefunds");

            migrationBuilder.DropColumn(
                name: "ReversedAmount",
                table: "PaymentRefunds");

            migrationBuilder.DropColumn(
                name: "ReversedUtc",
                table: "PaymentRefunds");

            migrationBuilder.CreateTable(
                name: "PaymentReconciliations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FailedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IgnoredUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentReconciliations", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_PaymentReconciliations_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTransfers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "money", nullable: false),
                    CommissionAmount = table.Column<decimal>(type: "money", nullable: false),
                    CompletedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WithheldAmount = table.Column<decimal>(type: "money", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTransfers", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_PaymentTransfers_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTransferReversals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActualAmount = table.Column<decimal>(type: "money", nullable: true),
                    Amount = table.Column<decimal>(type: "money", nullable: false),
                    CompletedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PaymentRefundId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentTransferId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTransferReversals", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_PaymentTransferReversals_PaymentTransfers_PaymentTransferId",
                        column: x => x.PaymentTransferId,
                        principalTable: "PaymentTransfers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReconciliations_PaymentId",
                table: "PaymentReconciliations",
                column: "PaymentId",
                unique: true)
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransferReversals_PaymentRefundId",
                table: "PaymentTransferReversals",
                column: "PaymentRefundId")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransferReversals_PaymentTransferId",
                table: "PaymentTransferReversals",
                column: "PaymentTransferId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransfers_PaymentId",
                table: "PaymentTransfers",
                column: "PaymentId",
                unique: true)
                .Annotation("SqlServer:Clustered", true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentReconciliations");

            migrationBuilder.DropTable(
                name: "PaymentTransferReversals");

            migrationBuilder.DropTable(
                name: "PaymentTransfers");

            migrationBuilder.AddColumn<decimal>(
                name: "ActualCommissionAmount",
                table: "Payments",
                type: "money",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ActualConnectedAccountAmount",
                table: "Payments",
                type: "money",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalTransferId",
                table: "Payments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReconciliationFailedUtc",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReconciliationFailureReason",
                table: "Payments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReconciliationIgnoredUtc",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TransferWithheldAmount",
                table: "Payments",
                type: "money",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TransferredUtc",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeclinedReason",
                table: "PaymentRefunds",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalReversalId",
                table: "PaymentRefunds",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ResolvedByMemberId",
                table: "PaymentRefunds",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResolvedUtc",
                table: "PaymentRefunds",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReversedAmount",
                table: "PaymentRefunds",
                type: "money",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReversedUtc",
                table: "PaymentRefunds",
                type: "datetime2",
                nullable: true);
        }
    }
}
