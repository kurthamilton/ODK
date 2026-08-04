using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Members;

namespace ODK.Data.EntityFramework.Mapping;

public class MemberPreferencesMap : IEntityTypeConfiguration<MemberPreferences>
{
    public void Configure(EntityTypeBuilder<MemberPreferences> builder)
    {
        builder.ToTable("MemberPreferences");

        builder.Property(x => x.DistanceUnit)
            .HasColumnName("DistanceUnitTypeId")
            .HasConversion<int>();

        builder.HasKey(x => x.MemberId);

        builder.HasOne<Member>()
            .WithOne()
            .HasForeignKey<MemberPreferences>(x => x.MemberId);
    }
}