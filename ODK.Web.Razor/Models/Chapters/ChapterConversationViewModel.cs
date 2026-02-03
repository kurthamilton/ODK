using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Data.Core.Chapters;

namespace ODK.Web.Razor.Models.Chapters;

public class ChapterConversationViewModel
{
    public required Chapter Chapter { get; init; }

    public required Member CurrentMember { get; init; }

    public required Guid MemberId { get; init; }

    public required IReadOnlyCollection<ChapterConversationMessageDto> Messages { get; init; }

    public TimeZoneInfo? TimeZone => CurrentMember.TimeZone ?? Chapter.TimeZone;
}
