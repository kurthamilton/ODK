using System.Collections.Generic;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.SocialMedia;
using ODK.Services.Events;

namespace ODK.Web.Common.Chapters;
public class ChapterPageViewModel : ChapterViewModel
{
    public required ChapterLinks Links { get; set; }

    public required IReadOnlyCollection<EventResponseViewModel> Events { get; set; }

    public required IReadOnlyCollection<InstagramPost> InstagramPosts { get; set; }

    public required IReadOnlyCollection<Member> LatestMembers { get; set; }

    public required ChapterTexts? Texts { get; set; }
}
