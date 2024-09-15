namespace ODK.Services.Authentication.OAuth;

public class OAuthUser
{
    public required string Email { get; init; }

    public required string Name { get; init; }

    public required OAuthProviderType Provider { get; init; }
}
