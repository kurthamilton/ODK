﻿using ODK.Core.Chapters;
using ODK.Core.Platforms;

namespace ODK.Services.Members.ViewModels;

public class SubscriptionCreateAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required ChapterPaymentSettings? PaymentSettings { get; init; }

    public required PlatformType Platform { get; init; }
}