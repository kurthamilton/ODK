using ODK.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class SuperAdminChaptersRowViewModel
{
    public required Chapter Chapter { get; init; }

    public required DateTime? SiteSubscriptionExpiresUtc { get; init; }

    public required string? SiteSubscriptionName { get; init; }
}