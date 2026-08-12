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
}
