using ODK.Core.Chapters;

namespace ODK.Services.ViewModels;

public abstract class ChapterViewModelBase : ViewModelBase
{
    public required Chapter Chapter { get; init; }
}
