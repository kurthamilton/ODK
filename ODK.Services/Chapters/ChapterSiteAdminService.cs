using ODK.Core;
using ODK.Core.Members;
using ODK.Data.Core;
using ODK.Services.Chapters.ViewModels;
using ODK.Services.Exceptions;
using ODK.Services.Members;

namespace ODK.Services.Chapters;

public class ChapterSiteAdminService : OdkAdminServiceBase, IChapterSiteAdminService
{
    private readonly IMemberEmailService _memberEmailService;
    private readonly IUnitOfWork _unitOfWork;

    public ChapterSiteAdminService(
        IUnitOfWork unitOfWork,
        IMemberEmailService memberEmailService)
        : base(unitOfWork)
    {
        _memberEmailService = memberEmailService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> ApproveChapter(MemberServiceRequest request, Guid chapterId)
    {
        var platform = request.Platform;

        var (chapter, members) = await GetSiteAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(platform, chapterId),
            x => x.MemberRepository.GetAllByChapterId(chapterId));

        var owner = members.FirstOrDefault(x => x.Id == chapter.OwnerId);
        OdkAssertions.Exists(owner);

        if (chapter.Approved())
        {
            return ServiceResult.Successful();
        }

        chapter.ApprovedUtc = DateTime.UtcNow;

        _unitOfWork.ChapterRepository.Update(chapter);
        await _unitOfWork.SaveChangesAsync();

        await _memberEmailService.SendGroupApprovedEmail(
            request,
            chapter,
            owner);

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> DeleteChapter(MemberServiceRequest request, Guid chapterId)
    {
        var platform = request.Platform;

        var chapter = await GetSiteAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(platform, chapterId));

        _unitOfWork.ChapterRepository.Delete(chapter);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ChapterPaymentSettingsAdminPageViewModel> GetChapterPaymentSettingsViewModel(
        MemberServiceRequest request, Guid chapterId)
    {
        var (paymentSettings, currencies) = await GetSiteAdminRestrictedContent(request,
            x => x.ChapterPaymentSettingsRepository.GetByChapterId(chapterId),
            x => x.CurrencyRepository.GetAll());

        return new ChapterPaymentSettingsAdminPageViewModel
        {
            Currencies = currencies,
            PaymentSettings = paymentSettings
        };
    }

    public async Task<SiteAdminChaptersViewModel> GetSiteAdminChaptersViewModel(MemberServiceRequest request)
    {
        var platform = request.Platform;

        var (chapters, subscriptions) = await GetSiteAdminRestrictedContent(request,
            x => x.ChapterRepository.GetAll(platform),
            x => x.MemberSiteSubscriptionRepository.GetAllChapterOwnerSubscriptions(platform));

        var subscriptionDictionary = subscriptions
            .GroupBy(x => x.MemberId)
            .ToDictionary(x => x.Key, x => x.ToArray());

        var approved = new List<SiteAdminChaptersRowViewModel>();
        var pending = new List<SiteAdminChaptersRowViewModel>();

        foreach (var chapter in chapters)
        {
            MemberSiteSubscription? chapterSubscription = null;
            if (chapter.OwnerId != null &&
                subscriptionDictionary.TryGetValue(chapter.OwnerId.Value, out var memberSubscriptions))
            {
                chapterSubscription = memberSubscriptions
                    .OrderByDescending(x => x.ExpiresUtc ?? DateTime.MaxValue)
                    .FirstOrDefault();
            }

            var rowViewModel = new SiteAdminChaptersRowViewModel
            {
                Chapter = chapter,
                SiteSubscriptionExpiresUtc = chapterSubscription?.ExpiresUtc,
                SiteSubscriptionName = chapterSubscription?.SiteSubscription.Name
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
        MemberServiceRequest request, Guid chapterId)
    {
        var platform = request.Platform;

        var (chapter, subscription, siteSubscriptions, sitePaymentSettings) = await GetSiteAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(platform, chapterId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapterId),
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
        MemberServiceRequest request,
        Guid chapterId,
        SiteAdminChapterUpdateViewModel viewModel)
    {
        var platform = request.Platform;

        var (chapter, subscription) = await GetSiteAdminRestrictedContent(request,
            x => x.ChapterRepository.GetById(platform, chapterId),
            x => x.MemberSiteSubscriptionRepository.GetByChapterId(chapterId));

        if (chapter.OwnerId == null)
        {
            throw new OdkServiceException($"Error updating group '{chapter.Id}': owner not found");
        }

        if (viewModel.SiteSubscriptionId == null)
        {
            throw new OdkServiceException($"Error updating group '{chapter.Id}': subscription not provided");
        }

        subscription ??= new MemberSiteSubscription
        {
            MemberId = chapter.OwnerId.Value,
            SiteSubscriptionId = viewModel.SiteSubscriptionId.Value
        };

        subscription.ExpiresUtc = viewModel.SubscriptionExpiresUtc;

        if (subscription.Id == default)
        {
            subscription.Id = Guid.NewGuid();
            _unitOfWork.MemberSiteSubscriptionRepository.Add(subscription);
        }
        else
        {
            _unitOfWork.MemberSiteSubscriptionRepository.Update(subscription);
        }

        await _unitOfWork.SaveChangesAsync();
        return ServiceResult.Successful();
    }
}