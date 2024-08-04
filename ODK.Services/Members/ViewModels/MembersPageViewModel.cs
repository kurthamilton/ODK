using ODK.Core.Members;

namespace ODK.Services.Members.ViewModels;

public class MembersPageViewModel
{
    public required string ChapterName { get; init; }

    public required IReadOnlyCollection<Member> Members { get; init; }
}
