using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Issues;
using ODK.Core.Members;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class IssueMap : IEntityTypeConfiguration<Issue>
{
    public void Configure(EntityTypeBuilder<Issue> builder)
    {
        builder.ToTable("Issues");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedUtc)
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(x => x.ClosedUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.Id)
            .HasColumnName("IssueId");

        builder.Property(x => x.Status)
            .HasColumnName("IssueStatusTypeId")
            .HasConversion<int>();

        builder.Property(x => x.Type)
            .HasColumnName("IssueTypeId")
            .HasConversion<int>();

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(x => x.MemberId);
    }
}
