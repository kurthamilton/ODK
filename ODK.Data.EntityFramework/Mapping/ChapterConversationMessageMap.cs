using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;
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

        builder.Property(x => x.Id)
            .HasColumnName("ChapterConversationMessageId");

        builder.HasOne<ChapterConversation>()
            .WithMany()
            .HasForeignKey(x => x.ChapterConversationId);

        builder.HasOne(x => x.Member)
            .WithMany()
            .HasForeignKey(x => x.MemberId);
    }
}
