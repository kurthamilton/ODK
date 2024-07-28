namespace ODK.Core.Chapters;

public class ContactRequest : IDatabaseEntity
{
    public Guid ChapterId { get; set; }

    public DateTime CreatedUtc { get; set; }

    public string FromAddress { get; set; } = "";

    public Guid Id { get; set; }

    public string Message { get; set; } = "";

    public bool Sent { get; set; }
}
