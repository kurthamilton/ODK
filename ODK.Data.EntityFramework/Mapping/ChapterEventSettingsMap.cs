using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterEventSettingsMap : IEntityTypeConfiguration<ChapterEventSettings>
{
    public void Configure(EntityTypeBuilder<ChapterEventSettings> builder)
    {
        builder.ToTable("ChapterEventSettings");

        builder.HasKey(x => x.ChapterId);

        // Pinned to the current column name - the columns are renamed to match in a later migration.
        builder.Property(x => x.DefaultDescriptionHtml)
            .HasColumnName("DefaultDescription");

        builder.HasOne<Chapter>()
            .WithOne()
            .HasForeignKey<ChapterEventSettings>(x => x.ChapterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
