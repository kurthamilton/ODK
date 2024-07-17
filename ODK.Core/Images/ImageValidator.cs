namespace ODK.Core.Images;

public static class ImageValidator
{
    public static IReadOnlyCollection<string> ValidFileExtensions { get; } = new[]
    {
        ".jpg",
        ".jpeg",
        ".gif",
        ".png"
    };

    public static IReadOnlyCollection<string> ValidMimeTypes { get; } = new[]
    {
        "image/jpg",
        "image/jpeg",
        "image/pjpeg",
        "image/gif",
        "image/x-png",
        "image/png"
    };

    public static bool IsValidMimeType(string mimeType)
    {
        return ValidMimeTypes.Contains(mimeType.ToLowerInvariant());
    }
}
