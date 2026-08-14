namespace ODK.Web.Razor.Models.Admin.Emails;

/// <summary>
/// The collapsed list of placeholders that insert at the caret in an email's body field.
/// </summary>
public class EmailPlaceholdersViewModel
{
    /// <summary>
    /// Whether the buttons may be clicked. False where the field cannot be typed into, since inserting into a
    /// locked editor would go nowhere.
    /// </summary>
    public required bool CanInsert { get; init; }

    public required IReadOnlyCollection<string> Placeholders { get; init; }
}
