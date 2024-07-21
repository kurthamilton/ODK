using ODK.Core.Chapters;
using ODK.Core.Countries;

namespace ODK.Web.Razor.Models.Home;

public class ChaptersSectionViewModel
{
    public required IReadOnlyCollection<Chapter> Chapters { get; init; }

    public required IReadOnlyCollection<Country> Countries { get; init; }
}
