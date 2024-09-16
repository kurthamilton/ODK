using ODK.Services.Authentication.OAuth;

namespace ODK.Services.Users.ViewModels;

public class OAuthDetailsFormViewModel
{
    public OAuthProviderType? Provider { get; set; }

    public string? Token { get; set; }
}
