namespace ODK.Services.Members.ViewModels;

public class SiteAdminFlaggedMembersRowViewModel
{
    public required bool Activated { get; init; }

    public required DateTime CreatedUtc { get; init; }

    public required string EmailAddress { get; init; }

    public required string FullName { get; init; }

    public required Guid MemberId { get; init; }

    public required double RecaptchaScore { get; init; }
}
