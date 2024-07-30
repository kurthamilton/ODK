using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Emails;

namespace ODK.Data.EntityFramework.Mapping;

public class EmailProviderMap : IEntityTypeConfiguration<EmailProvider>
{
    public void Configure(EntityTypeBuilder<EmailProvider> builder)
    {
        builder.ToTable("EmailProviders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("EmailProviderId");
    }
}
