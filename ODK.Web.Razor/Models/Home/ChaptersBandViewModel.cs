using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Platforms;

namespace ODK.Web.Razor.Models.Home;

public class ChaptersBandViewModel
{
    public required IReadOnlyCollection<Chapter> Chapters { get; init; }

    public required IReadOnlyCollection<Country> Countries { get; init; }

    public required PlatformType Platform { get; init; }
}
