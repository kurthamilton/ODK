using System;
using Microsoft.EntityFrameworkCore.Migrations;
using ODK.Core.Platforms;
using ODK.Data.EntityFramework.Migrations.Enums;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class SitePaymentProductsAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SitePaymentProducts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlatformTypeId = table.Column<int>(type: "int", nullable: false),
                    SitePaymentSettingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SitePaymentProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SitePaymentProducts_SitePaymentSettings_SitePaymentSettingId",
                        column: x => x.SitePaymentSettingId,
                        principalTable: "SitePaymentSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SitePaymentProducts_PlatformTypeId_SitePaymentSettingId",
                table: "SitePaymentProducts",
                columns: new[] { "PlatformTypeId", "SitePaymentSettingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SitePaymentProducts_SitePaymentSettingId",
                table: "SitePaymentProducts",
                column: "SitePaymentSettingId");

            migrationBuilder.AddEnumForeignKey<PlatformType>("SitePaymentProducts", "PlatformTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SitePaymentProducts");
        }
    }
}
