using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class MemberSubscriptionRecordMap : IEntityTypeConfiguration<MemberSubscriptionRecord>
{
    public void Configure(EntityTypeBuilder<MemberSubscriptionRecord> builder)
    {
        builder.ToTable("MemberSubscriptionLog");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PurchasedUtc)
            .HasColumnName("PurchaseDate")
            .HasConversion<UtcDateTimeConverter>();

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
