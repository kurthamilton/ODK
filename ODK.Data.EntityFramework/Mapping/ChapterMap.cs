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

        // Column not being implicitly included for some reason
        builder.Property(x => x.BannerImageUrl).HasColumnName("BannerImageUrl");

        builder.Property(x => x.Id)
            .HasColumnName("ChapterId");

        builder.Property(x => x.TimeZone)
            .HasConversion<TimeZoneConverter>();
    }
}
