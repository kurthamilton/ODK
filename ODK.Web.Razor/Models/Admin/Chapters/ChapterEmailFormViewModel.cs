namespace ODK.Web.Razor.Models.Admin.Chapters;

public class ChapterEmailFormViewModel : ChapterEmailFormSubmitViewModel
{
    /// <summary>
    /// Whether the group may write its own wording, from its subscription. Without it the fields are shown
    /// but locked, while a customisation already in place can still be turned off - see UpdateChapterEmail.
    /// </summary>
    public required bool CanOverride { get; init; }

    /// <summary>Whether the body may be typed into: the group customises it and is allowed to.</summary>
    public bool CanEditContent => CanOverride && OverridesContent;

    /// <inheritdoc cref="CanEditContent" />
    public bool CanEditSubject => CanOverride && OverridesSubject;

    /// <summary>
    /// Whether the body's Customise toggle works. A group that cannot write wording can still turn off a
    /// customisation it already has, but turning one on would only reveal a field it cannot type into.
    /// </summary>
    public bool CanToggleContent => CanOverride || OverridesContent;

    /// <inheritdoc cref="CanToggleContent" />
    public bool CanToggleSubject => CanOverride || OverridesSubject;

    /// <summary>
    /// The body the group would send if it stopped overriding, shown for comparison while it does. The field
    /// itself already holds this where the group does not override it.
    /// </summary>
    public required string InheritedContent { get; init; }

    /// <inheritdoc cref="InheritedContent" />
    public required string InheritedSubject { get; init; }

    /// <summary>
    /// Whether the group overrides the body. A field it does not override shows the site's, disabled - so it
    /// reads as the email the group actually sends, and posts nothing, which is what leaves it inheriting.
    /// </summary>
    public required bool OverridesContent { get; init; }

    /// <inheritdoc cref="OverridesContent" />
    public required bool OverridesSubject { get; init; }

    /// <summary>
    /// The placeholders this template may use, offered as insertable buttons. Render-only, so it sits
    /// here rather than on the submit model - the form posts the content, not the list it was built from.
    /// </summary>
    public required IReadOnlyCollection<string> Placeholders { get; init; }

    /// <summary>
    /// Endpoint the Preview button posts the whole form to, which renders it and hands back the email. An
    /// endpoint per page for the same reason as <see cref="ValidateUrl"/>.
    /// </summary>
    public required string PreviewUrl { get; init; }

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
