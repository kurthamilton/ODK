using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Members;

namespace ODK.Data.EntityFramework.Mapping;

public class MemberSubscriptionMap : IEntityTypeConfiguration<MemberSubscription>
{
    public void Configure(EntityTypeBuilder<MemberSubscription> builder)
    {
        builder.ToTable("Members");

        builder.HasKey(x => x.MemberId);

        builder.Property(x => x.Type)
            .HasColumnName("SubscriptionTypeId")
            .HasConversion<int>();

        builder.Property(x => x.ExpiryDate)
            .HasColumnName("SubscriptionExpiryDate");

        builder.HasOne<Member>()
            .WithOne()
            .HasForeignKey<MemberSubscription>(x => x.MemberId);
    }
}
