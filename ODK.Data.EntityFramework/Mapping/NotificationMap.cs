using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Notifications;
using ODK.Data.EntityFramework.Converters;

namespace ODK.Data.EntityFramework.Mapping;

public class NotificationMap : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedUtc)
            .HasConversion<UtcDateTimeConverter>();

        builder.Property(x => x.ExpiresUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.Id)
            .HasColumnName("NotificationId");

        builder.Property(x => x.ReadUtc)
            .HasConversion<NullableUtcDateTimeConverter>();

        builder.Property(x => x.Type)
            .HasColumnName("NotificationTypeId")
            .HasConversion<int>();

        builder.HasOne<Chapter>()
            .WithMany()
            .HasForeignKey(x => x.ChapterId);

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(x => x.MemberId);
    }
}