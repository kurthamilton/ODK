using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Emails;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Mapping;

public class EmailMap : IEntityTypeConfiguration<Email>
{
    public void Configure(EntityTypeBuilder<Email> builder)
    {
        builder.ToTable("Emails");

        builder.HasKey(x => x.Type);

        builder.DualWriteColumn(x => x.BodyHtml, writesTo: "Body", mirrorsTo: "BodyHtml");

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
