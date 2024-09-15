namespace ODK.Services.Authentication.OAuth;

public interface IOAuthProvider
{
    Task<OAuthUser> GetUser(string token);
}
