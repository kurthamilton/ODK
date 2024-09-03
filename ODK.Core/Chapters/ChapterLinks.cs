namespace ODK.Core.Chapters;

public class ChapterLinks : IVersioned, IChapterEntity
{
    public Guid ChapterId { get; set; }

    public string? FacebookName { get; set; }

    public bool HasLinks => 
        !string.IsNullOrEmpty(FacebookName) || 
        !string.IsNullOrEmpty(InstagramName) || 
        !string.IsNullOrEmpty(TwitterName);

    public string? InstagramName { get; set; }

    public string? TwitterName { get; set; }

    public byte[] Version { get; set; } = [];
}
