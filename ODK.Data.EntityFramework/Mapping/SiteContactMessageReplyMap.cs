using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Messages;
using ODK.Data.EntityFramework.Converters;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Mapping;

public class SiteContactMessageReplyMap : IEntityTypeConfiguration<SiteContactMessageReply>
{
    public void Configure(EntityTypeBuilder<SiteContactMessageReply> builder)
    {
        builder.ToTable("SiteContactMessageReplies");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedUtc)
            .HasConversion<UtcDateTimeConverter>();

        builder.DualWriteColumn(x => x.MessageHtml, writesTo: "Message", mirrorsTo: "MessageHtml");

        builder.HasOne<SiteContactMessage>()
            .WithMany()
            .HasForeignKey(x => x.SiteContactMessageId);

        builder.HasOne(x => x.Member)
            .WithMany()
            .HasForeignKey(x => x.MemberId);
    }
}
