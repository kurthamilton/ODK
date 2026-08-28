using ODK.Core.Members;
using ODK.Core.Workflows;
using ODK.Data.Core;
using ODK.Data.Core.Members;
using ODK.Services.Chapters.ViewModels;
using ODK.Services.Chapters.Workflows;
using ODK.Services.Exceptions;
using ODK.Services.Subscriptions;
using ODK.Services.Workflows;

namespace ODK.Services.Chapters;

public class ChapterSiteAdminService : OdkAdminServiceBase, IChapterSiteAdminService
{
    private readonly StateMachineRunner<
        ChapterPublicationState, ChapterPublicationTrigger, ChapterPublicationContext> _chapterPublicationWorkflow;
    private readonly IMemberSiteSubscriptionWriter _memberSiteSubscriptionWriter;
    private readonly IUnitOfWork _unitOfWork;

    public ChapterSiteAdminService(
        IUnitOfWork unitOfWork,
        IMemberSiteSubscriptionWriter memberSiteSubscriptionWriter,
        StateMachineRunner<ChapterPublicationState, ChapterPublicationTrigger, ChapterPublicationContext>
            chapterPublicationWorkflow)
        : base(unitOfWork)
    {
        _chapterPublicationWorkflow = chapterPublicationWorkflow;
        _memberSiteSubscriptionWriter = memberSiteSubscriptionWriter;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> ApproveChapter(IMemberServiceRequest request, Guid chapterId)
    {
        var platform = request.Platform;

        /* Loaded here rather than by a context factory: the securable is enforced by the wrapper that does the
           loading, so a factory would have to sit inside it. The service loads and maps, as the member import
           does. */
        var (chapter, owner) = await GetSiteAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(platform, chapterId),
            x => x.MemberRepository.GetChapterOwner(chapterId));

        /* Approving a group that is already approved is a legal no-op rather than a failure, so there is no
           check for it here - the machine has an Approve edge out of every state and only the one out of Draft
           does any work. */
        var result = await _chapterPublicationWorkflow.Fire(
            ChapterPublicationTrigger.Approve,
            new ChapterPublicationContext
            {
                Chapter = chapter,
                Owner = owner,
                Request = request
            });

        return result.ToServiceResult();
    }

    public async Task<ServiceResult> DeleteChapter(IMemberServiceRequest request, Guid chapterId)
    {
        var platform = request.Platform;

        var chapter = await GetSiteAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(platform, chapterId));

        _unitOfWork.ChapterRepository.Delete(chapter);
        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    public async Task<ChapterAdminMembersSiteAdminPageViewModel> GetChapterAdminMembersViewModel(
        IMemberChapterServiceRequest request)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var adminMembers = await GetSiteAdminRestrictedContent(request,
            x => x.ChapterAdminMemberRepository.GetByChapterId(platform, chapter.Id));

