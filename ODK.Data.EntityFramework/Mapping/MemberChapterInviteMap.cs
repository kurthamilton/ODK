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

        /* Id is the key but not what this table is read by, so it does not earn the clustered index. Every
           lookup here starts from a member - whether they have an invitation to a given chapter, and what
           invitations to carry over when their unactivated account is recreated - so the rows are ordered by
           member instead. ChapterId keeps the index EF gives it for the foreign key, which serves the import's
           read of a whole chapter's invitations. */
        builder.HasKey(x => x.Id)
            .IsClustered(false);

        builder.Property(x => x.CreatedUtc)
            .HasConversion<UtcDateTimeConverter>();

        // Looked up by token when an invitation link is followed, and one token belongs to one invitation.
        builder.HasIndex(x => x.Token)
            .IsUnique();

        builder.Property(x => x.Token)
            .HasMaxLength(255);

        /* At most one outstanding invitation per member per chapter: a second would make "is this member invited"
           ambiguous, and give one member two links into the same group. MemberId leads so the same index answers
           "this member's invitations" as well as the exact pair. */
        builder.HasIndex(x => new { x.MemberId, x.ChapterId })
            .IsUnique()
            .IsClustered();

        /* Both cascade, and both need to. DeleteChapter relies entirely on database cascades - it deletes the
           chapter row and nothing else - and deleting an unactivated member is how CreateAccount handles someone
           signing up again instead of using their activation link, which is exactly the state an imported member
           is in. An invitation outliving either parent would block that delete rather than tidy itself away.

           Two cascades into one table is what SQL Server rejects when both trace back to the same root, and
           these do not: Chapters.OwnerId is Restrict, so deleting a member cannot reach this table by way of the
           chapters they own. */
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
