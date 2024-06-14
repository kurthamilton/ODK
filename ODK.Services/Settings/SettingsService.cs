using ODK.Core.Chapters;
using ODK.Core.Settings;

namespace ODK.Services.Settings;

public class SettingsService : OdkAdminServiceBase, ISettingsService
{
    private readonly ISettingsRepository _settingsRepository;

    public SettingsService(ISettingsRepository settingsRepository,
        IChapterRepository chapterRepository)
        : base(chapterRepository)
    {
        _settingsRepository = settingsRepository;
    }

    public async Task<SiteSettings> GetSiteSettings()
    {
        SiteSettings? settings = await _settingsRepository.GetSiteSettings();
        return settings!;
    }

    public async Task<ServiceResult> UpdateInstagramSettings(Guid chapterId, Guid currentMemberId, bool scrape,
        string scraperUserAgent)
    {
        await AssertMemberIsChapterSuperAdmin(currentMemberId, chapterId);

        SiteSettings settings = await GetSiteSettings();

        SiteSettings update = new SiteSettings(settings.GoogleMapsApiKey, settings.InstagramUsername,
            settings.InstagramPassword, scraperUserAgent, scrape, 0, settings.RecaptchaSiteKey,
            settings.RecaptchaSecretKey);

        await _settingsRepository.UpdateSiteSettings(update);

        return ServiceResult.Successful();
    }
}
