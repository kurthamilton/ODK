using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Services.ViewModels;

public abstract class ChapterViewModelBase : ViewModelBase
{
    public required Chapter Chapter { get; init; }

    public required PlatformType Platform { get; init; }
}
