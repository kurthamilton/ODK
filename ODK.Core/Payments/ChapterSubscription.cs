using System;

namespace ODK.Core.Payments
{
    public class ChapterSubscription
    {
        public ChapterSubscription(Guid id, Guid chapterId, Guid subscriptionTypeId, string name, string title, string description, double amount)
        {
            Amount = amount;
            ChapterId = chapterId;
            Description = description;
            Id = id;
            Name = name;
            SubscriptionTypeId = subscriptionTypeId;
            Title = title;
        }

        public double Amount { get; }

        public Guid ChapterId { get; }

        public string Description { get; }

        public Guid Id { get; }

        public string Name { get; }

        public Guid SubscriptionTypeId { get; }

        public string Title { get; }
    }
}
