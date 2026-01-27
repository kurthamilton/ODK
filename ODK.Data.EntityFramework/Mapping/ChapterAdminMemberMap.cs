using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterAdminMemberMap : IEntityTypeConfiguration<ChapterAdminMember>
{
    public void Configure(EntityTypeBuilder<ChapterAdminMember> builder)
    {
        builder.ToTable("ChapterAdminMembers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("ChapterAdminMemberId");

        builder.Property(x => x.Role)
            .HasColumnName("AdminRoleId")
            .HasConversion<int>();

        builder.HasOne(x => x.Member)
            .WithMany()
            .HasForeignKey(x => x.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}