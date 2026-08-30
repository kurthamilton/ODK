using ODK.Services.Emails;

namespace ODK.Web.Razor.Models.Admin.Emails;

/// <summary>
/// The response the preview reads - see odk.email-preview.js. <see cref="BodyHtml"/> is the whole email, layout
/// included, for a sandboxed iframe to display; the other two are what a recipient sees before opening it.
/// </summary>
public class EmailPreviewViewModel
{
    public required string BodyHtml { get; init; }

    public required string From { get; init; }

    public required string Subject { get; init; }

    public static EmailPreviewViewModel FromRendered(RenderedEmail rendered) => new()
    {
        BodyHtml = rendered.BodyHtml,
        From = $"{rendered.FromName} <{rendered.FromEmailAddress}>",
        Subject = rendered.Subject
    };
}
