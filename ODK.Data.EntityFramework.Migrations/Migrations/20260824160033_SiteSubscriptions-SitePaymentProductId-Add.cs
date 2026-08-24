using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class SiteSubscriptionsSitePaymentProductIdAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SitePaymentProductId",
                table: "SiteSubscriptions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SiteSubscriptions_SitePaymentProductId",
                table: "SiteSubscriptions",
                column: "SitePaymentProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_SiteSubscriptions_SitePaymentProducts_SitePaymentProductId",
                table: "SiteSubscriptions",
                column: "SitePaymentProductId",
                principalTable: "SitePaymentProducts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SiteSubscriptions_SitePaymentProducts_SitePaymentProductId",
                table: "SiteSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_SiteSubscriptions_SitePaymentProductId",
                table: "SiteSubscriptions");

            migrationBuilder.DropColumn(
                name: "SitePaymentProductId",
                table: "SiteSubscriptions");
        }
    }
}
