using ODK.Services.Geolocation.ViewModels;

namespace ODK.Services.Geolocation;

public interface IIpLocationDatabaseService
{
    Task<ServiceResult> Delete(IMemberServiceRequest request, string fileName);

    Task<IpLocationDatabaseViewModel> GetViewModel(IMemberServiceRequest request);

    Task<ServiceResult> Update();
}
