using ODK.Core.Chapters;
using ODK.Core.Utils;

namespace ODK.Core.Emails;

/// <summary>
/// Which wording an email refers to its group by.
/// </summary>
/// <remarks>
/// Shared by the send path and the admin pages that preview it, so what an admin is shown is what a member
/// or admin receives.
/// </remarks>
public static class EmailTitle
{
    /// <summary>
    /// The title template for an email written for <paramref name="recipientType"/>: the group's wording
    /// where it has set one and the site's otherwise. Blank reads as unset, so a group that has never
    /// filled its settings form in takes every title from the site.
    /// </summary>
    /// <returns>
    /// A template in its own right - it may refer to other parameters, so it resolves to a value only once
    /// those have been interpolated.
    /// </returns>
    public static string For(
        SiteEmailSettings siteSettings,
        ChapterEmailSettings? chapterEmailSettings,
        EmailRecipientType recipientType)
        => recipientType == EmailRecipientType.Admins
            ? StringUtils.Coalesce(chapterEmailSettings?.AdminTitle, siteSettings.AdminTitle)
            : StringUtils.Coalesce(chapterEmailSettings?.MemberTitle, siteSettings.MemberTitle);
}
