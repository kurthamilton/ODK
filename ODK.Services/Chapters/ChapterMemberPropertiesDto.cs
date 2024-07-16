using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Services.Chapters;
public class ChapterMemberPropertiesDto
{
    public required IReadOnlyCollection<ChapterProperty> ChapterProperties { get; set; }

    public required IReadOnlyCollection<ChapterPropertyOption> ChapterPropertyOptions { get; set; }

    public required IReadOnlyCollection<MemberProperty> MemberProperties { get; set; }

    public required ChapterMembershipSettings MembershipSettings { get; set; }
}
