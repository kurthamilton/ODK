namespace ODK.Web.Razor.Models.Admin.Emails;

/// <summary>
/// The buttons that act on an email's body field, shared by the group and site forms.
/// </summary>
public class EmailEditorButtonsViewModel
{
    public required string PreviewUrl { get; init; }
}
