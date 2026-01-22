using ODK.Core.Chapters;

namespace ODK.Data.Core.Chapters;

public class ChapterSubscriptionAdminDto
{
    public required ChapterSubscription ChapterSubscription { get; init; }

    public required bool Used { get; init; }
}