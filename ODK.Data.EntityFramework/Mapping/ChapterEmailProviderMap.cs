using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterEmailProviderMap : IEntityTypeConfiguration<ChapterEmailProvider>
{
    public void Configure(EntityTypeBuilder<ChapterEmailProvider> builder)
    {
        builder.ToTable("ChapterEmailProviders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("ChapterEmailProviderId");

        builder.HasOne<Chapter>()
            .WithMany()
            .HasForeignKey(x => x.ChapterId);
    }
}
