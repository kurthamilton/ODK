using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Data.EntityFramework.Mapping;

public class MemberChapterMap : IEntityTypeConfiguration<MemberChapter>
{
    public void Configure(EntityTypeBuilder<MemberChapter> builder)
    {
        builder.ToTable("MemberChapters");

        builder.HasKey(x => new { x.ChapterId, x.MemberId });

        builder.HasOne<Chapter>()
            .WithMany()
            .HasForeignKey(x => x.ChapterId);
    }
}