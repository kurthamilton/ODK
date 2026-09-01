using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class ChapterPropertiesDisplayNameMakeRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The rows are filled in by hand, so this guard has to say so: a display name is the only
            // thing naming a required profile question in its "The X field is required." message, and no
            // value the migration could invent is the one the group meant. Never give the AlterColumn a
            // defaultValue - it emits an UPDATE that turns every NULL into an empty string, satisfying
            // NOT NULL while leaving the message reading "The  field is required.".
            migrationBuilder.Sql(
                """
                IF EXISTS (SELECT 1 FROM ChapterProperties WHERE DisplayName IS NULL OR DisplayName = '')
                    THROW 50000, 'ChapterProperties.DisplayName still has NULL or empty rows. Set a display name on each of them (the Label is usually the right value) before applying this migration.', 1;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "ChapterProperties",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "ChapterProperties",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);
        }
    }
}
