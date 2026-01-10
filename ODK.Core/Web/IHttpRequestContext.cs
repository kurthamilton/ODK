namespace ODK.Core.Web;

public interface IHttpRequestContext
{
    string BaseUrl { get; }

    string RequestUrl { get; }
}