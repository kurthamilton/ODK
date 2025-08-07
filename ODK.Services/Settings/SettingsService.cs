using ODK.Core.Emails;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Core.Settings;
using ODK.Data.Core;

namespace ODK.Services.Settings;

public class SettingsService : OdkAdminServiceBase, ISettingsService
{
    private readonly IPlatformProvider _platformProvider;
    private readonly IUnitOfWork _unitOfWork;

    public SettingsService(IUnitOfWork unitOfWork, IPlatformProvider platformProvider)
        : base(unitOfWork)
    {
        _platformProvider = platformProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> ActivatePaymentSettings(Guid currentMemberId, Guid id)
    {
        var paymentSettings = await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.SitePaymentSettingsRepository.GetAll());

        foreach (var paymentSetting in paymentSettings)
        {
            paymentSetting.Active = paymentSetting.Id == id;
        }

        if (paymentSettings.All(x => !x.Active))
        {
            return ServiceResult.Failure("Id not found");
        }

        _unitOfWork.SitePaymentSettingsRepository.UpdateMany(paymentSettings);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
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

        var isValid = provider.IsValid();
        if (!isValid)
        {
            return ServiceResult.Failure("Some required fields are missing");
        }

        _unitOfWork.EmailProviderRepository.Add(provider);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> CreatePaymentSettings(Guid currentMemberId,
        PaymentProviderType provider, string name, string publicKey, string secretKey, decimal commission)
    {
        _unitOfWork.SitePaymentSettingsRepository.Add(new SitePaymentSettings
        {
            ApiPublicKey = publicKey,
            ApiSecretKey = secretKey,
            Commission = commission,
            Name = name,
            Provider = provider
        });

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
        return await _unitOfWork.SiteSettingsRepository.Get().Run();        
    }

    public async Task<SiteEmailSettings> GetSiteEmailSettings()
    {
        var platform = _platformProvider.GetPlatform();
        return await _unitOfWork.SiteEmailSettingsRepository.Get(platform).Run();
    }    

    public async Task<IReadOnlyCollection<SitePaymentSettings>> GetSitePaymentSettings(Guid currentMemberId)
    {
        return await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.SitePaymentSettingsRepository.GetAll());
    }

    public async Task<SitePaymentSettings> GetSitePaymentSettings(Guid currentMemberId, Guid id)
    {
        return await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.SitePaymentSettingsRepository.GetById(id));
    }

    public async Task<ServiceResult> UpdateEmailSettings(Guid currentMemberId, 
        string fromEmailAddress, 
        string fromEmailName, 
        string emailTitle,
        string contactEmailAddress)
    {
        var platform = _platformProvider.GetPlatform();

        var settings = await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.SiteEmailSettingsRepository.Get(platform));

        settings.ContactEmailAddress = contactEmailAddress;
        settings.FromEmailAddress = fromEmailAddress;
        settings.FromName = fromEmailName;
        settings.Title = emailTitle;

        _unitOfWork.SiteEmailSettingsRepository.Update(settings);
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

        var isValid = provider.IsValid();
        if (!isValid)
        {
            return ServiceResult.Failure("Some required fields are missing");
        }

        _unitOfWork.EmailProviderRepository.Update(provider);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
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

    public async Task<ServiceResult> UpdatePaymentSettings(Guid currentMemberId, 
        Guid id, string name, string publicKey, string secretKey, decimal commission)
    {
        var settings = await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.SitePaymentSettingsRepository.GetById(id));

        settings.Commission = commission;
        settings.Name = name;
        settings.ApiPublicKey = publicKey;
        settings.ApiSecretKey = secretKey;

        _unitOfWork.SitePaymentSettingsRepository.Update(settings);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }
}
