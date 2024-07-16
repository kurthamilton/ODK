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

        builder.Property(x => x.Type)
            .HasColumnName("EmailTypeId")
            .HasConversion<int>();

        builder.Property(x => x.HtmlContent)
            .HasColumnName("Body");
    }
}
