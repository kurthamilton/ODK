using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class MemberRecaptchaAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RecaptchaFlagged",
                table: "Members",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "RecaptchaScore",
                table: "Members",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecaptchaFlagged",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "RecaptchaScore",
                table: "Members");
        }
    }
}
