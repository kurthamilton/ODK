using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Payments;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class SitePaymentSettingsMap : IEntityTypeConfiguration<SitePaymentSettings>
{
    public void Configure(EntityTypeBuilder<SitePaymentSettings> builder)
    {
        builder.ToTable("SitePaymentSettings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Commission)
            .HasPrecision(19, 4);

        builder.Property(x => x.ExternalId)
            .HasMaxLength(255);

        builder.Property(x => x.ExternalUrl)
            .HasMaxLength(1024);

        builder.Property(x => x.Platform)
            .HasColumnName("PlatformTypeId")
            .HasConversion<int>();

        builder.Property(x => x.Provider)
            .HasConversion<EnumStringConverter<PaymentProviderType>>()
            .HasMaxLength(255);
    }
}
