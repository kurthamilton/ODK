using Microsoft.EntityFrameworkCore.Migrations;
using ODK.Core.Emails;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class EmailsEmailRecipientTypeIdMakeRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql =
                "UPDATE Emails " +
                $"SET EmailRecipientTypeId = {(int)EmailRecipientType.Admins} " +
                "WHERE EmailTypeId IN " +
                "(" +
                $"  {(int)EmailType.ContactRequest}, " +
                $"  {(int)EmailType.EventComment}, " +
                $"  {(int)EmailType.NewMemberAdmin} " +
                ");" +
                "" +
                // NB this includes Layout - but it's better to have recipient as required with one exception
                // rather than treat all other templates as possibly optional
                $"UPDATE Emails " +
                $"SET EmailRecipientTypeId = {(int)EmailRecipientType.Members} " +
                $"WHERE EmailRecipientTypeId IS NULL;";
            migrationBuilder.Sql(sql);

            /* Deliberately no defaultValue, which the scaffold would otherwise add: it would be 0, and 0 is
               the reserved None sentinel that EmailRecipientTypes has no row for - so a later insert
               omitting the column would default to a value the foreign key rejects. The backfill above has
               already cleared every NULL, so there is nothing for a default to protect against. */
            migrationBuilder.AlterColumn<int>(
                name: "EmailRecipientTypeId",
                table: "Emails",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "EmailRecipientTypeId",
                table: "Emails",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
