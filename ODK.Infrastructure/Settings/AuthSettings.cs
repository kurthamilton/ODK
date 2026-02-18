namespace ODK.Infrastructure.Settings;

public class AuthSettings
{
    public required int CookieLifetimeDays { get; init; }

    public required int PasswordResetTokenLifetimeMinutes { get; init; }

    public required AuthPasswordsSettings Passwords { get; init; }
}
