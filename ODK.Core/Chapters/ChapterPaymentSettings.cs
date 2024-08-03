namespace ODK.Core.Chapters;

public class ChapterPaymentSettings : IChapterEntity
{
    public string? ApiPublicKey { get; set; }

    public string? ApiSecretKey { get; set; }

    public Guid ChapterId { get; set; }

    public string? Provider { get; set; }
}
