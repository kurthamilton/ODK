using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;
using ODK.Data.EntityFramework.Converters;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterPaymentAdjustmentRecoveryMap : IEntityTypeConfiguration<ChapterPaymentAdjustmentRecovery>
{
    public void Configure(EntityTypeBuilder<ChapterPaymentAdjustmentRecovery> builder)
    {
        builder.ToTable("ChapterPaymentAdjustmentRecoveries");

        // Read by the adjustment they settle, never by their own key.
        builder.HasKey(x => x.Id)
            .IsClustered(false);

        builder.HasIndex(x => x.ChapterPaymentAdjustmentId)
            .IsClustered();

        builder.Property(x => x.Amount)
            .IsMoneyType();

        builder.Property(x => x.CreatedUtc)
            .HasConversion<UtcDateTimeConverter>();

        // Answers "what did this payment's transfer absorb", which is how a smaller-than-expected transfer
        // is explained from the payment end.
        builder.HasIndex(x => x.PaymentId);

        /* PaymentId carries no foreign key: Chapter cascades to both Payments and ChapterPaymentAdjustments,
           so a key on it would be a second cascade path from Chapter into this table, which SQL Server
           rejects. */
        builder.HasOne<ChapterPaymentAdjustment>()
            .WithMany()
            .HasForeignKey(x => x.ChapterPaymentAdjustmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
