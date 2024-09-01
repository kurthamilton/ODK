using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterMessagesAdminPageViewModel
{
    public required Chapter Chapter { get; init; } 

    public required IReadOnlyCollection<ContactRequest> Messages { get; init; }

    public required PlatformType Platform { get; init; }
}
