using ODK.Core.Chapters;
using ODK.Core.SocialMedia;

namespace ODK.Web.Razor.Models.SocialMedia;

public class InstagramPostsViewModel
{
    public required Chapter Chapter { get; init; }

    public required IReadOnlyCollection<InstagramPost> Posts { get; init; }
}
