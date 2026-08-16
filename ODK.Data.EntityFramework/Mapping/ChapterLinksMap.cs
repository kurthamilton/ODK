using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterLinksMap : IEntityTypeConfiguration<ChapterLinks>
{
    public void Configure(EntityTypeBuilder<ChapterLinks> builder)
    {
        builder.ToTable("ChapterLinks");

        builder.HasKey(x => x.ChapterId);

        builder.Property(x => x.Version)
            .IsRowVersion();

        builder.HasOne<Chapter>()
            .WithOne()
            // No action, matching the constraint the database already enforces - unlike its siblings, which
            // cascade.
            .HasForeignKey<ChapterLinks>(x => x.ChapterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
