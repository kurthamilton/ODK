using ODK.Core.Web;

namespace ODK.Services.Tasks;

/// <summary>
/// The request context a background job runs under, which is to say almost none: a job has a site to build
/// URLs against and no HTTP request behind it.
/// </summary>
/// <remarks>
/// Empty rather than absent so the job path can reuse everything downstream of the request store
/// unchanged. Each member below is empty because a job genuinely has no answer for it - not because the
/// answer was dropped on the way through the queue. The consumers of the request facts (error logging, the
/// ignore-exception rules, geolocation from an IP address) all run inside the request pipeline, so none of
/// them ever sees one of these.
/// </remarks>
public class JobHttpRequestContext : IHttpRequestContext
{
    public required string BaseUrl { get; init; }

    public IReadOnlyDictionary<string, string[]> Headers { get; } = new Dictionary<string, string[]>();

    public string IpAddress { get; } = string.Empty;

    /// <summary>
    /// Always null, so formatting falls back to the default culture. A job's output is read by its recipient
    /// rather than by whoever triggered it, so it formats against the recipient's stored locale through
    /// <c>IMemberLocaleService</c>; the triggering request's locale would be the wrong one.
    /// </summary>
    public string? Locale { get; }

    public string RequestPath { get; } = string.Empty;

    public string RequestUrl { get; } = string.Empty;

    public IReadOnlyDictionary<string, string?> RouteValues { get; } = new Dictionary<string, string?>();

    public string UserAgent { get; } = string.Empty;
}
