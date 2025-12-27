using ODK.Core.Chapters;

namespace ODK.Core.Notifications;

public class Notification : IDatabaseEntity
{
    public Chapter? Chapter { get; set; }

    public Guid? ChapterId { get; set; }

    public DateTime CreatedUtc { get; set; }

    public Guid? EntityId { get; set; }

    public DateTime? ExpiresUtc { get; set; }

    public Guid Id { get; set; }

    public Guid MemberId { get; set; }

    public DateTime? ReadUtc { get; set; }

    public string Text { get; set; } = string.Empty;

    public NotificationType Type { get; set; }

    public Notification Clone()
    {
        return new Notification
        {
            Chapter = Chapter,
            ChapterId = ChapterId,
            CreatedUtc = CreatedUtc,
            EntityId = EntityId,
            ExpiresUtc = ExpiresUtc,
            Id = Id,
            MemberId = MemberId,
            Text = Text,
            Type = Type,
            ReadUtc = ReadUtc
        };
    }
}
