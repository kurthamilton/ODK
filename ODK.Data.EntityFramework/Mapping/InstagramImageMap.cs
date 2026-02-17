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

        builder.Property(x => x.Id)
            .HasColumnName("InstagramImageId");

        builder.Property(x => x.Version)
            .IsRowVersion();

        builder.Property(x => x.VersionInt)
            .ValueGeneratedOnAddOrUpdate();

        builder.HasOne<InstagramPost>()
            .WithMany()
            .HasForeignKey(x => x.InstagramPostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}