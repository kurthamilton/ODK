using Microsoft.EntityFrameworkCore.Migrations;
using ODK.Core.Payments;
using ODK.Core.Platforms;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class SitePaymentSettingsBreakDependenciesMakeEnumForeignKeysRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE ChapterPaymentAccounts " +
                "SET " +
                "   EnvironmentTypeId = EnvironmentTypes.Id, " +
                "   PaymentProviderTypeId = PaymentProviderTypes.Id " +
                "FROM ChapterPaymentAccounts " +
                "JOIN SitePaymentSettings ON ChapterPaymentAccounts.SitePaymentSettingId = SitePaymentSettings.Id " +
                "JOIN EnvironmentTypes ON SitePaymentSettings.Environment = EnvironmentTypes.Name " +
                "JOIN PaymentProviderTypes ON SitePaymentSettings.Provider = PaymentProviderTypes.Name;");

            migrationBuilder.Sql(
                "UPDATE ChapterSubscriptions " +
                "SET " +
                "   EnvironmentTypeId = EnvironmentTypes.Id, " +
                "   PaymentProviderTypeId = PaymentProviderTypes.Id " +
                "FROM ChapterSubscriptions " +
                "JOIN SitePaymentSettings ON ChapterSubscriptions.SitePaymentSettingId = SitePaymentSettings.Id " +
                "JOIN EnvironmentTypes ON SitePaymentSettings.Environment = EnvironmentTypes.Name " +
                "JOIN PaymentProviderTypes ON SitePaymentSettings.Provider = PaymentProviderTypes.Name; ");

            migrationBuilder.Sql(
                "UPDATE Payments " +
                "SET " +
                "   EnvironmentTypeId = EnvironmentTypes.Id, " +
                "   PlatformTypeId = SitePaymentSettings.PlatformTypeId, " +
                "   PaymentProviderTypeId = PaymentProviderTypes.Id " +
                "FROM Payments " +
                "JOIN SitePaymentSettings ON Payments.SitePaymentSettingId = SitePaymentSettings.Id " +
                "JOIN EnvironmentTypes ON SitePaymentSettings.Environment = EnvironmentTypes.Name " +
                "JOIN PaymentProviderTypes ON SitePaymentSettings.Provider = PaymentProviderTypes.Name; ");

            migrationBuilder.Sql(
                "UPDATE Payments " +
                $"SET EnvironmentTypeId = {(int)EnvironmentType.Prod} " +
                "WHERE EnvironmentTypeId IS NULL;");

            migrationBuilder.Sql(
                "UPDATE Payments " +
                $"SET PlatformTypeId = {(int)PlatformType.DrunkenKnitwits} " +
                "WHERE PlatformTypeId IS NULL;");

            migrationBuilder.Sql(
                "UPDATE Payments " +
                $"SET PaymentProviderTypeId = {(int)PaymentProviderType.PayPal} " +
                "WHERE PaymentProviderTypeId IS NULL;");

            migrationBuilder.Sql(
                "UPDATE SitePaymentProducts " +
                "SET " +
                "   EnvironmentTypeId = EnvironmentTypes.Id, " +
                "   PaymentProviderTypeId = PaymentProviderTypes.Id " +
                "FROM SitePaymentProducts " +
                "JOIN SitePaymentSettings ON SitePaymentProducts.SitePaymentSettingId = SitePaymentSettings.Id " +
                "JOIN EnvironmentTypes ON SitePaymentSettings.Environment = EnvironmentTypes.Name " +
                "JOIN PaymentProviderTypes ON SitePaymentSettings.Provider = PaymentProviderTypes.Name;");
        
            migrationBuilder.Sql(
                "UPDATE SiteSubscriptions " +
                "SET " +
                "   EnvironmentTypeId = EnvironmentTypes.Id, " +
                "   PlatformTypeId = SitePaymentSettings.PlatformTypeId, " +
                "   PaymentProviderTypeId = PaymentProviderTypes.Id " +
                "FROM Payments " +
                "JOIN SitePaymentSettings ON Payments.SitePaymentSettingId = SitePaymentSettings.Id " +
                "JOIN EnvironmentTypes ON SitePaymentSettings.Environment = EnvironmentTypes.Name " +
                "JOIN PaymentProviderTypes ON SitePaymentSettings.Provider = PaymentProviderTypes.Name;");

            migrationBuilder.DropIndex(
                name: "IX_SitePaymentProducts_PlatformTypeId_EnvironmentTypeId_PaymentProviderTypeId",
                table: "SitePaymentProducts");

            migrationBuilder.DropIndex(
                name: "IX_ChapterPaymentAccounts_ChapterId_EnvironmentTypeId_PaymentProviderTypeId",
                table: "ChapterPaymentAccounts");

            migrationBuilder.AlterColumn<int>(
                name: "PaymentProviderTypeId",
                table: "SiteSubscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EnvironmentTypeId",
                table: "SiteSubscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PaymentProviderTypeId",
                table: "SitePaymentProducts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EnvironmentTypeId",
                table: "SitePaymentProducts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PlatformTypeId",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PaymentProviderTypeId",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EnvironmentTypeId",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PaymentProviderTypeId",
                table: "ChapterSubscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EnvironmentTypeId",
                table: "ChapterSubscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PaymentProviderTypeId",
                table: "ChapterPaymentAccounts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EnvironmentTypeId",
                table: "ChapterPaymentAccounts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SitePaymentProducts_PlatformTypeId_EnvironmentTypeId_PaymentProviderTypeId",
                table: "SitePaymentProducts",
                columns: new[] { "PlatformTypeId", "EnvironmentTypeId", "PaymentProviderTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChapterPaymentAccounts_ChapterId_EnvironmentTypeId_PaymentProviderTypeId",
                table: "ChapterPaymentAccounts",
                columns: new[] { "ChapterId", "EnvironmentTypeId", "PaymentProviderTypeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SitePaymentProducts_PlatformTypeId_EnvironmentTypeId_PaymentProviderTypeId",
                table: "SitePaymentProducts");

            migrationBuilder.DropIndex(
                name: "IX_ChapterPaymentAccounts_ChapterId_EnvironmentTypeId_PaymentProviderTypeId",
                table: "ChapterPaymentAccounts");

            migrationBuilder.AlterColumn<int>(
                name: "PaymentProviderTypeId",
                table: "SiteSubscriptions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "EnvironmentTypeId",
                table: "SiteSubscriptions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "PaymentProviderTypeId",
                table: "SitePaymentProducts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "EnvironmentTypeId",
                table: "SitePaymentProducts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "PlatformTypeId",
                table: "Payments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "PaymentProviderTypeId",
                table: "Payments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "EnvironmentTypeId",
                table: "Payments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "PaymentProviderTypeId",
                table: "ChapterSubscriptions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "EnvironmentTypeId",
                table: "ChapterSubscriptions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "PaymentProviderTypeId",
                table: "ChapterPaymentAccounts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "EnvironmentTypeId",
                table: "ChapterPaymentAccounts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_SitePaymentProducts_PlatformTypeId_EnvironmentTypeId_PaymentProviderTypeId",
                table: "SitePaymentProducts",
                columns: new[] { "PlatformTypeId", "EnvironmentTypeId", "PaymentProviderTypeId" },
                unique: true,
                filter: "[EnvironmentTypeId] IS NOT NULL AND [PaymentProviderTypeId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ChapterPaymentAccounts_ChapterId_EnvironmentTypeId_PaymentProviderTypeId",
                table: "ChapterPaymentAccounts",
                columns: new[] { "ChapterId", "EnvironmentTypeId", "PaymentProviderTypeId" },
                unique: true,
                filter: "[EnvironmentTypeId] IS NOT NULL AND [PaymentProviderTypeId] IS NOT NULL");
        }
    }
}
