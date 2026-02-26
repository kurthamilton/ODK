namespace ODK.Core.Notifications;

public class Notification : IDatabaseEntity
{
    public Guid? ChapterId { get; set; }

    public DateTime CreatedUtc { get; set; }

    public Guid? EntityId { get; set; }

    public DateTime? ExpiresUtc { get; set; }

    public Guid Id { get; set; }

    public Guid MemberId { get; set; }

    public DateTime? ReadUtc { get; set; }

    public string Text { get; set; } = string.Empty;

    public NotificationType Type { get; set; }
}