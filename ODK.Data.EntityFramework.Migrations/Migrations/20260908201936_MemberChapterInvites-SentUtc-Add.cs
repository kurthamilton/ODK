using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class MemberChapterInvitesSentUtcAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SentUtc",
                table: "MemberChapterInvites",
                type: "datetime2",
                nullable: true);

            /* Null means an invite the group is holding, so every row that predates the column has to be
               stamped: an invite raised before it existed was emailed as it was raised, and publishing the
               group would otherwise email it a second time. */
            migrationBuilder.Sql(
                """
                UPDATE [MemberChapterInvites]
                SET [SentUtc] = [CreatedUtc]
                WHERE [SentUtc] IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SentUtc",
                table: "MemberChapterInvites");
        }
    }
}
