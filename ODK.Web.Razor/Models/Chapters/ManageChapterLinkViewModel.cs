using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Web.Razor.Models.Chapters;

public class ManageChapterLinkViewModel
{
    public required Chapter Chapter { get; init; }

    public required PlatformType Platform { get; init; }
}
