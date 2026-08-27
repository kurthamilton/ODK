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

    public async Task<ServiceResult> ActivatePaymentSettings(IMemberServiceRequest request, Guid id)
    {
        // Scoped to the platform, so activating its settings cannot deactivate another platform's.
        var paymentSettings = await GetSiteAdminRestrictedContent(request,
            x => x.SitePaymentSettingsRepository.GetAll(request.Platform));

        var activating = paymentSettings.FirstOrDefault(x => x.Id == id);
        if (activating == null)
        {
            return ServiceResult.Failure("Id not found");
        }

        /* Checked before anything is deactivated: the platform resolves payments through its active row, so
           activating a row nothing can be bought through would leave it unable to take a payment at all. */
        if (!activating.Enabled)
        {
            return ServiceResult.Failure("Disabled payment settings cannot be made active");
        }

        foreach (var paymentSetting in paymentSettings)
        {
            paymentSetting.Active = paymentSetting.Id == id;
        }

        _unitOfWork.SitePaymentSettingsRepository.UpdateMany(paymentSettings);
        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> CreatePaymentSettings(
        IMemberServiceRequest request, SitePaymentSettingsCreateModel model)
    {
        AssertMemberIsSiteAdmin(request.CurrentMember);

        _unitOfWork.SitePaymentSettingsRepository.Add(new SitePaymentSettings
        {
            ApiPublicKey = model.ApiPublicKey,
            ApiSecretKey = model.ApiSecretKey,
            Commission = model.Commission,
            Enabled = model.Enabled,
            Environment = model.Environment,
            ExternalId = model.ExternalId,
            ExternalUrl = model.ExternalUrl,
            Name = model.Name,
            Platform = request.Platform,
            Provider = model.Provider
        });

        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    public async Task<SiteEmailSettings> GetSiteEmailSettings(IMemberServiceRequest request)
    {
        return await GetSiteAdminRestrictedContent(request,
            x => x.SiteEmailSettingsRepository.Get(request.Platform));
    }

    public async Task<IReadOnlyCollection<SitePaymentSettings>> GetSitePaymentSettings(IMemberServiceRequest request)
    {
        return await GetSiteAdminRestrictedContent(request,
            x => x.SitePaymentSettingsRepository.GetAll(request.Platform));
    }

    public async Task<SitePaymentSettings> GetSitePaymentSettings(IMemberServiceRequest request, Guid id)
    {
        return await GetSiteAdminRestrictedContent(request,
            x => x.SitePaymentSettingsRepository.GetById(id));
    }

    public async Task<ServiceResult> UpdateEmailSettings(IMemberServiceRequest request, EmailSettingsUpdateModel model)
    {
        var settings = await GetSiteAdminRestrictedContent(request,
            x => x.SiteEmailSettingsRepository.Get(request.Platform));

        settings.AdminTitle = model.AdminTitle;
        settings.FromEmailAddress = model.FromEmailAddress;
        settings.MemberTitle = model.MemberTitle;

        _unitOfWork.SiteEmailSettingsRepository.Update(settings);
        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> UpdatePaymentSettings(
        IMemberServiceRequest request, Guid id, SitePaymentSettingsUpdateModel model)
    {
        var settings = await GetSiteAdminRestrictedContent(request,
            x => x.SitePaymentSettingsRepository.GetById(id));

        // The platform transacts through its active row, so disabling that row would stop it taking payments.
        if (settings.Active && !model.Enabled)
        {
            return ServiceResult.Failure("Active payment settings cannot be disabled");
        }

        settings.ApiPublicKey = model.ApiPublicKey;
        settings.ApiSecretKey = model.ApiSecretKey;
        settings.Commission = model.Commission;
        settings.Enabled = model.Enabled;
        settings.Environment = model.Environment;
        settings.ExternalId = model.ExternalId;
        settings.ExternalUrl = model.ExternalUrl;
        settings.Name = model.Name;

        _unitOfWork.SitePaymentSettingsRepository.Update(settings);
        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }
}