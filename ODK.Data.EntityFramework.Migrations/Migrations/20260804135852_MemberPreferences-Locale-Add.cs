using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class MemberPreferencesLocaleAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Locale",
                table: "MemberPreferences",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            // Default all members to en-GB
            // Current app default is en-GB, and all active users are based in UK/IE, so
            // en-GB is a sensible default.
            const string sql =
                "INSERT INTO MemberPreferences (MemberId) " +
                "SELECT MemberId " +
                "FROM Members " +
                "WHERE NOT EXISTS (SELECT * FROM MemberPreferences WHERE MemberId = Members.MemberId); " +
                "UPDATE MemberPreferences SET Locale = 'en-GB'; ";
            migrationBuilder.Sql(sql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Locale",
                table: "MemberPreferences");
        }
    }
}
