﻿using ODK.Core.Members;

namespace ODK.Services.Chapters.Models;

public class CreateChapterSubscription
{
    public required double Amount { get; init; }

    public required Guid ChapterId { get; init; }

    public required string Description { get; init; }

    public required bool Disabled { get; init; }

    public required int Months { get; init; }

    public required string Name { get; init; }

    public required bool Recurring { get; init; }

    public required string Title { get; init; }

    public required SubscriptionType Type { get; init; }
}
