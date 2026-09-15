using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterChecklistItemMap : IEntityTypeConfiguration<ChapterChecklistItem>
{
    public void Configure(EntityTypeBuilder<ChapterChecklistItem> builder)
    {
        builder.ToTable("ChapterChecklistItems");

        // Every read is one group's whole checklist, so the key leads with the chapter and the same index
        // answers both that and the single-step lookup a write pins itself to.
        builder.HasKey(x => new { x.ChapterId, x.ChecklistItemType })
            .IsClustered();

        builder.Property(x => x.ChecklistItemType)
            .HasColumnName("ChecklistItemTypeId")
            .HasConversion<int>();

        builder.Property(x => x.CompletedUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.DismissedUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.HasOne<Chapter>()
            .WithMany()
            .HasForeignKey(x => x.ChapterId)
            .OnDelete(DeleteBehavior.Cascade);

        /* Points at the step rather than at the blueprint row's id, and restricted rather than cascading
           unlike the chapter above it. A group going away takes its checklist with it, but a blueprint row
           is the definition of a step - dropping one must not be a way to erase every group's record of
           having taken it. */
        builder.HasOne<ChecklistItem>()
            .WithMany()
            .HasForeignKey(x => x.ChecklistItemType)
            .HasPrincipalKey(x => x.Type)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
