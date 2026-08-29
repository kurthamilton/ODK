using ODK.Core.Emails;
using ODK.Services.Settings.Models;

namespace ODK.Services.Settings;

public interface ISettingsService
{
    Task<SiteEmailSettings> GetSiteEmailSettings(IMemberServiceRequest request);

    Task<ServiceResult> UpdateEmailSettings(IMemberServiceRequest request, EmailSettingsUpdateModel model);
}