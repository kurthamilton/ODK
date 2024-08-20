using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterLocationMap : IEntityTypeConfiguration<ChapterLocation>
{
    public void Configure(EntityTypeBuilder<ChapterLocation> builder)
    {
        builder.ToTable("ChapterLocations");

        builder.HasKey(x => x.ChapterId);

        builder.Property(x => x.LatLong)
            .HasConversion<LatLongConverter>();

        builder.HasOne<Chapter>()
            .WithOne()
            .HasForeignKey<ChapterLocation>(x => x.ChapterId);
    }
}
