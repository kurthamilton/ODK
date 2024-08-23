using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterPrivacySettingsMap : IEntityTypeConfiguration<ChapterPrivacySettings>
{
    public void Configure(EntityTypeBuilder<ChapterPrivacySettings> builder)
    {
        builder.ToTable("ChapterPrivacySettings");

        builder.HasKey(x => x.ChapterId);

        builder.Property(x => x.EventResponseVisibility)
            .HasConversion<int?>();

        builder.Property(x => x.EventVisibility)
            .HasConversion<int?>();

        builder.Property(x => x.MemberVisibility)
            .HasConversion<int?>();

        builder.Property(x => x.VenueVisibility)
            .HasConversion<int?>();

        builder.HasOne<Chapter>()
            .WithOne()
            .HasForeignKey<ChapterPrivacySettings>(x => x.ChapterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}