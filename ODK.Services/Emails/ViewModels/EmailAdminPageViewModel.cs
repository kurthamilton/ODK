using ODK.Core.Emails;

namespace ODK.Services.Emails.ViewModels;

public class EmailAdminPageViewModel
{
    public required Email Email { get; init; }

    /// <summary>
    /// The title this email resolves <c>{title}</c> to, given its audience. The site's, since a site admin
    /// is editing the template every group starts from. Still a template, so it may itself contain
    /// parameters.
    /// </summary>
    public required string Title { get; init; }
}
