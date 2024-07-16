using ODK.Core.Chapters;
using ODK.Core.Settings;
using ODK.Data.Core;

namespace ODK.Services.Settings;

public class SettingsService : OdkAdminServiceBase, ISettingsService
{
    private readonly IUnitOfWork _unitOfWork;

    public SettingsService(IUnitOfWork unitOfWork)
        : base(unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SiteSettings> GetSiteSettings()
    {
        var settings = await _unitOfWork.SiteSettingsRepository.Get().RunAsync();
        return settings!;
    }

    public async Task<ServiceResult> UpdateInstagramSettings(Guid chapterId, Guid currentMemberId, bool scrape,
        string scraperUserAgent)
    {
        var (chapterAdminMembers, currentMember, settings) = await _unitOfWork.RunAsync(
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.SiteSettingsRepository.Get());

        AssertMemberIsChapterSuperAdmin(currentMember, chapterId, chapterAdminMembers);

        settings.ScrapeInstagram = scrape;
        settings.InstagramScraperUserAgent = scraperUserAgent;

        _unitOfWork.SiteSettingsRepository.Update(settings);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }
}
