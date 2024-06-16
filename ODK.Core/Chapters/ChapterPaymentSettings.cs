namespace ODK.Core.Chapters;

public class ChapterPaymentSettings
{
    public ChapterPaymentSettings(Guid chapterId, string? apiPublicKey, string? apiSecretKey, string? provider)
    {
        ApiPublicKey = apiPublicKey;
        ApiSecretKey = apiSecretKey;
        ChapterId = chapterId;
        Provider = provider;
    }

    public string? ApiPublicKey { get; }

    public string? ApiSecretKey { get; }

    public Guid ChapterId { get; }

    public string? Provider { get; }
}
