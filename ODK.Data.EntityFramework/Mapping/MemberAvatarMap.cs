using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Members;

namespace ODK.Data.EntityFramework.Mapping;

public class MemberAvatarMap : IEntityTypeConfiguration<MemberAvatar>
{
    public void Configure(EntityTypeBuilder<MemberAvatar> builder)
    {
        builder.ToTable("MemberAvatars");

        builder.HasKey(x => x.MemberId);

        builder.Property(x => x.Version)
            .IsRowVersion();

        builder.Property(x => x.VersionInt)
            .ValueGeneratedOnAddOrUpdate();

        builder.HasOne<Member>()
            .WithOne()
            .HasForeignKey<MemberAvatar>(x => x.MemberId);
    }
}