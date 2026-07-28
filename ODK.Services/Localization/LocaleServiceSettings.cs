namespace ODK.Services.Localization;

public class LocaleServiceSettings
{
    /// <summary>The app-level fallback culture name used when a member has no preference or country.</summary>
    public required string DefaultLocale { get; init; }
}
