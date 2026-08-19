using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Members;
using ODK.Core.Messages;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class SiteConversationMessageMap : IEntityTypeConfiguration<SiteConversationMessage>
{
    public void Configure(EntityTypeBuilder<SiteConversationMessage> builder)
    {
        builder.ToTable("SiteConversationMessages");

        /* Clustered on the conversation and then the time: every read of this table is one thread in the
           order it was written, and the last message of each thread is what both list views show. Ordering
           the rows that way is the whole job the table has. */
        builder.HasKey(x => x.Id)
            .IsClustered(false);

        builder.HasIndex(x => new { x.SiteConversationId, x.CreatedUtc })
            .IsClustered();

        builder.Property(x => x.CreatedUtc)
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(x => x.FirstReadByMemberUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.FirstReadBySiteAdminUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.HasOne<SiteConversation>()
            .WithMany()
            .HasForeignKey(x => x.SiteConversationId);

        /* NO ACTION rather than the default cascade: Members already cascades to this table through
           SiteConversations, and SQL Server refuses a second cascade path to the same table. */
        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(x => x.MemberId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
