using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterAdminMemberMap : IEntityTypeConfiguration<ChapterAdminMember>
{
    public void Configure(EntityTypeBuilder<ChapterAdminMember> builder)
    {
        builder.ToTable("ChapterAdminMembers");

        builder.HasKey(x => new { x.ChapterId, x.MemberId });
    }
}
