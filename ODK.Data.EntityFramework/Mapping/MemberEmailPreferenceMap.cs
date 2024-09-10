using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Members;

namespace ODK.Data.EntityFramework.Mapping;

public class MemberEmailPreferenceMap : IEntityTypeConfiguration<MemberEmailPreference>
{
    public void Configure(EntityTypeBuilder<MemberEmailPreference> builder)
    {
        builder.ToTable("MemberEmailPreferences");

        builder.HasKey(x => new { x.MemberId, x.Type });

        builder.Property(x => x.Type)
            .HasColumnName("MemberEmailPreferenceTypeId");

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(x => x.MemberId);
    }
}
