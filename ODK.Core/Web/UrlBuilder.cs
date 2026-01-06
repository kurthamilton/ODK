using System.Collections.Specialized;
using System.Text;
using System.Web;

namespace ODK.Core.Web;

public class UrlBuilder
{
    private readonly string _baseUrl;
    private string _path = string.Empty;
    private readonly NameValueCollection _query = [];

    private UrlBuilder(string baseUrl)
    {
        _baseUrl = baseUrl;
    }

    public static UrlBuilder Base(string baseUrl) => new UrlBuilder(baseUrl);

    public string Build()
    {
        var url = new StringBuilder(_baseUrl);

        if (!string.IsNullOrEmpty(_path))
        {
            url.Append(_path);
        }

        if (_query.Count > 0)
        {
            url.Append('?');

            var keyValues = _query
                .AllKeys
                .SelectMany(key => _query
                    .GetValues(key)?
                    .Select(value => $"{key}={HttpUtility.UrlEncode(value)}") ?? []);

            var queryString = string.Join('&', keyValues);

            url.Append(queryString);
        }

        return url.ToString();
    }

    public UrlBuilder Query(string key, string value)
    {
        _query.Add(key, value);
        return this;
    }

    public UrlBuilder Path(string path)
    {
        _path = path;
        return this;
    }
}
