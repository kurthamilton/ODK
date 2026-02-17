using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Members;

namespace ODK.Data.EntityFramework.Mapping;

public class MemberImageMap : IEntityTypeConfiguration<MemberImage>
{
    public void Configure(EntityTypeBuilder<MemberImage> builder)
    {
        builder.ToTable("MemberImages");

        builder.HasKey(x => x.MemberId);

        builder.Property(x => x.Version)
            .IsRowVersion();

        builder.Property(x => x.VersionInt)
            .ValueGeneratedOnAddOrUpdate();

        builder.HasOne<Member>()
            .WithOne()
            .HasForeignKey<MemberImage>(x => x.MemberId);
    }
}