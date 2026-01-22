using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;
using ODK.Core.Countries;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterPaymentSettingsMap : IEntityTypeConfiguration<ChapterPaymentSettings>
{
    public void Configure(EntityTypeBuilder<ChapterPaymentSettings> builder)
    {
        builder.ToTable("ChapterPaymentSettings");

        builder.HasKey(x => x.ChapterId);

        builder.HasOne<Chapter>()
            .WithOne()
            .HasForeignKey<ChapterPaymentSettings>(x => x.ChapterId);

        builder.HasOne<Currency>()
            .WithMany()
            .HasForeignKey(x => x.CurrencyId);
    }
}