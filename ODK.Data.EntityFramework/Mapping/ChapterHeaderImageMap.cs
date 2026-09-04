using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterHeaderImageMap : IEntityTypeConfiguration<ChapterHeaderImage>
{
    public void Configure(EntityTypeBuilder<ChapterHeaderImage> builder)
    {
        builder.ToTable("ChapterHeaderImages");

        builder.HasKey(x => x.ChapterId);

        builder.Property(x => x.Version)
            .IsRowVersion();

        // Derived by the database from the rowversion, so a new upload gets a new number and the
        // cache-busting url built from it changes with it.
        builder.Property(x => x.VersionInt)
            .HasComputedColumnSql("CONVERT([int],CONVERT([bigint],[Version]))");

        builder.HasOne<Chapter>()
            .WithOne()
            .HasForeignKey<ChapterHeaderImage>(x => x.ChapterId);
    }
}
