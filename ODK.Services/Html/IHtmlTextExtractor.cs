namespace ODK.Services.Html;

/// <summary>
/// Reduces admin-authored rich text to the plain text inside it, for the places a value has to be text
/// rather than markup - a meta description, an email subject, a search index entry.
/// </summary>
public interface IHtmlTextExtractor
{
    /// <summary>
    /// The readable text of <paramref name="html"/>, with markup removed, entities decoded and whitespace
    /// collapsed to single spaces. Returns an empty string for null or empty input.
    /// </summary>
    string ToPlainText(string? html);
}
