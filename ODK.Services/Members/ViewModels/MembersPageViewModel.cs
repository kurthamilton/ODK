using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Services.Chapters.ViewModels;

namespace ODK.Services.Members.ViewModels;

public class MembersPageViewModel : GroupPageViewModel
{
    public required ChapterPage? ChapterPage { get; init; }

    public required IReadOnlyCollection<Member> Members { get; init; }
}