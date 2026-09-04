using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetTopologySuite.Geometries;
using ODK.Core.Chapters;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterLocationMap : IEntityTypeConfiguration<ChapterLocation>
{
    public void Configure(EntityTypeBuilder<ChapterLocation> builder)
    {
        builder.ToTable("ChapterLocations");

        builder.HasKey(x => x.ChapterId);

        /* Shadow property mapped to the LatLong column to enable server-side spatial queries. The
           database derives the point from the two coordinates, so nothing writes it. */
        builder.Property<Point>("LatLongPoint")
            .HasColumnName("LatLong")
            .HasColumnType("geography")
            .HasComputedColumnSql("[geography]::Point([Latitude],[Longitude],(4326))", stored: true);

        builder.HasOne<Chapter>()
            .WithOne()
            .HasForeignKey<ChapterLocation>(x => x.ChapterId);
    }
}