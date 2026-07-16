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

        builder.Property(x => x.InitiatorId)
            .HasMaxLength(255);

        // Enforce uniqueness only where a value is present: historic records have a null InitiatorId,
        // but any populated value (the initiating webhook event id) must be unique so a retried event
        // cannot extend a subscription twice. A plain unique index would allow only one null on SQL Server.
        builder.HasIndex(x => x.InitiatorId)
            .IsUnique()
            .HasFilter("[InitiatorId] IS NOT NULL");

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

        builder.HasIndex(x => x.InitiatorId);
    }
}
