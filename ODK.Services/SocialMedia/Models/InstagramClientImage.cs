namespace ODK.Services.SocialMedia.Models;

public class InstagramClientImage
{
    public required byte[] ImageData { get; init; }

    public required string? MimeType { get; init; }
}
