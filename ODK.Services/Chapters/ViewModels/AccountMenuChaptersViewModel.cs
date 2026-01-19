using ODK.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class AccountMenuChaptersViewModel
{
    public required IReadOnlyCollection<Chapter> AdminChapters { get; init; }

    public required IReadOnlyCollection<Chapter> MemberChapters { get; init; }
}