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
    }
}
