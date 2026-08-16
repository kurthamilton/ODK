using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterConversationMessageMap : IEntityTypeConfiguration<ChapterConversationMessage>
{
    public void Configure(EntityTypeBuilder<ChapterConversationMessage> builder)
    {
        builder.ToTable("ChapterConversationMessages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedUtc)
            .HasConversion<UtcDateTimeConverter>();

        builder.HasOne<ChapterConversation>()
            .WithMany()
            .HasForeignKey(x => x.ChapterConversationId);

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(x => x.MemberId);
    }
}
