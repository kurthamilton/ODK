using ODK.Core.Emails;
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

    public async Task<ServiceResult> AddEmailProvider(Guid currentMemberId, UpdateEmailProvider model)
    {
        var existing = await GetSuperAdminRestrictedContent(currentMemberId, 
            x => x.EmailProviderRepository.GetAll());

        var provider = new EmailProvider
        {
            BatchSize = model.BatchSize,
            DailyLimit = model.DailyLimit,
            Order = existing.Count + 1,
            SmtpLogin = model.SmtpLogin,
            SmtpPassword = model.SmtpPassword,
            SmtpPort = model.SmtpPort,
            SmtpServer = model.SmtpServer
        };

        var validationResult = ValidateEmailProvider(provider);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        _unitOfWork.EmailProviderRepository.Add(provider);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> DeleteEmailProvider(Guid currentMemberId, Guid emailProviderId)
    {
        var provider = await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.EmailProviderRepository.GetById(emailProviderId));

        _unitOfWork.EmailProviderRepository.Delete(provider);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<EmailProvider> GetEmailProvider(Guid currentMemberId, Guid emailProviderId)
    {
        return await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.EmailProviderRepository.GetById(emailProviderId));
    }

    public async Task<IReadOnlyCollection<EmailProvider>> GetEmailProviders(Guid currentMemberId)
    {
        return await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.EmailProviderRepository.GetAll());
    }

    public async Task<SiteSettings> GetSiteSettings()
    {
        var settings = await _unitOfWork.SiteSettingsRepository.Get().RunAsync();
        return settings!;
    }

    public async Task<ServiceResult> UpdateInstagramSettings(Guid currentMemberId, string scraperUserAgent)
    {
        var settings = await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.SiteSettingsRepository.Get());

        settings.InstagramScraperUserAgent = scraperUserAgent;

        _unitOfWork.SiteSettingsRepository.Update(settings);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdateEmailProvider(Guid currentMemberId, Guid emailProviderId, UpdateEmailProvider model)
    {
        var provider = await GetEmailProvider(currentMemberId, emailProviderId);

        provider.BatchSize = model.BatchSize;
        provider.DailyLimit = model.DailyLimit;
        provider.SmtpLogin = model.SmtpLogin;
        provider.SmtpPassword = model.SmtpPassword;
        provider.SmtpPort = model.SmtpPort;
        provider.SmtpServer = model.SmtpServer;

        var validationResult = ValidateEmailProvider(provider);
        if (!validationResult.Success)
        {
            return validationResult;
        }

        _unitOfWork.EmailProviderRepository.Update(provider);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    private static ServiceResult ValidateEmailProvider(EmailProvider provider)
    {
        if (string.IsNullOrWhiteSpace(provider.SmtpLogin) ||
            string.IsNullOrWhiteSpace(provider.SmtpPassword) ||
            provider.SmtpPort == 0 ||
            provider.DailyLimit <= 0 ||
            provider.BatchSize <= 0)
        {
            return ServiceResult.Failure("Some required fields are missing");
        }

        return ServiceResult.Successful();
    }
}
