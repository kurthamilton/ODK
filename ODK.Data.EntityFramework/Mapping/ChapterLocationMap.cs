using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetTopologySuite.Geometries;
using ODK.Core.Chapters;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterLocationMap : IEntityTypeConfiguration<ChapterLocation>
{
    public void Configure(EntityTypeBuilder<ChapterLocation> builder)
    {
        builder.ToTable("ChapterLocations");

        builder.HasKey(x => x.ChapterId);

        builder.Property(x => x.Latitude)
            .HasColumnType("decimal(9,6)")
            .ValueGeneratedOnAddOrUpdate();

        builder.Property(x => x.LatLong)
            .HasConversion<LatLongConverter>();

        // Shadow property mapped to the LatLong column to enable server-side spatial queries
        builder.Property<Point>("LatLong")
            .HasColumnType("geography")
            .IsRequired();

        builder.Property(x => x.Longitude)
            .HasColumnType("decimal(9,6)")
            .ValueGeneratedOnAddOrUpdate();

        builder.HasOne<Chapter>()
            .WithOne()
            .HasForeignKey<ChapterLocation>(x => x.ChapterId);
    }
}