using ODK.Core.Media;

namespace ODK.Services.Media;

public interface IMediaFileProvider
{
    Task<MediaFile?> GetMediaFile(Guid chapterId, string name);

    Task<string?> GetMediaFilePath(Guid chapterId, string name);

    Task<IReadOnlyCollection<MediaFile>> GetMediaFiles(Guid chapterId);
}
