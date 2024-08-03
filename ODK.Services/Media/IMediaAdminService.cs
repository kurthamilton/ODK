using ODK.Core.Media;

namespace ODK.Services.Media;

public interface IMediaAdminService
{
    Task<IReadOnlyCollection<MediaFile>> DeleteMediaFile(AdminServiceRequest request, string name);

    Task<IReadOnlyCollection<MediaFile>> GetMediaFiles(AdminServiceRequest request);

    Task<MediaFile?> SaveMediaFile(AdminServiceRequest request, string name, byte[] data);
}
