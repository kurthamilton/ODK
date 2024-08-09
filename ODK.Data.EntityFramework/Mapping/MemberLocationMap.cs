using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Members;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class MemberLocationMap : IEntityTypeConfiguration<MemberLocation>
{
    public void Configure(EntityTypeBuilder<MemberLocation> builder)
    {
        builder.ToTable("MemberLocations");

        builder.HasKey(x => x.MemberId);

        builder.Property(x => x.LatLong)
            .HasConversion<NullableLatLongConverter>();

        builder.HasOne<Member>()
            .WithOne()
            .HasForeignKey<MemberLocation>(x => x.MemberId);
    }
}
