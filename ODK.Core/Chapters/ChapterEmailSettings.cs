namespace ODK.Core.Chapters;

public class ChapterEmailSettings
{
    public Guid ChapterId { get; set; }

    public string FromEmailAddress { get; set; } = "";

    public string FromName { get; set; } = "";
}
