using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Emails;

namespace ODK.Data.EntityFramework.Mapping;

public class SiteEmailSettingsMap : IEntityTypeConfiguration<SiteEmailSettings>
{
    public void Configure(EntityTypeBuilder<SiteEmailSettings> builder)
    {
        builder.ToTable("SiteEmailSettings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AdminTitle)
            .HasMaxLength(255);

        builder.Property(x => x.FromEmailAddress)
            .HasMaxLength(255);

        builder.Property(x => x.FromName)
            .HasMaxLength(255);

        builder.Property(x => x.MemberTitle)
            .HasMaxLength(255);

        builder.Property(x => x.Platform)
            .HasColumnName("PlatformTypeId")
            .HasConversion<int>();

        builder.Property(x => x.PlatformTitle)
            .HasMaxLength(255);

        builder.Property(x => x.Title)
            .HasMaxLength(255);
    }
}
