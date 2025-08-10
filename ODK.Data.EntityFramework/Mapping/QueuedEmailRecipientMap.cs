using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Emails;

namespace ODK.Data.EntityFramework.Mapping;

public class QueuedEmailRecipientMap : IEntityTypeConfiguration<QueuedEmailRecipient>
{
    public void Configure(EntityTypeBuilder<QueuedEmailRecipient> builder)
    {
        builder.ToTable("QueuedEmailRecipients");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("QueuedEmailRecipientId");

        builder.HasOne<QueuedEmail>()
            .WithMany()
            .HasForeignKey(x => x.QueuedEmailId);
    }
}
