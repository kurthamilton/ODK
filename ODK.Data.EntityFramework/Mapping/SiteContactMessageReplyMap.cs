using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Messages;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class SiteContactMessageReplyMap : IEntityTypeConfiguration<SiteContactMessageReply>
{
    public void Configure(EntityTypeBuilder<SiteContactMessageReply> builder)
    {
        builder.ToTable("SiteContactMessageReplies");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedUtc)
            .HasConversion<UtcDateTimeConverter>();

        // Pinned to the current column name - the columns are renamed to match in a later migration.
        builder.Property(x => x.MessageHtml)
            .HasColumnName("Message");

        builder.HasOne<SiteContactMessage>()
            .WithMany()
            .HasForeignKey(x => x.SiteContactMessageId);

        builder.HasOne(x => x.Member)
            .WithMany()
            .HasForeignKey(x => x.MemberId);
    }
}
