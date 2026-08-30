using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class SitePaymentSettingsDrop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropConstraintIfExists(
                name: "FK_Payments_SitePaymentsSettings_SitePaymentSettingId",
                table: "Payments");

            migrationBuilder.DropConstraintIfExists(
                name: "FK_SiteSubscriptions_SitePaymentSettings",
                table: "SiteSubscriptions");

            migrationBuilder.DropTable(
                name: "SitePaymentSettings");

            migrationBuilder.DropColumn(
                name: "SitePaymentSettingId",
                table: "SiteSubscriptions");

            migrationBuilder.DropColumn(
                name: "SitePaymentSettingId",
                table: "SitePaymentProducts");

            migrationBuilder.DropColumn(
                name: "SitePaymentSettingId",
                table: "ChapterSubscriptions");

            migrationBuilder.DropColumn(
                name: "SitePaymentSettingId",
                table: "ChapterPaymentAccounts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SitePaymentSettingId",
                table: "SiteSubscriptions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SitePaymentSettingId",
                table: "SitePaymentProducts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SitePaymentSettingId",
                table: "ChapterSubscriptions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SitePaymentSettingId",
                table: "ChapterPaymentAccounts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SitePaymentSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    ApiPublicKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ApiSecretKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Commission = table.Column<decimal>(type: "decimal(19,4)", precision: 19, scale: 4, nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    Environment = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExternalId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ExternalUrl = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlatformTypeId = table.Column<int>(type: "int", nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SitePaymentSettings", x => x.Id);
                });
        }
    }
}
