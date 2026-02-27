using ODK.Core.Members;

namespace ODK.Data.Core.Members;

public class MemberChapterNotificationSettingsDto
{
    public required MemberChapterDto MemberChapter { get; init; }

    public required MemberChapterNotificationSettings Settings { get; init; }
}