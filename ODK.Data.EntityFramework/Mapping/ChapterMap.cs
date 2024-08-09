using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterMap : IEntityTypeConfiguration<Chapter>
{
    public void Configure(EntityTypeBuilder<Chapter> builder)
    {
        builder.ToTable("Chapters");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ApprovedUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        // Column not being implicitly included for some reason
        builder.Property(x => x.BannerImageUrl).HasColumnName("BannerImageUrl");

        builder.Property(x => x.CreatedUtc)
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(x => x.Location)
            .HasConversion<NullableLatLongConverter>();

        builder.Property(x => x.Platform)
            .HasColumnName("PlatformTypeId")
            .HasConversion<int>();

        builder.Property(x => x.PublishedUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.Id)
            .HasColumnName("ChapterId");

        builder.Property(x => x.TimeZone)
            .HasConversion<TimeZoneConverter>();
    }
}
