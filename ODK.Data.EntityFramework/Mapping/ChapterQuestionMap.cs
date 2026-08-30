using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterQuestionMap : IEntityTypeConfiguration<ChapterQuestion>
{
    public void Configure(EntityTypeBuilder<ChapterQuestion> builder)
    {
        builder.ToTable("ChapterQuestions");

        builder.HasKey(x => x.Id);

        // Pinned to the current column name - the columns are renamed to match in a later migration.
        builder.Property(x => x.AnswerHtml)
            .HasColumnName("Answer");

        builder.Property(x => x.Name)
            .HasMaxLength(255);

        builder.Property(x => x.Version)
            .IsRowVersion();

        builder.HasOne<Chapter>()
            .WithMany()
            .HasForeignKey(x => x.ChapterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
