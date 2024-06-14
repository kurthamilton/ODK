namespace ODK.Services.Authentication;

public class AuthenticationServiceSettings
{
    public AuthenticationServiceSettings(string eventsUrl, int passwordResetTokenLifetimeMinutes,
        string passwordResetUrl)
    {
        EventsUrl = eventsUrl;
        PasswordResetTokenLifetimeMinutes = passwordResetTokenLifetimeMinutes;
        PasswordResetUrl = passwordResetUrl;
    }

    public string EventsUrl { get; }

    public int PasswordResetTokenLifetimeMinutes { get; }

    public string PasswordResetUrl { get; }
}
