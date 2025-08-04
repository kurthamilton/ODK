﻿using System.ComponentModel.DataAnnotations;

namespace ODK.Services.Chapters.ViewModels;

public class SuperAdminChapterUpdateViewModel
{
    [Required]
    [Display(Name = "Subscription")]
    public required Guid? SiteSubscriptionId { get; init; }

    [Display(Name = "Subscription expiry")]
    public required DateTime? SubscriptionExpiresUtc { get; init; }
}
