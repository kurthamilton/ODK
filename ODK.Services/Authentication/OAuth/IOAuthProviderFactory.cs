namespace ODK.Services.Authentication.OAuth;

public interface IOAuthProviderFactory
{
    IOAuthProvider GetProvider(OAuthProviderType type);
}
