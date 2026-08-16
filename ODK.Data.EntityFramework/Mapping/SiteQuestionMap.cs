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

        builder.Property(x => x.Id)
            .HasColumnName("SiteQuestionId");

        builder.HasRenamedIdColumn();

        builder.Property(x => x.Platform)
            .HasColumnName("PlatformTypeId")
            .HasConversion<int>();

        builder.Property(x => x.Version)
            .IsRowVersion();
    }
}
