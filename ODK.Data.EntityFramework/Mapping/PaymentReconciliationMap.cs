using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;
using ODK.Core.Payments;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class PaymentReconciliationMap : IEntityTypeConfiguration<PaymentReconciliation>
{
    public void Configure(EntityTypeBuilder<PaymentReconciliation> builder)
    {
        builder.ToTable("PaymentReconciliations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedUtc)
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(x => x.Id)
            .HasColumnName("PaymentReconciliationId");

        builder.Property(x => x.PaymentReference)
            .HasMaxLength(255)
            .IsRequired();

        builder.HasOne<Chapter>()
            .WithMany()
            .HasForeignKey(x => x.ChapterId);
    }
}
