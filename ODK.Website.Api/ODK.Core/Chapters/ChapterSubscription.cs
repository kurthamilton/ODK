﻿using System;
using ODK.Core.Members;

namespace ODK.Core.Chapters
{
    public class ChapterSubscription
    {
        public ChapterSubscription(Guid id, Guid chapterId, SubscriptionType subscriptionType, string name, string title,
            string description, double amount)
        {
            Amount = amount;
            ChapterId = chapterId;
            Description = description;
            Id = id;
            Name = name;
            SubscriptionType = subscriptionType;
            Title = title;
        }

        public double Amount { get; }

        public Guid ChapterId { get; }

        public string Description { get; }

        public Guid Id { get; }

        public string Name { get; }

        public SubscriptionType SubscriptionType { get; }

        public string Title { get; }
    }
}