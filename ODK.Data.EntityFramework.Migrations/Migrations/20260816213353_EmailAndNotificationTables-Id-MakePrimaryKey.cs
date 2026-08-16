using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class EmailAndNotificationTablesIdMakePrimaryKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* Fill Id for anything the previous migration missed: it backfilled when it was applied, and
               the build that was live for the rest of that deploy knew only the old column. EXEC because
               the generated script batches statements together - see LookupTables-Id-Add. */
            migrationBuilder.Sql("EXEC('UPDATE SentEmails SET Id = SentEmailId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE SentEmailEvents SET Id = SentEmailEventId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE QueuedEmails SET Id = QueuedEmailId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE QueuedEmailRecipients SET Id = QueuedEmailRecipientId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE Notifications SET Id = NotificationId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE InstagramImages SET Id = InstagramImageId WHERE Id IS NULL')");
            migrationBuilder.Sql("EXEC('UPDATE InstagramFetchLog SET Id = InstagramFetchLogId WHERE Id IS NULL')");

            migrationBuilder.DropForeignKeys("QueuedEmailRecipients", "QueuedEmailId");

            migrationBuilder.DropForeignKeys("SentEmailEvents", "SentEmailId");

            /* Found by table rather than by name, and skipped where there is none. SentEmailEvents is
               mapped with HasKey but the table never had the constraint, so the scaffolded drop failed
               the migration - and a key created by hand would not carry the name EF guesses either. */
            migrationBuilder.DropPrimaryKeyIfExists("SentEmails");

            migrationBuilder.DropPrimaryKeyIfExists("SentEmailEvents");

            migrationBuilder.DropPrimaryKeyIfExists("QueuedEmails");

            migrationBuilder.DropPrimaryKeyIfExists("QueuedEmailRecipients");

            migrationBuilder.DropPrimaryKeyIfExists("Notifications");

            migrationBuilder.DropPrimaryKeyIfExists("InstagramImages");

            migrationBuilder.DropPrimaryKeyIfExists("InstagramFetchLog");

            migrationBuilder.AlterColumn<Guid>(
                name: "SentEmailId",
                table: "SentEmails",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "SentEmailEventId",
                table: "SentEmailEvents",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "QueuedEmailId",
                table: "QueuedEmails",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "QueuedEmailRecipientId",
                table: "QueuedEmailRecipients",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "NotificationId",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "InstagramImageId",
                table: "InstagramImages",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "InstagramFetchLogId",
                table: "InstagramFetchLog",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "SentEmails",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "SentEmailEvents",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "QueuedEmails",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "QueuedEmailRecipients",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "InstagramImages",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "InstagramFetchLog",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SentEmails",
                table: "SentEmails",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SentEmailEvents",
                table: "SentEmailEvents",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QueuedEmails",
                table: "QueuedEmails",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QueuedEmailRecipients",
                table: "QueuedEmailRecipients",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notifications",
                table: "Notifications",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InstagramImages",
                table: "InstagramImages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InstagramFetchLog",
                table: "InstagramFetchLog",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QueuedEmailRecipients_QueuedEmails_QueuedEmailId",
                table: "QueuedEmailRecipients",
                column: "QueuedEmailId",
                principalTable: "QueuedEmails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SentEmailEvents_SentEmails_SentEmailId",
                table: "SentEmailEvents",
                column: "SentEmailId",
                principalTable: "SentEmails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QueuedEmailRecipients_QueuedEmails_QueuedEmailId",
                table: "QueuedEmailRecipients");

            migrationBuilder.DropForeignKey(
                name: "FK_SentEmailEvents_SentEmails_SentEmailId",
                table: "SentEmailEvents");

            migrationBuilder.DropPrimaryKeyIfExists("SentEmails");

            migrationBuilder.DropPrimaryKeyIfExists("SentEmailEvents");

            migrationBuilder.DropPrimaryKeyIfExists("QueuedEmails");

            migrationBuilder.DropPrimaryKeyIfExists("QueuedEmailRecipients");

            migrationBuilder.DropPrimaryKeyIfExists("Notifications");

            migrationBuilder.DropPrimaryKeyIfExists("InstagramImages");

            migrationBuilder.DropPrimaryKeyIfExists("InstagramFetchLog");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "SentEmails",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE SentEmails SET SentEmailId = Id WHERE SentEmailId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "SentEmailId",
                table: "SentEmails",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "SentEmailEvents",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE SentEmailEvents SET SentEmailEventId = Id WHERE SentEmailEventId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "SentEmailEventId",
                table: "SentEmailEvents",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "QueuedEmails",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE QueuedEmails SET QueuedEmailId = Id WHERE QueuedEmailId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "QueuedEmailId",
                table: "QueuedEmails",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "QueuedEmailRecipients",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE QueuedEmailRecipients SET QueuedEmailRecipientId = Id WHERE QueuedEmailRecipientId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "QueuedEmailRecipientId",
                table: "QueuedEmailRecipients",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE Notifications SET NotificationId = Id WHERE NotificationId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "NotificationId",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "InstagramImages",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE InstagramImages SET InstagramImageId = Id WHERE InstagramImageId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "InstagramImageId",
                table: "InstagramImages",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "InstagramFetchLog",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.Sql("EXEC('UPDATE InstagramFetchLog SET InstagramFetchLogId = Id WHERE InstagramFetchLogId IS NULL')");

            migrationBuilder.AlterColumn<Guid>(
                name: "InstagramFetchLogId",
                table: "InstagramFetchLog",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_SentEmails",
                table: "SentEmails",
                column: "SentEmailId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SentEmailEvents",
                table: "SentEmailEvents",
                column: "SentEmailEventId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QueuedEmails",
                table: "QueuedEmails",
                column: "QueuedEmailId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QueuedEmailRecipients",
                table: "QueuedEmailRecipients",
                column: "QueuedEmailRecipientId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notifications",
                table: "Notifications",
                column: "NotificationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InstagramImages",
                table: "InstagramImages",
                column: "InstagramImageId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InstagramFetchLog",
                table: "InstagramFetchLog",
                column: "InstagramFetchLogId");

            migrationBuilder.AddForeignKey(
                name: "FK_QueuedEmailRecipients_QueuedEmails_QueuedEmailId",
                table: "QueuedEmailRecipients",
                column: "QueuedEmailId",
                principalTable: "QueuedEmails",
                principalColumn: "QueuedEmailId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SentEmailEvents_SentEmails_SentEmailId",
                table: "SentEmailEvents",
                column: "SentEmailId",
                principalTable: "SentEmails",
                principalColumn: "SentEmailId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
