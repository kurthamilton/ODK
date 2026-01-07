using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Chapters;

public class ChapterConversationViewModel
{
    public required Chapter Chapter { get; init; }

    public required Member CurrentMember { get; init; }

    public required Guid MemberId { get; init; }

    public required IReadOnlyCollection<ChapterConversationMessage> Messages { get; init; }

    public TimeZoneInfo? TimeZone => CurrentMember.TimeZone ?? Chapter.TimeZone;
}
