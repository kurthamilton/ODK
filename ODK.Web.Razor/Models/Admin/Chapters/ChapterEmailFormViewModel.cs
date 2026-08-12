namespace ODK.Web.Razor.Models.Admin.Chapters;

public class ChapterEmailFormViewModel : ChapterEmailFormSubmitViewModel
{
    /// <summary>
    /// The placeholders this template may use, offered as insertable buttons. Render-only, so it sits
    /// here rather than on the submit model - the form posts the content, not the list it was built from.
    /// </summary>
    public required IReadOnlyCollection<string> Placeholders { get; init; }

    /// <summary>
    /// Set when the group's subscription does not include custom emails. The content is still shown -
    /// an existing override keeps being sent - but nothing on the form can be changed.
    /// </summary>
    public required bool ReadOnly { get; init; }

    /// <summary>
    /// Endpoint the editor posts the content to for the HTML checks that cannot run in the browser. The
    /// form is shared by the group and site-admin pages, which authorise differently and so have an
    /// endpoint each - hence supplied by the caller rather than built in the partial.
    /// </summary>
    public required string ValidateUrl { get; init; }

    /// <summary>
    /// Every placeholder the send path supplies for this email, which the editor accepts. A superset of
    /// <see cref="Placeholders"/>: a group is offered fewer than it may use, and a template already
    /// referencing one of the others still resolves, so validation must not reject it.
    /// </summary>
    public required IReadOnlyCollection<string> ValidPlaceholders { get; init; }
}
