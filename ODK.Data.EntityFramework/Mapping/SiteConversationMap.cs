using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Members;
using ODK.Core.Messages;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class SiteConversationMap : IEntityTypeConfiguration<SiteConversation>
{
    public void Configure(EntityTypeBuilder<SiteConversation> builder)
    {
        builder.ToTable("SiteConversations");

        /* Clustered on the member rather than the key: a conversation is almost always read as "this
           member's threads", and the site admin list is a scan whichever way the rows are ordered. Leading
           on MemberId means one index answers both that list and the lookup of a single row. */
        builder.HasKey(x => x.Id)
            .IsClustered(false);

        builder.HasIndex(x => new { x.MemberId, x.Id })
            .IsClustered();

        builder.Property(x => x.ArchivedUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.CreatedUtc)
            .HasConversion<UtcDateTimeConverter>();

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(x => x.MemberId);
    }
}
