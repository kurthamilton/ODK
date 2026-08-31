using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Payments;
using ODK.Data.EntityFramework.Converters;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Mapping;

public class PaymentRefundMap : IEntityTypeConfiguration<PaymentRefund>
{
    public void Configure(EntityTypeBuilder<PaymentRefund> builder)
    {
        builder.ToTable("PaymentRefunds");

        /* Id is the key but not what this table is read by. A refund is always reached from the payment it
           belongs to - rendering a payment's refunds, and checking a new one against what the payment has
           already given back - so the rows are ordered by payment instead. */
        builder.HasKey(x => x.Id)
            .IsClustered(false);

        builder.HasIndex(x => x.PaymentId)
            .IsClustered();

        builder.Property(x => x.ActualAmount)
            .IsMoneyType();

        builder.Property(x => x.Amount)
            .IsMoneyType();

        builder.Property(x => x.ChapterAmount)
            .IsMoneyType();

        builder.Property(x => x.DeclinedReason)
            .HasMaxLength(500);

        builder.Property(x => x.ExternalId)
            .HasMaxLength(100);

        builder.Property(x => x.ExternalReversalId)
            .HasMaxLength(100);

        builder.Property(x => x.FailureReason)
            .HasMaxLength(500);

        builder.Property(x => x.FeeReturnedAmount)
            .IsMoneyType();

        builder.Property(x => x.Reason)
            .HasMaxLength(500);

        builder.Property(x => x.RefundedUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.RequestedUtc)
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(x => x.ResolvedUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.ReversedAmount)
            .IsMoneyType();

        builder.Property(x => x.ReversedUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.SettlementCurrencyCode)
            .HasMaxLength(3);

        builder.Property(x => x.Status)
            .HasColumnName("PaymentRefundStatusTypeId")
            .HasConversion<int>();

        /* Cascade, so that deleting a chapter or a member still works: both reach Payments that way and a
           refund left behind would block it.

           This is also why RequestedByMemberId and ResolvedByMemberId carry no foreign key. Member already
           cascades to Payments and Payments cascades to here, so a key on either of those columns would be
           a second cascade path from Member to this table, and SQL Server rejects that outright. */
        builder.HasOne<Payment>()
            .WithMany()
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
