using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class MemberAvatarsMemberIdMakePrimaryKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* The model keys this table on MemberId; the database keys it on Version. The snapshot already
               matches the model, so there is nothing for the scaffolder to emit and the whole body is
               written by hand. */

            /* The table already has this relationship, under the name EF would choose. Drop it rather than
               leave it: its delete behaviour is not what put it there, and re-adding from the model is what
               makes the two agree. */
            migrationBuilder.DropForeignKeys("MemberAvatars", "MemberId");

            // Superseded by the primary key below, which keys the same single column.
            migrationBuilder.DropIndexes("MemberAvatars", "MemberId");

            /* Version is a rowversion, so it takes a new value on every write: as a key it identifies a row
               by something that changes whenever the row changes, and while it is clustered each update
               relocates the row. Look the constraint up rather than naming it - it was created by hand. */
            migrationBuilder.DropPrimaryKeyIfExists("MemberAvatars");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MemberAvatars",
                table: "MemberAvatars",
                column: "MemberId");

            migrationBuilder.AddForeignKey(
                name: "FK_MemberAvatars_Members_MemberId",
                table: "MemberAvatars",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MemberAvatars_Members_MemberId",
                table: "MemberAvatars");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MemberAvatars",
                table: "MemberAvatars");

            // The key the table has without this migration, rowversion and all, so Up reverses exactly.
            migrationBuilder.AddPrimaryKey(
                name: "PK_MemberAvatars",
                table: "MemberAvatars",
                column: "Version");

            // The relationship is not what this migration changes, so it goes back as the model declares it.
            migrationBuilder.AddForeignKey(
                name: "FK_MemberAvatars_Members_MemberId",
                table: "MemberAvatars",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
