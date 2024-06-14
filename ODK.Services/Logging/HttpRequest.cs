namespace ODK.Services.Logging;

public class HttpRequest
{
    public HttpRequest(string url, string method, string? username, IDictionary<string, string> headers,
        IDictionary<string, string> form)
    {
        Headers = headers;
        Form = form;
        Method = method;
        Url = url;
        Username = username;
    }

    public IDictionary<string, string> Form { get; }

    public IDictionary<string, string> Headers { get; }
    
    public string Method { get; }

    public string? Username { get; }

    public string Url { get; }
}
