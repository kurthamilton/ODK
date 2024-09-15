using Google.Apis.Auth;
using ODK.Services.Authentication.OAuth;

namespace ODK.Services.Integrations.OAuth.Google;

public class GoogleOAuthProvider : IOAuthProvider
{
    public async Task<OAuthUser> GetUser(string token)
    {
        var payload = await GoogleJsonWebSignature.ValidateAsync(token);
        
        return new OAuthUser
        {
            Email = payload.Email,
            Name = payload.Name,
            Provider = OAuthProviderType.Google
        };
    }
}
