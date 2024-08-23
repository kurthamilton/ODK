using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class MemberSubscriptionMap : IEntityTypeConfiguration<MemberSubscription>
{
    public void Configure(EntityTypeBuilder<MemberSubscription> builder)
    {
        builder.ToTable("MemberSubscriptions");

        builder.HasKey(x => new { x.MemberId, x.ChapterId });

        builder.Property(x => x.ExpiresUtc)
            .HasColumnName("ExpiryDate")
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.Type)
            .HasColumnName("SubscriptionTypeId")
            .HasConversion<int>();

        builder.HasOne<Chapter>()
            .WithMany()
            .HasForeignKey(x => x.ChapterId);

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(x => x.MemberId);
    }
}
