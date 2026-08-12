namespace ODK.Core.Chapters;

public class ChapterEmailSettings : IDatabaseEntity
{
    public string? AdminTitle { get; set; }

    public Guid ChapterId { get; set; }

    public Guid Id { get; set; }

    public string? MemberTitle { get; set; }
}
