using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Services.Chapters.ViewModels;

namespace ODK.Services.Members.ViewModels;

public class MemberPageViewModel : GroupPageViewModel
{
    public required int? AvatarVersion { get; init; }

    public required IReadOnlyCollection<ChapterProperty> ChapterProperties { get; init; }

    public required Member Member { get; init; }

    public required IReadOnlyCollection<MemberProperty> MemberProperties { get; init; }
}
