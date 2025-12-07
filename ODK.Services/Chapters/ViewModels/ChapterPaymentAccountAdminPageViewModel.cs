using ODK.Core.Chapters;

namespace ODK.Services.Chapters.ViewModels;

public class ChapterPaymentAccountAdminPageViewModel
{
    public required Chapter Chapter { get; init; }

    public required bool Enabled { get; init; }

    public required string? ExternalId { get; init; }    

    public required bool HasPermission { get; init; }

    public required string? OnboardingUrl { get; init; }

    public required IReadOnlyCollection<string> RemainingSteps { get; init; }
}
