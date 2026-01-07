using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Members;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class MemberSiteSubscriptionMap : IEntityTypeConfiguration<MemberSiteSubscription>
{
    public void Configure(EntityTypeBuilder<MemberSiteSubscription> builder)
    {
        builder.ToTable("MemberSiteSubscriptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ExpiresUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.Id)
            .HasColumnName("MemberSiteSubscriptionId");

        builder.HasOne<Member>()
            .WithOne()
            .HasForeignKey<MemberSiteSubscription>(x => x.MemberId);

        builder.HasOne(x => x.SiteSubscription)
            .WithOne()
            .HasForeignKey<MemberSiteSubscription>(x => x.SiteSubscriptionId);

        builder.HasOne(x => x.SiteSubscriptionPrice)
            .WithOne()
            .HasForeignKey<MemberSiteSubscription>(x => x.SiteSubscriptionPriceId);
    }
}
