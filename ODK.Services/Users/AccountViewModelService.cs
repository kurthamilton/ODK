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
        IChapterServiceRequest request)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var (
            chapterProperties,
            chapterPropertyOptions,
            chapterTexts) = await _unitOfWork.RunAsync(
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
        IMemberChapterServiceRequest request)
    {
        var (chapter, currentMember) = (request.Chapter, request.CurrentMember);

        var (avatar, image) = await _unitOfWork.RunAsync(
            x => x.MemberAvatarRepository.GetVersionDtoByMemberId(currentMember.Id),
            x => x.MemberImageRepository.GetVersionDtoByMemberId(currentMember.Id));

        OdkAssertions.MemberOf(currentMember, chapter.Id);

        return new ChapterPicturePageViewModel
        {
            AvatarVersion = avatar?.Version,
            ImageVersion = image?.Version,
            Chapter = chapter,
            CurrentMember = currentMember
        };
    }

    public async Task<ChapterProfilePageViewModel> GetChapterProfilePage(
        IMemberChapterServiceRequest request)
    {
        var (currentMember, platform, chapter) = (request.CurrentMember, request.Platform, request.Chapter);

        var (
                chapterProperties,
                chapterPropertyOptions,
                memberProperties
            ) = await _unitOfWork.RunAsync(
                x => x.ChapterPropertyRepository.GetByChapterId(chapter.Id),
                x => x.ChapterPropertyOptionRepository.GetByChapterId(chapter.Id),
                x => x.MemberPropertyRepository.GetByMemberId(currentMember.Id, chapter.Id));

        OdkAssertions.MemberOf(currentMember, chapter.Id);

        chapterProperties = chapterProperties
            .Where(x => !x.ApplicationOnly)
            .OrderBy(x => x.DisplayOrder)
            .ToArray();

        return new ChapterProfilePageViewModel
        {
            Chapter = chapter,
            CurrentMember = currentMember,
            ChapterProfile = CreateProfileFormViewModel(
                platform,
                chapter,
                chapterProperties,
                chapterPropertyOptions,
                currentMember,
                memberProperties)
        };
    }

    public async Task<MemberChapterPaymentsPageViewModel> GetMemberChapterPaymentsPage(
        IMemberChapterServiceRequest request)
    {
        var (currentMember, platform, chapter) = (request.CurrentMember, request.Platform, request.Chapter);

        var payments = await _unitOfWork.PaymentRepository.GetChapterDtosByMemberId(currentMember.Id).Run();

        return new MemberChapterPaymentsPageViewModel
        {
            Chapter = chapter,
            CurrentMember = currentMember,
            Payments = payments
                .Select(x => new MemberPaymentsPageViewModelPayment(x))
                .ToArray(),
            Platform = platform
        };
    }

    public async Task<MemberEmailPreferencesPageViewModel> GetMemberEmailPreferencesPage(
        IMemberServiceRequest request)
    {
        var (currentMember, platform) = (request.CurrentMember, request.Platform);

        var preferences = await _unitOfWork.MemberEmailPreferenceRepository.GetByMemberId(currentMember.Id).Run();

        return new MemberEmailPreferencesPageViewModel
        {
            CurrentMember = currentMember,
            Platform = platform,
            Preferences = preferences
        };
    }

    public async Task<MemberPaymentsPageViewModel> GetMemberPaymentsPage(IMemberServiceRequest request)
    {
        var (currentMember, platform) = (request.CurrentMember, request.Platform);

        var (chapterPayments, sitePayments) = await _unitOfWork.RunAsync(
            x => x.PaymentRepository.GetChapterDtosByMemberId(currentMember.Id),
            x => x.PaymentRepository.GetSitePaymentsByMemberId(currentMember.Id));

        return new MemberPaymentsPageViewModel
        {
            CurrentMember = currentMember,
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
             x => x.MemberAvatarRepository.GetVersionDtoByMemberId(currentMemberId),
             x => x.MemberImageRepository.GetVersionDtoByMemberId(currentMemberId));

        return new SitePicturePageViewModel
        {
            AvatarVersion = avatar?.Version,
            CurrentMember = member,
            ImageVersion = image?.Version
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