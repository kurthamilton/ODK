using ODK.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterPrivacyAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required ChapterPrivacySettings? PrivacySettings { get; init; }
}
