using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.SocialMedia;

namespace ODK.Data.EntityFramework.Mapping;

public class InstagramImageMap : IEntityTypeConfiguration<InstagramImage>
{
    public void Configure(EntityTypeBuilder<InstagramImage> builder)
    {
        builder.ToTable("InstagramImages");

        builder.HasKey(x => x.InstagramPostId);

        builder.Property(x => x.Version)
            .IsRowVersion();

        builder.HasOne<InstagramPost>()
            .WithMany()
            .HasForeignKey(x => x.InstagramPostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
