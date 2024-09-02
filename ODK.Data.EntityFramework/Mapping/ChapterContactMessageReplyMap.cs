using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class ChapterContactMessageReplyMap : IEntityTypeConfiguration<ChapterContactMessageReply>
{
    public void Configure(EntityTypeBuilder<ChapterContactMessageReply> builder)
    {
        builder.ToTable("ChapterContactMessageReplies");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedUtc)
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(x => x.Id)
            .HasColumnName("ChapterContactMessageReplyId");

        builder.HasOne<ChapterContactMessage>()
            .WithMany()
            .HasForeignKey(x => x.ChapterContactMessageId);

        builder.HasOne(x => x.Member)
            .WithMany()
            .HasForeignKey(x => x.MemberId);
    }
}
