﻿using ODK.Core.Members;

namespace ODK.Services.Chapters;

public class CreateChapterSubscription
{
    public double Amount { get; set; }

    public Guid ChapterId { get; set; }

    public string Description { get; set; } = "";

    public int Months { get; set; }

    public string Name { get; set; } = "";

    public string Title { get; set; } = "";

    public SubscriptionType Type { get; set; }
}
