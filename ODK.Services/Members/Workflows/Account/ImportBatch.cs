using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Core.Subscriptions;

namespace ODK.Services.Members.Workflows.Account;

/// <summary>
/// What a whole import needs, read once. Every row's context is a projection of this, so importing a file
/// costs the same number of queries whether it holds one row or a thousand.
/// </summary>
public sealed class ImportBatch
{
    public required IReadOnlyCollection<MemberChapterInvite> OutstandingInvites { get; init; }

    public ChapterLocation? ChapterLocation { get; init; }

    public Country? Country { get; init; }

    public Currency? Currency { get; init; }

    public required IReadOnlyCollection<Member> ExistingMembers { get; init; }

    public required SiteSubscription SiteSubscription { get; init; }

    /// <summary>The account already registered against an address in the file, where there is one.</summary>
    public Member? ExistingMember(string emailAddress) => ExistingMembers
        .FirstOrDefault(x => string.Equals(x.EmailAddress, emailAddress, StringComparison.OrdinalIgnoreCase));

    /// <summary>The invitation the group already has outstanding for a member, where there is one.</summary>
    public MemberChapterInvite? OutstandingInvite(Guid memberId) => OutstandingInvites
        .FirstOrDefault(x => x.MemberId == memberId);
}
