namespace ODK.Web.Razor.Services;

public class RequestStoreSettings
{
    public required IReadOnlyCollection<string> IgnoreNotFoundPaths { get; init; }

    public required IReadOnlyCollection<string> IgnoreNotFoundPathPatterns { get; init; }

    public required IReadOnlyCollection<string> WarningNotFoundUserAgents { get; init; }
}