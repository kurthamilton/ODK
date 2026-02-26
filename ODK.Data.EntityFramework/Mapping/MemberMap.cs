using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Members;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class MemberMap : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("Members");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedUtc)
            .HasColumnName("CreatedDate")
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(x => x.Id)
            .HasColumnName("MemberId");

        builder.Property(x => x.SiteAdmin)
            .HasColumnName("SuperAdmin");

        builder.Property(x => x.TimeZone)
            .HasConversion<TimeZoneConverter>();

        builder.Property(x => x.Version)
            .IsRowVersion();
    }
}