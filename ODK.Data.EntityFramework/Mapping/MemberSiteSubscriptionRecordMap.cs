using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Core.Subscriptions;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class MemberSiteSubscriptionRecordMap : IEntityTypeConfiguration<MemberSiteSubscriptionRecord>
{
    public void Configure(EntityTypeBuilder<MemberSiteSubscriptionRecord> builder)
    {
        builder.ToTable("MemberSiteSubscriptionLog");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CancelledUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.CreatedUtc)
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(x => x.ExpiresUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.InitiatorId)
            .HasMaxLength(255);

        // The current record per member, denormalised via IsCurrent so current state can be read with a
        // single filtered seek. Non-unique (soft flag): "current" is definitionally the latest record.
        builder.HasIndex(x => x.MemberId, "IX_MemberSiteSubscriptionLog_MemberId_Current")
            .HasFilter("[IsCurrent] = 1");

        // Keep a plain MemberId index for the FK: the filtered index above leads with MemberId, so without
        // this EF would treat it as covering the FK and drop the FK's own (unfiltered) index.
        builder.HasIndex(x => x.MemberId);

        // Enforce uniqueness only where a value is present: historic records have a null InitiatorId,
        // but any populated value (the initiating webhook event id) must be unique so a retried event
        // cannot extend a subscription twice. A plain unique index would allow only one null on SQL Server.
        builder.HasIndex(x => x.InitiatorId)
            .IsUnique()
            .HasFilter("[InitiatorId] IS NOT NULL");

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(x => x.MemberId);

        builder.HasOne<Payment>()
            .WithMany()
            .HasForeignKey(x => x.PaymentId);

        builder.HasOne<SiteSubscription>()
            .WithMany()
            .HasForeignKey(x => x.SiteSubscriptionId);

        builder.HasOne<SiteSubscriptionPrice>()
            .WithMany()
            .HasForeignKey(x => x.SiteSubscriptionPriceId);
    }
}