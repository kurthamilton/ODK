using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Payments;
using ODK.Data.EntityFramework.Converters;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Mapping;

public class PaymentTransferMap : IEntityTypeConfiguration<PaymentTransfer>
{
    public void Configure(EntityTypeBuilder<PaymentTransfer> builder)
    {
        builder.ToTable("PaymentTransfers");

        /* Id is the key but not what this table is read by. A transfer is always reached from the payment
           it belongs to - the transfer job, the refund flow and the group's payments page all start from
           a payment - so the rows are ordered by payment instead. */
        builder.HasKey(x => x.Id)
            .IsClustered(false);

        // Unique: a payment's share is worked out once and moved once.
        builder.HasIndex(x => x.PaymentId)
            .IsClustered()
            .IsUnique();

        builder.Property(x => x.Amount)
            .IsMoneyType();

        builder.Property(x => x.CommissionAmount)
            .IsMoneyType();

        builder.Property(x => x.CompletedUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.CreatedUtc)
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(x => x.ExternalId)
            .HasMaxLength(100);

        builder.Property(x => x.WithheldAmount)
            .IsMoneyType();

        builder.HasOne<Payment>()
            .WithMany()
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
