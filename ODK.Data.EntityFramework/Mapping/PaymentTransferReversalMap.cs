 using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Payments;
using ODK.Data.EntityFramework.Converters;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Mapping;

public class PaymentTransferReversalMap : IEntityTypeConfiguration<PaymentTransferReversal>
{
    public void Configure(EntityTypeBuilder<PaymentTransferReversal> builder)
    {
        builder.ToTable("PaymentTransferReversals");

        /* Id is the key but not what this table is read by. A reversal is reached from the refund that
           raised it - rendering a page of refunds asks for theirs in one go - so the rows are ordered that
           way. The read from the other direction, summing what a transfer has already given back, is
           served by the index the foreign key below carries. */
        builder.HasKey(x => x.Id)
            .IsClustered(false);

        builder.HasIndex(x => x.PaymentRefundId)
            .IsClustered();

        builder.Property(x => x.ActualAmount)
            .IsMoneyType();

        builder.Property(x => x.Amount)
            .IsMoneyType();

        builder.Property(x => x.CompletedUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.CreatedUtc)
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(x => x.ExternalId)
            .HasMaxLength(100);

        /* Cascade, so that deleting a chapter or a member still works: both reach Payments that way, and
           a reversal left behind would block the transfer's own deletion.

           This is also why PaymentRefundId carries no foreign key. Payments cascades to PaymentRefunds as
           well as to PaymentTransfers, so a key on that column would be a second cascade path from
           Payments to this table, and SQL Server rejects that outright. */
        builder.HasOne<PaymentTransfer>()
            .WithMany()
            .HasForeignKey(x => x.PaymentTransferId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
