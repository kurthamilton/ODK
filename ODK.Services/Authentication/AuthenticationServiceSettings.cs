namespace ODK.Services.Authentication;

public class AuthenticationServiceSettings
{
    public required int PasswordResetTokenLifetimeMinutes { get; init; }

    public required string PasswordResetUrlPath { get; init; }
}
