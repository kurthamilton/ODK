using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.SocialMedia;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class InstagramFetchLogEntryMap : IEntityTypeConfiguration<InstagramFetchLogEntry>
{
    public void Configure(EntityTypeBuilder<InstagramFetchLogEntry> builder)
    {
        builder.ToTable("InstagramFetchLog");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedUtc)
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(x => x.Id)
            .HasColumnName("InstagramFetchLogId");
    }
}