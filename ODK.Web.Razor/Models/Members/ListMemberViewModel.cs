using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;

namespace ODK.Web.Razor.Models.Members;

public class ListMemberViewModel
{
    public required int? AvatarVersion { get; init; }

    public required Chapter Chapter { get; init; }

    public bool HideName => Size is "sm" or "xs";

    public int MaxWidth { get; init; }

    public required Member Member { get; init; }

    public required PlatformType Platform { get; init; }

    public string? Size { get; set; }
}
