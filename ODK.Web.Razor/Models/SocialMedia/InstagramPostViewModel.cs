using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.SocialMedia;

public class InstagramPostViewModel
{
    public InstagramPostViewModel()
    {
    }

    public InstagramPostViewModel(Chapter chapter, Guid instagramPostId, string? caption,
        string externalId)
    {
        Caption = caption;
        Chapter = chapter;
        ExternalId = externalId;
        InstagramPostId = instagramPostId;
    }

    public required string? Caption { get; init; }

    public required Chapter Chapter { get; init; }

    public required string ExternalId { get; init; }

    public required Guid InstagramPostId { get; init; }
}
