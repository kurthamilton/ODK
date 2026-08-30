using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Emails;

namespace ODK.Data.EntityFramework.Mapping;

public class EmailMap : IEntityTypeConfiguration<Email>
{
    public void Configure(EntityTypeBuilder<Email> builder)
    {
        builder.ToTable("Emails");

        builder.HasKey(x => x.Type);

        // Pinned to the current column name - the columns are renamed to match in a later migration.
        builder.Property(x => x.BodyHtml)
            .HasColumnName("Body");

        builder.Property(x => x.RecipientType)
            .HasConversion<int>()
            .HasColumnName("EmailRecipientTypeId");

        builder.Property(x => x.Subject)
            .HasMaxLength(255);

        builder.Property(x => x.Type)
            .HasColumnName("Id")
            .HasConversion<int>();
    }
}
