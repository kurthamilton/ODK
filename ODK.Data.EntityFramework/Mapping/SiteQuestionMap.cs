using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Web;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Mapping;

public class SiteQuestionMap : IEntityTypeConfiguration<SiteQuestion>
{
    public void Configure(EntityTypeBuilder<SiteQuestion> builder)
    {
        builder.ToTable("SiteQuestions");

        builder.HasKey(x => x.Id);

        builder.DualWriteColumn(x => x.AnswerHtml, writesTo: "Answer", mirrorsTo: "AnswerHtml");

        builder.Property(x => x.Platform)
            .HasColumnName("PlatformTypeId")
            .HasConversion<int>();

        builder.Property(x => x.Version)
            .IsRowVersion();
    }
}
