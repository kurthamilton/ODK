using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;
using ODK.Core.Countries;
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
        builder.Property(x => x.BannerImageUrl)
            .HasColumnName("BannerImageUrl");

        builder.Property(x => x.CreatedUtc)
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(x => x.Id)
            .HasColumnName("ChapterId");

        builder.Property(x => x.Name)
            .HasMaxLength(255);

        builder.Property(x => x.Platform)
            .HasColumnName("PlatformTypeId")
            .HasConversion<int>();

        builder.Property(x => x.PublishedUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.Slug)
            .HasMaxLength(255);

        builder.Ignore(x => x.TimeZone);

        builder.Property(x => x.TimeZoneId)
            .HasMaxLength(255)
            .HasColumnName("TimeZone");

        builder.HasIndex(x => x.Name)
            .IsUnique();

        builder.HasIndex(x => x.Slug)
            .IsUnique();

        builder.HasOne<Country>()
            .WithMany()
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
