namespace ODK.Services.Html;

/// <summary>
/// Rejects admin-authored rich text containing markup the editor could not have produced.
/// </summary>
/// <remarks>
/// Reports rather than strips: content an admin typed is theirs, and silently deleting part of it on
/// save leaves them with no idea what happened or why. A failure names what was not accepted so they
/// can remove it themselves.
/// </remarks>
public interface IHtmlValidator
{
    ServiceResult Validate(string? html);

    ServiceResult Validate(string? html, HtmlValidatorOptions options);
}
