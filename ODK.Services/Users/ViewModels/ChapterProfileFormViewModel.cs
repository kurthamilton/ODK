using ODK.Core.Chapters;

namespace ODK.Services.Users.ViewModels;

public class ChapterProfileFormViewModel : ChapterProfileFormSubmitViewModel
{
    public required string ChapterName { get; init; }

    public required IReadOnlyCollection<ChapterProperty> ChapterProperties { get; init; }

    public required IReadOnlyCollection<ChapterPropertyOption> ChapterPropertyOptions { get; init; }

    public required int TrialPeriodMonths { get; init; }
}
