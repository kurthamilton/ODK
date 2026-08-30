using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterQuestionMap : IEntityTypeConfiguration<ChapterQuestion>
{
    public void Configure(EntityTypeBuilder<ChapterQuestion> builder)
    {
        builder.ToTable("ChapterQuestions");

        builder.HasKey(x => x.Id);

        builder.DualWriteColumn(x => x.AnswerHtml, writesTo: "Answer", mirrorsTo: "AnswerHtml");

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
