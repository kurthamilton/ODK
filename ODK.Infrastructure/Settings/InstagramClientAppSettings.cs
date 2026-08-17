namespace ODK.Infrastructure.Settings;

public class InstagramClientAppSettings
{
    /// <summary>
    /// Null where config supplies none. Deployed environments get these from Doppler as a JSON object secret.
    /// </summary>
    /// <remarks>
    /// Nullable because an empty dictionary cannot be stated in configuration: <c>{}</c> produces no keys, so the
    /// binder has nothing to bind and leaves the property alone. Declaring it non-null would make the annotation
    /// a promise the binder cannot keep, and let the null reach <c>InstagramClient</c>, which enumerates it.
    /// </remarks>
    public required Dictionary<string, string>? Cookies { get; init; }

    public required InstagramClientGraphQLSettings GraphQL { get; init; }

    /// <inheritdoc cref="Cookies"/>
    public required Dictionary<string, string>? Headers { get; init; }
}
