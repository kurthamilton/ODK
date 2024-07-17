using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Members;

namespace ODK.Data.EntityFramework.Mapping;

public class MemberSubscriptionRecordMap : IEntityTypeConfiguration<MemberSubscriptionRecord>
{
    public void Configure(EntityTypeBuilder<MemberSubscriptionRecord> builder)
    {
        builder.ToTable("MemberSubscriptionLog");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
            .HasColumnName("SubscriptionTypeId")
            .HasConversion<int>();

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(x => x.MemberId);
    }
}
