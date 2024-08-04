using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Exceptions;
using ODK.Core.Members;
using ODK.Core.Settings;
using ODK.Data.Core;
using ODK.Services.Caching;
using ODK.Services.Chapters;
using ODK.Services.Users.ViewModels;

namespace ODK.Services.Users;

public class AccountViewModelService : IAccountViewModelService
{
    private readonly IUnitOfWork _unitOfWork;

    public AccountViewModelService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AccountPageViewModel> GetAccountPageViewModel(Guid currentMemberId, string chapterName)
    {
        var chapter = await _unitOfWork.ChapterRepository.GetByName(chapterName).RunAsync();
        OdkAssertions.Exists(chapter);

        var (
                chapterProperties, 
                chapterPropertyOptions, 
                member, 
                memberProperties,
                avatar
            ) = await _unitOfWork.RunAsync(            
                x => x.ChapterPropertyRepository.GetByChapterId(chapter.Id),
                x => x.ChapterPropertyOptionRepository.GetByChapterId(chapter.Id),
                x => x.MemberRepository.GetById(currentMemberId),
                x => x.MemberPropertyRepository.GetByMemberId(currentMemberId, chapter.Id),
                x => x.MemberAvatarRepository.GetByMemberId(currentMemberId));

        OdkAssertions.MeetsCondition(chapter, x => member.IsMemberOf(chapter.Id));        

        return new AccountPageViewModel 
        { 
            Avatar = avatar,
            Chapter = chapter,
            CurrentMember = member,
            Profile = CreateProfileFormViewModel(
                chapter, 
                chapterProperties, 
                chapterPropertyOptions, 
                null, 
                null,
                member, 
                memberProperties)
        };
    }

    public async Task<AccountViewModel> GetAccountViewModel(Guid currentMemberId, string chapterName)
    {
        var (member, chapter) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.ChapterRepository.GetByName(chapterName));

        OdkAssertions.MeetsCondition(chapter, x => member.IsMemberOf(chapter.Id));

        return new AccountViewModel 
        { 
            ChapterName = chapter.Name,
            CurrentMember = member 
        };
    }

    public async Task<JoinPageViewModel> GetJoinPageViewModel(string chapterName)
    {
        var chapter = await _unitOfWork.ChapterRepository.GetByName(chapterName).RunAsync();
        OdkAssertions.Exists(chapter);

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

        return new JoinPageViewModel
        {
            ChapterName = chapter.Name,
            Texts = chapterTexts,
            Profile = CreateProfileFormViewModel(chapter, chapterProperties, chapterPropertyOptions, membershipSettings, siteSettings, null, []),
        };
    }

    private ProfileFormViewModel CreateProfileFormViewModel(
        Chapter chapter, 
        IReadOnlyCollection<ChapterProperty> chapterProperties,
        IReadOnlyCollection<ChapterPropertyOption> chapterPropertyOptions,
        ChapterMembershipSettings? membershipSettings,
        SiteSettings? siteSettings,
        Member? member,
        IReadOnlyCollection<MemberProperty> memberProperties)
    {
        var memberPropertyDictionary = memberProperties.ToDictionary(x => x.ChapterPropertyId);

        return new ProfileFormViewModel
        {
            ChapterName = chapter.Name,
            ChapterProperties = chapterProperties,
            ChapterPropertyOptions = chapterPropertyOptions,
            TrialPeriodMonths = membershipSettings?.TrialPeriodMonths ?? siteSettings?.DefaultTrialPeriodMonths ?? 0,

            EmailAddress = member?.EmailAddress ?? "",
            EmailOptIn = member?.EmailOptIn ?? true,
            FirstName = member?.FirstName ?? "",
            LastName = member?.LastName ?? "",
            Properties = chapterProperties.Select(x => new ProfileFormPropertyViewModel
            {
                ChapterPropertyId = x.Id,
                Value = memberPropertyDictionary.TryGetValue(x.Id, out var memberProperty) ? memberProperty.Value : ""
            }).ToList()
        };
    }
}
