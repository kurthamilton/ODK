using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.SocialMedia;

namespace ODK.Data.EntityFramework.Mapping;

public class InstagramImageMap : IEntityTypeConfiguration<InstagramImage>
{
    public void Configure(EntityTypeBuilder<InstagramImage> builder)
    {
        builder.ToTable("InstagramImages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Version)
            .IsRowVersion();

        // Derived by the database from the rowversion, so a write gets a new number and the
        // cache-busting url built from it changes with it.
        builder.Property(x => x.VersionInt)
            .HasComputedColumnSql("CONVERT([int],CONVERT([bigint],[Version]))");

        builder.HasOne<InstagramPost>()
            .WithMany()
            .HasForeignKey(x => x.InstagramPostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}