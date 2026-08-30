using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Web;

namespace ODK.Data.EntityFramework.Mapping;

public class SiteQuestionMap : IEntityTypeConfiguration<SiteQuestion>
{
    public void Configure(EntityTypeBuilder<SiteQuestion> builder)
    {
        builder.ToTable("SiteQuestions");

        builder.HasKey(x => x.Id);

        // Pinned to the current column name - the columns are renamed to match in a later migration.
        builder.Property(x => x.AnswerHtml)
            .HasColumnName("Answer");

        builder.Property(x => x.Platform)
            .HasColumnName("PlatformTypeId")
            .HasConversion<int>();

        builder.Property(x => x.Version)
            .IsRowVersion();
    }
}
