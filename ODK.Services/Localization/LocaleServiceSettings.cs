namespace ODK.Services.Localization;

public class LocaleServiceSettings
{
    /// <summary>The app-level fallback culture name used when the request has no usable locale.</summary>
    public required string DefaultLocale { get; init; }
}
