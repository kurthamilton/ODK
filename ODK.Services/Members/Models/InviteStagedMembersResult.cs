using ODK.Core.Utils;

namespace ODK.Services.Members.Models;

/// <summary>
/// What inviting a group's held addresses did: how many people it asked in, what kind of account each of
/// them has, and whether the emails went out or are being held for publication.
/// </summary>
/// <remarks>
/// A named result rather than a <see cref="ServiceResult{T}"/> of counts, because a tuple of five integers
/// says nothing about which is which at the use site - the same reason
/// <see cref="StageMemberImportResult"/> next to it is one.
/// <para>
/// The upload reports what a file contained and this reports what the group did, which is the only split
/// that can work: the addresses a file classified as already in the group are counted and discarded, since
/// there is nothing to do about them, so no later reader can recover them.
/// </para>
/// </remarks>
public class InviteStagedMembersResult : ServiceResult
{
    private InviteStagedMembersResult(bool success, string? message = null)
        : base(success, message)
    {
    }

    /// <summary>Held addresses skipped because the group had already invited them.</summary>
    public int AlreadyInvited { get; private init; }

    /// <summary>Held addresses skipped because they belong to members the group already has.</summary>
    public int AlreadyInGroup { get; private init; }

    /// <summary>People invited who already had an account here.</summary>
    public int ExistingAccounts { get; private init; }

    /// <summary>
    /// Whether the invites are being held rather than emailed, which is what an unpublished group does with
    /// them - the link an invite carries would land on a group nobody outside it can see.
    /// </summary>
    public bool Held { get; private init; }

    /// <summary>Held addresses that cannot be emailed as they stand, so nothing was raised for them.</summary>
    public int Invalid { get; private init; }

    /// <summary>How many people the group asked in.</summary>
    public int Invited => ExistingAccounts + NewAccounts;

    /// <summary>People invited who had no account here, so one was raised for them to activate.</summary>
    public int NewAccounts { get; private init; }

    public new static InviteStagedMembersResult Failure(string message) => new(false, message);

    /// <summary>
    /// The held addresses were acted on. <paramref name="statuses"/> is what every held row classified as,
    /// and <paramref name="invited"/> the statuses of the rows that actually produced an invite.
    /// </summary>
    /// <remarks>
    /// Two lists rather than one, because classifying as importable is not the same as having been
    /// invited: a guard can still refuse a row the capacity check let through. What the group did is
    /// counted from the invites that were raised, and what it skipped from the classification.
    /// </remarks>
    public static InviteStagedMembersResult Recorded(
        IReadOnlyCollection<MemberImportRowStatus> statuses,
        IReadOnlyCollection<MemberImportRowStatus> invited,
        bool held)
    {
        var existingAccounts = invited.Count(x => x == MemberImportRowStatus.ExistingNotInGroup);
        var newAccounts = invited.Count(x => x == MemberImportRowStatus.New);

        return new InviteStagedMembersResult(true, Describe(existingAccounts + newAccounts, newAccounts, held))
        {
            AlreadyInGroup = statuses.Count(x => x == MemberImportRowStatus.ExistingInGroup),
            AlreadyInvited = statuses.Count(x => x == MemberImportRowStatus.AlreadyInvited),
            ExistingAccounts = existingAccounts,
            Held = held,
            Invalid = statuses.Count(x => x == MemberImportRowStatus.Invalid),
            NewAccounts = newAccounts
        };
    }

    /// <summary>
    /// One or two lines, because it is read as a toast. The second line is whichever fact the admin cannot
    /// work out from the page they are about to land on: that nothing has been emailed yet, or how many of
    /// the people invited have an account to activate first.
    /// </summary>
    private static string Describe(int invited, int newAccounts, bool held)
    {
        if (invited == 0)
        {
            return "Nobody was invited";
        }

        var asked = $"{invited} {StringUtils.Pluralise(invited, "person", "people")} invited.";

        if (held)
        {
            return $"{asked} Nobody has been emailed yet - publish the group to send the invites.";
        }

        return newAccounts > 0
            ? $"{asked} {newAccounts} will need to activate an account."
            : asked;
    }
}
