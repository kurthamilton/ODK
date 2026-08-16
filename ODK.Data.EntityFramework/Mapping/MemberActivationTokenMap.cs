using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Data.EntityFramework.Mapping;

public class MemberActivationTokenMap : IEntityTypeConfiguration<MemberActivationToken>
{
    public void Configure(EntityTypeBuilder<MemberActivationToken> builder)
    {
        builder.ToTable("MemberActivationTokens");

        builder.HasKey(x => x.MemberId);

        builder.HasOne<Member>()
            .WithOne()
            .HasForeignKey<MemberActivationToken>(x => x.MemberId);

        // Optional - a token is not always raised against a group. Cascade, matching the constraint the
        // database already enforces.
        builder.HasOne<Chapter>()
            .WithMany()
            .HasForeignKey(x => x.ChapterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
