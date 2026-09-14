using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Services.Authentication.OAuth;
using ODK.Services.Topics.Models;

namespace ODK.Services.Members.Models;

public class AccountCreateModel
{
    public required string EmailAddress { get; init; }

    /// <summary>The referral the member signed up from, when they arrived via a referral link.</summary>
    public Guid? ReferralId { get; init; }

    public required string FirstName { get; init; }

    /// <summary>
    /// What the sign-up was started in order to do, where the link that began it said so. Null for a
    /// sign-up to the site itself, which is most of them.
    /// </summary>
    public SignUpIntentType? Intent { get; init; }

    public required string LastName { get; init; }

    public required LatLong? Location { get; init; }

    public required string LocationName { get; init; }

    public required IReadOnlyCollection<NewTopicModel> NewTopics { get; init; }

    public required OAuthProviderType? OAuthProviderType { get; init; }

    public required string? OAuthToken { get; init; }

    public required string RecaptchaToken { get; init; }

    public required IReadOnlyCollection<Guid> TopicIds { get; init; }
}
