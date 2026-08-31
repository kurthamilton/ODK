using System;
using Microsoft.EntityFrameworkCore.Migrations;
using ODK.Core.Payments;
using ODK.Data.EntityFramework.Migrations.Enums;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class PaymentRefundsAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .CreateEnumTable<PaymentRefundStatusType>()
                .InsertAllEnumValues<PaymentRefundStatusType>();

            migrationBuilder.CreateTable(
                name: "PaymentRefunds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActualAmount = table.Column<decimal>(type: "money", nullable: true),
                    Amount = table.Column<decimal>(type: "money", nullable: false),
                    ChapterAmount = table.Column<decimal>(type: "money", nullable: true),
                    DeclinedReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ExternalId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExternalReversalId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FeeReturnedAmount = table.Column<decimal>(type: "money", nullable: true),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RefundedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RequestedByMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedByMemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResolvedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReversedAmount = table.Column<decimal>(type: "money", nullable: true),
                    ReversedUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SettlementCurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    PaymentRefundStatusTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentRefunds", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_PaymentRefunds_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRefunds_PaymentId",
                table: "PaymentRefunds",
                column: "PaymentId")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.AddEnumForeignKey<PaymentRefundStatusType>(
                "PaymentRefunds", "PaymentRefundStatusTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The table first: DropEnumTable fails while anything still references the lookup.
            migrationBuilder.DropTable(
                name: "PaymentRefunds");

            migrationBuilder.DropEnumTable<PaymentRefundStatusType>();
        }
    }
}
