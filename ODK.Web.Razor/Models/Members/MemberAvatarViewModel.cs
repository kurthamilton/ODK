using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Members;

public class MemberAvatarViewModel
{
    public required string ChapterName { get; init; }

    public bool IsTop { get; set; }

    public int MaxWidth { get; set; }

    public required Member Member { get; init; }
}
