namespace ODK.Core.Web;

public interface IHttpRequestContext
{
    string BaseUrl { get; }

    string RequestPath { get; }

    string RequestUrl { get; }

    string UserAgent { get; }
}