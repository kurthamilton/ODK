namespace ODK.Services.Chapters.ViewModels;

/// <summary>
/// Wording an organiser can paste wherever their old community still is. Four destinations, because the
/// same sentence does not work as a search result, a post, an email and a chat message.
/// </summary>
/// <remarks>
/// <para>
/// Plain <see langword="string"/>, deliberately - no <c>Html</c> suffix, because every destination is a
/// plain-text field and a copy button that put markup on the clipboard would paste tags. This is the one
/// place where the absence of the suffix is the statement.
/// </para>
/// <para>
/// Composed here rather than in a view: two platforms' admin pages render it, and a view that built the
/// wording is a view the other one has to build again.
/// </para>
/// </remarks>
public class ChapterMigrationShareViewModel
{
    /// <summary>A few lines for a post or announcement on the platform the group came from.</summary>
    public required string Announcement { get; init; }

    /// <summary>
    /// The longest of the four, and the only one that mentions the invitation - the people who receive it
    /// are the ones an import has already invited.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// One paragraph for the top of the old group's description. Leads with the address, because a
    /// description is read as a search result and may never be clicked.
    /// </summary>
    public required string OldGroupDescription { get; init; }

    /// <summary>Two lines for a chat or a group message, short enough to survive a link preview.</summary>
    public required string ShortMessage { get; init; }

    /// <summary>
    /// <paramref name="previousPlatformName"/> is what the organiser called the place they came from, and
    /// is usually unset - the wording reads generically without it, because the answer is different for
    /// every group and some of them have no single name for it.
    /// </summary>
    public static ChapterMigrationShareViewModel Create(
        string groupName,
        string groupUrl,
        string movedPageUrl,
        string? previousPlatformName)
    {
        var from = !string.IsNullOrWhiteSpace(previousPlatformName)
            ? $" from {previousPlatformName}"
            : string.Empty;

        return new ChapterMigrationShareViewModel
        {
            OldGroupDescription =
                $"{groupName} has moved. You will find us at {movedPageUrl} - events, members and " +
                "everything else are all there now. This page is no longer updated.",

            Announcement =
                $"""
                {groupName} has moved.

                Our new home is {movedPageUrl}. Events, RSVPs and everything else happen there from now on.

                We are leaving this group where it is so nobody loses the thread, but it will not be
                updated. Come and find us at the new address.
                """,

            Email =
                $"""
                Hello,

                {groupName} has moved{from}. You can find us at {groupUrl}.

                If you were on our member list, there is an invitation waiting in your inbox - follow the
                link in it and your membership is already set up, with nothing to fill in. If you cannot
                find it, you can join at the address above instead.

                There is more about the move at {movedPageUrl}.

                See you there.
                """,

            ShortMessage =
                $"{groupName} has moved{from} - we are at {movedPageUrl} now. Come and join us."
        };
    }
}
