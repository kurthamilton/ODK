using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Services.Chapters.ViewModels;

public class GroupHomePageViewModel
{
    public required Chapter Chapter { get; init; }

    public required Member? CurrentMember { get; init; }
}
