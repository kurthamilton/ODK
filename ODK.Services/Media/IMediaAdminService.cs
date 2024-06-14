using ODK.Core.Media;

namespace ODK.Services.Media;

public interface IMediaAdminService
{
    Task<IReadOnlyCollection<MediaFile>> DeleteMediaFile(Guid currentMemberId, Guid chapterId, string name);

    Task<IReadOnlyCollection<MediaFile>> GetMediaFiles(Guid currentMemberId, Guid chapterId);

    Task<MediaFile?> SaveMediaFile(Guid currentMemberId, Guid chapterId, string name, byte[] data);
}
