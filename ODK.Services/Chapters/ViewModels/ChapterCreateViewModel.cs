using ODK.Core.Countries;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterCreateViewModel
{
    public required int ChapterCount { get; set; }

    public required int? ChapterLimit { get; set; }

    public required IReadOnlyCollection<Country> Countries { get; set; }
}
