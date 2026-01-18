using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Subscriptions;

namespace ODK.Data.EntityFramework.Mapping;

public class SiteSubscriptionFeatureMap : IEntityTypeConfiguration<SiteSubscriptionFeature>
{
    public void Configure(EntityTypeBuilder<SiteSubscriptionFeature> builder)
    {
        builder.ToTable("SiteSubscriptionFeatures");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("SiteSubscriptionFeatureId");

        builder.Property(x => x.Feature)
            .HasColumnName("SiteFeatureId")
            .HasConversion<int>();

        builder.HasOne<SiteSubscription>()
            .WithMany()
            .HasForeignKey(x => x.SiteSubscriptionId);
    }
}