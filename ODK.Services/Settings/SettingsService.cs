using ODK.Core.Emails;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Data.Core;
using ODK.Services.Settings.Models;

namespace ODK.Services.Settings;

public class SettingsService : OdkAdminServiceBase, ISettingsService
{
    private readonly IUnitOfWork _unitOfWork;

    public SettingsService(IUnitOfWork unitOfWork)
        : base(unitOfWork)
    {
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

    public async Task<ServiceResult> CreatePaymentSettings(
        Guid currentMemberId,
        PaymentProviderType provider,
        string name,
        string publicKey,
        string secretKey,
        decimal commission,
        bool enabled)
    {
        _unitOfWork.SitePaymentSettingsRepository.Add(new SitePaymentSettings
        {
            ApiPublicKey = publicKey,
            ApiSecretKey = secretKey,
            Commission = commission,
            Enabled = enabled,
            Name = name,
            Provider = provider
        });

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<SiteEmailSettings> GetSiteEmailSettings(PlatformType platform)
    {
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

    public async Task<ServiceResult> UpdateEmailSettings(MemberServiceRequest request, UpdateEmailSettings model)
    {
        var settings = await GetSuperAdminRestrictedContent(request.CurrentMemberId,
            x => x.SiteEmailSettingsRepository.Get(request.Platform));

        settings.ContactEmailAddress = model.ContactEmailAddress;
        settings.FromEmailAddress = model.FromEmailAddress;
        settings.FromName = model.FromEmailName;
        settings.PlatformTitle = model.PlatformTitle;
        settings.Title = model.Title;

        _unitOfWork.SiteEmailSettingsRepository.Update(settings);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdatePaymentSettings(
        Guid currentMemberId,
        Guid id,
        string name,
        string publicKey,
        string secretKey,
        decimal commission,
        bool enabled)
    {
        var settings = await GetSuperAdminRestrictedContent(currentMemberId,
            x => x.SitePaymentSettingsRepository.GetById(id));

        settings.Commission = commission;
        settings.Name = name;
        settings.ApiPublicKey = publicKey;
        settings.ApiSecretKey = secretKey;
        settings.Enabled = enabled;

        _unitOfWork.SitePaymentSettingsRepository.Update(settings);
        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }
}