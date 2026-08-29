using Microsoft.EntityFrameworkCore.Migrations;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Data.EntityFramework.Migrations.Enums;

#nullable disable

namespace ODK.Data.EntityFramework.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class SitePaymentSettingsBreakDependenciesAddEnumForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .AddEnumForeignKey<EnvironmentType>("ChapterPaymentAccounts", "EnvironmentTypeId")
                .AddEnumForeignKey<PaymentProviderType>("ChapterPaymentAccounts", "PaymentProviderTypeId")
                .AddEnumForeignKey<EnvironmentType>("ChapterSubscriptions", "EnvironmentTypeId")
                .AddEnumForeignKey<PaymentProviderType>("ChapterSubscriptions", "PaymentProviderTypeId")
                .AddEnumForeignKey<EnvironmentType>("Payments", "EnvironmentTypeId")
                .AddEnumForeignKey<PaymentProviderType>("Payments", "PaymentProviderTypeId")
                .AddEnumForeignKey<PlatformType>("Payments", "PlatformTypeId")
                .AddEnumForeignKey<EnvironmentType>("SitePaymentProducts", "EnvironmentTypeId")
                .AddEnumForeignKey<PaymentProviderType>("SitePaymentProducts", "PaymentProviderTypeId")
                .AddEnumForeignKey<EnvironmentType>("SiteSubscriptions", "EnvironmentTypeId")
                .AddEnumForeignKey<PaymentProviderType>("SiteSubscriptions", "PaymentProviderTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder
                .DropEnumForeignKey<EnvironmentType>("ChapterPaymentAccounts", "EnvironmentTypeId")
                .DropEnumForeignKey<PaymentProviderType>("ChapterPaymentAccounts", "PaymentProviderTypeId")
                .DropEnumForeignKey<EnvironmentType>("ChapterSubscriptions", "EnvironmentTypeId")
                .DropEnumForeignKey<PaymentProviderType>("ChapterSubscriptions", "PaymentProviderTypeId")
                .DropEnumForeignKey<EnvironmentType>("Payments", "EnvironmentTypeId")
                .DropEnumForeignKey<PaymentProviderType>("Payments", "PaymentProviderTypeId")
                .DropEnumForeignKey<PlatformType>("Payments", "PlatformTypeId")
                .DropEnumForeignKey<EnvironmentType>("SitePaymentProducts", "EnvironmentTypeId")
                .DropEnumForeignKey<PaymentProviderType>("SitePaymentProducts", "PaymentProviderTypeId")
                .DropEnumForeignKey<EnvironmentType>("SiteSubscriptions", "EnvironmentTypeId")
                .DropEnumForeignKey<PaymentProviderType>("SiteSubscriptions", "PaymentProviderTypeId");
        }
    }
}
