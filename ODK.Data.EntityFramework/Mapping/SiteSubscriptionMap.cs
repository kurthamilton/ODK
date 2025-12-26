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

        builder.Property(x => x.Id)
            .HasColumnName("SiteSubscriptionId");

        builder.Property(x => x.Platform)
            .HasColumnName("PlatformTypeId")
            .HasConversion<int>();

        builder.HasOne<SitePaymentSettings>()
            .WithMany()
            .HasForeignKey(x => x.SitePaymentSettingId);

        builder.HasOne<SiteSubscription>()
            .WithMany()
            .HasForeignKey(x => x.FallbackSiteSubscriptionId);
    }
}
