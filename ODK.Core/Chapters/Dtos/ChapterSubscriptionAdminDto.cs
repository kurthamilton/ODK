namespace ODK.Core.Chapters.Dtos;

public class ChapterSubscriptionAdminDto
{
    public required ChapterSubscription ChapterSubscription { get; init; }

    public required bool Used { get; init; }
}
