using ODK.Core.Emails;

namespace ODK.Services.Emails.ViewModels;

public class ChapterEmailAdminPageViewModel
{
    /// <summary>
    /// Whether the group's subscription includes custom emails. Without it an existing override keeps being
    /// sent and can still be turned off - what it withholds is writing new wording.
    /// </summary>
    public required bool CanOverride { get; init; }

    /// <summary>
    /// The group's override. Its subject and body are set independently, and either may be unset - meaning
    /// the group has not overridden that field and the send uses the site's.
    /// </summary>
    public required ChapterEmail Email { get; init; }

    /// <summary>The parameters this template may use, as offered to a group, each with what it puts in the email.</summary>
    public required IReadOnlyCollection<EmailParameterViewModel> Parameters { get; init; }

    /// <summary>
    /// Who the email is written for, read from the site's row - an override changes the wording and not the
    /// audience.
    /// </summary>
    public required EmailRecipientType RecipientType { get; init; }

    /// <summary>The site's email, which is what each field the group has not overridden sends.</summary>
    public required Email SiteEmail { get; init; }

    /// <summary>
    /// The title this email resolves <c>{title}</c> to, given its audience and whatever the group has set.
    /// Still a template, so it may itself contain parameters.
    /// </summary>
    public required string Title { get; init; }
}
