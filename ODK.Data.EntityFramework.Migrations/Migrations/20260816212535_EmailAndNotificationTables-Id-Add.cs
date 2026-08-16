using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class EmailAndNotificationTablesIdAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "SentEmails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "SentEmailEvents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "QueuedEmails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "QueuedEmailRecipients",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "InstagramImages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "InstagramFetchLog",
                type: "uniqueidentifier",
                nullable: true);

            /* Copy the key across. Nullable for now and left that way deliberately: the build running while
               this is applied knows only the old column, so a row it inserts arrives with Id unset. The
               migration that makes Id the key backfills again before it adds the constraint.

               EXEC because the generated script puts the ALTER above and the UPDATE in one batch, and SQL
               Server binds column names when it parses a batch - see LookupTables-Id-Add. */
            migrationBuilder.Sql("EXEC('UPDATE SentEmails SET Id = SentEmailId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE SentEmailEvents SET Id = SentEmailEventId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE QueuedEmails SET Id = QueuedEmailId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE QueuedEmailRecipients SET Id = QueuedEmailRecipientId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE Notifications SET Id = NotificationId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE InstagramImages SET Id = InstagramImageId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE InstagramFetchLog SET Id = InstagramFetchLogId WHERE Id IS NULL')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "SentEmails");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "SentEmailEvents");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "QueuedEmails");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "QueuedEmailRecipients");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "InstagramImages");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "InstagramFetchLog");
        }
    }
}
