using ODK.Core.Emails;
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

    public async Task<SiteEmailSettings> GetSiteEmailSettings(IMemberServiceRequest request)
    {
        return await GetSiteAdminRestrictedContent(request,
            x => x.SiteEmailSettingsRepository.Get(request.Platform));
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
}