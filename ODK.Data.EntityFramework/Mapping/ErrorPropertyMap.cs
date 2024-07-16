using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Logging;

namespace ODK.Data.EntityFramework.Mapping;

public class ErrorPropertyMap : IEntityTypeConfiguration<ErrorProperty>
{
    public void Configure(EntityTypeBuilder<ErrorProperty> builder)
    {
        builder.ToTable("ErrorProperties");

        builder.HasKey(x => x.Id);

        builder.HasOne<Error>()
            .WithMany()
            .HasForeignKey(x => x.ErrorId);
    }
}
