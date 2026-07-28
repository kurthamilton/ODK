using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.SocialMedia;

namespace ODK.Data.EntityFramework.Mapping;

public class InstagramPostMap : IEntityTypeConfiguration<InstagramPost>
{
    public void Configure(EntityTypeBuilder<InstagramPost> builder)
    {
        builder.ToTable("InstagramPosts");

        builder.HasKey(x => x.Id);

        // Transition shadow for the UTC column-name standardisation (Date -> PostedUtc). The Date property
        // also renames in a later step; this adds and backfills the column now.
        builder.Property<DateTime?>("PostedUtcColumn")
            .HasColumnName("PostedUtc");
    }
}
