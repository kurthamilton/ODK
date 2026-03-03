using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Data.Core.Chapters;

namespace ODK.Web.Razor.Models.Chapters;

public class ChapterConversationsViewModel
{
    public required int ActiveConversationCount { get; init; }

    public required bool Archived { get; init; }

    public required int ArchivedConversationCount { get; init; }

    public required Chapter? Chapter { get; init; }

    public required IEnumerable<Chapter>? Chapters { get; init; }

    public required IReadOnlyCollection<ChapterConversationDto> Conversations { get; init; }

    public required Func<bool, string> ConversationsUrlFunc { get; init; }

    public required Func<Guid, string> ConversationUrlFunc { get; init; }

    public required Member CurrentMember { get; init; }

    public TimeZoneInfo? TimeZone => CurrentMember.TimeZone ?? Chapter?.TimeZone;
}