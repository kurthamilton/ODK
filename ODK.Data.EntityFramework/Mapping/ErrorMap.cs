using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Logging;

namespace ODK.Data.EntityFramework.Mapping;

public class ErrorMap : IEntityTypeConfiguration<Error>
{
    public void Configure(EntityTypeBuilder<Error> builder)
    {
        builder.ToTable("Errors");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedUtc)
            .HasColumnName("CreatedDate");
    }
}
