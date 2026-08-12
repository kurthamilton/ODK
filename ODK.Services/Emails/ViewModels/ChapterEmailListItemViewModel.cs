using ODK.Core.Emails;

namespace ODK.Services.Emails.ViewModels;

/// <summary>
/// One row of a group's email template list.
/// </summary>
public class ChapterEmailListItemViewModel
{
    /// <summary>
    /// The group's override where it has one, and the site's template standing in where it does not -
    /// <see cref="ChapterEmail.IsDefault"/> tells them apart.
    /// </summary>
    public required ChapterEmail Email { get; init; }

    /// <summary>
    /// Read from the site's row for this type, which is where it lives: an override changes the wording and
    /// not who the email is for.
    /// </summary>
    public required EmailRecipientType RecipientType { get; init; }
}
