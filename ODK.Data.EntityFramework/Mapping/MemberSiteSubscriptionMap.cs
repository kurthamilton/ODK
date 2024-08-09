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

        builder.HasKey(x => new { x.MemberId, x.SiteSubscriptionId });

        builder.Property(x => x.ExpiresUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.HasOne<Member>()
            .WithOne()
            .HasForeignKey<MemberSiteSubscription>(x => x.MemberId);

        builder.HasOne(x => x.SiteSubscription)
            .WithMany()
            .HasForeignKey(x => x.SiteSubscriptionId);
    }
}
