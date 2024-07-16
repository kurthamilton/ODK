using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Members;

namespace ODK.Data.EntityFramework.Mapping;

public class MemberRefreshTokenMap : IEntityTypeConfiguration<MemberRefreshToken>
{
    public void Configure(EntityTypeBuilder<MemberRefreshToken> builder)
    {
        builder.ToTable("MemberRefreshTokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("MemberRefreshTokenId");
    }
}
