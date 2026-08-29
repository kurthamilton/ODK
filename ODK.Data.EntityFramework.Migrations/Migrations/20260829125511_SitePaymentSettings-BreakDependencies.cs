using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class SitePaymentSettingsBreakDependencies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropConstraintIfExists(
                name: "FK_ChapterPaymentAccounts_SitePaymentSettings_SitePaymentSettingId",
                table: "ChapterPaymentAccounts");

            migrationBuilder.DropConstraintIfExists(
                name: "UQ_ChapterSubscriptions_ChapterId_Name_SitePaymentSettingId",
                table: "ChapterSubscriptions");

            migrationBuilder.DropConstraintIfExists(
                name: "FK_ChapterSubscriptions_SitePaymentSettings_SitePaymentSettingId",
                table: "ChapterSubscriptions");

            migrationBuilder.DropConstraintIfExists(
                name: "FK_ChapterSubscriptions_SitePaymentSettings",
                table: "ChapterSubscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_SitePaymentProducts_SitePaymentSettings_SitePaymentSettingId",
                table: "SitePaymentProducts");

            migrationBuilder.DropConstraintIfExists(
                name: "FK_SiteSubscriptions_SitePaymentSettings_SitePaymentSettingId",
                table: "SiteSubscriptions");

            migrationBuilder.DropConstraintIfExists(
                name: "FK_ChapterSubscriptions_SitePaymentSettings",
                table: "SiteSubscriptions");

            migrationBuilder.DropConstraintIfExists(
                name: "UQ_ChapterPaymentAccounts_ChapterId",
                table: "ChapterPaymentAccounts");

            migrationBuilder.DropIndexIfExists(
                name: "IX_SiteSubscriptions_SitePaymentSettingId",
                table: "SiteSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_SitePaymentProducts_PlatformTypeId_SitePaymentSettingId",
                table: "SitePaymentProducts");

            migrationBuilder.DropIndex(
                name: "IX_SitePaymentProducts_SitePaymentSettingId",
                table: "SitePaymentProducts");

            migrationBuilder.DropIndexIfExists(
                name: "IX_ChapterSubscriptions_SitePaymentSettingId",
                table: "ChapterSubscriptions");

            migrationBuilder.DropIndexIfExists(
                name: "IX_ChapterPaymentAccounts_ChapterId",
                table: "ChapterPaymentAccounts");

            migrationBuilder.DropIndexIfExists(
                name: "IX_ChapterPaymentAccounts_SitePaymentSettingId",
                table: "ChapterPaymentAccounts");

            migrationBuilder.AlterColumn<Guid>(
                name: "SitePaymentSettingId",
                table: "SiteSubscriptions",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<int>(
                name: "EnvironmentTypeId",
                table: "SiteSubscriptions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentProviderTypeId",
                table: "SiteSubscriptions",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "SitePaymentSettingId",
                table: "SitePaymentProducts",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<int>(
                name: "EnvironmentTypeId",
                table: "SitePaymentProducts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentProviderTypeId",
                table: "SitePaymentProducts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EnvironmentTypeId",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentProviderTypeId",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlatformTypeId",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EnvironmentTypeId",
                table: "ChapterSubscriptions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentProviderTypeId",
                table: "ChapterSubscriptions",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "SitePaymentSettingId",
                table: "ChapterPaymentAccounts",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<int>(
                name: "EnvironmentTypeId",
                table: "ChapterPaymentAccounts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentProviderTypeId",
                table: "ChapterPaymentAccounts",
                type: "int",
                nullable: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SitePaymentProducts_PlatformTypeId_EnvironmentTypeId_PaymentProviderTypeId",
                table: "SitePaymentProducts");

            migrationBuilder.DropIndex(
                name: "IX_ChapterPaymentAccounts_ChapterId_EnvironmentTypeId_PaymentProviderTypeId",
                table: "ChapterPaymentAccounts");

            migrationBuilder.DropColumn(
                name: "EnvironmentTypeId",
                table: "SiteSubscriptions");

            migrationBuilder.DropColumn(
                name: "PaymentProviderTypeId",
                table: "SiteSubscriptions");

            migrationBuilder.DropColumn(
                name: "EnvironmentTypeId",
                table: "SitePaymentProducts");

            migrationBuilder.DropColumn(
                name: "PaymentProviderTypeId",
                table: "SitePaymentProducts");

            migrationBuilder.DropColumn(
                name: "EnvironmentTypeId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaymentProviderTypeId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PlatformTypeId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "EnvironmentTypeId",
                table: "ChapterSubscriptions");

            migrationBuilder.DropColumn(
                name: "PaymentProviderTypeId",
                table: "ChapterSubscriptions");

            migrationBuilder.DropColumn(
                name: "EnvironmentTypeId",
                table: "ChapterPaymentAccounts");

            migrationBuilder.DropColumn(
                name: "PaymentProviderTypeId",
                table: "ChapterPaymentAccounts");

            migrationBuilder.AlterColumn<Guid>(
                name: "SitePaymentSettingId",
                table: "SiteSubscriptions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "SitePaymentSettingId",
                table: "SitePaymentProducts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "SitePaymentSettingId",
                table: "ChapterPaymentAccounts",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SiteSubscriptions_SitePaymentSettingId",
                table: "SiteSubscriptions",
                column: "SitePaymentSettingId");

            migrationBuilder.CreateIndex(
                name: "IX_SitePaymentProducts_PlatformTypeId_SitePaymentSettingId",
                table: "SitePaymentProducts",
                columns: new[] { "PlatformTypeId", "SitePaymentSettingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SitePaymentProducts_SitePaymentSettingId",
                table: "SitePaymentProducts",
                column: "SitePaymentSettingId");

            migrationBuilder.CreateIndex(
                name: "IX_ChapterSubscriptions_SitePaymentSettingId",
                table: "ChapterSubscriptions",
                column: "SitePaymentSettingId");

            migrationBuilder.CreateIndex(
                name: "IX_ChapterPaymentAccounts_ChapterId",
                table: "ChapterPaymentAccounts",
                column: "ChapterId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChapterPaymentAccounts_SitePaymentSettingId",
                table: "ChapterPaymentAccounts",
                column: "SitePaymentSettingId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterPaymentAccounts_SitePaymentSettings_SitePaymentSettingId",
                table: "ChapterPaymentAccounts",
                column: "SitePaymentSettingId",
                principalTable: "SitePaymentSettings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterSubscriptions_SitePaymentSettings_SitePaymentSettingId",
                table: "ChapterSubscriptions",
                column: "SitePaymentSettingId",
                principalTable: "SitePaymentSettings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SitePaymentProducts_SitePaymentSettings_SitePaymentSettingId",
                table: "SitePaymentProducts",
                column: "SitePaymentSettingId",
                principalTable: "SitePaymentSettings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SiteSubscriptions_SitePaymentSettings_SitePaymentSettingId",
                table: "SiteSubscriptions",
                column: "SitePaymentSettingId",
                principalTable: "SitePaymentSettings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
