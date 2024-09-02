using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Messages;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class SiteContactMessageMap : IEntityTypeConfiguration<SiteContactMessage>
{
    public void Configure(EntityTypeBuilder<SiteContactMessage> builder)
    {
        builder.ToTable("SiteContactMessages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedUtc)
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(x => x.Id)
            .HasColumnName("SiteContactMessageId");

        builder.Property(x => x.RepliedUtc)
            .HasConversion<NullableUtcDateTimeConverter>();
    }
}
