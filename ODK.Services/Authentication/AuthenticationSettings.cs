namespace ODK.Services.Authentication
{
    public class AuthenticationSettings
    {
        public int AccessTokenLifetimeMinutes { get; set; }

        public string ActivateAccountUrl { get; set; }

        public string Key { get; set; }

        public int PasswordResetTokenLifetimeMinutes { get; set; }

        public string PasswordResetUrl { get; set; }

        public int RefreshTokenLifetimeDays { get; set; }
    }
}
