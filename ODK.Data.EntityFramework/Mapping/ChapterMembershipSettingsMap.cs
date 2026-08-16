using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterMembershipSettingsMap : IEntityTypeConfiguration<ChapterMembershipSettings>
{
    public void Configure(EntityTypeBuilder<ChapterMembershipSettings> builder)
    {
        builder.ToTable("ChapterMembershipSettings");

        builder.HasKey(x => x.ChapterId);

        builder.HasOne<Chapter>()
            .WithOne()
            .HasForeignKey<ChapterMembershipSettings>(x => x.ChapterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
