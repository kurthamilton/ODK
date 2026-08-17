namespace ODK.Web.Common.Settings;

/// <summary>
/// What the <c>_GoogleMap</c> partial needs from <c>Google:Maps</c> configuration, mapped in
/// <c>DependencyRegistrar</c>.
/// </summary>
/// <remarks>
/// Separate from <see cref="GoogleLocationViewSettings"/> even though both currently hold only the API key. Each
/// consumer declaring what it uses is the point of these types: the embed partial and the Places script partial
/// are different consumers, and either can gain a setting without handing it to the other.
/// </remarks>
public class GoogleMapViewSettings
{
    public required string ApiKey { get; init; }
}
