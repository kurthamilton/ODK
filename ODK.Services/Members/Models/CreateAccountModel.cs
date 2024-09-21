using ODK.Core.Countries;
using ODK.Services.Authentication.OAuth;
using ODK.Services.Topics.Models;

namespace ODK.Services.Members.Models;

public class CreateAccountModel
{
    public required string EmailAddress { get; init; }

    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public required LatLong? Location { get; init; }

    public required string LocationName { get; init; }

    public required IReadOnlyCollection<NewTopicModel> NewTopics { get; init; }

    public required OAuthProviderType? OAuthProviderType { get; init; }

    public required string? OAuthToken { get; init; }

    public required string TimeZoneId { get; init; }

    public required IReadOnlyCollection<Guid> TopicIds { get; init; }
}
