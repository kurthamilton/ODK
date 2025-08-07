using ODK.Core.Emails;

namespace ODK.Core.Chapters;

public class ChapterEmailProvider : EmailProvider, IDatabaseEntity, IChapterEntity
{
    public Guid ChapterId { get; set; }
}
