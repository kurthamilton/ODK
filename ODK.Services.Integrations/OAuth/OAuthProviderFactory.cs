using ODK.Services.Authentication.OAuth;
using ODK.Services.Integrations.OAuth.Google;

namespace ODK.Services.Integrations.OAuth;

public class OAuthProviderFactory : IOAuthProviderFactory
{
    public IOAuthProvider GetProvider(OAuthProviderType type) => type switch
    {
        OAuthProviderType.Google => new GoogleOAuthProvider(),
        _ => throw new NotSupportedException()
    };
}
