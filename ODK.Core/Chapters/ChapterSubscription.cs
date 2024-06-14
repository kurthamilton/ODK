using ODK.Core.Members;

namespace ODK.Core.Chapters;

public class ChapterSubscription
{
    public ChapterSubscription(Guid id, Guid chapterId, SubscriptionType type, string name, string title,
        string description, double amount, int months)
    {
        Amount = amount;
        ChapterId = chapterId;
        Description = description;
        Id = id;
        Months = months;
        Name = name;
        Title = title;
        Type = type;
    }

    public double Amount { get; set; }

    public Guid ChapterId { get; }

    public string Description { get; set; }

    public Guid Id { get; }

    public int Months { get; set; }

    public string Name { get; set; }

    public string Title { get; set; }

    public SubscriptionType Type { get; set; }
}
