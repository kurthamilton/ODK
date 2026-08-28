using ODK.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterAdminMembersSiteAdminPageViewModel
{
    public required IReadOnlyCollection<ChapterAdminMember> AdminMembers { get; init; }

    public required Chapter Chapter { get; init; }
}
