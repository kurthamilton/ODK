using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Members;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class MemberSubscriptionMap : IEntityTypeConfiguration<MemberSubscription>
{
    public void Configure(EntityTypeBuilder<MemberSubscription> builder)
    {
        builder.ToTable("MemberSubscriptions");

        builder.HasKey(x => x.MemberId);

        builder.Property(x => x.ExpiresUtc)
            .HasColumnName("ExpiryDate")
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(x => x.Type)
            .HasColumnName("SubscriptionTypeId")
            .HasConversion<int>();

        builder.HasOne<Member>()
            .WithOne()
            .HasForeignKey<MemberSubscription>(x => x.MemberId);
    }
}
