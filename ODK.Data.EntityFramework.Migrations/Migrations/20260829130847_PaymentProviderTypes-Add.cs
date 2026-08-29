using Microsoft.EntityFrameworkCore.Migrations;
using ODK.Core.Payments;
using ODK.Data.EntityFramework.Migrations.Enums;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class PaymentProviderTypesAdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .CreateEnumTable<PaymentProviderType>()
                .InsertAllEnumValues<PaymentProviderType>();
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropEnumTable<PaymentProviderType>();
        }
    }
}
