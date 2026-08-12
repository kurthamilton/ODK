namespace ODK.Services.Emails.Models;

public class ChapterEmailSettingsUpdateModel
{
    /// <summary>
    /// Null where the group has not set one, which is how it inherits the site's. The form posts an empty
    /// box rather than nothing, so the service is what turns blank into unset.
    /// </summary>
    public required string? AdminTitle { get; init; }

    /// <inheritdoc cref="AdminTitle" />
    public required string? MemberTitle { get; init; }
}
