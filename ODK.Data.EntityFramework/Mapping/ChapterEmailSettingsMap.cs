using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterEmailSettingsMap : IEntityTypeConfiguration<ChapterEmailSettings>
{
    public void Configure(EntityTypeBuilder<ChapterEmailSettings> builder)
    {
        builder.ToTable("ChapterEmailSettings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AdminTitle)
            .HasMaxLength(255);

        builder.Property(x => x.MemberTitle)
            .HasMaxLength(255);

        builder.HasIndex(x => x.ChapterId)
            .IsUnique();

        builder.HasOne<Chapter>()
            .WithOne()
            .HasForeignKey<ChapterEmailSettings>(x => x.ChapterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
