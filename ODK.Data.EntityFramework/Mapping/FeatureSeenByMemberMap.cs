using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Features;
using ODK.Core.Members;

namespace ODK.Data.EntityFramework.Mapping;

public class FeatureSeenByMemberMap : IEntityTypeConfiguration<FeatureSeenByMember>
{
    public void Configure(EntityTypeBuilder<FeatureSeenByMember> builder)
    {
        builder.ToTable("FeatureSeenByMembers");

        builder.HasKey(x => new { x.FeatureId, x.MemberId });

        builder.HasOne<Feature>()
            .WithMany()
            .HasForeignKey(x => x.FeatureId);

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(x => x.MemberId);
    }
}
