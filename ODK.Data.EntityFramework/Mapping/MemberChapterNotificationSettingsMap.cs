using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Members;

namespace ODK.Data.EntityFramework.Mapping;

public class MemberChapterNotificationSettingsMap : IEntityTypeConfiguration<MemberChapterNotificationSettings>
{
    public void Configure(EntityTypeBuilder<MemberChapterNotificationSettings> builder)
    {
        builder.ToTable("MemberChapterNotificationSettings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("MemberChapterNotificationSettingId");

        builder.Property(x => x.NotificationType)
            .HasColumnName("NotificationTypeId")
            .HasConversion<int>();

        builder.HasOne<MemberChapter>()
            .WithMany()
            .HasForeignKey(x => x.MemberChapterId);
    }
}