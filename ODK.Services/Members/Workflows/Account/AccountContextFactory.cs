using ODK.Core.Cryptography;
using ODK.Core.Members;
using ODK.Core.Referrals;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Services.Authentication.OAuth;
using ODK.Services.Members.Models;

namespace ODK.Services.Members.Workflows.Account;

public sealed class AccountContextFactory : IAccountContextFactory
{
    private readonly IOAuthProviderFactory _oauthProviderFactory;
    private readonly IUnitOfWork _unitOfWork;

    public AccountContextFactory(IUnitOfWork unitOfWork, IOAuthProviderFactory oauthProviderFactory)
    {
        _oauthProviderFactory = oauthProviderFactory;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// One row of an import. Synchronous and issues no query, because an import loads what the whole file needs
    /// once - a factory that queried here would turn a file of a thousand rows into a thousand round-trips.
    /// </summary>
    public AccountContext CreateForImport(
        IChapterServiceRequest request,
        MemberImportModel import,
        ImportBatch batch) => new()
    {
        ActivationToken = TokenGenerator.GenerateBase64Token(64),
        Chapter = request.Chapter,
        ChapterLocation = batch.ChapterLocation,
        Country = batch.Country,
        Currency = batch.Currency,
        Import = import,
        Member = batch.ExistingMember(import.EmailAddress),
        Request = request,
        SiteSubscription = batch.SiteSubscription,
        VerifiedByOAuth = false
    };

    public async Task<AccountContext> CreateForSiteSignUp(
        IServiceRequest request,
        AccountCreateModel profile)
    {
        var (existing, siteSubscription, topics, referral) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetByEmailAddress(profile.EmailAddress),
            x => x.SiteSubscriptionRepository.GetDefault(request.Platform),
            x => x.TopicRepository.GetByIds(profile.TopicIds),
            x => profile.ReferralId != null
                ? x.ReferralRepository.GetByIdOrDefault(profile.ReferralId.Value)
                : new DefaultDeferredQuerySingleOrDefault<Referral>());

        var (reusableActivationToken, carriedOverInvites) = await ReadDiscardedAccount(existing);

        /* Resolved here rather than in a step because it decides which edge is taken, and a guard reads only
           what is already on the context. It costs a call to the provider, so it is only made when the sign-up
           actually carries a token.  */
        var verifiedByOAuth = await IsVerifiedByOAuth(profile);

        return new AccountContext
        {
            ActivationToken = reusableActivationToken ?? TokenGenerator.GenerateBase64Token(64),
            CarriedOverInvites = carriedOverInvites,
            Member = existing,
            Referral = referral,
            Request = request,
            SiteProfile = profile,
            SiteSubscription = siteSubscription,
            Topics = topics,
            VerifiedByOAuth = verifiedByOAuth
        };
    }

    public async Task<AccountContext> CreateForGroupSignUp(
        IChapterServiceRequest request,
        MemberCreateProfile profile)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var (
            chapterProperties,
            membershipSettings,
            existing,
            siteSubscription,
            ownerSubscription,
            chapterLocation,
            memberCount
        ) = await _unitOfWork.RunAsync(
            x => x.ChapterPropertyRepository.GetByChapterId(chapter.Id),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id),
            x => x.MemberRepository.GetByEmailAddress(profile.EmailAddress),
            x => x.SiteSubscriptionRepository.GetDefault(platform),
            x => x.MemberSiteSubscriptionRecordRepository
                .Query(x => x.Current().ForChapterOwner(chapter.Id).Active())
                .SiteSubscription()
                .WithFeatures()
                .GetSingleOrDefault(),
            x => x.ChapterLocationRepository.GetByChapterId(chapter.Id),
            x => x.MemberRepository.GetCountByChapterId(chapter.Id));

        var (reusableActivationToken, carriedOverInvites) = await ReadDiscardedAccount(existing);

        return new AccountContext
        {
            ActivationToken = reusableActivationToken ?? TokenGenerator.GenerateBase64Token(64),
            CarriedOverInvites = carriedOverInvites,
            Chapter = chapter,
            ChapterLocation = chapterLocation,
            ChapterProperties = chapterProperties,
            Member = existing,
            MembershipSettings = membershipSettings,
            MemberCount = memberCount,
            OwnerSubscription = ownerSubscription?.SiteSubscription,
            OwnerSubscriptionFeatures = ownerSubscription?.Features ?? [],
            Profile = profile,
            Request = request,
            SiteSubscription = siteSubscription,
            VerifiedByOAuth = false
        };
    }

    /// <summary>
    /// What an account that has never been activated holds, read before a sign-up discards and recreates it:
    /// its activation token, so a link already emailed still works, and its invitations, which are an admin's
    /// record that the member was asked to join and are what lets them skip approval. The delete would cascade
    /// both away.
    /// </summary>
    private async Task<(string? ActivationToken, IReadOnlyCollection<MemberChapterInvite> Invites)>
        ReadDiscardedAccount(Member? existing)
    {
        if (existing is not { Activated: false })
        {
            return (null, []);
        }

        var (token, invites) = await _unitOfWork.RunAsync(
            x => x.MemberActivationTokenRepository.GetByMemberId(existing.Id),
            x => x.MemberChapterInviteRepository.GetByMemberId(existing.Id));

        return (token?.ActivationToken, invites);
    }

    /// <summary>
    /// Whether an OAuth provider confirms the address being registered belongs to the signer-up. A token for a
    /// different address proves nothing about this one, so the addresses have to match.
    /// </summary>
    private async Task<bool> IsVerifiedByOAuth(AccountCreateModel profile)
    {
        if (profile.OAuthProviderType == null || string.IsNullOrEmpty(profile.OAuthToken))
        {
            return false;
        }

        var provider = _oauthProviderFactory.GetProvider(profile.OAuthProviderType.Value);
        var user = await provider.GetUser(profile.OAuthToken);

        return string.Equals(user.Email, profile.EmailAddress, StringComparison.InvariantCultureIgnoreCase);
    }
}
