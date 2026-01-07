using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Header;

public class AccountMenuViewModel
{
    public Chapter? CurrentChapter { get; init; }

    public Member? Member { get; init; }

    public required IReadOnlyCollection<Chapter> MemberChapters { get; init; }
}
