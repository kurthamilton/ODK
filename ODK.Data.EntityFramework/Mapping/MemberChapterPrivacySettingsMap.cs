using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Data.EntityFramework.Mapping;

public class MemberChapterPrivacySettingsMap : IEntityTypeConfiguration<MemberChapterPrivacySettings>
{
    public void Configure(EntityTypeBuilder<MemberChapterPrivacySettings> builder)
    {
        builder.ToTable("MemberChapterPrivacySettings");

        builder.HasKey(x => new { x.MemberId, x.ChapterId });

        builder.HasOne<Chapter>()
            .WithMany()
            .HasForeignKey(x => x.ChapterId);

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(x => x.MemberId);
    }
}
