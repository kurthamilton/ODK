namespace ODK.Services.Authentication;

public class AuthenticationServiceSettings
{
    public required string EventsUrlPath { get; init; }

    public required int PasswordResetTokenLifetimeMinutes { get; init; }

    public required string PasswordResetUrlPath { get; init; }
}
