using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Members;

namespace ODK.Data.EntityFramework.Mapping;

public class MemberPasswordMap : IEntityTypeConfiguration<MemberPassword>
{
    public void Configure(EntityTypeBuilder<MemberPassword> builder)
    {
        builder.ToTable("MemberPasswords");

        builder.HasKey(x => x.MemberId);

        builder.HasOne<Member>()
            .WithOne()
            .HasForeignKey<MemberPassword>(x => x.MemberId);
    }
}
