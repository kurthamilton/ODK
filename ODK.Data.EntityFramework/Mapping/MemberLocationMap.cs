using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetTopologySuite.Geometries;
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

        // Shadow property mapped to the LatLong column to enable server-side spatial queries
        builder.Property<Point>("LatLongPoint")
            .HasColumnName("LatLong")
            .HasColumnType("geography")
            .ValueGeneratedOnAddOrUpdate();

        builder.HasOne<Country>()
            .WithMany()
            .HasForeignKey(x => x.CountryId);

        builder.HasOne<Member>()
            .WithOne()
            .HasForeignKey<MemberLocation>(x => x.MemberId);
    }
}