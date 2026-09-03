using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Referrals;
using ODK.Core.Platforms;
using ODK.Core.Web;
using ODK.Services;
using ODK.Services.Tasks;

namespace ODK.Web.Common.Services;

public interface IRequestStore
{
    Chapter Chapter { get; }

    /// <summary>
    /// The referral campaign a member could send a referral for, loaded with the current member so the
    /// account menu can decide whether to offer it without a round trip of its own. Null when nobody is
    /// signed in, on DrunkenKnitwits, or when no campaign is running.
    /// </summary>
    ReferralCampaign? ActiveReferralCampaign { get; }

    Chapter? ChapterOrDefault { get; }

    IChapterServiceRequest ChapterServiceRequest { get; }

    Member CurrentMember { get; }

    Member? CurrentMemberOrDefault { get; }

    bool Loaded { get; }

    IMemberChapterServiceRequest MemberChapterServiceRequest { get; }

    IMemberServiceRequest MemberServiceRequest { get; }

    PlatformType Platform { get; }

    IServiceRequest ServiceRequest { get; }

    /// <summary>
    /// The members signed in on the same auth cookie, oldest sign-in first, loaded with the current member
    /// so the account menu can offer a switch without a round trip of its own. Empty unless more than one
    /// is signed in, which only a site admin switching accounts ever arranges.
    /// </summary>
    IReadOnlyCollection<Member> SignedInMembers { get; }

    Task<ChapterAdminMember?> GetCurrentChapterAdminMember();

    /// <summary>
    /// Loads from a request, deriving the platform from its URL and the chapter from its route values.
    /// <paramref name="signedInMemberIds"/> is every member the auth cookie holds, which is the current
    /// member alone for all but a site admin switching accounts.
    /// </summary>
    Task<IRequestStore> Load(
        IHttpRequestContext context,
        Guid? currentMemberIdOrDefault,
        IReadOnlyCollection<Guid> signedInMemberIds);

    /// <summary>
    /// Loads from values already resolved, for a background job. A job has no URL to derive a platform from
    /// and no route values to find a chapter in, and re-deriving either would let routing configuration
    /// change what a queued job means while it waits.
    /// </summary>
    Task<IRequestStore> Load(JobRequest request);

    void Reset();
}