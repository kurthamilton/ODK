using ODK.Core.Chapters;
using ODK.Core.Countries;

namespace ODK.Services.Chapters.ViewModels;

public class ChaptersHomePageViewModel
{
    public required IReadOnlyCollection<Chapter> Chapters { get; set; }

    public required IReadOnlyCollection<Country> Countries { get; set; }
}
