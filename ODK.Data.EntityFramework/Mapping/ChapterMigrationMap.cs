using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterMigrationMap : IEntityTypeConfiguration<ChapterMigration>
{
    public void Configure(EntityTypeBuilder<ChapterMigration> builder)
    {
        builder.ToTable("ChapterMigrations");

        // One row per chapter and every read is by chapter, so the key is the only column worth
        // clustering on.
        builder.HasKey(x => x.ChapterId);

        builder.HasOne<Chapter>()
            .WithMany()
            .HasForeignKey(x => x.ChapterId);

        builder.Property(x => x.PreviousPlatformName)
            .HasMaxLength(100);
    }
}
