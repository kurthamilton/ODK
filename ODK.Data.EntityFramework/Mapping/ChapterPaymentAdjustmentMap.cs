using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Data.EntityFramework.Converters;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterPaymentAdjustmentMap : IEntityTypeConfiguration<ChapterPaymentAdjustment>
{
    public void Configure(EntityTypeBuilder<ChapterPaymentAdjustment> builder)
    {
        builder.ToTable("ChapterPaymentAdjustments");

        /* The only read this table exists for is "what does this group currently owe", which is a scan of
           one group's rows, so they are ordered by group rather than by key. */
        builder.HasKey(x => x.Id)
            .IsClustered(false);

        builder.HasIndex(x => x.ChapterId)
            .IsClustered();

        builder.Property(x => x.Amount)
            .IsMoneyType();

        builder.Property(x => x.CreatedUtc)
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.RecoveredAmount)
            .IsMoneyType();

        builder.Property(x => x.Type)
            .HasColumnName("ChapterPaymentAdjustmentTypeId")
            .HasConversion<int>();

        /* Cascade, matching Payments: deleting a chapter is done entirely by database cascades, and an
           adjustment for a group that no longer exists is owed by nobody.

           PaymentRefundId carries no foreign key for the same reason it cascades: Chapter already reaches
           PaymentRefunds through Payments, so a key here would be a second cascade path from Chapter into
           this table, which SQL Server rejects. */
        builder.HasOne<Chapter>()
            .WithMany()
            .HasForeignKey(x => x.ChapterId)
            .OnDelete(DeleteBehavior.Cascade);

        // Restricted: a currency is a lookup, and losing a group's balances to one being tidied up would
        // be a silent write-off.
        builder.HasOne<Currency>()
            .WithMany()
            .HasForeignKey(x => x.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
