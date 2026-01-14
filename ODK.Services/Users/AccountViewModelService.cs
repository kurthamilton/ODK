using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Data.Core;
using ODK.Services.Users.ViewModels;

namespace ODK.Services.Users;

public class AccountViewModelService : IAccountViewModelService
{
    private readonly AccountViewModelServiceSettings _settings;
    private readonly IUnitOfWork _unitOfWork;

    public AccountViewModelService(
        IUnitOfWork unitOfWork,
        AccountViewModelServiceSettings settings)
    {
        _settings = settings;
        _unitOfWork = unitOfWork;
    }

    public async Task<AccountCreatePageViewModel> GetAccountCreatePage()
    {
        var (topicGroups, topics) = await _unitOfWork.RunAsync(
            x => x.TopicGroupRepository.GetAll(),
            x => x.TopicRepository.GetAll());

        return new AccountCreatePageViewModel
        {
            GoogleClientId = _settings.GoogleClientId,
            TopicGroups = topicGroups,
            Topics = topics
        };
    }

    public async Task<ChapterJoinPageViewModel> GetChapterJoinPage(
        ServiceRequest request, Chapter chapter)
    {
        var platform = request.Platform;

        var (
                chapterProperties,
                chapterPropertyOptions,
                chapterTexts
            ) = await _unitOfWork.RunAsync(
                x => x.ChapterPropertyRepository.GetByChapterId(chapter.Id),
                x => x.ChapterPropertyOptionRepository.GetByChapterId(chapter.Id),
                x => x.ChapterTextsRepository.GetByChapterId(chapter.Id));

        return new ChapterJoinPageViewModel
        {
            Chapter = chapter,
            Profile = CreateProfileFormViewModel(
                request.Platform,
                chapter,
                chapterProperties,
                chapterPropertyOptions,
                null,
                []),
            Texts = chapterTexts
        };
    }

    public Task<ChapterLoginPageViewModel> GetChapterLoginPage()
    {
        return Task.FromResult(new ChapterLoginPageViewModel
        {
            GoogleClientId = _settings.GoogleClientId
        });
    }

    public async Task<ChapterPicturePageViewModel> GetChapterPicturePage(
        Guid currentMemberId, Chapter chapter)
    {
        var (
                member,
                avatar,
                image
            ) = await _unitOfWork.RunAsync(
                x => x.MemberRepository.GetById(currentMemberId),
                x => x.MemberAvatarRepository.GetByMemberId(currentMemberId),
                x => x.MemberImageRepository.GetByMemberId(currentMemberId));

        OdkAssertions.MemberOf(member, chapter.Id);

        return new ChapterPicturePageViewModel
        {
            Avatar = avatar,
            Image = image,
            Chapter = chapter,
            CurrentMember = member
        };
    }

    public async Task<ChapterProfilePageViewModel> GetChapterProfilePage(
        MemberServiceRequest request, Chapter chapter)
    {
        var (currentMemberId, platform) = (request.CurrentMemberId, request.Platform);

        var member = await _unitOfWork.MemberRepository.GetById(currentMemberId).Run();

        var (
                chapterProperties,
                chapterPropertyOptions,
                memberProperties
            ) = await _unitOfWork.RunAsync(
                x => x.ChapterPropertyRepository.GetByChapterId(chapter.Id),
                x => x.ChapterPropertyOptionRepository.GetByChapterId(chapter.Id),
                x => x.MemberPropertyRepository.GetByMemberId(currentMemberId, chapter.Id));

        OdkAssertions.MemberOf(member, chapter.Id);

        chapterProperties = chapterProperties
            .Where(x => !x.ApplicationOnly)
            .OrderBy(x => x.DisplayOrder)
            .ToArray();

        return new ChapterProfilePageViewModel
        {
            Chapter = chapter,
            CurrentMember = member,
            ChapterProfile = CreateProfileFormViewModel(
                platform,
                chapter,
                chapterProperties,
                chapterPropertyOptions,
                member,
                memberProperties)
        };
    }

    public async Task<MemberChapterPaymentsPageViewModel> GetMemberChapterPaymentsPage(
        MemberServiceRequest request, Chapter chapter)
    {
        var (currentMemberId, platform) = (request.CurrentMemberId, request.Platform);

        var (member, payments) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.PaymentRepository.GetChapterDtosByMemberId(currentMemberId));

        return new MemberChapterPaymentsPageViewModel
        {
            Chapter = chapter,
            CurrentMember = member,
            Payments = payments
                .Select(x => new MemberPaymentsPageViewModelPayment(x))
                .ToArray(),
            Platform = platform
        };
    }

    public async Task<MemberEmailPreferencesPageViewModel> GetMemberEmailPreferencesPage(
        MemberServiceRequest request)
    {
        var (currentMemberId, platform) = (request.CurrentMemberId, request.Platform);

        var (member, preferences) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.MemberEmailPreferenceRepository.GetByMemberId(currentMemberId));

        return new MemberEmailPreferencesPageViewModel
        {
            CurrentMember = member,
            Platform = platform,
            Preferences = preferences
        };
    }

    public async Task<MemberPaymentsPageViewModel> GetMemberPaymentsPage(MemberServiceRequest request)
    {
        var (currentMemberId, platform) = (request.CurrentMemberId, request.Platform);

        var (member, chapterPayments, sitePayments) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.PaymentRepository.GetChapterDtosByMemberId(currentMemberId),
            x => x.PaymentRepository.GetSitePaymentsByMemberId(currentMemberId));

        return new MemberPaymentsPageViewModel
        {
            CurrentMember = member,
            Payments = chapterPayments
                .Select(x => new MemberPaymentsPageViewModelPayment(x))
                .Union(sitePayments.Select(x => new MemberPaymentsPageViewModelPayment(x)))
                .ToArray(),
            Platform = platform
        };
    }

    public Task<SiteLoginPageViewModel> GetSiteLoginPage()
    {
        return Task.FromResult(new SiteLoginPageViewModel
        {
            GoogleClientId = _settings.GoogleClientId
        });
    }

    public async Task<SitePicturePageViewModel> GetSitePicturePage(Guid currentMemberId)
    {
        var (member, avatar, image) = await _unitOfWork.RunAsync(
             x => x.MemberRepository.GetById(currentMemberId),
             x => x.MemberAvatarRepository.GetByMemberId(currentMemberId),
             x => x.MemberImageRepository.GetByMemberId(currentMemberId));

        return new SitePicturePageViewModel
        {
            Avatar = avatar,
            CurrentMember = member,
            Image = image
        };
    }

    private ChapterProfileFormViewModel CreateProfileFormViewModel(
        PlatformType platform,
        Chapter chapter,
        IReadOnlyCollection<ChapterProperty> chapterProperties,
        IReadOnlyCollection<ChapterPropertyOption> chapterPropertyOptions,
        Member? member,
        IReadOnlyCollection<MemberProperty> memberProperties)
    {
        var memberPropertyDictionary = memberProperties.ToDictionary(x => x.ChapterPropertyId);

        return new ChapterProfileFormViewModel
        {
            ChapterName = chapter.GetDisplayName(platform),
            ChapterProperties = chapterProperties,
            ChapterPropertyOptions = chapterPropertyOptions,
            Properties = chapterProperties.Select(x => new ChapterProfileFormPropertyViewModel
            {
                ChapterPropertyId = x.Id,
                Value = memberPropertyDictionary.TryGetValue(x.Id, out var memberProperty) ? memberProperty.Value : string.Empty
            }).ToList()
        };
    }
}