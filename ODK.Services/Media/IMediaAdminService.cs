using ODK.Core.Media;

namespace ODK.Services.Media;

public interface IMediaAdminService
{
    Task<IReadOnlyCollection<MediaFile>> DeleteMediaFile(MemberChapterServiceRequest request, string name);

    Task<IReadOnlyCollection<MediaFile>> GetMediaFiles(MemberChapterServiceRequest request);

    Task<MediaFile?> SaveMediaFile(MemberChapterServiceRequest request, string name, byte[] data);
}
