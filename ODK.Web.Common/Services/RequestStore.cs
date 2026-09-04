using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Exceptions;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Referrals;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Services;
using ODK.Services.Exceptions;
using ODK.Services.Logging;
using ODK.Services.Members;
using ODK.Services.Platforms;
using ODK.Services.Tasks;
using ChapterServiceRequestImpl = ODK.Services.ChapterServiceRequest;
using MemberChapterServiceRequestImpl = ODK.Services.MemberChapterServiceRequest;
using MemberServiceRequestImpl = ODK.Services.MemberServiceRequest;

namespace ODK.Web.Common.Services;

/// <summary>
/// Only to be injected into UI classes, otherwise instances should be created
/// from the <see cref="RequestStoreFactory"/>
/// </summary>
public class RequestStore : IRequestStore
{
    private readonly IBackgroundTaskService _backgroundTaskService;
    private Chapter? _chapter;
    private ChapterAdminMember? _currentChapterAdminMember;
    private bool _currentChapterAdminMemberLoaded;
    private readonly ILoggingService _loggingService;
    private readonly IMemberLocaleService _memberLocaleService;
    private readonly IPlatformProvider _platformProvider;
    private IServiceRequest? _serviceRequest;
    private readonly RequestStoreSettings _settings;
    private readonly IUnitOfWork _unitOfWork;

    private readonly Lazy<IChapterServiceRequest> _chapterServiceRequest;
    private readonly Lazy<IMemberChapterServiceRequest> _memberChapterServiceRequest;
    private readonly Lazy<IMemberServiceRequest> _memberServiceRequest;

    public RequestStore(
        IUnitOfWork unitOfWork,
        ILoggingService loggingService,
        IPlatformProvider platformProvider,
        IBackgroundTaskService backgroundTaskService,
        IMemberLocaleService memberLocaleService,
        RequestStoreSettings settings)
    {
        _backgroundTaskService = backgroundTaskService;
        _loggingService = loggingService;
        _memberLocaleService = memberLocaleService;
        _platformProvider = platformProvider;
        _settings = settings;
        _unitOfWork = unitOfWork;

        _chapterServiceRequest = new(() => ChapterServiceRequestImpl.Create(Chapter, ServiceRequest));
        _memberChapterServiceRequest = new(() => MemberChapterServiceRequestImpl.Create(Chapter, MemberServiceRequest));
        _memberServiceRequest = new(() => MemberServiceRequestImpl.Create(CurrentMember, ServiceRequest));
    }

    public Chapter Chapter => _chapter ?? throw new OdkNotFoundException();

    public Chapter? ChapterOrDefault => _chapter;

    public IChapterServiceRequest ChapterServiceRequest => _chapterServiceRequest.Value;

    public Member CurrentMember => ServiceRequest.CurrentMemberOrDefault ?? throw new OdkNotAuthenticatedException();

    public ReferralCampaign? ActiveReferralCampaign { get; private set; }

    public Member? CurrentMemberOrDefault => ServiceRequest.CurrentMemberOrDefault;

    public bool Loaded { get; private set; }

    public IMemberChapterServiceRequest MemberChapterServiceRequest => _memberChapterServiceRequest.Value;

    public IMemberServiceRequest MemberServiceRequest => _memberServiceRequest.Value;

    public PlatformType Platform { get; private set; }

    public IServiceRequest ServiceRequest => _serviceRequest!;

    public IReadOnlyCollection<Member> SignedInMembers { get; private set; } = [];

    public async Task<ChapterAdminMember?> GetCurrentChapterAdminMember()
    {
        if (_currentChapterAdminMemberLoaded)
        {
            return _currentChapterAdminMember;
        }

        _currentChapterAdminMember = await _unitOfWork.ChapterAdminMemberRepository
            .GetByMemberId(Platform, CurrentMember.Id, Chapter.Id).Run();
        _currentChapterAdminMemberLoaded = true;
        return _currentChapterAdminMember;
    }

    /// <summary>
    /// Called from middleware and <see cref="RequestStoreFactory"/>
    /// </summary>
    public Task<IRequestStore> Load(
        IHttpRequestContext context,
        Guid? currentMemberIdOrDefault,
        IReadOnlyCollection<Guid> signedInMemberIds)
        => Load(context, currentMemberIdOrDefault, signedInMemberIds, verbose: false);

    public Task<IRequestStore> Load(JobRequest request) => Load(
        new JobHttpRequestContext { BaseUrl = request.BaseUrl },
        request.Platform,
        x => request.ChapterId != null
            ? x.ChapterRepository.GetByIdOrDefault(request.Platform, request.ChapterId.Value)
            : new DefaultDeferredQuerySingleOrDefault<Chapter>(),
        request.CurrentMemberId,
        // A job runs as one member; there is no cookie behind it to hold any others.
        []);

    public void Reset()
    {
        _chapter = null;
        _currentChapterAdminMember = null;
        _serviceRequest = null;
        Loaded = false;
        SignedInMembers = [];
    }

