using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterImageMap : IEntityTypeConfiguration<ChapterImage>
{
    public void Configure(EntityTypeBuilder<ChapterImage> builder)
    {
        builder.ToTable("ChapterImages");

        builder.HasKey(x => x.ChapterId);

        builder.Property(x => x.Version)
            .IsRowVersion();

        builder.Property(x => x.VersionInt)
            .ValueGeneratedOnAddOrUpdate();

        builder.HasOne<Chapter>()
            .WithOne()
            .HasForeignKey<ChapterImage>(x => x.ChapterId);
    }
}