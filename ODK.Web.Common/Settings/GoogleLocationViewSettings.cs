namespace ODK.Web.Common.Settings;

/// <summary>
/// What the <c>_GoogleLocation</c> partial needs from <c>Google:Maps</c> configuration, mapped in
/// <c>DependencyRegistrar</c>.
/// </summary>
/// <inheritdoc cref="GoogleMapViewSettings" path="/remarks"/>
public class GoogleLocationViewSettings
{
    public required string ApiKey { get; init; }
}
