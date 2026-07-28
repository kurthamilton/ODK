using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class DateColumnsUtcAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SentUtc",
                table: "SentEmails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidUtc",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresUtc",
                table: "MemberSubscriptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PurchasedUtc",
                table: "MemberSubscriptionLog",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedUtc",
                table: "Members",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedUtc",
                table: "MemberPasswordResetRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresUtc",
                table: "MemberPasswordResetRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PostedUtc",
                table: "InstagramPosts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedUtc",
                table: "Features",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateUtc",
                table: "Events",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SentUtc",
                table: "EventInvites",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledUtc",
                table: "EventEmails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SentUtc",
                table: "EventEmails",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedUtc",
                table: "Errors",
                type: "datetime2",
                nullable: true);

            // Backfill the new UTC columns from the legacy columns they replace. The app dual-writes both
            // from the next deploy on, so these one-off copies only need to cover existing rows.
            migrationBuilder.Sql("UPDATE [SentEmails] SET [SentUtc] = [SentDate]");
            migrationBuilder.Sql("UPDATE [Payments] SET [PaidUtc] = [PaidDate]");
            migrationBuilder.Sql("UPDATE [MemberSubscriptions] SET [ExpiresUtc] = [ExpiryDate]");
            migrationBuilder.Sql("UPDATE [MemberSubscriptionLog] SET [PurchasedUtc] = [PurchaseDate]");
            migrationBuilder.Sql("UPDATE [Members] SET [CreatedUtc] = [CreatedDate]");
            migrationBuilder.Sql("UPDATE [MemberPasswordResetRequests] SET [CreatedUtc] = [Created]");
            migrationBuilder.Sql("UPDATE [MemberPasswordResetRequests] SET [ExpiresUtc] = [Expires]");
            migrationBuilder.Sql("UPDATE [InstagramPosts] SET [PostedUtc] = [Date]");
            migrationBuilder.Sql("UPDATE [Features] SET [CreatedUtc] = [Created]");
            migrationBuilder.Sql("UPDATE [Events] SET [DateUtc] = [Date]");
            migrationBuilder.Sql("UPDATE [EventInvites] SET [SentUtc] = [SentDate]");
            migrationBuilder.Sql("UPDATE [EventEmails] SET [ScheduledUtc] = [ScheduledDate]");
            migrationBuilder.Sql("UPDATE [EventEmails] SET [SentUtc] = [SentDate]");
            migrationBuilder.Sql("UPDATE [Errors] SET [CreatedUtc] = [CreatedDate]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SentUtc",
                table: "SentEmails");

            migrationBuilder.DropColumn(
                name: "PaidUtc",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ExpiresUtc",
                table: "MemberSubscriptions");

            migrationBuilder.DropColumn(
                name: "PurchasedUtc",
                table: "MemberSubscriptionLog");

            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                table: "MemberPasswordResetRequests");

            migrationBuilder.DropColumn(
                name: "ExpiresUtc",
                table: "MemberPasswordResetRequests");

            migrationBuilder.DropColumn(
                name: "PostedUtc",
                table: "InstagramPosts");

            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                table: "Features");

            migrationBuilder.DropColumn(
                name: "DateUtc",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "SentUtc",
                table: "EventInvites");

            migrationBuilder.DropColumn(
                name: "ScheduledUtc",
                table: "EventEmails");

            migrationBuilder.DropColumn(
                name: "SentUtc",
                table: "EventEmails");

            migrationBuilder.DropColumn(
                name: "CreatedUtc",
                table: "Errors");
        }
    }
}
