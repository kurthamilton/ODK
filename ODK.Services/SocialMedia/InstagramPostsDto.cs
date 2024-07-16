using ODK.Core.Chapters;
using ODK.Core.SocialMedia;

namespace ODK.Services.SocialMedia;
public class InstagramPostsDto
{
    public required ChapterLinks Links { get; set; }

    public required IReadOnlyCollection<InstagramPost> Posts { get; set; }
}
