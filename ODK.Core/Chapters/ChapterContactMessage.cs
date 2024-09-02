namespace ODK.Core.Chapters;

public class ChapterContactMessage : IDatabaseEntity, IChapterEntity
{
    public Guid ChapterId { get; set; }

    public DateTime CreatedUtc { get; set; }

    public string FromAddress { get; set; } = "";

    public Guid Id { get; set; }

    public string Message { get; set; } = "";

    public double? RecaptchaScore { get; set; }

    public DateTime? RepliedUtc { get; set; }
}
