namespace ODK.Core.Web;

public interface IHttpRequestContext
{
    string BaseUrl { get; }

    string IpAddress { get; }

    /// <summary>
    /// The request's preferred formatting locale (a specific culture name, e.g. "en-GB") parsed from the
    /// Accept-Language header, or null when none is a valid specific culture. Date/time/number formatting is
    /// resolved from this per request; consumers fall back to the sitewide default locale when it's null.
    /// </summary>
    string? Locale { get; }

    string RequestPath { get; }

    string RequestUrl { get; }

    IReadOnlyDictionary<string, string?> RouteValues { get; }

    string UserAgent { get; }
}