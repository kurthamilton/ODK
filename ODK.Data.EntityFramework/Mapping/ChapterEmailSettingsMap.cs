using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterEmailSettingsMap : IEntityTypeConfiguration<ChapterEmailSettings>
{
    public void Configure(EntityTypeBuilder<ChapterEmailSettings> builder)
    {
        builder.ToTable("ChapterEmailSettings");

        builder.HasKey(x => x.ChapterId);        
    }
}
