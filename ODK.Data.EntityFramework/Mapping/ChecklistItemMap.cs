using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;

namespace ODK.Data.EntityFramework.Mapping;

public class ChecklistItemMap : IEntityTypeConfiguration<ChecklistItem>
{
    public void Configure(EntityTypeBuilder<ChecklistItem> builder)
    {
        builder.ToTable("ChecklistItems");

        // A row per step and every read is the whole table in display order, so there is no column here
        // worth clustering on over the key.
        builder.HasKey(x => x.Id);

        /* Unique, and what ChapterChecklistItems points at - a group's record names the step it took
           rather than a row id, so the rows say what they are without a join. Declared as an alternate
           key because a foreign key needs one to reference. */
        builder.HasAlternateKey(x => x.Type);

        builder.Property(x => x.Type)
            .HasColumnName("ChecklistItemTypeId")
            .HasConversion<int>();

        builder.Property(x => x.Name)
            .HasMaxLength(255);
    }
}
