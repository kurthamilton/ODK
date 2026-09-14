using Microsoft.EntityFrameworkCore.Migrations;
using ODK.Core.Members;
using ODK.Data.EntityFramework.Migrations.Enums;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class MemberActivationTokensSignUpIntentTypeIdAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .CreateEnumTable<SignUpIntentType>()
                .InsertAllEnumValues<SignUpIntentType>();

            /* Nullable, and stays nullable. A sign-up that states no intent leaves it null rather than
               storing None, which InsertAllEnumValues deliberately leaves out of the lookup table and the
               foreign key below would refuse. */
            migrationBuilder.AddColumn<int>(
                name: "SignUpIntentTypeId",
                table: "MemberActivationTokens",
                type: "int",
                nullable: true);

            migrationBuilder.AddEnumForeignKey<SignUpIntentType>(
                "MemberActivationTokens", "SignUpIntentTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropEnumForeignKey<SignUpIntentType>(
                "MemberActivationTokens", "SignUpIntentTypeId");

            migrationBuilder.DropColumn(
                name: "SignUpIntentTypeId",
                table: "MemberActivationTokens");

            // After the column: DropEnumTable fails while anything still references it.
            migrationBuilder.DropEnumTable<SignUpIntentType>();
        }
    }
}
