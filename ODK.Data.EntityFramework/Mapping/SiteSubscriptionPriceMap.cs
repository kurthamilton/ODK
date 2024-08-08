using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Subscriptions;

namespace ODK.Data.EntityFramework.Mapping;

public class SiteSubscriptionPriceMap : IEntityTypeConfiguration<SiteSubscriptionPrice>
{
    public void Configure(EntityTypeBuilder<SiteSubscriptionPrice> builder)
    {
        builder.ToTable("SiteSubscriptionPrices");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("SiteSubscriptionPriceId");

        builder.HasOne(x => x.Currency)
            .WithMany()
            .HasForeignKey(x => x.CurrencyId);

        builder.HasOne<SiteSubscription>()
            .WithMany()
            .HasForeignKey(x => x.SiteSubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
