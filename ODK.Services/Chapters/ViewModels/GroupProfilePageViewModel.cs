using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Services.Chapters.ViewModels;

public class GroupProfilePageViewModel : GroupPageViewModel
{
    public required IReadOnlyCollection<ChapterProperty> ChapterProperties { get; init; }

    public required IReadOnlyCollection<ChapterPropertyOption> ChapterPropertyOptions { get; init; }

    public required IReadOnlyCollection<MemberProperty> MemberProperties { get; init; }    
}
