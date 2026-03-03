using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Data.Core.Chapters;
using ODK.Web.Razor.Models.Components;

namespace ODK.Web.Razor.Models.Chapters;

public class ChapterConversationViewModel
{
    public required IReadOnlyCollection<MenuItem> Breadcrumbs { get; init; }

    public required Chapter Chapter { get; init; }

    public required ChapterConversation Conversation { get; init; }

    public required Func<Guid, string> ConversationUrlFunc { get; init; }

    public required Member CurrentMember { get; init; }

    public required IReadOnlyCollection<ChapterConversationMessageDto> Messages { get; init; }

    public required IReadOnlyCollection<ChapterConversationDto> OtherConversations { get; init; }

    public TimeZoneInfo? TimeZone => CurrentMember.TimeZone ?? Chapter.TimeZone;
}