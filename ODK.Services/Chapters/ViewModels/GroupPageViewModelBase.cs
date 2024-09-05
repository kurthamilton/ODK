using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;

namespace ODK.Services.Chapters.ViewModels;

public abstract class GroupPageViewModelBase
{
    public required Chapter Chapter { get; init; }    

    public required Member? CurrentMember { get; init; }

    public required bool HasQuestions { get; init; }

    public required bool IsAdmin { get; init; }

    public required bool IsMember { get; init; }

    public required MemberSiteSubscription? OwnerSubscription { get; init; }

    public required PlatformType Platform { get; init; }

    public TimeZoneInfo? TimeZone => CurrentMember?.TimeZone ?? Chapter.TimeZone;
}
