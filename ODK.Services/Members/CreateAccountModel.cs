using ODK.Core.Countries;
using ODK.Services.Authentication.OAuth;

namespace ODK.Services.Members;

public class CreateAccountModel
{
    public required string EmailAddress { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public required LatLong? Location { get; set; }

    public required string LocationName { get; set; }

    public required OAuthProviderType? OAuthProviderType { get; set; }

    public required string? OAuthToken { get; set; }

    public required string TimeZoneId { get; set; }
}
