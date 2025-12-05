namespace ODK.Services.Chapters.ViewModels;

public class ChapterPaymentAccountAdminPageViewModel
{
    public required bool Enabled { get; init; }

    public required string? ExternalId { get; init; }    

    public required string? OnboardingUrl { get; init; }
}
