namespace ODK.Services.Integrations.Instagram.Models;

public class InstagramImageResponse
{
    public required byte[] ImageData { get; init; }

    public required string? MimeType { get; init; }
}