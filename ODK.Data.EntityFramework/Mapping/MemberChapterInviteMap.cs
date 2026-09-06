using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class MemberChapterInviteMap : IEntityTypeConfiguration<MemberChapterInvite>
{
    public void Configure(EntityTypeBuilder<MemberChapterInvite> builder)
    {
        builder.ToTable("MemberChapterInvites");

        builder.HasKey(x => x.Id)
            .IsClustered(false);

        builder.Property(x => x.CreatedUtc)
            .HasConversion<UtcDateTimeConverter>();

        builder.HasIndex(x => x.Token)
            .IsUnique();

        builder.Property(x => x.Token)
            .HasMaxLength(255);

        builder.HasIndex(x => new { x.MemberId, x.ChapterId })
            .IsUnique()
            .IsClustered();

        builder.HasOne<Chapter>()
            .WithMany()
            .HasForeignKey(x => x.ChapterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(x => x.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
