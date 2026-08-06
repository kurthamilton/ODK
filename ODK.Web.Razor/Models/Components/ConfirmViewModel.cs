namespace ODK.Web.Razor.Models.Components;

/// <summary>
/// Renders inside a form to require confirmation before it submits. See the _Confirm component and
/// bindConfirms in odk.js.
/// </summary>
public class ConfirmViewModel
{
    /// <summary>
    /// The question shown in the dialog body, e.g. "Are you sure you want to delete this feature?".
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Overrides the accept button's label. Leave unset for the default "OK" - the <see cref="Title"/>
    /// already names the action, so a bespoke label is rarely needed.
    /// </summary>
    public string? OkText { get; init; }

    /// <summary>
    /// Dialog heading naming the action, e.g. "Delete feature".
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Bootstrap button variant for the accept button (e.g. "danger", "primary"). Defaults to danger.
    /// </summary>
    public string? Variant { get; init; }
}
