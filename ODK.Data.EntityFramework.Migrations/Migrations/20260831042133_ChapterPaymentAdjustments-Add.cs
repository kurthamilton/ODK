using System;
using Microsoft.EntityFrameworkCore.Migrations;
using ODK.Core.Chapters;
using ODK.Data.EntityFramework.Migrations.Enums;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class ChapterPaymentAdjustmentsAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .CreateEnumTable<ChapterPaymentAdjustmentType>()
                .InsertAllEnumValues<ChapterPaymentAdjustmentType>();

            migrationBuilder.CreateTable(
                name: "ChapterPaymentAdjustments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "money", nullable: false),
                    ChapterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CurrencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PaymentRefundId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RecoveredAmount = table.Column<decimal>(type: "money", nullable: false),
                    ChapterPaymentAdjustmentTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChapterPaymentAdjustments", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_ChapterPaymentAdjustments_Chapters_ChapterId",
                        column: x => x.ChapterId,
                        principalTable: "Chapters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChapterPaymentAdjustments_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChapterPaymentAdjustmentRecoveries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "money", nullable: false),
                    ChapterPaymentAdjustmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChapterPaymentAdjustmentRecoveries", x => x.Id)
                        .Annotation("SqlServer:Clustered", false);
                    table.ForeignKey(
                        name: "FK_ChapterPaymentAdjustmentRecoveries_ChapterPaymentAdjustments_ChapterPaymentAdjustmentId",
                        column: x => x.ChapterPaymentAdjustmentId,
                        principalTable: "ChapterPaymentAdjustments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChapterPaymentAdjustmentRecoveries_ChapterPaymentAdjustmentId",
                table: "ChapterPaymentAdjustmentRecoveries",
                column: "ChapterPaymentAdjustmentId")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_ChapterPaymentAdjustmentRecoveries_PaymentId",
                table: "ChapterPaymentAdjustmentRecoveries",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_ChapterPaymentAdjustments_ChapterId",
                table: "ChapterPaymentAdjustments",
                column: "ChapterId")
                .Annotation("SqlServer:Clustered", true);

            migrationBuilder.CreateIndex(
                name: "IX_ChapterPaymentAdjustments_CurrencyId",
                table: "ChapterPaymentAdjustments",
                column: "CurrencyId");

            migrationBuilder.AddEnumForeignKey<ChapterPaymentAdjustmentType>(
                "ChapterPaymentAdjustments", "ChapterPaymentAdjustmentTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChapterPaymentAdjustmentRecoveries");

            // Before the lookup: DropEnumTable fails while anything still references it.
            migrationBuilder.DropTable(
                name: "ChapterPaymentAdjustments");

            migrationBuilder.DropEnumTable<ChapterPaymentAdjustmentType>();
        }
    }
}
