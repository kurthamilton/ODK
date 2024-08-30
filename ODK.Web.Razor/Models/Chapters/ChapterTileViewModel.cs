using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Web.Razor.Models.Chapters;

public class ChapterTileViewModel
{
    public required Chapter Chapter { get; init; }

    public bool IsAdmin { get; init; }

    public bool IsOwner { get; init; }

    public required PlatformType Platform { get; init; }

    public required ChapterTexts? Texts { get; init; }
}
