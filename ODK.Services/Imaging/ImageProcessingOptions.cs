namespace ODK.Services.Imaging;

public class ImageProcessingOptions
{
    public decimal? AspectRatio { get; init; }

    public int? MaxWidth { get; init; }

    public string? MimeType { get; init; }
}
