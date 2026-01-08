using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class MemberSubscriptionRecordMap : IEntityTypeConfiguration<MemberSubscriptionRecord>
{
    public void Configure(EntityTypeBuilder<MemberSubscriptionRecord> builder)
    {
        builder.ToTable("MemberSubscriptionLog");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CancelledUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.PurchasedUtc)
            .HasColumnName("PurchaseDate")
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(x => x.Type)
            .HasColumnName("SubscriptionTypeId")
            .HasConversion<int>();

        builder.HasOne<Chapter>()
            .WithMany()
            .HasForeignKey(x => x.ChapterId);

        builder.HasOne<ChapterSubscription>()
            .WithMany()
            .HasForeignKey(x => x.ChapterSubscriptionId);

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(x => x.MemberId);

        builder.HasOne<Payment>()
            .WithOne()
            .HasForeignKey<MemberSubscriptionRecord>(x => x.PaymentId);
    }
}
