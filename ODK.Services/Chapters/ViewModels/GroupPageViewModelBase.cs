using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Services.Chapters.ViewModels;

public abstract class GroupPageViewModelBase
{
    public required Chapter Chapter { get; init; }    

    public required Member? CurrentMember { get; init; }

    public required bool IsAdmin { get; init; }

    public bool PublicView { get; init; }
}
