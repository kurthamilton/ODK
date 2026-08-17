using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class EmailsIdMakePrimaryKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* Second of three releases moving this table's key from EmailTypeId to Id. The scaffolder offers a
               plain RenameColumn, which is what makes this hand-written: a rename would take EmailTypeId away
               from the build still running while this applies. EmailTypeId stays, keeping its values, and is
               dropped a release later.

               Nothing needs to write both columns. The app never inserts into Emails or changes an existing
               row's key - EmailRepository exposes GetAll, GetByType and Update, and rows come from migrations -
               so both builds address the same row through different columns holding the same value. */

            // A migration between the two releases may have inserted with EmailTypeId alone.
            migrationBuilder.Sql("UPDATE [Emails] SET [Id] = [EmailTypeId] WHERE [Id] IS NULL;");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Emails",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            /* ChapterEmails references the old key column, so the constraint goes before the key moves. Dropped
               by column rather than by name: the name is known here, but a lookup cannot be wrong about it. */
            migrationBuilder.DropForeignKeys("ChapterEmails", "EmailTypeId");
            migrationBuilder.DropPrimaryKeyIfExists("Emails");

            /* Made nullable so a migration writing the new column set can insert without it. EmailTypeId is not
               in the model, so this is stated rather than scaffolded. */
            migrationBuilder.Sql("ALTER TABLE [Emails] ALTER COLUMN [EmailTypeId] int NULL;");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Emails",
                table: "Emails",
                column: "Id");

            // Same name and same delete behaviour as ChapterEmails-AddEmailsFK gave it; only the target moves.
            migrationBuilder.AddForeignKey(
                name: "FK_ChapterEmails_Emails_EmailTypeId",
                table: "ChapterEmails",
                column: "EmailTypeId",
                principalTable: "Emails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKeys("ChapterEmails", "EmailTypeId");
            migrationBuilder.DropPrimaryKeyIfExists("Emails");

            // Restore the values before the column carries the key again.
            migrationBuilder.Sql("UPDATE [Emails] SET [EmailTypeId] = [Id] WHERE [EmailTypeId] IS NULL;");
            migrationBuilder.Sql("ALTER TABLE [Emails] ALTER COLUMN [EmailTypeId] int NOT NULL;");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Emails",
                table: "Emails",
                column: "EmailTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterEmails_Emails_EmailTypeId",
                table: "ChapterEmails",
                column: "EmailTypeId",
                principalTable: "Emails",
                principalColumn: "EmailTypeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Emails",
                nullable: true,
                oldClrType: typeof(int),
                oldNullable: false);
        }
    }
}
