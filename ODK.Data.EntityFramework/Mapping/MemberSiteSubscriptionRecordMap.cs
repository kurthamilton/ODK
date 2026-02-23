using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Core.Subscriptions;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class MemberSiteSubscriptionRecordMap : IEntityTypeConfiguration<MemberSiteSubscriptionRecord>
{
    public void Configure(EntityTypeBuilder<MemberSiteSubscriptionRecord> builder)
    {
        builder.ToTable("MemberSiteSubscriptionLog");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedUtc)
            .HasConversion<UtcDateTimeConverter>();

        builder.HasOne<Payment>()
            .WithMany()
            .HasForeignKey(x => x.PaymentId);

        builder.HasOne<SiteSubscription>()
            .WithMany()
            .HasForeignKey(x => x.SiteSubscriptionId);

        builder.HasOne<SiteSubscriptionPrice>()
            .WithMany()
            .HasForeignKey(x => x.SiteSubscriptionPriceId);
    }
}
