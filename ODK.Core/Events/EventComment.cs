namespace ODK.Core.Events;

public class EventComment : IDatabaseEntity
{
    public DateTime CreatedUtc { get; set; }

    public Guid EventId { get; set; }

    public bool Hidden { get; set; }

    public Guid Id { get; set; }

    public Guid MemberId { get; set; }

    public Guid? ParentEventCommentId { get; set; }

    public string Text { get; set; } = string.Empty;
}
