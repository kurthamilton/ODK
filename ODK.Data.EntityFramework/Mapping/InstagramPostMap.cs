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

        // Transition shadow keeps the legacy Date column populated until it is dropped.
        builder.Property<DateTime?>("PostedUtcColumn")
            .HasColumnName("Date");
    }
}
