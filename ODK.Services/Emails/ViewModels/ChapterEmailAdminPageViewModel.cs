using ODK.Core.Emails;

namespace ODK.Services.Emails.ViewModels;

public class ChapterEmailAdminPageViewModel
{
    /// <summary>
    /// Whether the group's subscription includes custom emails. An existing override keeps being sent
    /// either way; this only decides whether it can be changed.
    /// </summary>
    public required bool CanEdit { get; init; }

    public required ChapterEmail Email { get; init; }

    /// <summary>
    /// Who the email is written for, read from the site's row - an override changes the wording and not the
    /// audience.
    /// </summary>
    public required EmailRecipientType RecipientType { get; init; }

    /// <summary>
    /// The title this email resolves <c>{title}</c> to, given its audience and whatever the group has set.
    /// Still a template, so it may itself contain parameters.
    /// </summary>
    public required string Title { get; init; }
}
