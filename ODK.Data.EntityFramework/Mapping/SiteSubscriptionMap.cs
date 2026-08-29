using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Payments;
using ODK.Core.Subscriptions;

namespace ODK.Data.EntityFramework.Mapping;

public class SiteSubscriptionMap : IEntityTypeConfiguration<SiteSubscription>
{
    public void Configure(EntityTypeBuilder<SiteSubscription> builder)
    {
        builder.ToTable("SiteSubscriptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Environment)
            .HasColumnName("EnvironmentTypeId");

        builder.Property(x => x.PaymentProvider)
            .HasColumnName("PaymentProviderTypeId");

        builder.Property(x => x.Platform)
            .HasColumnName("PlatformTypeId")
            .HasConversion<int>();

        /* Restricted rather than the cascade a required relationship gets by default: one product is
           shared by every subscription on the platform, so cascading its delete would take all of them. */
        builder.HasOne<SitePaymentProduct>()
            .WithMany()
            .HasForeignKey(x => x.SitePaymentProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<SiteSubscription>()
            .WithMany()
            .HasForeignKey(x => x.FallbackSiteSubscriptionId);
    }
}