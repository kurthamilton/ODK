using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class SiteSubscriptionsSitePaymentProductIdMakeRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* A subscription's product is the one for its platform and payment settings account, which is
               what SitePaymentProducts is keyed on, so every link is derivable rather than needing to be
               supplied. A subscription whose platform and account have no product row keeps its null and
               fails the alter below: there is nothing to point it at, and creating one means creating it in
               the payment provider first, which a migration cannot do. */
            migrationBuilder.Sql(
                """
                UPDATE siteSubscription
                SET siteSubscription.SitePaymentProductId = sitePaymentProduct.Id
                FROM SiteSubscriptions siteSubscription
                INNER JOIN SitePaymentProducts sitePaymentProduct
                    ON sitePaymentProduct.PlatformTypeId = siteSubscription.PlatformTypeId
                    AND sitePaymentProduct.SitePaymentSettingId = siteSubscription.SitePaymentSettingId
                WHERE siteSubscription.SitePaymentProductId IS NULL;
                """);

            // SQL Server will not modify a column a foreign key constraint is defined on, so the key comes
            // off for the alter and goes back on after it.
            migrationBuilder.DropForeignKey(
                name: "FK_SiteSubscriptions_SitePaymentProducts_SitePaymentProductId",
                table: "SiteSubscriptions");

            /* No defaultValue, which the scaffolder supplies as an empty guid: the backfill above leaves no
               nulls for one to fill, and an empty guid matches no product, so a row it missed would only
               fail the foreign key below instead - reporting a broken relationship rather than the missing
               product that is the actual problem. */
            migrationBuilder.AlterColumn<Guid>(
                name: "SitePaymentProductId",
                table: "SiteSubscriptions",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SiteSubscriptions_SitePaymentProducts_SitePaymentProductId",
                table: "SiteSubscriptions",
                column: "SitePaymentProductId",
                principalTable: "SitePaymentProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /* The backfilled links stay. Which rows the backfill wrote is not recorded, so none of them can
               be returned to null - and the round trip still lands correctly, because a second Up finds
               nothing to fill and tightens the same column over the same data. */
            migrationBuilder.DropForeignKey(
                name: "FK_SiteSubscriptions_SitePaymentProducts_SitePaymentProductId",
                table: "SiteSubscriptions");

            migrationBuilder.AlterColumn<Guid>(
                name: "SitePaymentProductId",
                table: "SiteSubscriptions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_SiteSubscriptions_SitePaymentProducts_SitePaymentProductId",
                table: "SiteSubscriptions",
                column: "SitePaymentProductId",
                principalTable: "SitePaymentProducts",
                principalColumn: "Id");
        }
    }
}
