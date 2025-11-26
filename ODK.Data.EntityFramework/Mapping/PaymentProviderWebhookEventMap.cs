using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Payments;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class PaymentProviderWebhookEventMap : IEntityTypeConfiguration<PaymentProviderWebhookEvent>
{
    public void Configure(EntityTypeBuilder<PaymentProviderWebhookEvent> builder)
    {
        builder.ToTable("PaymentProviderWebhookEvents");

        builder.HasKey(x => new { x.PaymentProviderType, x.ExternalId });

        builder.Property(x => x.ExternalId)
            .HasMaxLength(255);

        builder.Property(x => x.PaymentProviderType)
            .HasColumnName("PaymentProviderId")
            .HasConversion<int>();

        builder.Property(x => x.ReceivedUtc)
            .HasConversion<UtcDateTimeConverter>();
    }
}
