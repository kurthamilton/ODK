namespace ODK.Services.Authentication
{
    public class AuthenticationSettings
    {
        public int AccessTokenLifetimeMinutes { get; set; }

        public string Key { get; set; }

        public int RefreshTokenLifetimeDays { get; set; }
    }
}
