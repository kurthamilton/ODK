using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Members;
using ODK.Core.Referrals;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class MemberMap : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("Members");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedUtc)
            .HasConversion<UtcDateTimeConverter>();
        builder.Property(x => x.Id)
            .HasColumnName("MemberId");

        builder.HasRenamedIdColumn();

        builder.Property(x => x.SiteAdmin)
            .HasColumnName("SuperAdmin");

        builder.Ignore(x => x.TimeZone);

        builder.Property(x => x.TimeZoneId)
            .HasMaxLength(255)
            .HasColumnName("TimeZone");

        // Restrict, not cascade: deleting a referral must never take the member who signed up from it.
        builder.HasOne<Referral>()
            .WithMany()
            .HasForeignKey(x => x.ReferralId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Version)
            .IsRowVersion();
    }
}