        return new ChapterAdminMembersSiteAdminPageViewModel
        {
            AdminMembers = adminMembers
                .OrderBy(x => x.Member.FullName)
                .ToArray(),
            Chapter = chapter
        };
    }

    public async Task<ChapterPaymentSettingsAdminPageViewModel> GetChapterPaymentSettingsViewModel(
        IMemberChapterServiceRequest request)
    {
        var chapter = request.Chapter;

        var (paymentSettings, currencies) = await GetSiteAdminRestrictedContent(request,
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapter.Id),
            x => x.CurrencyRepository.GetAllDtos());

        return new ChapterPaymentSettingsAdminPageViewModel
        {
            Chapter = chapter,
            Currencies = currencies,
            PaymentSettings = paymentSettings
        };
    }

    public async Task<ChapterSubscriptionsAdminPageViewModel> GetChapterSubscriptionsViewModel(
        IMemberChapterServiceRequest request)
    {
        var chapter = request.Chapter;

        /* Disabled subscriptions included, and nothing filtered by payment settings: a site admin is here
           precisely to see what a group admin cannot. The group's own page drops any subscription whose
           settings are missing or disabled, so from there it is indistinguishable from one that was never
           created. */
        var (subscriptions, sitePaymentSettings) = await GetSiteAdminRestrictedContent(request,
            x => x.ChapterSubscriptionRepository.GetAdminDtosByChapterId(chapter.Id, includeDisabled: true),
            x => x.SitePaymentSettingsRepository.GetAll());

        return new ChapterSubscriptionsAdminPageViewModel
        {
            Chapter = chapter,
            Subscriptions = subscriptions
                .Select(x => x.ChapterSubscription)
                .OrderBy(x => x.Name)
                .Select(x => new ChapterSubscriptionSiteAdminViewModel
                {
                    ChapterSubscription = x,
                    SitePaymentSettings = sitePaymentSettings
                        .FirstOrDefault(setting => setting.Id == x.SitePaymentSettingId),
                    VisibleToGroupAdmins = x.IsVisibleToAdmins(sitePaymentSettings)
                })
                .ToArray()
        };
    }

    public async Task<SiteAdminChaptersViewModel> GetSiteAdminChaptersViewModel(IMemberServiceRequest request)
    {
        var platform = request.Platform;

        var (chapters, subscriptions) = await GetSiteAdminRestrictedContent(request,
            x => x.ChapterRepository.GetAll(platform, includeUnpublished: true),
            x => x.MemberSiteSubscriptionRecordRepository.GetAllChapterOwnerSubscriptionDtos(platform));

        var subscriptionDictionary = subscriptions
            .GroupBy(x => x.MemberSiteSubscription.MemberId)
            .ToDictionary(x => x.Key, x => x.ToArray());

        var approved = new List<SiteAdminChaptersRowViewModel>();
        var pending = new List<SiteAdminChaptersRowViewModel>();

        foreach (var chapter in chapters)
        {
            MemberSiteSubscriptionDto? chapterSubscriptionDto = null;
            if (subscriptionDictionary.TryGetValue(chapter.OwnerId, out var memberSubscriptions))
            {
                chapterSubscriptionDto = memberSubscriptions
                    .OrderByDescending(x => x.MemberSiteSubscription.ExpiresUtc ?? DateTime.MaxValue)
                    .FirstOrDefault();
            }

            var rowViewModel = new SiteAdminChaptersRowViewModel
            {
                Chapter = chapter,
                SiteSubscriptionExpiresUtc = chapterSubscriptionDto?.MemberSiteSubscription.ExpiresUtc,
                SiteSubscriptionName = chapterSubscriptionDto?.SiteSubscription.Name
            };

            if (chapter.Approved())
            {
                approved.Add(rowViewModel);
            }
            else
            {
                pending.Add(rowViewModel);
            }
        }

        return new SiteAdminChaptersViewModel
        {
            Approved = approved
                .OrderBy(x => x.Chapter.Name)
                .ToArray(),
            Pending = pending
                .OrderBy(x => x.Chapter.CreatedUtc)
                .ToArray(),
            Platform = request.Platform
        };
    }

    public async Task<SiteAdminChapterViewModel> GetSiteAdminChapterViewModel(
        IMemberChapterServiceRequest request)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var (subscription, siteSubscriptions, prices, sitePaymentSettings) = await GetSiteAdminRestrictedContent(request,
            x => x.MemberSiteSubscriptionRecordRepository.Query().Current().ForChapterOwner(chapter.Id).ToState().GetSingleOrDefault(),
            x => x.SiteSubscriptionRepository.GetAll(platform),
            x => x.SiteSubscriptionPriceRepository.GetAll(platform),
            x => x.SitePaymentSettingsRepository.GetAll());

        var sitePaymentSettingsDictionary = sitePaymentSettings.ToDictionary(x => x.Id);

        return new SiteAdminChapterViewModel
        {
            Chapter = chapter,
            Platform = platform,
            SitePaymentSettings = sitePaymentSettingsDictionary,
            /* The subscriptions an owner can be put on, plus whichever they are on already - a plan that has
               since stopped being usable still has to appear, or saving the form would move them off it. */
            SiteSubscriptions = siteSubscriptions
                .Where(x => x.IsActive(
                        prices.Where(price => price.SiteSubscriptionId == x.Id),
                        sitePaymentSettingsDictionary[x.SitePaymentSettingId]) ||
                    subscription?.SiteSubscriptionId == x.Id)
                .ToArray(),
            Subscription = subscription
        };
    }

    public async Task<ServiceResult> UpdateSiteAdminChapter(
        IMemberChapterServiceRequest request,
        SiteAdminChapterUpdateViewModel viewModel)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        if (viewModel.SiteSubscriptionId == null)
        {
            throw new OdkServiceException($"Error updating group '{chapter.Id}': subscription not provided");
        }

        var (currentRecord, siteSubscriptions, prices, sitePaymentSettings) = await GetSiteAdminRestrictedContent(request,
            x => x.MemberSiteSubscriptionRecordRepository.Query().Current().ForMember(chapter.OwnerId).GetSingleOrDefault(),
            x => x.SiteSubscriptionRepository.GetAll(platform),
            x => x.SiteSubscriptionPriceRepository.GetAll(platform),
            x => x.SitePaymentSettingsRepository.GetAll());

        var siteSubscription = siteSubscriptions
            .FirstOrDefault(x => x.Id == viewModel.SiteSubscriptionId.Value);
        if (siteSubscription == null)
        {
            return ServiceResult.Failure("Subscription not found");
        }

        // Staying on the subscription they are already on is always allowed, however it stands now.
        var staysOnCurrentSubscription = siteSubscription.Id == currentRecord?.SiteSubscriptionId;
        if (!staysOnCurrentSubscription &&
            !siteSubscription.IsActive(
                prices.Where(x => x.SiteSubscriptionId == siteSubscription.Id),
                sitePaymentSettings.Single(x => x.Id == siteSubscription.SitePaymentSettingId)))
        {
            return ServiceResult.Failure("Subscription is not available");
        }

        /* A free subscription never expires, so it takes no expiry however the form was filled in. The price
           and external id belong to the subscription that was bought, so they are left behind when the plan
           changes - carrying them onto another plan would report a payment against it that was never made. */
        _memberSiteSubscriptionWriter.MakeRecordCurrent(
            newRecord: new MemberSiteSubscriptionRecord
            {
                CreatedUtc = DateTime.UtcNow,
                ExpiresUtc = siteSubscription.Free ? null : viewModel.SubscriptionExpiresUtc,
                ExternalId = staysOnCurrentSubscription ? currentRecord?.ExternalId : null,
                MemberId = chapter.OwnerId,
                SiteSubscriptionId = siteSubscription.Id,
                SiteSubscriptionPriceId = staysOnCurrentSubscription ? currentRecord?.SiteSubscriptionPriceId : null
            },
            existingCurrent: currentRecord);

        await _unitOfWork.SaveChanges();
        return ServiceResult.Successful();
    }
}