using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Members;

namespace ODK.Data.EntityFramework.Mapping;

public class MemberEmailAddressUpdateTokenMap : IEntityTypeConfiguration<MemberEmailAddressUpdateToken>
{
    public void Configure(EntityTypeBuilder<MemberEmailAddressUpdateToken> builder)
    {
        builder.ToTable("MemberEmailAddressUpdateTokens");

        builder.HasKey(x => x.MemberId);

        builder.HasOne<Member>()
            .WithOne()
            .HasForeignKey<MemberEmailAddressUpdateToken>(x => x.MemberId);
    }
}
