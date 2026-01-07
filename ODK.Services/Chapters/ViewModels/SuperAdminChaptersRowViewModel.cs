namespace ODK.Services.Chapters.ViewModels;

public class SuperAdminChaptersRowViewModel
{
    public required DateTime CreatedUtc { get; init; }

    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required DateTime? SiteSubscriptionExpiresUtc { get; init; }

    public required string? SiteSubscriptionName { get; init; }
}
