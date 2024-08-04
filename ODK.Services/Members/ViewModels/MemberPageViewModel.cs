using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Services.Members.ViewModels;

public class MemberPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required IReadOnlyCollection<ChapterProperty> ChapterProperties { get; init; }

    public required Member CurrentMember { get; init; }

    public required Member Member { get; init; }

    public required IReadOnlyCollection<MemberProperty> MemberProperties { get; init; }
}
