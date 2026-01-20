namespace ODK.Services.SocialMedia.ViewModels;

public class InstagramPostViewModel
{
    public required string? Caption { get; init; }

    public required IReadOnlyCollection<InstagramImageMetadataViewModel> Images { get; init; }

    public required string Url { get; init; }
}