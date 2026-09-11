using Microsoft.EntityFrameworkCore.Migrations;
using ODK.Core.Platforms;
using ODK.Data.EntityFramework.Migrations.Enums;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class PlatformTypesGroupSquirrelRename : Migration
    {
        /* PlatformTypes is not in the EF model, so the row keeps the name it was inserted under until a
           migration says otherwise. The number is the contract - nothing referencing the row changes. */
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
            => migrationBuilder.RenameEnumValue(PlatformType.GroupSquirrel, "Default");

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
            => migrationBuilder.Sql(
                "UPDATE [PlatformTypes] SET [Name] = N'Default' " +
                "WHERE [Id] = 1 AND [Name] = N'GroupSquirrel';");
    }
}
