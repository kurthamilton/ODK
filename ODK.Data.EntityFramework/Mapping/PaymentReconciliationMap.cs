using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Payments;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class PaymentReconciliationMap : IEntityTypeConfiguration<PaymentReconciliation>
{
    public void Configure(EntityTypeBuilder<PaymentReconciliation> builder)
    {
        builder.ToTable("PaymentReconciliations");

        /* Id is the key but not what this table is read by: every read reaches it from the payment it
           belongs to, either one at a time or as the left side of the reconciliation page's listing. */
        builder.HasKey(x => x.Id)
            .IsClustered(false);

        // Unique: a payment has one reconciliation state, not a history of them.
        builder.HasIndex(x => x.PaymentId)
            .IsClustered()
            .IsUnique();

        builder.Property(x => x.FailedUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.FailureReason)
            .HasMaxLength(500);

        builder.Property(x => x.IgnoredUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.HasOne<Payment>()
            .WithMany()
            .HasForeignKey(x => x.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
