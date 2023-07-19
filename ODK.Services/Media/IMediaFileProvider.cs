using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Core.Media;

namespace ODK.Services.Media
{
    public interface IMediaFileProvider
    {
        Task<MediaFile> GetMediaFile(Guid chapterId, string name);

        Task<MediaFile> GetMediaFile(string chapter, string name);

        Task<string> GetMediaFilePath(Guid chapterId, string name);

        Task<IReadOnlyCollection<MediaFile>> GetMediaFiles(Guid chapterId);
    }
}
