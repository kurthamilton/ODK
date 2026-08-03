using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class MemberSiteSubscriptionsRemove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemberSiteSubscriptions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MemberSiteSubscriptions",
                columns: table => new
                {
                    MemberSiteSubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExpiresUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExternalId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MemberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SiteSubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SiteSubscriptionPriceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberSiteSubscriptions", x => x.MemberSiteSubscriptionId);
                    table.ForeignKey(
                        name: "FK_MemberSiteSubscriptions_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "MemberId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MemberSiteSubscriptions_SiteSubscriptionPrices_SiteSubscriptionPriceId",
                        column: x => x.SiteSubscriptionPriceId,
                        principalTable: "SiteSubscriptionPrices",
                        principalColumn: "SiteSubscriptionPriceId");
                    table.ForeignKey(
                        name: "FK_MemberSiteSubscriptions_SiteSubscriptions_SiteSubscriptionId",
                        column: x => x.SiteSubscriptionId,
                        principalTable: "SiteSubscriptions",
                        principalColumn: "SiteSubscriptionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MemberSiteSubscriptions_MemberId",
                table: "MemberSiteSubscriptions",
                column: "MemberId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MemberSiteSubscriptions_SiteSubscriptionId",
                table: "MemberSiteSubscriptions",
                column: "SiteSubscriptionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MemberSiteSubscriptions_SiteSubscriptionPriceId",
                table: "MemberSiteSubscriptions",
                column: "SiteSubscriptionPriceId",
                unique: true,
                filter: "[SiteSubscriptionPriceId] IS NOT NULL");
        }
    }
}
