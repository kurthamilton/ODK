namespace ODK.Core.Web;

public interface IHtmlSanitizer
{
    string Encode(string html);

    string Encode(string html, IEnumerable<string> tagWhitelist);
}
