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

        builder.Property(x => x.Platform)
            .HasColumnName("PlatformTypeId")
            .HasConversion<int>();
    }
}
