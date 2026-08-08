using System;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Referrals;
using ODK.Core.Platforms;
using ODK.Core.Web;
using ODK.Services;

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

    Task<ChapterAdminMember?> GetCurrentChapterAdminMember();

    Task<IRequestStore> Load(IHttpRequestContext context, Guid? currentMemberIdOrDefault);

    void Reset();
}