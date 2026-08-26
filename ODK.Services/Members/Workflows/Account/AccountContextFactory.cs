using ODK.Core.Cryptography;
using ODK.Core.Members;
using ODK.Core.Notifications;
using ODK.Core.Referrals;
using ODK.Core.Subscriptions;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Services.Authentication.OAuth;
using ODK.Services.Members.Models;

namespace ODK.Services.Members.Workflows.Account;

public sealed class AccountContextFactory : IAccountContextFactory
{
    private readonly IOAuthProviderFactory _oauthProviderFactory;
    private readonly SiteSubscriptionCooldown _siteSubscriptionCooldown;
    private readonly IUnitOfWork _unitOfWork;

    public AccountContextFactory(
        IUnitOfWork unitOfWork,
        IOAuthProviderFactory oauthProviderFactory,
        SiteSubscriptionCooldown siteSubscriptionCooldown)
    {
        _oauthProviderFactory = oauthProviderFactory;
        _siteSubscriptionCooldown = siteSubscriptionCooldown;
        _unitOfWork = unitOfWork;
    }

    public async Task<AccountContext> CreateForAcceptInvite(
        IChapterServiceRequest request, MemberChapterInvite invite, InvitationAcceptModel model)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var (
            adminMembers,
            notificationSettings,
            member,
            memberPassword,
            pendingActivation,
            chapterProperties,
            membershipSettings,
            ownerSubscription,
            memberCount
        ) = await _unitOfWork.Run(
            x => x.ChapterAdminMemberRepository.GetByChapterId(platform, chapter.Id),
            x => x.MemberNotificationSettingsRepository.GetByChapterId(chapter.Id, NotificationType.NewMember),
            x => x.MemberRepository.GetById(invite.MemberId),
            x => x.MemberPasswordRepository.GetByMemberId(invite.MemberId),
            x => x.MemberActivationTokenRepository.GetByMemberId(invite.MemberId),
            x => x.ChapterPropertyRepository.GetByChapterId(chapter.Id),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id),
            x => x.MemberSiteSubscriptionRecordRepository
                .Query(x => x.Current().ForChapterOwner(chapter.Id).Active(_siteSubscriptionCooldown))
                .SiteSubscription()
                .WithFeatures()
                .GetSingleOrDefault(),
            x => x.MemberRepository.GetCountByChapterId(chapter.Id));

        return new AccountContext
        {
            AcceptedInvite = invite,
            AdminMembers = adminMembers,
            Chapter = chapter,
            ChapterProperties = chapterProperties,
            Invitation = model,
            Member = member,
            MemberCount = memberCount,
            MemberPassword = memberPassword,
            /* The answers as domain rows, for the email the group's admins get. The membership machine writes
               its own copies from the same submission - these are read, not written. */
            MemberProperties = model.Properties.Select(x => x.ToMemberProperty(invite.MemberId)).ToArray(),
            MembershipSettings = membershipSettings,
            NewPassword = model.Password,
            NotificationSettings = notificationSettings,
            OwnerSubscription = ownerSubscription?.SiteSubscription,
            OwnerSubscriptionFeatures = ownerSubscription?.Features ?? [],
            PendingActivation = pendingActivation,
            Request = request,
            /* Nothing here is a sign-up, so no provider has vouched for anything - holding the invitation is
               itself the proof the address was reachable. */
            VerifiedByOAuth = false
        };
    }

    public async Task<AccountContext> CreateForChapterActivation(
        IChapterServiceRequest request, MemberActivationToken token, string password)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var (adminMembers, notificationSettings, member, memberPassword, chapterProperties, memberProperties) =
            await _unitOfWork.Run(
                x => x.ChapterAdminMemberRepository.GetByChapterId(platform, chapter.Id),
                x => x.MemberNotificationSettingsRepository.GetByChapterId(chapter.Id, NotificationType.NewMember),
                x => x.MemberRepository.GetById(token.MemberId),
                x => x.MemberPasswordRepository.GetByMemberId(token.MemberId),
                x => x.ChapterPropertyRepository.GetByChapterId(chapter.Id),
                x => x.MemberPropertyRepository.GetByMemberId(token.MemberId, chapter.Id));

        return new AccountContext
        {
            AdminMembers = adminMembers,
            Chapter = chapter,
            ChapterProperties = chapterProperties,
            Member = member,
            MemberPassword = memberPassword,
            MemberProperties = memberProperties,
            NewPassword = password,
            NotificationSettings = notificationSettings,
            PendingActivation = token,
            Request = request,
            /* Nothing here is a sign-up, so no provider has vouched for anything - following the link is
               itself the proof the address was reachable. */
            VerifiedByOAuth = false
        };
    }

    public async Task<AccountContext> CreateForSiteActivation(
        IServiceRequest request, MemberActivationToken token, string password)
    {
        var (member, memberPassword) = await _unitOfWork.Run(
            x => x.MemberRepository.GetById(token.MemberId),
            x => x.MemberPasswordRepository.GetByMemberId(token.MemberId));

        return new AccountContext
        {
            /* No group, so no admins to notify and no answers to pass on: everything the group edge reads is
               left empty, and the absent chapter is what picks the edge that ignores it. */
            Member = member,
            MemberPassword = memberPassword,
            NewPassword = password,
            PendingActivation = token,
            Request = request,
            VerifiedByOAuth = false
        };
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
        var (existing, siteSubscription, topics, referral) = await _unitOfWork.Run(
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
        ) = await _unitOfWork.Run(
            x => x.ChapterPropertyRepository.GetByChapterId(chapter.Id),
            x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id),
            x => x.MemberRepository.GetByEmailAddress(profile.EmailAddress),
            x => x.SiteSubscriptionRepository.GetDefault(platform),
            x => x.MemberSiteSubscriptionRecordRepository
                .Query(x => x.Current().ForChapterOwner(chapter.Id).Active(_siteSubscriptionCooldown))
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

        var (token, invites) = await _unitOfWork.Run(
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
