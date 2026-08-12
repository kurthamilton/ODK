using Microsoft.EntityFrameworkCore.Migrations;
using ODK.Core.Emails;
using ODK.Data.EntityFramework.Migrations.Enums;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class EmailsEmailRecipientTypeIdAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {            
            migrationBuilder.AddColumn<int>(
                name: "EmailRecipientTypeId",
                table: "Emails",
                type: "int",
                nullable: true);

            migrationBuilder
                .CreateEnumTable<EmailRecipientType>()
                .InsertAllEnumValues<EmailRecipientType>()
                .AddEnumForeignKey<EmailRecipientType>("Emails", "EmailRecipientTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .DropEnumForeignKey<EmailRecipientType>("Emails", "EmailRecipientTypeId")
                .DropEnumTable<EmailRecipientType>();

            migrationBuilder.DropColumn(
                name: "EmailRecipientTypeId",
                table: "Emails");
        }
    }
}
