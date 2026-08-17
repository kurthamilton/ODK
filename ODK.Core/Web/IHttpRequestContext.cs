namespace ODK.Core.Web;

public interface IHttpRequestContext
{
    string BaseUrl { get; }

    /// <summary>
    /// Every header on the request. A header can arrive more than once, so each key carries all of its values
    /// rather than one joined string - joining would make an exact-value match fail on a repeated header.
    /// </summary>
    /// <remarks>
    /// The implementation keys these case-insensitively, but code matching a header by name should compare the
    /// keys itself rather than rely on that: <see cref="IReadOnlyDictionary{TKey, TValue}"/> offers no way to
    /// ask which comparer it was built with, so a lookup here is only as case-insensitive as whoever supplied
    /// the dictionary.
    /// </remarks>
    IReadOnlyDictionary<string, string[]> Headers { get; }

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