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

        builder.Property(x => x.Id)
            .HasColumnName("ChapterQuestionId");

        builder.Property(x => x.Version)
            .IsRowVersion();
    }
}
