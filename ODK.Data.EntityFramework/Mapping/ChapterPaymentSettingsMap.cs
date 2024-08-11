using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterPaymentSettingsMap : IEntityTypeConfiguration<ChapterPaymentSettings>
{
    public void Configure(EntityTypeBuilder<ChapterPaymentSettings> builder)
    {
        builder.ToTable("ChapterPaymentSettings");

        builder.HasKey(x => x.ChapterId);

        builder.HasOne(x => x.Currency)
            .WithMany()
            .HasForeignKey(x => x.CurrencyId);
    }
}
