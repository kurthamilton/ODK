namespace ODK.Core.Web;

public interface IHttpRequestContext
{
    string BaseUrl { get; }

    string IpAddress { get; }

    string RequestPath { get; }

    string RequestUrl { get; }

    IReadOnlyDictionary<string, string?> RouteValues { get; }

    string UserAgent { get; }
}