using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Countries;
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
            .HasConversion<LatLongConverter>();

        builder.HasOne<Country>()
            .WithMany()
            .HasForeignKey(x => x.CountryId);

        builder.HasOne<Member>()
            .WithOne()
            .HasForeignKey<MemberLocation>(x => x.MemberId);
    }
}
