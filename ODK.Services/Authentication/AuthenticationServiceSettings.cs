namespace ODK.Services.Authentication
{
    public class AuthenticationServiceSettings
    {
        public string EventsUrl { get; set; }

        public int PasswordResetTokenLifetimeMinutes { get; set; }

        public string PasswordResetUrl { get; set; }
    }
}
