using ODK.Core.Members;

namespace ODK.Services.Chapters.Models;

public class CreateChapterSubscription
{
    public required double Amount { get; set; }

    public required Guid ChapterId { get; set; }

    public required string Description { get; set; } = "";

    public required int Months { get; set; }

    public required string Name { get; set; } = "";

    public string Title { get; set; } = "";

    public required SubscriptionType Type { get; set; }
}
