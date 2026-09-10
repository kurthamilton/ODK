using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Core.Platforms;
using ODK.Services.Emails.ViewModels;

namespace ODK.Web.Razor.Models.Admin.Chapters;

public class ChapterEmailContentViewModel
{
    /// <summary>
    /// Whether the group may write its own wording. Turning customisation off stays available either way.
    /// </summary>
    public required bool CanOverride { get; init; }

    public required Chapter Chapter { get; init; }

    /// <summary>
    /// The group's override. Either field may be unset, meaning that one inherits from
    /// <see cref="SiteEmail"/>.
    /// </summary>
    public required ChapterEmail Email { get; init; }

    /// <summary>The parameters this template may use, listed for reference below the form.</summary>
    public required IReadOnlyCollection<EmailParameterViewModel> Parameters { get; init; }

    public required PlatformType Platform { get; init; }

    public required EmailRecipientType RecipientType { get; init; }

    /// <summary>What each field the group has not overridden sends.</summary>
    public required Email SiteEmail { get; init; }
}
