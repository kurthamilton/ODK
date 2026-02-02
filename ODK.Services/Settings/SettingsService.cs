using ODK.Core.Emails;
using ODK.Core.Payments;
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

    public async Task<ServiceResult> ActivatePaymentSettings(MemberServiceRequest request, Guid id)
    {
        var paymentSettings = await GetSiteAdminRestrictedContent(request,
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
        MemberServiceRequest request,
        PaymentProviderType provider,
        string name,
        string publicKey,
        string secretKey,
        decimal commission,
        bool enabled)
    {
        AssertMemberIsSiteAdmin(request.CurrentMember);

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

    public async Task<SiteEmailSettings> GetSiteEmailSettings(MemberServiceRequest request)
    {
        return await GetSiteAdminRestrictedContent(request, 
            x => x.SiteEmailSettingsRepository.Get(request.Platform));
    }

    public async Task<IReadOnlyCollection<SitePaymentSettings>> GetSitePaymentSettings(MemberServiceRequest request)
    {
        return await GetSiteAdminRestrictedContent(request,
            x => x.SitePaymentSettingsRepository.GetAll());
    }

    public async Task<SitePaymentSettings> GetSitePaymentSettings(MemberServiceRequest request, Guid id)
    {
        return await GetSiteAdminRestrictedContent(request,
            x => x.SitePaymentSettingsRepository.GetById(id));
    }

    public async Task<ServiceResult> UpdateEmailSettings(MemberServiceRequest request, EmailSettingsUpdateModel model)
    {
        var settings = await GetSiteAdminRestrictedContent(request,
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
        MemberServiceRequest request,
        Guid id,
        string name,
        string publicKey,
        string secretKey,
        decimal commission,
        bool enabled)
    {
        var settings = await GetSiteAdminRestrictedContent(request,
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