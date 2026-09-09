namespace ODK.Services.Members.Models;

/// <summary>
/// The outcome of an upload: how many of its addresses the group is now holding, and how many it had
/// nothing to do about.
/// </summary>
/// <remarks>
/// A named result rather than a <see cref="ServiceResult{T}"/> of counts, because a tuple of five integers
/// says nothing about which is which at the use site.
/// <para>
/// <see cref="Staged"/>, <see cref="Updated"/>, <see cref="AlreadyInGroup"/> and
/// <see cref="AlreadyInvited"/> partition the file's distinct addresses. <see cref="Invalid"/> is a subset
/// of the first two - an unusable address is held, so the group can see it and correct it - so the five do
/// not sum to the file.
/// </para>
/// </remarks>
public class StageMemberImportResult : ServiceResult
{
    private StageMemberImportResult(bool success, string? message = null)
        : base(success, message)
    {
    }

    /// <summary>Addresses skipped because they belong to members the group already has.</summary>
    public int AlreadyInGroup { get; private init; }

    /// <summary>
    /// Addresses skipped because the group already has an invite outstanding for them. The outstanding
    /// invite is the ask, so there is nothing to hold.
    /// </summary>
    public int AlreadyInvited { get; private init; }

    /// <summary>Held addresses that cannot be emailed as they stand.</summary>
    public int Invalid { get; private init; }

    /// <summary>Addresses the group was not already holding.</summary>
    public int Staged { get; private init; }

    /// <summary>Addresses the group was already holding, whose details this upload refreshed.</summary>
    public int Updated { get; private init; }

    public new static StageMemberImportResult Failure(string message) => new(false, message);

    /// <summary>Carries a failure raised elsewhere - reading the file, validation - through unchanged.</summary>
    public static StageMemberImportResult FromResult(ServiceResult result) => new(result.Success, result.Message);

    /// <summary>
    /// The upload was read and acted on. <paramref name="statuses"/> is what each distinct address in it
    /// classified as, and <paramref name="updated"/> how many of the held ones already had a row - so the
    /// counts are derived from one pass rather than tallied separately and hoped to agree.
    /// </summary>
    public static StageMemberImportResult Recorded(
        IReadOnlyCollection<MemberImportRowStatus> statuses,
        int updated)
    {
        var alreadyInGroup = statuses.Count(x => x == MemberImportRowStatus.ExistingInGroup);
        var alreadyInvited = statuses.Count(x => x == MemberImportRowStatus.AlreadyInvited);

        return new StageMemberImportResult(true)
        {
            AlreadyInGroup = alreadyInGroup,
            AlreadyInvited = alreadyInvited,
            Invalid = statuses.Count(x => x == MemberImportRowStatus.Invalid),
            Staged = statuses.Count - alreadyInGroup - alreadyInvited - updated,
            Updated = updated
        };
    }
}