    private IDeferredQuerySingleOrDefault<Chapter> GetChapterQuery(
        IHttpRequestContext context,
        IUnitOfWork unitOfWork,
        bool verbose)
    {
        if (Platform == PlatformType.DrunkenKnitwits)
        {
            context.RouteValues.TryGetValue("chapterName", out var chapterName);

            if (!string.IsNullOrEmpty(chapterName))
            {
                chapterName = Chapter.GetFullName(PlatformType.DrunkenKnitwits, chapterName);

                if (verbose)
                {
                    _loggingService.Info($"RequestStore: getting chapter by name: '{chapterName}'");
                }

                return unitOfWork.ChapterRepository.GetByName(Platform, chapterName);
            }
        }
        else
        {
            context.RouteValues.TryGetValue("slug", out var slug);

            if (!string.IsNullOrEmpty(slug))
            {
                if (verbose)
                {
                    _loggingService.Info($"RequestStore: getting chapter by slug: '{slug}'");
                }

                return unitOfWork.ChapterRepository.GetBySlug(Platform, slug);
            }
        }

        var chapterId = context.RouteValues.TryGetValue("chapterId", out var chapterIdRouteValue) &&
            Guid.TryParse(chapterIdRouteValue, out Guid id)
                ? id
                : default(Guid?);

        if (chapterId != null)
        {
            if (verbose)
            {
                _loggingService.Info($"RequestStore: getting chapter by id: '{chapterId}'");
            }

            return _unitOfWork.ChapterRepository.GetByIdOrDefault(Platform, chapterId.Value);
        }

        if (verbose)
        {
            var message =
                "RequestStore: could not use the request URL to determine chapter";

            var properties = new Dictionary<string, string?>
            {
                { "Url", ServiceRequest.HttpRequestContext.RequestUrl }
            };

            foreach (var routeValue in ServiceRequest.HttpRequestContext.RouteValues)
            {
                properties.Add(routeValue.Key, routeValue.Value);
            }

            _loggingService.Warn(message, properties);
        }

        return new DefaultDeferredQuerySingleOrDefault<Chapter>();
    }

    private Task<IRequestStore> Load(
        IHttpRequestContext context,
        Guid? currentMemberIdOrDefault,
        IReadOnlyCollection<Guid> signedInMemberIds,
        bool verbose)
        => Load(
            context,
            _platformProvider.Platform,
            x => GetChapterQuery(context, x, verbose),
            currentMemberIdOrDefault,
            signedInMemberIds);

    private async Task<IRequestStore> Load(
        IHttpRequestContext context,
        PlatformType platform,
        Func<IUnitOfWork, IDeferredQuerySingleOrDefault<Chapter>> chapterQuery,
        Guid? currentMemberIdOrDefault,
        IReadOnlyCollection<Guid> signedInMemberIds)
    {
        if (Loaded)
        {
            return this;
        }

        // Set the platform directly to persist when resetting other state
        Platform = platform;

        var (chapter, currentMember, memberPreferences, activeReferralCampaign, signedInMembers) = await _unitOfWork.Run(
            chapterQuery,
            x => currentMemberIdOrDefault != null
                ? x.MemberRepository.GetByIdOrDefault(currentMemberIdOrDefault.Value)
                : new DefaultDeferredQuerySingleOrDefault<Member>(),
            x => currentMemberIdOrDefault != null
                ? x.MemberPreferencesRepository.GetByMemberIdOrDefault(currentMemberIdOrDefault.Value)
                : new DefaultDeferredQuerySingleOrDefault<MemberPreferences>(),
            // Only signed-in members on a platform that offers referrals can act on a campaign, so
            // everyone else costs no query at all.
            x => currentMemberIdOrDefault != null && Platform != PlatformType.DrunkenKnitwits
                ? x.ReferralCampaignRepository.GetMostRecentActive(DateTime.UtcNow)
                : new DefaultDeferredQuerySingleOrDefault<ReferralCampaign>(),
            // A single signed-in member is the current member, whom the query above already loads, so
            // only a cookie holding several costs a query here.
            x => signedInMemberIds.Count > 1
                ? x.MemberRepository.GetByIds(signedInMemberIds)
                : new DefaultDeferredQueryMultiple<Member>());

        ActiveReferralCampaign = activeReferralCampaign;

        // GetByIds answers in whatever order the database returns; the cookie's order is sign-in order,
        // which is what the account menu lists.
        SignedInMembers = signedInMemberIds
            .Select(x => signedInMembers.FirstOrDefault(y => y.Id == x))
            .Where(x => x != null)
            .Select(x => x!)
            .ToArray();

        _chapter = chapter;
        _serviceRequest = new ServiceRequest
        {
            CurrentMemberOrDefault = currentMember,
            Environment = _settings.Environment,
            HttpRequestContext = context,
            Platform = Platform
        };
        Loaded = true;

        RefreshMemberLocale(currentMember, memberPreferences, context.Locale);

        return this;
    }

    // If the member's stored locale differs from the request locale, persist the request locale in the
    // background (idempotent). The stored locale formats request-independent output (emails, notifications).
    private void RefreshMemberLocale(Member? currentMember, MemberPreferences? memberPreferences, string? requestLocale)
    {
        if (currentMember == null || requestLocale == null || memberPreferences?.Locale == requestLocale)
        {
            return;
        }

        var memberId = currentMember.Id;
        _backgroundTaskService.Enqueue(
            () => _memberLocaleService.UpdateLocale(memberId, requestLocale),
            BackgroundTaskQueueType.General);
    }
}