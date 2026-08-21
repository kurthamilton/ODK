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

    public async Task<ChapterPaymentSettingsAdminPageViewModel> GetChapterPaymentSettingsViewModel(
        IMemberChapterServiceRequest request)
    {
        var chapter = request.Chapter;

        var (paymentSettings, currencies) = await GetSiteAdminRestrictedContent(request,
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapter.Id),
            x => x.CurrencyRepository.GetAllDtos());

        return new ChapterPaymentSettingsAdminPageViewModel
        {
            Currencies = currencies,
            PaymentSettings = paymentSettings
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

        var (subscription, siteSubscriptions, sitePaymentSettings) = await GetSiteAdminRestrictedContent(request,
            x => x.MemberSiteSubscriptionRecordRepository.Query().Current().ForChapterOwner(chapter.Id).ToState().GetSingleOrDefault(),
            x => x.SiteSubscriptionRepository.GetAll(platform),
            x => x.SitePaymentSettingsRepository.GetAll());

        return new SiteAdminChapterViewModel
        {
            Chapter = chapter,
            Platform = platform,
            SitePaymentSettings = sitePaymentSettings.ToDictionary(x => x.Id),
            SiteSubscriptions = siteSubscriptions
                .Where(x => x.Enabled || subscription?.SiteSubscriptionId == x.Id)
                .ToArray(),
            Subscription = subscription
        };
    }

    public async Task<ServiceResult> UpdateSiteAdminChapter(
        IMemberChapterServiceRequest request,
        SiteAdminChapterUpdateViewModel viewModel)
    {
        var chapter = request.Chapter;

        var currentRecord = await GetSiteAdminRestrictedContent(request,
            x => x.MemberSiteSubscriptionRecordRepository.Query().Current().ForMember(chapter.OwnerId).GetSingleOrDefault());

        if (viewModel.SiteSubscriptionId == null)
        {
            throw new OdkServiceException($"Error updating group '{chapter.Id}': subscription not provided");
        }

        // Only the expiry is edited here; the plan/price/external id carry over from the current record
        // (or default for a member who has none yet).
        _memberSiteSubscriptionWriter.MakeRecordCurrent(
            newRecord: new MemberSiteSubscriptionRecord
            {
                CreatedUtc = DateTime.UtcNow,
                ExpiresUtc = viewModel.SubscriptionExpiresUtc,
                ExternalId = currentRecord?.ExternalId,
                MemberId = chapter.OwnerId,
                SiteSubscriptionId = currentRecord?.SiteSubscriptionId ?? viewModel.SiteSubscriptionId.Value,
                SiteSubscriptionPriceId = currentRecord?.SiteSubscriptionPriceId
            },
            existingCurrent: currentRecord);

        await _unitOfWork.SaveChanges();
        return ServiceResult.Successful();
    }
}