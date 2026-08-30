using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;
using ODK.Core.Payments;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterSubscriptionMap : IEntityTypeConfiguration<ChapterSubscription>
{
    public void Configure(EntityTypeBuilder<ChapterSubscription> builder)
    {
        builder.ToTable("ChapterSubscriptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        // Pinned to the current column name - the columns are renamed to match in a later migration.
        builder.Property(x => x.DescriptionHtml)
            .HasColumnName("Description");

        builder.Property(x => x.Environment)
            .HasColumnName("EnvironmentTypeId");

        builder.Property(x => x.ExternalId)
            .HasMaxLength(255);

        builder.Property(x => x.PaymentProvider)
            .HasColumnName("PaymentProviderTypeId");

        builder.Property(x => x.Type)
            .HasColumnName("SubscriptionTypeId")
            .HasConversion<int>();

        builder.HasOne(x => x.Currency)
            .WithMany()
            .HasForeignKey(x => x.CurrencyId);

        builder.HasOne<Chapter>()
            .WithMany()
            .HasForeignKey(x => x.ChapterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}