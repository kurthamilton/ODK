using ODK.Core.Pages;

namespace ODK.Core.Chapters;

public class ChapterPage : IDatabaseEntity, IChapterEntity
{
    public Guid ChapterId { get; set; }

    public Guid Id { get; set; }

    public PageType PageType { get; set; }

    public bool Hidden { get; set; }

    public string? Title { get; set; }
}