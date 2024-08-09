using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Settings;
using ODK.Data.Core;
using ODK.Services.Users.ViewModels;

namespace ODK.Services.Users;

public class AccountViewModelService : IAccountViewModelService
{
    private readonly IUnitOfWork _unitOfWork;

    public AccountViewModelService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }    

    public async Task<ChapterAccountViewModel> GetChapterAccountViewModel(Guid currentMemberId, string chapterName)
    {
        var (member, chapter) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.ChapterRepository.GetByName(chapterName));

        OdkAssertions.Exists(chapter);
        OdkAssertions.MemberOf(member, chapter.Id);

        return new ChapterAccountViewModel
        {
            ChapterName = chapter.Name,
            CurrentMember = member
        };
    }         

    public async Task<ChapterJoinPageViewModel> GetChapterJoinPage(string chapterName)
    {
        var chapter = await _unitOfWork.ChapterRepository.GetByName(chapterName).RunAsync();
        OdkAssertions.Exists(chapter);
        OdkAssertions.MeetsCondition(chapter, x => x.IsOpenForRegistration());

        var (
                siteSettings,
                chapterProperties,
                chapterPropertyOptions,
                chapterTexts,
                membershipSettings
            ) = await _unitOfWork.RunAsync(
                x => x.SiteSettingsRepository.Get(),
                x => x.ChapterPropertyRepository.GetByChapterId(chapter.Id),
                x => x.ChapterPropertyOptionRepository.GetByChapterId(chapter.Id),
                x => x.ChapterTextsRepository.GetByChapterId(chapter.Id),
                x => x.ChapterMembershipSettingsRepository.GetByChapterId(chapter.Id));

        return new ChapterJoinPageViewModel
        {
            ChapterName = chapter.Name,
            Profile = CreateProfileFormViewModel(chapter, chapterProperties, chapterPropertyOptions, membershipSettings, siteSettings, null, []),
            Texts = chapterTexts
        };
    }

    public async Task<ChapterPicturePageViewModel> GetChapterPicturePage(Guid currentMemberId, string chapterName)
    {
        var chapter = await _unitOfWork.ChapterRepository.GetByName(chapterName).RunAsync();
        OdkAssertions.Exists(chapter);

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

    public async Task<ChapterProfilePageViewModel> GetChapterProfilePage(Guid currentMemberId, string chapterName)
    {        
        var chapter = await _unitOfWork.ChapterRepository.GetByName(chapterName).RunAsync();
        OdkAssertions.Exists(chapter);

        var member = await _unitOfWork.MemberRepository.GetById(currentMemberId).RunAsync();

        var (
                chapterProperties,
                chapterPropertyOptions,
                memberProperties
            ) = await _unitOfWork.RunAsync(
                x => x.ChapterPropertyRepository.GetByChapterId(chapter.Id),
                x => x.ChapterPropertyOptionRepository.GetByChapterId(chapter.Id),
                x => x.MemberPropertyRepository.GetByMemberId(currentMemberId, chapter.Id));

        OdkAssertions.MemberOf(member, chapter.Id);

        return new ChapterProfilePageViewModel
        {
            Chapter = chapter,
            CurrentMember = member,
            ChapterProfile = CreateProfileFormViewModel(
                chapter,
                chapterProperties,
                chapterPropertyOptions,
                null,
                null,
                member,
                memberProperties)
        };
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
        Chapter chapter, 
        IReadOnlyCollection<ChapterProperty> chapterProperties,
        IReadOnlyCollection<ChapterPropertyOption> chapterPropertyOptions,
        ChapterMembershipSettings? membershipSettings,
        SiteSettings? siteSettings,
        Member? member,
        IReadOnlyCollection<MemberProperty> memberProperties)
    {
        var memberPropertyDictionary = memberProperties.ToDictionary(x => x.ChapterPropertyId);

        return new ChapterProfileFormViewModel
        {
            ChapterName = chapter.Name,
            ChapterProperties = chapterProperties,
            ChapterPropertyOptions = chapterPropertyOptions,            
            TrialPeriodMonths = membershipSettings?.TrialPeriodMonths ?? siteSettings?.DefaultTrialPeriodMonths ?? 0,
            Properties = chapterProperties.Select(x => new ChapterProfileFormPropertyViewModel
            {
                ChapterPropertyId = x.Id,
                Value = memberPropertyDictionary.TryGetValue(x.Id, out var memberProperty) ? memberProperty.Value : ""
            }).ToList()
        };
    }
}
