using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <summary>
    /// Drops the columns the email, notification and Instagram tables were keyed on before
    /// EmailAndNotificationTables-Id-MakePrimaryKey moved them onto Id. Nothing has read or written them
    /// since the build that migration shipped with.
    /// </summary>
    /// <remarks>
    /// Written by hand: the mapping stopped naming these columns two migrations ago, so the model snapshot
    /// has not known about them since, and EF scaffolds an empty migration. The same goes for the last phase
    /// of every batch.
    /// </remarks>
    public partial class EmailAndNotificationTablesOldIdColumnsRemove : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SentEmailId",
                table: "SentEmails");

            migrationBuilder.DropColumn(
                name: "SentEmailEventId",
                table: "SentEmailEvents");

            migrationBuilder.DropColumn(
                name: "QueuedEmailId",
                table: "QueuedEmails");

            migrationBuilder.DropColumn(
                name: "QueuedEmailRecipientId",
                table: "QueuedEmailRecipients");

            migrationBuilder.DropColumn(
                name: "NotificationId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "InstagramImageId",
                table: "InstagramImages");

            migrationBuilder.DropColumn(
                name: "InstagramFetchLogId",
                table: "InstagramFetchLog");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            /* Nullable, and only the data restored: these columns were the key when this migration's Up ran,
               but putting that back is the previous migration's Down, not this one's. Going back through both
               returns the tables to where they started, and nothing is lost either way - the values are Id's.

               SentEmailEvents is the exception, and only in one direction: it had no primary key before this
               batch, so its Down leaves the table without one rather than inventing a constraint it never
               had - that is the previous migration's business too. */
            migrationBuilder.AddColumn<Guid>(
                name: "SentEmailId",
                table: "SentEmails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SentEmailEventId",
                table: "SentEmailEvents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "QueuedEmailId",
                table: "QueuedEmails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "QueuedEmailRecipientId",
                table: "QueuedEmailRecipients",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "NotificationId",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InstagramImageId",
                table: "InstagramImages",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InstagramFetchLogId",
                table: "InstagramFetchLog",
                type: "uniqueidentifier",
                nullable: true);

            // EXEC because the generated script puts these in the same batch as the columns they write to,
            // and SQL Server binds column names when it parses a batch - see LookupTables-Id-Add.
            migrationBuilder.Sql("EXEC('UPDATE SentEmails SET SentEmailId = Id WHERE SentEmailId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE SentEmailEvents SET SentEmailEventId = Id WHERE SentEmailEventId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE QueuedEmails SET QueuedEmailId = Id WHERE QueuedEmailId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE QueuedEmailRecipients SET QueuedEmailRecipientId = Id WHERE QueuedEmailRecipientId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE Notifications SET NotificationId = Id WHERE NotificationId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE InstagramImages SET InstagramImageId = Id WHERE InstagramImageId IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE InstagramFetchLog SET InstagramFetchLogId = Id WHERE InstagramFetchLogId IS NULL')");
        }
    }
}
