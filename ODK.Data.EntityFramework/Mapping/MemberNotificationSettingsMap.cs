using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ODK.Core.Members;

namespace ODK.Data.EntityFramework.Mapping;

public class MemberNotificationSettingsMap : IEntityTypeConfiguration<MemberNotificationSettings>
{
    public void Configure(EntityTypeBuilder<MemberNotificationSettings> builder)
    {
        builder.ToTable("MemberNotificationSettings");

        builder.HasKey(x => new { x.MemberId, x.NotificationType });

        builder.Property(x => x.NotificationType)
            .HasColumnName("NotificationTypeId")
            .HasConversion<int>();

        builder.HasOne<Member>()
            .WithMany()
            .HasForeignKey(x => x.MemberId);
    }
}
