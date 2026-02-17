using ODK.Data.Core.Members;
using ODK.Services.Chapters.ViewModels;

namespace ODK.Services.Members.ViewModels;

public class MembersPageViewModel : GroupPageViewModel
{
    public required IReadOnlyCollection<MemberWithAvatarDto> Members { get; init; }
}