namespace ODK.Core.Web;

public interface IHtmlSanitizer
{
    string Sanitize(string html);

    string Sanitize(string html, IEnumerable<string> tagWhitelist);
}
