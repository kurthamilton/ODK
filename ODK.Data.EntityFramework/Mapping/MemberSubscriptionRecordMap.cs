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

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.Property(x => x.CancelledUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.ExpiresUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.InitiatorId)
            .HasMaxLength(255);

        // The current record per member+chapter, denormalised via IsCurrent so the members list can read
        // current state with a single filtered seek. Non-unique (soft flag): "current" is definitionally
        // the latest record, so this is a performance cache, not an integrity constraint.
        builder.HasIndex(x => new { x.ChapterId, x.MemberId })
            .HasFilter("[IsCurrent] = 1");

        // Keep a plain ChapterId index for the FK: the filtered composite above leads with ChapterId, so
        // without this EF would treat it as covering the FK and drop the FK's own (unfiltered) index.
        builder.HasIndex(x => x.ChapterId);

        // Enforce uniqueness only where a value is present: historic records have a null InitiatorId,
        // but any populated value (the initiating webhook event id) must be unique so a retried event
        // cannot extend a subscription twice. A plain unique index would allow only one null on SQL Server.
        builder.HasIndex(x => x.InitiatorId)
            .IsUnique()
            .HasFilter("[InitiatorId] IS NOT NULL");

        builder.Property(x => x.PurchasedUtc)
            .HasConversion<UtcDateTimeConverter>();
        builder.Property(x => x.Type)
            .HasColumnName("SubscriptionTypeId")
            .HasConversion<int>();

        // Stated rather than left to convention, which would cascade: the database restricts, and a
        // subscription record is a financial log that outlives the group it was for.
        builder.HasOne<Chapter>()
            .WithMany()
            .HasForeignKey(x => x.ChapterId)
            .OnDelete(DeleteBehavior.Restrict);

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
