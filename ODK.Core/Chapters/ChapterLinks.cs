namespace ODK.Core.Chapters;

public class ChapterLinks : IVersioned, IChapterEntity
{
    public Guid ChapterId { get; set; }

    public string? FacebookName { get; set; }

    // WhatsApp purposefully excluded as its not public
    public bool HasLinks =>
        !string.IsNullOrEmpty(FacebookName) ||
        !string.IsNullOrEmpty(InstagramName) ||
        !string.IsNullOrEmpty(TwitterName);

    public string? InstagramName { get; set; }

    public string? TwitterName { get; set; }

    public byte[] Version { get; set; } = [];

    public string? WhatsApp { get; set; }
}